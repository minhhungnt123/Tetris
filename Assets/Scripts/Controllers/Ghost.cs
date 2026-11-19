using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Hiển thị hình chiếu (Ghost Piece) vị trí tiếp đất của khối đang rơi (SRP).
/// </summary>
public class Ghost : MonoBehaviour
{
    [SerializeField] private Tile _tile;
    [SerializeField] private Board _board;
    [SerializeField] private Piece _trackingPiece;

    private Tilemap _tilemap;
    private Vector3Int[] _cells;
    private Vector3Int _position;

    public Tilemap Tilemap => _tilemap;
    public Vector3Int[] Cells => _cells;
    public Vector3Int Position => _position;

    // Backward-compatibility aliases
    public Tile tile
    {
        get => _tile;
        set => _tile = value;
    }
    public Board board
    {
        get => _board;
        set => _board = value;
    }
    public Piece trackingPiece
    {
        get => _trackingPiece;
        set => _trackingPiece = value;
    }

    private void Awake()
    {
        _tilemap = GetComponentInChildren<Tilemap>();
        _cells = new Vector3Int[4];
    }

    private void Start()
    {
        if (_trackingPiece != null)
        {
            _trackingPiece.OnPieceUpdated += HandlePieceUpdated;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += HandleGameOver;
            GameManager.Instance.OnShowStartMenu += HandleShowStartMenu;
        }
    }

    private void OnDestroy()
    {
        if (_trackingPiece != null)
        {
            _trackingPiece.OnPieceUpdated -= HandlePieceUpdated;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= HandleGameOver;
            GameManager.Instance.OnShowStartMenu -= HandleShowStartMenu;
        }
    }

    private void HandlePieceUpdated()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    private void HandleGameOver()
    {
        ClearGhostTilemap();
    }

    private void HandleShowStartMenu()
    {
        ClearGhostTilemap();
    }

    private void ClearGhostTilemap()
    {
        if (_tilemap != null) _tilemap.ClearAllTiles();
    }

    private void Clear()
    {
        if (_tilemap == null || _cells == null) return;

        for (int i = 0; i < _cells.Length; i++)
        {
            Vector3Int tilePosition = _cells[i] + _position;
            _tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        if (_trackingPiece == null || _trackingPiece.Cells == null) return;

        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i] = _trackingPiece.Cells[i];
        }
    }

    private void Drop()
    {
        if (_trackingPiece == null || _board == null) return;

        Vector3Int currentPos = _trackingPiece.Position;
        int current = currentPos.y;
        int bottom = -_board.BoardSize.y / 2 - 1;

        _board.Clear(_trackingPiece);

        for (int row = current; row >= bottom; row--)
        {
            currentPos.y = row;
            if (_board.IsValidPosition(_trackingPiece, currentPos))
            {
                _position = currentPos;
            }
            else
            {
                break;
            }
        }

        _board.Set(_trackingPiece);
    }

    private void Set()
    {
        if (_tilemap == null || _cells == null) return;

        for (int i = 0; i < _cells.Length; i++)
        {
            Vector3Int tilePosition = _cells[i] + _position;
            _tilemap.SetTile(tilePosition, _tile);
        }
    }
}
