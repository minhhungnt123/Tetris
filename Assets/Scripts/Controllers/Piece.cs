using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Điều khiển khối tetromino đang hoạt động: di chuyển, xoay (SRS), trọng lực và khóa khối (SRP).
/// </summary>
public class Piece : MonoBehaviour
{
    public event Action OnPieceUpdated;
    public event Action<Piece> OnPieceLocked;

    [Header("Timers & Delays")]
    [SerializeField] private float _stepDelay = 1.0f;
    [SerializeField] private float _lockDelay = 0.5f;

    private Board _board;
    private TetrominoData _data;
    private Vector3Int[] _cells;
    private Vector3Int _position;
    private int _rotationIndex;

    private float _stepTime;
    private float _lockTime;
    private bool _isLocking;

    // PascalCase Properties (Chuẩn C#)
    public Board Board => _board;
    public TetrominoData Data => _data;
    public Vector3Int[] Cells => _cells;
    public Vector3Int Position => _position;
    public int RotationIndex => _rotationIndex;
    public float StepDelay
    {
        get => _stepDelay;
        set => _stepDelay = value;
    }
    public float LockDelay
    {
        get => _lockDelay;
        set => _lockDelay = value;
    }
    public bool IsLocking => _isLocking;

    // Backward-compatibility aliases
    public Board board => _board;
    public TetrominoData data => _data;
    public Vector3Int[] cells => _cells;
    public Vector3Int position => _position;
    public int rotationIndex => _rotationIndex;
    public float stepDelay
    {
        get => _stepDelay;
        set => _stepDelay = value;
    }
    public float lockDelay
    {
        get => _lockDelay;
        set => _lockDelay = value;
    }

    /// <summary>
    /// Khởi tạo khối mới với dữ liệu và vị trí xuất phát.
    /// </summary>
    public void Initialize(Board board, Vector3Int spawnPosition, TetrominoData tetrominoData)
    {
        _board = board;
        _position = spawnPosition;
        _data = tetrominoData;
        _rotationIndex = 0;
        _stepTime = Time.time + _stepDelay;
        _lockTime = 0f;
        _isLocking = false;

        if (_cells == null || _cells.Length != _data.cells.Length)
        {
            _cells = new Vector3Int[_data.cells.Length];
        }

        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i] = (Vector3Int)_data.cells[i];
        }
    }

    // Alias cho phương thức cũ
    public void Initialized(Board board, Vector3Int spawnPosition, TetrominoData tetrominoData)
    {
        Initialize(board, spawnPosition, tetrominoData);
    }

    private void Update()
    {
        if (_board == null || _isLocking) return;

        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
            return;

        _lockTime += Time.deltaTime;

        if (Time.time > _stepTime)
        {
            _board.Clear(this);
            Vector3Int oldPosition = _position;

            Step();

            _board.Set(this);
            if (_position != oldPosition)
            {
                OnPieceUpdated?.Invoke();
            }
        }
    }

    public void MovePiece(Vector2Int transition)
    {
        if (_board == null || _isLocking) return;

        _board.Clear(this);
        Vector3Int oldPosition = _position;

        Move(transition);

        _board.Set(this);
        if (_position != oldPosition)
        {
            OnPieceUpdated?.Invoke();
        }
    }

    public void RotatePiece(int direction)
    {
        if (_board == null || _isLocking) return;

        _board.Clear(this);
        int oldRotation = _rotationIndex;

        Rotate(direction);

        _board.Set(this);
        if (_rotationIndex != oldRotation)
        {
            OnPieceUpdated?.Invoke();
        }
    }

    public void HardDropPiece()
    {
        if (_board == null || _isLocking) return;

        _board.Clear(this);
        Vector3Int oldPosition = _position;

        HardDrop();

        if (_position != oldPosition)
        {
            OnPieceUpdated?.Invoke();
        }
    }

    public void Hold()
    {
        if (_board != null)
        {
            _board.HoldPiece();
        }
    }

    public void SetFastDrop(bool isFast)
    {
        UpdateStepDelay(isFast ? 0.1f : 1.0f);
    }

    public void UpdateStepDelay(float newStepDelay)
    {
        _stepDelay = newStepDelay;
    }

    public void TriggerPieceUpdated()
    {
        OnPieceUpdated?.Invoke();
    }

    private void Step()
    {
        _stepTime = Time.time + _stepDelay;

        Move(Vector2Int.down);

        if (_lockTime > _lockDelay)
        {
            Lock();
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }
        Lock();
    }

    private void Lock()
    {
        if (_isLocking) return;
        _isLocking = true;

        // Bắn sự kiện thay vì tự can thiệp vòng đời của Board
        OnPieceLocked?.Invoke(this);
    }

    private bool Move(Vector2Int transition)
    {
        Vector3Int newPosition = _position;
        newPosition.x += transition.x;
        newPosition.y += transition.y;

        bool valid = _board.IsValidPosition(this, newPosition);

        if (valid)
        {
            _position = newPosition;
            _lockTime = 0f;
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        int originalRotation = _rotationIndex;
        _rotationIndex = Wrap(_rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);
        if (!TestWallKicks(_rotationIndex, direction))
        {
            _rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            Vector3 cell = _cells[i];
            int x, y;
            switch (_data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * global::Data.RotationMatrix[0] * direction) + (cell.y * global::Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * global::Data.RotationMatrix[2] * direction) + (cell.y * global::Data.RotationMatrix[3] * direction));
                    break;
                default:
                    x = Mathf.RoundToInt((cell.x * global::Data.RotationMatrix[0] * direction) + (cell.y * global::Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * global::Data.RotationMatrix[2] * direction) + (cell.y * global::Data.RotationMatrix[3] * direction));
                    break;
            }
            _cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKicksIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < _data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = _data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }
        return false;
    }

    private int GetWallKicksIndex(int rotationIndex, int rotationDirection)
    {
        int wallKicksIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKicksIndex--;
        }
        return Wrap(wallKicksIndex, 0, _data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}
