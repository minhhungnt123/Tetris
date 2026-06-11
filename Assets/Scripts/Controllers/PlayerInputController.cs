using UnityEngine;

[RequireComponent(typeof(Piece))]
public class PlayerInputController : MonoBehaviour
{
    private Piece piece;

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
            inputManager.OnRotate -= OnRotate;
            inputManager.OnHardDrop -= OnHardDrop;
            inputManager.OnHold -= OnHold;
            inputManager.OnSoftDropStart -= OnSoftDropStart;
            inputManager.OnSoftDropCancel -= OnSoftDropCancel;
        }
    }

    private bool IsInputAllowed()
    {
        if (piece == null || piece.board == null) 
        {
            Debug.Log("PlayerInputController: IsInputAllowed -> false (piece or board is null)");
            return false;
        }
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) 
        {
            Debug.Log("PlayerInputController: IsInputAllowed -> false (GameOver or Paused)");
            return false;
        }
        return true;
    }

    private void OnMove(Vector2 direction)
    {
        Debug.Log("PlayerInputController: Received OnMove with direction " + direction);
        if (!IsInputAllowed()) return;

        if (direction.x < -0.5f) piece.MovePiece(Vector2Int.left);
        else if (direction.x > 0.5f) piece.MovePiece(Vector2Int.right);
        else if (direction.y < -0.5f) piece.MovePiece(Vector2Int.down);
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
