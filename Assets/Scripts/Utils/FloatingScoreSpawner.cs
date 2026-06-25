using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Spawn và quản lý các FloatingScoreEffect theo cơ chế Object Pool.
/// Subscribe vào <see cref="GameManager.OnLineClearScore"/> để nhận số điểm vừa được kiếm
/// và vị trí thế giới của dòng bị xóa.
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
    [SerializeField] private Vector3 _spawnOffset = new Vector3(0f, 0f, 0f);

    [Header("Pool Settings")]
    [Tooltip("Số đối tượng khởi tạo sẵn trong pool.")]
    [SerializeField] private int _poolInitialSize = 6;

    // ─── Private state ───────────────────────────────────────────────────────────
    private readonly Queue<FloatingScoreEffect> _pool = new Queue<FloatingScoreEffect>();

    // ─── Unity callbacks ─────────────────────────────────────────────────────────

    private void Awake()
    {
        for (int i = 0; i < _poolInitialSize; i++)
        {
            FloatingScoreEffect instance = CreateNewEffect();
            instance.gameObject.SetActive(false);
            _pool.Enqueue(instance);
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLineClearScore += HandleLineClearScore;
        }
        else
        {
            Debug.LogWarning("[FloatingScoreSpawner] Không tìm thấy GameManager.Instance. " +
                             "Kiểm tra GameManager có tồn tại trong Scene không.");
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLineClearScore -= HandleLineClearScore;
        }
    }

    // ─── Event handler ───────────────────────────────────────────────────────────

    private void HandleLineClearScore(int points, List<int> clearedRows)
    {
        if (points <= 0 || clearedRows == null || clearedRows.Count == 0) return;

        int midRow = clearedRows[clearedRows.Count / 2];
        Vector3 worldPos = CalculateWorldCenter(midRow) + _spawnOffset;

        FloatingScoreEffect effect = GetFromPool();
        effect.gameObject.SetActive(true);
        effect.Play(points, worldPos, ReturnToPool);
    }

    // ─── Pool management ─────────────────────────────────────────────────────────

    private void ReturnToPool(FloatingScoreEffect effect)
    {
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

    /// <summary>
    /// Tính world position tại trung tâm ngang của Board, trên hàng <paramref name="row"/>.
    /// Ưu tiên dùng Board.Bounds (cố định) thay vì cellBounds (thay đổi theo tiles).
    /// </summary>
    private Vector3 CalculateWorldCenter(int row)
    {
        if (_boardTilemap == null) return Vector3.zero;

        // Tính center X từ Board.Bounds cố định (ví dụ: -5 đến 5 → center = 0)
        // Nếu không có Board reference, fallback về 0
        int centerCellX = 0;
        if (_board != null)
        {
            RectInt bounds = _board.Bounds;
            centerCellX = (bounds.xMin + bounds.xMax) / 2;
        }

        // Chuyển cell coordinate → world position
        Vector3 worldPos = _boardTilemap.CellToWorld(new Vector3Int(centerCellX, row, 0));

        // Cộng nửa ô để vào đúng trung tâm của cell
        worldPos += _boardTilemap.cellSize * 0.5f;

        // Giữ Z = 0 để text luôn nhìn thấy từ camera Orthographic
        worldPos.z = 0f;

        return worldPos;
    }

    // ─── Editor helper ───────────────────────────────────────────────────────────

    /// <summary>
    /// Vẽ gizmo trong Scene View để dễ căn chỉnh vị trí spawn.
    /// </summary>
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

        // Vẽ đường giới hạn board
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Vector3 boardMin = _boardTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
        Vector3 boardMax = _boardTilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMax, 0));
        Gizmos.DrawWireCube((boardMin + boardMax) * 0.5f, boardMax - boardMin);
    }
}
