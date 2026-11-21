using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Quản lý ma trận ô cờ (Grid/Tilemap), kiểm tra va chạm, xóa dòng, tích hợp Skill Effect và vòng đời khối gạch (SRP).
/// Đã loại bỏ GameObject.Find, hỗ trợ 4 khối Next Queue và tối ưu Coroutine/Buffer để giảm thiểu GC (Zero-GC).
/// </summary>
public class Board : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TetrisGameSettingsSO _gameSettings;

    [Header("Tetromino Data")]
    public TetrominoData[] tetrominoes;

    [Header("Tilemaps & Display")]
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private Tilemap _nextPieceTilemap;
    [SerializeField] private Tilemap _holdPieceTilemap;

    [Header("Next Queue Settings")]
    [SerializeField] private int _nextQueuePreviewCount = 4;
    [SerializeField] private int _nextPieceVerticalSpacing = 4;

    [Header("Line Clear Animation Settings")]
    [SerializeField] private float _lineClearFlashDuration = 0.3f;
    [SerializeField] private int _lineClearFlashCount = 3;

    // Events theo nguyên lý DIP để PlayfieldContainer lắng nghe mà không phụ thuộc Singleton
    public event Action<int, List<int>> OnLinesCleared;
    public event Action OnTopOut;
    public event Action<IReadOnlyList<TetrominoData>> OnNextQueueUpdated;
    public event Action OnBoardCleared;
    public event Action OnActiveSkillExecuted;

    private Piece _activePiece;
    private HoldPieceManager _holdPieceManager;
    private IPieceRandomizer _pieceRandomizer;
    private Coroutine _lockCoroutine;

    private bool _isManagedByContainer;
    private bool _isSubscribedToGameManager;
    private bool _isBoardOver;

    // Trạng thái Skill đang gắn trên khối gạch hiện tại
    private ISkillEffect _activeSkillEffect;
    private TileBase _activeSkillOverrideTile;
    private int _lastSkillLinesCleared;
    private Action<int> _cachedOnSkillLinesCleared;

    // Reusable buffers & cached WaitForSeconds giúp giảm tối đa GC khi xóa dòng / thực thi Skill
    private readonly List<TetrominoData> _nextQueueBuffer = new List<TetrominoData>(4);
    private readonly List<int> _clearedRowsBuffer = new List<int>(4);
    private readonly List<int> _skillAffectedRowsBuffer = new List<int>(8);
    private readonly List<int> _sortedRowsBuffer = new List<int>(8);
    private readonly List<Vector3Int> _flashPositionsBuffer = new List<Vector3Int>(40);
    private readonly List<TileBase> _flashTilesBuffer = new List<TileBase>(40);
    private WaitForSeconds _cachedFlashWait;
    private float _cachedFlashInterval = -1f;

    // PascalCase Properties (Chuẩn C#)
    public Tilemap Tilemap => _tilemap;
    public Piece ActivePiece => _activePiece;
    public HoldPieceManager HoldManager => _holdPieceManager;
    public bool IsAnimatingLineClear { get; private set; }
    public bool HasActiveSkillModifier => _activeSkillEffect != null;
    public Vector2Int BoardSize => _gameSettings != null ? _gameSettings.BoardSize : new Vector2Int(10, 20);
    public Vector3Int SpawnPosition => _gameSettings != null ? _gameSettings.SpawnPosition : new Vector3Int(-1, 8, 0);

    public RectInt Bounds
    {
        get
        {
            Vector2Int size = BoardSize;
            Vector2Int pos = new Vector2Int(-size.x / 2, -size.y / 2);
            return new RectInt(pos, size);
        }
    }

    // Backward-compatibility aliases
    public Tilemap tilemap => _tilemap;
    public Piece activePiece => _activePiece;
    public Vector2Int boardSize
    {
        get => BoardSize;
        set { }
    }
    public Vector3Int spawnPosition
    {
        get => SpawnPosition;
        set { }
    }
    public Tilemap nextPieceTilemap
    {
        get => _nextPieceTilemap;
        set => _nextPieceTilemap = value;
    }
    public Tilemap holdPieceTilemap
    {
        get => _holdPieceTilemap;
        set => _holdPieceTilemap = value;
    }

    private void Awake()
    {
        _cachedOnSkillLinesCleared = HandleSkillLinesClearedCallback;

        _activePiece = GetComponentInChildren<Piece>();
        if (_tilemap == null)
        {
            _tilemap = GetComponentInChildren<Tilemap>();
        }

        _holdPieceManager = GetComponent<HoldPieceManager>();
        if (_holdPieceManager == null)
        {
            _holdPieceManager = gameObject.AddComponent<HoldPieceManager>();
        }

        _holdPieceManager.Initialize(_holdPieceTilemap);

        _pieceRandomizer = new SevenBagRandomizer();
        if (tetrominoes != null)
        {
            for (int i = 0; i < tetrominoes.Length; i++)
            {
                tetrominoes[i].Initialize();
            }
            _pieceRandomizer.Initialize(tetrominoes);
        }

        ApplySettings(_gameSettings);
    }

    private void Start()
    {
        if (!_isManagedByContainer)
        {
            SubscribeToGameManager();
        }

        if (_activePiece != null)
        {
            _activePiece.OnPieceLocked += HandlePieceLocked;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromGameManager();

        if (_activePiece != null)
        {
            _activePiece.OnPieceLocked -= HandlePieceLocked;
        }
    }

    /// <summary>
    /// Tiêm toàn bộ cấu hình và Tilemap phụ thuộc từ PlayfieldContainer (DIP).
    /// </summary>
    public void Initialize(
        TetrisGameSettingsSO settings,
        Tilemap nextPieceTilemap,
        Tilemap holdPieceTilemap,
        int nextQueuePreviewCount = 4)
    {
        SetManagedByContainer(true);

        if (nextPieceTilemap != null) _nextPieceTilemap = nextPieceTilemap;
        if (holdPieceTilemap != null) _holdPieceTilemap = holdPieceTilemap;
        _nextQueuePreviewCount = Mathf.Max(1, nextQueuePreviewCount);

        ApplySettings(settings);

        if (_holdPieceManager != null)
        {
            _holdPieceManager.Initialize(_holdPieceTilemap);
        }

        if (_activePiece != null)
        {
            _activePiece.SetManagedByContainer(true);
        }
    }

    public void SetManagedByContainer(bool isManaged)
    {
        _isManagedByContainer = isManaged;
        if (isManaged)
        {
            UnsubscribeFromGameManager();
            if (_activePiece != null)
            {
                _activePiece.SetManagedByContainer(true);
            }
        }
    }

    public void InitializeSettings(TetrisGameSettingsSO settings)
    {
        ApplySettings(settings);
    }

    public void SetSimulationPaused(bool isPaused)
    {
        if (_activePiece != null)
        {
            _activePiece.IsSimulationPaused = isPaused;
        }
    }

    /// <summary>
    /// Gắn hiệu ứng Skill lên khối gạch đang rơi và cập nhật ngay diện mạo Special Block.
    /// </summary>
    public void SetActiveSkillModifier(ISkillEffect skillEffect, TileBase overrideTile = null)
    {
        _activeSkillEffect = skillEffect;
        _activeSkillOverrideTile = overrideTile;

        if (_activePiece != null && !_activePiece.IsLocking && !_isBoardOver)
        {
            Set(_activePiece);
            _activePiece.TriggerPieceUpdated();
        }
    }

    public void ClearActiveSkillModifier()
    {
        _activeSkillEffect = null;
        _activeSkillOverrideTile = null;
    }

    public void ResetBoard()
    {
        if (_lockCoroutine != null)
        {
            StopCoroutine(_lockCoroutine);
            _lockCoroutine = null;
        }

        IsAnimatingLineClear = false;
        _isBoardOver = false;
        ClearActiveSkillModifier();
        ClearAll();

        if (_activePiece != null)
        {
            float baseDelay = _gameSettings != null ? _gameSettings.BaseStepDelay : 1.0f;
            _activePiece.UpdateStepDelay(baseDelay);
            _activePiece.SetFastDrop(false);
            _activePiece.LockDelay = _gameSettings != null ? _gameSettings.LockDelay : 0.5f;
            _activePiece.IsSimulationPaused = false;
        }

        _pieceRandomizer.Reset();
        if (_holdPieceManager != null) _holdPieceManager.Clear();

        SpawnPiece();
    }

    public void StopBoard()
    {
        _isBoardOver = true;
        if (_lockCoroutine != null)
        {
            StopCoroutine(_lockCoroutine);
            _lockCoroutine = null;
        }

        IsAnimatingLineClear = false;
        ClearActiveSkillModifier();
        SetSimulationPaused(true);
        ClearAll();
    }

    public void ClearAll()
    {
        if (_tilemap != null) _tilemap.ClearAllTiles();
        if (_nextPieceTilemap != null) _nextPieceTilemap.ClearAllTiles();
        if (_holdPieceManager != null) _holdPieceManager.Clear();
        OnBoardCleared?.Invoke();
    }

    public void ApplyLevelSpeed(int level)
    {
        if (_activePiece == null) return;

        float newDelay = _gameSettings != null
            ? _gameSettings.GetStepDelayForLevel(level)
            : Mathf.Max(0.1f, 1.0f * Mathf.Pow(0.9f, level - 1));

        _activePiece.UpdateStepDelay(newDelay);
    }

    public void SpawnPiece(TetrominoData? specificData = null)
    {
        if (_isBoardOver) return;
        if (!_isManagedByContainer && GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        TetrominoData data = specificData.HasValue
            ? specificData.Value
            : _pieceRandomizer.GetNextPiece();

        _activePiece.Initialize(this, SpawnPosition, data);

        if (IsValidPosition(_activePiece, SpawnPosition))
        {
            Set(_activePiece);
            _activePiece.TriggerPieceUpdated();
        }
        else
        {
            _isBoardOver = true;
            if (_isManagedByContainer)
            {
                OnTopOut?.Invoke();
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            return;
        }

        RefreshNextQueueDisplay();
        if (_holdPieceManager != null) _holdPieceManager.ResetTurnHoldFlag();
    }

    public void HoldPiece()
    {
        if (_holdPieceManager == null || _activePiece == null || _isBoardOver) return;

        Clear(_activePiece);

        if (_holdPieceManager.TryHold(_activePiece.Data, out TetrominoData? pieceToSpawn))
        {
            if (pieceToSpawn.HasValue)
            {
                SpawnPiece(pieceToSpawn.Value);
            }
            else
            {
                SpawnPiece();
            }
        }
        else
        {
            Set(_activePiece);
        }
    }

    /// <summary>
    /// Xóa danh sách các hàng bất kỳ (không cần đầy) và dồn gạch phía trên rơi xuống đúng thứ tự (Zero-GC).
    /// Dùng chung cho các kỹ năng như Row Detonator, Bottom Purge...
    /// </summary>
    public void CollapseRows(List<int> rowsToClear)
    {
        if (_tilemap == null || rowsToClear == null || rowsToClear.Count == 0) return;

        RectInt bounds = Bounds;
        _sortedRowsBuffer.Clear();

        for (int i = 0; i < rowsToClear.Count; i++)
        {
            int row = rowsToClear[i];
            if (row < bounds.yMin || row >= bounds.yMax) continue;

            bool exists = false;
            for (int j = 0; j < _sortedRowsBuffer.Count; j++)
            {
                if (_sortedRowsBuffer[j] == row)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists) _sortedRowsBuffer.Add(row);
        }

        // Sắp xếp giảm dần (Insertion Sort tại chỗ - 0 bytes GC) để xóa từ trên xuống dưới không lệch chỉ số hàng
        for (int i = 1; i < _sortedRowsBuffer.Count; i++)
        {
            int key = _sortedRowsBuffer[i];
            int j = i - 1;
            while (j >= 0 && _sortedRowsBuffer[j] < key)
            {
                _sortedRowsBuffer[j + 1] = _sortedRowsBuffer[j];
                j--;
            }
            _sortedRowsBuffer[j + 1] = key;
        }

        for (int i = 0; i < _sortedRowsBuffer.Count; i++)
        {
            LineClear(_sortedRowsBuffer[i]);
        }
    }

    private void HandleSkillLinesClearedCallback(int linesCleared)
    {
        _lastSkillLinesCleared = linesCleared;
    }

    private void ApplySettings(TetrisGameSettingsSO settings)
    {
        _gameSettings = settings;
        if (settings != null)
        {
            _lineClearFlashDuration = settings.LineClearFlashDuration;
            _lineClearFlashCount = settings.LineClearFlashCount;
        }
        UpdateCachedFlashWait();
    }

    private void UpdateCachedFlashWait()
    {
        int flashCount = Mathf.Max(1, _lineClearFlashCount);
        float interval = _lineClearFlashDuration / (flashCount * 2f);
        if (!Mathf.Approximately(interval, _cachedFlashInterval) || _cachedFlashWait == null)
        {
            _cachedFlashInterval = interval;
            _cachedFlashWait = new WaitForSeconds(interval);
        }
    }

    private void SubscribeToGameManager()
    {
        if (GameManager.Instance != null && !_isSubscribedToGameManager)
        {
            GameManager.Instance.OnGameStarted += ResetBoard;
            GameManager.Instance.OnGameOver += StopBoard;
            GameManager.Instance.OnLevelChanged += ApplyLevelSpeed;
            GameManager.Instance.OnShowStartMenu += ClearAll;
            _isSubscribedToGameManager = true;
        }
    }

    private void UnsubscribeFromGameManager()
    {
        if (GameManager.Instance != null && _isSubscribedToGameManager)
        {
            GameManager.Instance.OnGameStarted -= ResetBoard;
            GameManager.Instance.OnGameOver -= StopBoard;
            GameManager.Instance.OnLevelChanged -= ApplyLevelSpeed;
            GameManager.Instance.OnShowStartMenu -= ClearAll;
            _isSubscribedToGameManager = false;
        }
    }

    private void HandlePieceLocked(Piece piece)
    {
        if (_lockCoroutine != null) StopCoroutine(_lockCoroutine);
        _lockCoroutine = StartCoroutine(LockSequenceRoutine(piece));
    }

    private IEnumerator LockSequenceRoutine(Piece piece)
    {
        Set(piece);

        if (_activeSkillEffect != null)
        {
            IsAnimatingLineClear = true;
            ISkillEffect effectToExecute = _activeSkillEffect;
            _lastSkillLinesCleared = 0;
            _skillAffectedRowsBuffer.Clear();

            yield return StartCoroutine(effectToExecute.ExecuteOnLock(
                this,
                piece,
                _skillAffectedRowsBuffer,
                _cachedOnSkillLinesCleared));

            ClearActiveSkillModifier();
            OnActiveSkillExecuted?.Invoke();

            if (_lastSkillLinesCleared > 0)
            {
                NotifyLinesCleared(_lastSkillLinesCleared, _skillAffectedRowsBuffer);
            }

            IsAnimatingLineClear = false;

            // Nếu Skill không tự xóa dòng (VD: Nova Bomb), vẫn kiểm tra xóa dòng bình thường nếu có hàng đầy
            if (_lastSkillLinesCleared == 0 && !_isBoardOver)
            {
                yield return StartCoroutine(ClearLinesAnimated());
            }
        }
        else
        {
            yield return StartCoroutine(ClearLinesAnimated());
        }

        if (!_isBoardOver)
        {
            SpawnPiece();
        }
        _lockCoroutine = null;
    }

    public void Set(Piece piece)
    {
        if (_tilemap == null || piece == null || piece.Cells == null) return;

        TileBase tileToUse = _activeSkillOverrideTile != null ? _activeSkillOverrideTile : piece.Data.tile;

        for (int i = 0; i < piece.Cells.Length; i++)
        {
            Vector3Int tilePosition = piece.Cells[i] + piece.Position;
            _tilemap.SetTile(tilePosition, tileToUse);
        }
    }

    public void Clear(Piece piece)
    {
        if (_tilemap == null || piece == null || piece.Cells == null) return;

        for (int i = 0; i < piece.Cells.Length; i++)
        {
            Vector3Int tilePosition = piece.Cells[i] + piece.Position;
            _tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        if (_tilemap == null || piece == null || piece.Cells == null) return false;

        RectInt bounds = Bounds;
        for (int i = 0; i < piece.Cells.Length; i++)
        {
            Vector3Int tilePosition = piece.Cells[i] + position;
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            if (_tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;
        _clearedRowsBuffer.Clear();

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                _clearedRowsBuffer.Add(row);
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++;
            }
        }

        NotifyLinesCleared(linesCleared, _clearedRowsBuffer);
    }

    public IEnumerator ClearLinesAnimated()
    {
        RectInt bounds = Bounds;
        _clearedRowsBuffer.Clear();

        for (int row = bounds.yMin; row < bounds.yMax; row++)
        {
            if (IsLineFull(row))
            {
                _clearedRowsBuffer.Add(row);
            }
        }

        if (_clearedRowsBuffer.Count == 0)
        {
            NotifyLinesCleared(0, null);
            yield break;
        }

        IsAnimatingLineClear = true;
        _flashPositionsBuffer.Clear();
        _flashTilesBuffer.Clear();

        int fullRowCount = _clearedRowsBuffer.Count;
        for (int r = 0; r < fullRowCount; r++)
        {
            int row = _clearedRowsBuffer[r];
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int pos = new Vector3Int(col, row, 0);
                _flashPositionsBuffer.Add(pos);
                _flashTilesBuffer.Add(_tilemap.GetTile(pos));
            }
        }

        UpdateCachedFlashWait();
        int tileCount = _flashPositionsBuffer.Count;

        for (int blink = 0; blink < _lineClearFlashCount; blink++)
        {
            for (int i = 0; i < tileCount; i++)
            {
                _tilemap.SetTile(_flashPositionsBuffer[i], null);
            }
            yield return _cachedFlashWait;

            for (int i = 0; i < tileCount; i++)
            {
                _tilemap.SetTile(_flashPositionsBuffer[i], _flashTilesBuffer[i]);
            }
            yield return _cachedFlashWait;
        }

        int rowIdx = bounds.yMin;
        int linesCleared = 0;

        while (rowIdx < bounds.yMax)
        {
            if (IsLineFull(rowIdx))
            {
                LineClear(rowIdx);
                linesCleared++;
            }
            else
            {
                rowIdx++;
            }
        }

        NotifyLinesCleared(linesCleared, _clearedRowsBuffer);
        IsAnimatingLineClear = false;
    }

    private void NotifyLinesCleared(int linesCleared, List<int> clearedRows)
    {
        if (_isManagedByContainer)
        {
            OnLinesCleared?.Invoke(linesCleared, clearedRows);
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(linesCleared, clearedRows);
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!_tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            _tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = _tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                _tilemap.SetTile(position, above);
            }
            row++;
        }
    }

    private void RefreshNextQueueDisplay()
    {
        _pieceRandomizer.PeekNextPieces(_nextQueuePreviewCount, _nextQueueBuffer);

        if (_nextPieceTilemap != null)
        {
            _nextPieceTilemap.ClearAllTiles();
            int count = _nextQueueBuffer.Count;
            for (int i = 0; i < count; i++)
            {
                Vector3Int slotOffset = new Vector3Int(0, -i * _nextPieceVerticalSpacing, 0);
                DrawPieceOnTilemap(_nextPieceTilemap, _nextQueueBuffer[i], slotOffset);
            }
        }

        OnNextQueueUpdated?.Invoke(_nextQueueBuffer);
    }

    public static void DrawPieceOnTilemap(Tilemap tilemap, TetrominoData data, Vector3Int offset)
    {
        if (tilemap == null || data.cells == null || data.cells.Length == 0) return;

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        for (int i = 0; i < data.cells.Length; i++)
        {
            minX = Mathf.Min(minX, data.cells[i].x);
            maxX = Mathf.Max(maxX, data.cells[i].x);
            minY = Mathf.Min(minY, data.cells[i].y);
            maxY = Mathf.Max(maxY, data.cells[i].y);
        }

        Vector3Int centerOffset = new Vector3Int(-(minX + maxX) / 2, -(minY + maxY) / 2, 0) + offset;

        for (int i = 0; i < data.cells.Length; i++)
        {
            Vector3Int tilePos = (Vector3Int)data.cells[i] + centerOffset;
            tilemap.SetTile(tilePos, data.tile);
        }
    }
}
