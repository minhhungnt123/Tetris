using UnityEngine;

[RequireComponent(typeof(Piece))]
public class PlayerInputController : MonoBehaviour
{
    private Piece piece;
    
    [Header("Movement Settings")]
    [Tooltip("Thời gian trễ trước khi khối bắt đầu tự động di chuyển liên tục (DAS).")]
    public float dasDelay = 0.15f;
    [Tooltip("Khoảng thời gian giữa mỗi lần di chuyển liên tục qua trái/phải (ARR).")]
    public float arrDelay = 0.05f;
    [Tooltip("Khoảng thời gian giữa mỗi lần rơi khi giữ nút xuống (Soft Drop ARR).")]
    public float softDropArrDelay = 0.05f;

    private Vector2 currentInput;
    private float dasTimer;
    private float arrTimer;
    private float softDropArrTimer;

    private void Awake()
    {
        piece = GetComponent<Piece>();
    }

    private InputManager inputManager;

    private void OnEnable()
    {
        inputManager = InputManager.Instance;
        if (inputManager == null)
        {
            inputManager = FindObjectOfType<InputManager>();
        }

        Debug.Log("PlayerInputController: OnEnable called. InputManager found: " + (inputManager != null));

        if (inputManager != null)
        {
            inputManager.OnMove += OnMove;
            inputManager.OnMoveCanceled += OnMoveCanceled;
            inputManager.OnRotate += OnRotate;
            inputManager.OnHardDrop += OnHardDrop;
            inputManager.OnHold += OnHold;
            inputManager.OnSoftDropStart += OnSoftDropStart;
            inputManager.OnSoftDropCancel += OnSoftDropCancel;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnMove -= OnMove;
            inputManager.OnMoveCanceled -= OnMoveCanceled;
            inputManager.OnRotate -= OnRotate;
            inputManager.OnHardDrop -= OnHardDrop;
            inputManager.OnHold -= OnHold;
            inputManager.OnSoftDropStart -= OnSoftDropStart;
            inputManager.OnSoftDropCancel -= OnSoftDropCancel;
        }
    }

    private bool IsInputAllowed()
    {
        if (piece == null || piece.board == null) return false;
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return false;
        if (piece.board.IsAnimatingLineClear) return false;
        return true;
    }

    private void Update()
    {
        if (!IsInputAllowed())
        {
            currentInput = Vector2.zero;
            return;
        }

        // Handle continuous horizontal movement
        if (Mathf.Abs(currentInput.x) > 0.5f)
        {
            dasTimer -= Time.deltaTime;
            if (dasTimer <= 0f)
            {
                arrTimer -= Time.deltaTime;
                while (arrTimer <= 0f)
                {
                    piece.MovePiece(currentInput.x < -0.5f ? Vector2Int.left : Vector2Int.right);
                    arrTimer += arrDelay;
                }
            }
        }
        
        // Handle continuous downward movement (Soft Drop từ trục Y)
        if (currentInput.y < -0.5f)
        {
            softDropArrTimer -= Time.deltaTime;
            while (softDropArrTimer <= 0f)
            {
                piece.MovePiece(Vector2Int.down);
                softDropArrTimer += softDropArrDelay;
            }
        }
    }

    private void OnMove(Vector2 direction)
    {
        if (!IsInputAllowed()) return;

        // Xử lý nhấn hoặc đổi hướng trục X (Trái / Phải)
        if (direction.x != 0 && direction.x != currentInput.x)
        {
            currentInput.x = direction.x;
            dasTimer = dasDelay;
            arrTimer = 0f;
            
            piece.MovePiece(currentInput.x < -0.5f ? Vector2Int.left : Vector2Int.right);
        }

        // Xử lý nhấn trục Y (Xuống)
        if (direction.y < -0.5f && direction.y != currentInput.y)
        {
            currentInput.y = direction.y;
            softDropArrTimer = 0f; // Di chuyển luôn ở khung hình tiếp theo
            
            piece.MovePiece(Vector2Int.down);
        }
    }

    private void OnMoveCanceled(Vector2 direction)
    {
        // Khi thả nút, reset input nếu trục X về 0
        if (Mathf.Abs(direction.x) < 0.5f)
        {
            currentInput.x = 0;
        }

        // Reset input nếu trục Y không còn trỏ xuống
        if (direction.y >= -0.5f)
        {
            currentInput.y = 0;
        }
    }

    private void OnRotate()
    {
        if (IsInputAllowed()) piece.RotatePiece(1);
    }

    private void OnHardDrop()
    {
        if (IsInputAllowed()) piece.HardDropPiece();
    }

    private void OnHold()
    {
        if (IsInputAllowed()) piece.Hold();
    }

    private void OnSoftDropStart()
    {
        if (piece != null) piece.SetFastDrop(true);
    }

    private void OnSoftDropCancel()
    {
        if (piece != null) piece.SetFastDrop(false);
    }
}
