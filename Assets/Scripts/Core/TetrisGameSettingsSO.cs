using UnityEngine;

[CreateAssetMenu(fileName = "TetrisGameSettings", menuName = "Tetris/Game Settings")]
public class TetrisGameSettingsSO : ScriptableObject
{
    [Header("Movement & Input (DAS / ARR)")]
    [Tooltip("Thời gian trễ trước khi khối tự động dịch chuyển liên tục (DAS).")]
    [SerializeField] private float _dasDelay = 0.15f;

    [Tooltip("Khoảng thời gian giữa mỗi bước dịch ngang (ARR).")]
    [SerializeField] private float _arrDelay = 0.05f;

    [Tooltip("Khoảng thời gian giữa mỗi bước rơi khi giữ nút xuống (Soft Drop ARR).")]
    [SerializeField] private float _softDropArrDelay = 0.05f;

    [Header("Gravity & Lock Delay")]
    [Tooltip("Thời gian rơi mặc định (giây) tại Level 1.")]
    [SerializeField] private float _baseStepDelay = 1.0f;

    [Tooltip("Thời gian rơi tối thiểu (tốc độ nhanh nhất).")]
    [SerializeField] private float _minStepDelay = 0.1f;

    [Tooltip("Hệ số gia tốc rơi theo từng level.")]
    [SerializeField] private float _speedMultiplier = 0.9f;

    [Tooltip("Thời gian chờ trước khi khóa khối chạm đất.")]
    [SerializeField] private float _lockDelay = 0.5f;

    [Header("Board Config")]
    [SerializeField] private Vector2Int _boardSize = new Vector2Int(10, 20);
    [SerializeField] private Vector3Int _spawnPosition = new Vector3Int(-1, 8, 0);

    [Header("Line Clear Animation")]
    [SerializeField] private float _lineClearFlashDuration = 0.3f;
    [SerializeField] private int _lineClearFlashCount = 3;

    // Public Properties
    public float DasDelay => _dasDelay;
    public float ArrDelay => _arrDelay;
    public float SoftDropArrDelay => _softDropArrDelay;
    public float BaseStepDelay => _baseStepDelay;
    public float MinStepDelay => _minStepDelay;
    public float LockDelay => _lockDelay;
    public Vector2Int BoardSize => _boardSize;
    public Vector3Int SpawnPosition => _spawnPosition;
    public float LineClearFlashDuration => _lineClearFlashDuration;
    public int LineClearFlashCount => _lineClearFlashCount;

    public float GetStepDelayForLevel(int level)
    {
        return Mathf.Max(_minStepDelay, _baseStepDelay * Mathf.Pow(_speedMultiplier, Mathf.Max(0, level - 1)));
    }
}
