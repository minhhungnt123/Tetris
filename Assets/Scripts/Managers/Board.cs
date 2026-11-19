using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Quản lý ma trận ô cờ (Grid/Tilemap), kiểm tra va chạm, xóa dòng và điều phối vòng đời khối gạch (SRP).
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

    [Header("Line Clear Animation Settings")]
    [SerializeField] private float _lineClearFlashDuration = 0.3f;
    [SerializeField] private int _lineClearFlashCount = 3;

    private Piece _activePiece;
    private HoldPieceManager _holdPieceManager;
    private IPieceRandomizer _pieceRandomizer;
    private TetrominoData _nextTetromino;
    private Coroutine _lockCoroutine;

    // PascalCase Properties (Chuẩn C#)
    public Tilemap Tilemap => _tilemap;
    public Piece ActivePiece => _activePiece;
    public bool IsAnimatingLineClear { get; private set; }
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
        _activePiece = GetComponentInChildren<Piece>();
        if (_tilemap == null)
        {
            _tilemap = GetComponentInChildren<Tilemap>();
        }

        // Tách biệt Hold Manager
        _holdPieceManager = GetComponent<HoldPieceManager>();
        if (_holdPieceManager == null)
        {
            _holdPieceManager = gameObject.AddComponent<HoldPieceManager>();
        }
        _holdPieceManager.Initialize(_holdPieceTilemap);

        // Khởi tạo thuật toán 7-Bag độc lập
        _pieceRandomizer = new SevenBagRandomizer();

        if (tetrominoes != null)
        {
            for (int i = 0; i < tetrominoes.Length; i++)
            {
                tetrominoes[i].Initialize();
            }
            _pieceRandomizer.Initialize(tetrominoes);
        }

        if (_gameSettings != null)
        {
            _lineClearFlashDuration = _gameSettings.LineClearFlashDuration;
            _lineClearFlashCount = _gameSettings.LineClearFlashCount;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += ResetGame;
            GameManager.Instance.OnGameOver += HandleGameOver;
            GameManager.Instance.OnLevelChanged += HandleLevelChanged;
            GameManager.Instance.OnShowStartMenu += ClearAll;
        }

        if (_activePiece != null)
        {
            _activePiece.OnPieceLocked += HandlePieceLocked;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= ResetGame;
            GameManager.Instance.OnGameOver -= HandleGameOver;
            GameManager.Instance.OnLevelChanged -= HandleLevelChanged;
            GameManager.Instance.OnShowStartMenu -= ClearAll;
        }

        if (_activePiece != null)
        {
            _activePiece.OnPieceLocked -= HandlePieceLocked;
        }
    }

    public void InitializeSettings(TetrisGameSettingsSO settings)
    {
        _gameSettings = settings;
        if (settings != null)
        {
            _lineClearFlashDuration = settings.LineClearFlashDuration;
            _lineClearFlashCount = settings.LineClearFlashCount;
        }
    }

    private void ResetGame()
    {
        ClearAll();

        if (_activePiece != null)
        {
            _activePiece.StepDelay = _gameSettings != null ? _gameSettings.BaseStepDelay : 1.0f;
            _activePiece.LockDelay = _gameSettings != null ? _gameSettings.LockDelay : 0.5f;
        }

        _pieceRandomizer.Reset();
        if (_holdPieceManager != null) _holdPieceManager.Clear();

        _nextTetromino = _pieceRandomizer.GetNextPiece();
        SpawnPiece();
    }

    private void HandleGameOver()
    {
        ClearAll();
    }

    private void ClearAll()
    {
        if (_tilemap != null) _tilemap.ClearAllTiles();
        if (_nextPieceTilemap != null) _nextPieceTilemap.ClearAllTiles();
        if (_holdPieceManager != null) _holdPieceManager.Clear();
    }

    private void HandleLevelChanged(int level)
    {
        if (_activePiece != null)
        {
            float newDelay = _gameSettings != null 
                ? _gameSettings.GetStepDelayForLevel(level) 
                : Mathf.Max(0.1f, 1.0f * Mathf.Pow(0.9f, level - 1));

            _activePiece.UpdateStepDelay(newDelay);
        }
    }

    public void SpawnPiece(TetrominoData? specificData = null)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        TetrominoData data = specificData ?? _nextTetromino;
        _activePiece.Initialize(this, SpawnPosition, data);

        if (IsValidPosition(_activePiece, SpawnPosition))
        {
            Set(_activePiece);
            _activePiece.TriggerPieceUpdated();
        }
        else
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            return;
        }

        if (!specificData.HasValue)
        {
            _nextTetromino = _pieceRandomizer.GetNextPiece();
        }

        DisplayNextPiece(_nextTetromino);
        if (_holdPieceManager != null) _holdPieceManager.ResetTurnHoldFlag();
    }

    public void HoldPiece()
    {
        if (_holdPieceManager == null || _activePiece == null) return;

        Clear(_activePiece);

        if (_holdPieceManager.TryHold(_activePiece.Data, out TetrominoData? pieceToSpawn))
        {
            if (pieceToSpawn.HasValue)
            {
                SpawnPiece(pieceToSpawn.Value);
            }
            else
            {
                _nextTetromino = _pieceRandomizer.GetNextPiece();
                SpawnPiece();
            }
        }
        else
        {
            // Nếu không hold được thì đặt lại khối cũ
            Set(_activePiece);
        }
    }

    /// <summary>
    /// Xử lý sự kiện khi Piece khóa vào bảng: đặt tile, chạy hoạt ảnh xóa dòng và spawn khối tiếp theo.
    /// </summary>
    private void HandlePieceLocked(Piece piece)
    {
        if (_lockCoroutine != null) StopCoroutine(_lockCoroutine);
        _lockCoroutine = StartCoroutine(LockSequenceRoutine(piece));
    }

    private IEnumerator LockSequenceRoutine(Piece piece)
    {
        Set(piece);
        yield return StartCoroutine(ClearLinesAnimated());
        SpawnPiece();
        _lockCoroutine = null;
    }

    public void Set(Piece piece)
    {
        if (_tilemap == null || piece == null || piece.Cells == null) return;

        for (int i = 0; i < piece.Cells.Length; i++)
        {
            Vector3Int tilePosition = piece.Cells[i] + piece.Position;
            _tilemap.SetTile(tilePosition, piece.Data.tile);
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
        var clearedRows = new List<int>();

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                clearedRows.Add(row);
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++;
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(linesCleared, clearedRows);
        }
    }

    public IEnumerator ClearLinesAnimated()
    {
        RectInt bounds = Bounds;
        var fullRows = new List<int>();

        for (int row = bounds.yMin; row < bounds.yMax; row++)
        {
            if (IsLineFull(row)) fullRows.Add(row);
        }

        if (fullRows.Count == 0)
        {
            if (GameManager.Instance != null) GameManager.Instance.AddScore(0);
            yield break;
        }

        IsAnimatingLineClear = true;
        var savedTiles = new Dictionary<Vector3Int, TileBase>();

        foreach (int row in fullRows)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                var pos = new Vector3Int(col, row, 0);
                savedTiles[pos] = _tilemap.GetTile(pos);
            }
        }

        float interval = _lineClearFlashDuration / (_lineClearFlashCount * 2f);
        for (int blink = 0; blink < _lineClearFlashCount; blink++)
        {
            foreach (var pos in savedTiles.Keys) _tilemap.SetTile(pos, null);
            yield return new WaitForSeconds(interval);
            foreach (var kvp in savedTiles) _tilemap.SetTile(kvp.Key, kvp.Value);
            yield return new WaitForSeconds(interval);
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(linesCleared, fullRows);
        }

        IsAnimatingLineClear = false;
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

    private void DisplayNextPiece(TetrominoData data)
    {
        if (_nextPieceTilemap == null) return;
        _nextPieceTilemap.ClearAllTiles();
        DrawPieceOnTilemap(_nextPieceTilemap, data, new Vector3Int(-1, 1, 0));
    }

    public static void DrawPieceOnTilemap(Tilemap tilemap, TetrominoData data, Vector3Int offset)
    {
        if (tilemap == null || data.cells == null) return;

        foreach (Vector2Int cell in data.cells)
        {
            Vector3Int tilePos = new Vector3Int(cell.x * 2, cell.y * 2, 0) + offset;
            tilemap.SetTile(tilePos, data.tile);
        }
    }
}
