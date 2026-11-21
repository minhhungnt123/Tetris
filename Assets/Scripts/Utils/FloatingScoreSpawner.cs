using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Spawn và quản lý các FloatingScoreEffect theo cơ chế Object Pool (Zero-GC trong runtime).
/// Hỗ trợ tiêm IScoreService cục bộ từ PlayfieldContainer cho chế độ 1P & 2P.
/// </summary>
public class FloatingScoreSpawner : MonoBehaviour
{
    // ─── Inspector config ───────────────────────────────────────────────────────
    [Header("References")]
    [Tooltip("Prefab chứa FloatingScoreEffect (Canvas + TMP_Text).")]
    [SerializeField] private FloatingScoreEffect _effectPrefab;

    [Tooltip("Board component — dùng để lấy vùng chơi cố định (Bounds).")]
    [SerializeField] private Board _board;

    [Tooltip("Tilemap chính của Board — dùng để chuyển cell → world position.")]
    [SerializeField] private Tilemap _boardTilemap;

    [Header("Position Tuning")]
    [Tooltip("Offset thêm vào vị trí spawn (X: dịch ngang, Y: dịch dọc so với dòng bị xóa).")]
    [SerializeField] private Vector3 _spawnOffset = Vector3.zero;

    [Header("Pool Settings")]
    [Tooltip("Số đối tượng khởi tạo sẵn trong pool.")]
    [SerializeField] private int _poolInitialSize = 6;

    // ─── Private state ───────────────────────────────────────────────────────────
    private readonly Queue<FloatingScoreEffect> _pool = new Queue<FloatingScoreEffect>();
    private Action<FloatingScoreEffect> _cachedReturnToPoolAction;
    private IScoreService _scoreService;
    private bool _isManagedByContainer;
    private bool _isSubscribedToGameManager;

    // ─── Unity callbacks ─────────────────────────────────────────────────────────

    private void Awake()
    {
        // Cache delegate một lần duy nhất để tránh cấp phát Action mới mỗi lần Play() (Zero-GC)
        _cachedReturnToPoolAction = ReturnToPool;

        if (_effectPrefab != null)
        {
            for (int i = 0; i < _poolInitialSize; i++)
            {
                FloatingScoreEffect instance = CreateNewEffect();
                instance.gameObject.SetActive(false);
                _pool.Enqueue(instance);
            }
        }
    }

    private void Start()
    {
        if (!_isManagedByContainer && _scoreService == null && GameManager.Instance != null)
        {
            GameManager.Instance.OnLineClearScore += HandleLineClearScore;
            _isSubscribedToGameManager = true;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeAll();
    }

    /// <summary>
    /// Tiêm phụ thuộc trực tiếp từ PlayfieldContainer để lắng nghe ScoreService cục bộ của từng người chơi.
    /// </summary>
    public void Initialize(Board board, Tilemap boardTilemap, IScoreService scoreService)
    {
        UnsubscribeAll();

        _isManagedByContainer = true;
        _board = board;
        _boardTilemap = boardTilemap;
        _scoreService = scoreService;

        if (_scoreService != null)
        {
            _scoreService.OnLineClearScore += HandleLineClearScore;
        }
    }

    private void UnsubscribeAll()
    {
        if (_scoreService != null)
        {
            _scoreService.OnLineClearScore -= HandleLineClearScore;
        }

        if (_isSubscribedToGameManager && GameManager.Instance != null)
        {
            GameManager.Instance.OnLineClearScore -= HandleLineClearScore;
            _isSubscribedToGameManager = false;
        }
    }

    // ─── Event handler ───────────────────────────────────────────────────────────

    private void HandleLineClearScore(int points, List<int> clearedRows)
    {
        if (_effectPrefab == null || points <= 0 || clearedRows == null || clearedRows.Count == 0) return;

        int midRow = clearedRows[clearedRows.Count / 2];
        Vector3 worldPos = CalculateWorldCenter(midRow) + _spawnOffset;

        FloatingScoreEffect effect = GetFromPool();
        effect.gameObject.SetActive(true);
        effect.Play(points, worldPos, _cachedReturnToPoolAction);
    }

    // ─── Pool management ─────────────────────────────────────────────────────────

    private void ReturnToPool(FloatingScoreEffect effect)
    {
        if (effect == null) return;
        effect.gameObject.SetActive(false);
        _pool.Enqueue(effect);
    }

    private FloatingScoreEffect GetFromPool()
    {
        while (_pool.Count > 0)
        {
            FloatingScoreEffect candidate = _pool.Dequeue();
            if (candidate != null) return candidate;
        }
        return CreateNewEffect();
    }

    private FloatingScoreEffect CreateNewEffect()
    {
        return Instantiate(_effectPrefab, transform);
    }

    // ─── Position calculation ─────────────────────────────────────────────────────

    private Vector3 CalculateWorldCenter(int row)
    {
        if (_boardTilemap == null) return Vector3.zero;

        int centerCellX = 0;
        if (_board != null)
        {
            RectInt bounds = _board.Bounds;
            centerCellX = (bounds.xMin + bounds.xMax) / 2;
        }

        Vector3 worldPos = _boardTilemap.CellToWorld(new Vector3Int(centerCellX, row, 0));
        worldPos += _boardTilemap.cellSize * 0.5f;
        worldPos.z = 0f;

        return worldPos;
    }

    // ─── Editor helper ───────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (_boardTilemap == null || _board == null) return;

        RectInt bounds = _board.Bounds;
        int centerCellX = (bounds.xMin + bounds.xMax) / 2;
        int midRow = (bounds.yMin + bounds.yMax) / 2;

        Vector3 previewPos = CalculateWorldCenter(midRow) + _spawnOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(previewPos, 0.3f);
        Gizmos.DrawLine(previewPos, previewPos + Vector3.up * 1.5f);

        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Vector3 boardMin = _boardTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
        Vector3 boardMax = _boardTilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMax, 0));
        Gizmos.DrawWireCube((boardMin + boardMax) * 0.5f, boardMax - boardMin);
    }
}
