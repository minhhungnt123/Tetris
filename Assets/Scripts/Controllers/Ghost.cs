using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

/// <summary>
/// Hiển thị hình chiếu (Ghost Piece) vị trí tiếp đất của khối đang rơi (SRP).
/// </summary>
public class Ghost : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("tile")] private Tile _tile;
    [SerializeField, FormerlySerializedAs("board")] private Board _board;
    [SerializeField, FormerlySerializedAs("trackingPiece")] private Piece _trackingPiece;
    [SerializeField] private Tilemap _tilemap;

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
        AutoWireDependencies();
    }

    private void Start()
    {
        AutoWireDependencies();

        if (_trackingPiece != null)
        {
            _trackingPiece.OnPieceUpdated += HandlePieceUpdated;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += HandleGameOver;
            GameManager.Instance.OnShowStartMenu += HandleShowStartMenu;
        }

        if (_trackingPiece != null && _trackingPiece.Cells != null && _trackingPiece.Cells.Length > 0)
        {
            HandlePieceUpdated();
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

    private void AutoWireDependencies()
    {
        if (_board == null)
        {
            _board = FindFirstObjectByType<Board>();
        }

        if (_trackingPiece == null && _board != null)
        {
            _trackingPiece = _board.GetComponent<Piece>();
        }

        if (_tilemap == null)
        {
            _tilemap = GetComponentInChildren<Tilemap>();
        }

        if (_tile == null)
        {
#if UNITY_EDITOR
            _tile = UnityEditor.AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles/Ghost.asset");
#endif
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
        if (_tilemap != null)
        {
            _tilemap.ClearAllTiles();
        }
    }

    private void Copy()
    {
        if (_trackingPiece == null || _trackingPiece.Cells == null || _trackingPiece.Cells.Length == 0) return;

        if (_cells == null || _cells.Length != _trackingPiece.Cells.Length)
        {
            _cells = new Vector3Int[_trackingPiece.Cells.Length];
        }

        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i] = _trackingPiece.Cells[i];
        }
    }

    private void Drop()
    {
        if (_trackingPiece == null || _board == null || _cells == null || _cells.Length == 0) return;

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
        if (_tilemap == null || _cells == null || _cells.Length == 0 || _tile == null) return;

        for (int i = 0; i < _cells.Length; i++)
        {
            Vector3Int tilePosition = _cells[i] + _position;
            _tilemap.SetTile(tilePosition, _tile);
        }
    }
}
