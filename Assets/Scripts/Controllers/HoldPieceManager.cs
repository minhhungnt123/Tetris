using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Quản lý cơ chế Hold Piece (giữ khối):
/// Lưu trữ khối được giữ, hiển thị preview trên Tilemap riêng và kiểm soát quyền hold trong 1 lượt (SRP).
/// Nhận Tilemap qua Dependency Injection (không dùng GameObject.Find) để tương thích 1P & 2P.
/// </summary>
public class HoldPieceManager : MonoBehaviour
{
    [SerializeField] private Tilemap _holdPieceTilemap;

    private TetrominoData? _heldPieceData;
    private bool _hasUsedHoldInCurrentTurn;

    public TetrominoData? HeldPieceData => _heldPieceData;
    public bool HasUsedHoldInCurrentTurn => _hasUsedHoldInCurrentTurn;

    public void Initialize(Tilemap holdTilemap)
    {
        if (holdTilemap != null)
        {
            _holdPieceTilemap = holdTilemap;
        }
        Clear();
    }

    /// <summary>
    /// Thực hiện thao tác Hold. Trả về true nếu hold thành công, kèm khối cần spawn thay thế.
    /// Nếu out pieceToSpawn là null, game cần rút khối tiếp theo từ queue.
    /// </summary>
    public bool TryHold(TetrominoData currentPiece, out TetrominoData? pieceToSpawn)
    {
        pieceToSpawn = null;
        if (_hasUsedHoldInCurrentTurn) return false;

        _hasUsedHoldInCurrentTurn = true;

        if (!_heldPieceData.HasValue || _heldPieceData.Value.cells == null || _heldPieceData.Value.cells.Length == 0)
        {
            _heldPieceData = currentPiece;
            pieceToSpawn = null;
        }
        else
        {
            TetrominoData previousHold = _heldPieceData.Value;
            _heldPieceData = currentPiece;
            pieceToSpawn = previousHold;
        }

        DisplayHeldPiece();
        return true;
    }

    /// <summary>
    /// Cho phép dùng hold cho lượt tiếp theo (gọi khi khối mới được spawn).
    /// </summary>
    public void ResetTurnHoldFlag()
    {
        _hasUsedHoldInCurrentTurn = false;
    }

    public void Clear()
    {
        _heldPieceData = null;
        _hasUsedHoldInCurrentTurn = false;
        if (_holdPieceTilemap != null)
        {
            _holdPieceTilemap.ClearAllTiles();
        }
    }

    private void DisplayHeldPiece()
    {
        if (_holdPieceTilemap == null) return;
        _holdPieceTilemap.ClearAllTiles();

        if (!_heldPieceData.HasValue || _heldPieceData.Value.cells == null) return;

        Board.DrawPieceOnTilemap(_holdPieceTilemap, _heldPieceData.Value, Vector3Int.zero);
    }
}
