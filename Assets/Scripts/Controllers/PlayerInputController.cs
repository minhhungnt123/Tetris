using UnityEngine;

/// <summary>
/// Xử lý tín hiệu điều khiển từ InputManager và các cơ chế lặp phím DAS/ARR, Hold và Activate Skill (Controller).
/// </summary>
[RequireComponent(typeof(Piece))]
public class PlayerInputController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TetrisGameSettingsSO _gameSettings;
    [SerializeField] private SkillController _skillController;

    [Header("Movement Settings (Fallback nếu không có Settings SO)")]
    [Tooltip("Thời gian trễ trước khi khối bắt đầu tự động di chuyển liên tục (DAS).")]
    [SerializeField] private float _dasDelay = 0.15f;

    [Tooltip("Khoảng thời gian giữa mỗi lần di chuyển liên tục qua trái/phải (ARR).")]
    [SerializeField] private float _arrDelay = 0.05f;

    [Tooltip("Khoảng thời gian giữa mỗi lần rơi khi giữ nút xuống (Soft Drop ARR).")]
    [SerializeField] private float _softDropArrDelay = 0.05f;

    private Piece _piece;
    private InputManager _inputManager;
    private Vector2 _currentInput;
    private float _dasTimer;
    private float _arrTimer;
    private float _softDropArrTimer;

    public float DasDelay => _gameSettings != null ? _gameSettings.DasDelay : _dasDelay;
    public float ArrDelay => _gameSettings != null ? _gameSettings.ArrDelay : _arrDelay;
    public float SoftDropArrDelay => _gameSettings != null ? _gameSettings.SoftDropArrDelay : _softDropArrDelay;

    private void Awake()
    {
        _piece = GetComponent<Piece>();
        if (_skillController == null)
        {
            _skillController = GetComponentInParent<SkillController>();
        }
    }

    private void OnEnable()
    {
        _inputManager = InputManager.Instance;
        if (_inputManager == null)
        {
            _inputManager = FindFirstObjectByType<InputManager>();
        }

        if (_inputManager != null)
        {
            _inputManager.OnMove += OnMove;
            _inputManager.OnMoveCanceled += OnMoveCanceled;
            _inputManager.OnRotate += OnRotate;
            _inputManager.OnHardDrop += OnHardDrop;
            _inputManager.OnHold += OnHold;
            _inputManager.OnSoftDropStart += OnSoftDropStart;
            _inputManager.OnSoftDropCancel += OnSoftDropCancel;
            _inputManager.OnActivateSkill += OnActivateSkill;
        }
    }

    private void OnDisable()
    {
        if (_inputManager != null)
        {
            _inputManager.OnMove -= OnMove;
            _inputManager.OnMoveCanceled -= OnMoveCanceled;
            _inputManager.OnRotate -= OnRotate;
            _inputManager.OnHardDrop -= OnHardDrop;
            _inputManager.OnHold -= OnHold;
            _inputManager.OnSoftDropStart -= OnSoftDropStart;
            _inputManager.OnSoftDropCancel -= OnSoftDropCancel;
            _inputManager.OnActivateSkill -= OnActivateSkill;
        }
    }

    public void InitializeSettings(TetrisGameSettingsSO settings)
    {
        _gameSettings = settings;
    }

    public void InitializeSkillController(SkillController skillController)
    {
        _skillController = skillController;
    }

    private bool IsInputAllowed()
    {
        if (_piece == null || _piece.Board == null) return false;
        if (_piece.IsSimulationPaused) return false;
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return false;
        if (_piece.Board.IsAnimatingLineClear) return false;
        return true;
    }

    private void Update()
    {
        if (!IsInputAllowed())
        {
            _currentInput = Vector2.zero;
            return;
        }

        // Xử lý di chuyển ngang tự động (DAS / ARR)
        if (Mathf.Abs(_currentInput.x) > 0.5f)
        {
            _dasTimer -= Time.deltaTime;
            if (_dasTimer <= 0f)
            {
                _arrTimer -= Time.deltaTime;
                float currentArrDelay = ArrDelay;
                while (_arrTimer <= 0f)
                {
                    _piece.MovePiece(_currentInput.x < -0.5f ? Vector2Int.left : Vector2Int.right);
                    _arrTimer += currentArrDelay;
                }
            }
        }

        // Xử lý rơi nhanh liên tục khi giữ nút xuống (Soft Drop ARR)
        if (_currentInput.y < -0.5f)
        {
            _softDropArrTimer -= Time.deltaTime;
            float currentSoftDropArrDelay = SoftDropArrDelay;
            while (_softDropArrTimer <= 0f)
            {
                _piece.MovePiece(Vector2Int.down);
                _softDropArrTimer += currentSoftDropArrDelay;
            }
        }
    }

    private void OnMove(Vector2 direction)
    {
        if (!IsInputAllowed()) return;

        if (direction.x != 0 && direction.x != _currentInput.x)
        {
            _currentInput.x = direction.x;
            _dasTimer = DasDelay;
            _arrTimer = 0f;

            _piece.MovePiece(_currentInput.x < -0.5f ? Vector2Int.left : Vector2Int.right);
        }

        if (direction.y < -0.5f && direction.y != _currentInput.y)
        {
            _currentInput.y = direction.y;
            _softDropArrTimer = 0f;

            _piece.MovePiece(Vector2Int.down);
        }
    }

    private void OnMoveCanceled(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) < 0.5f)
        {
            _currentInput.x = 0;
        }

        if (direction.y >= -0.5f)
        {
            _currentInput.y = 0;
        }
    }

    private void OnRotate()
    {
        if (IsInputAllowed()) _piece.RotatePiece(1);
    }

    private void OnHardDrop()
    {
        if (IsInputAllowed()) _piece.HardDropPiece();
    }

    private void OnHold()
    {
        if (IsInputAllowed()) _piece.Hold();
    }

    private void OnSoftDropStart()
    {
        if (_piece != null) _piece.SetFastDrop(true);
    }

    private void OnSoftDropCancel()
    {
        if (_piece != null) _piece.SetFastDrop(false);
    }

    private void OnActivateSkill()
    {
        if (!IsInputAllowed() || _skillController == null) return;
        _skillController.TryActivateSkill();
    }
}
