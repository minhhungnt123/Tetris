using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

/// <summary>
/// Hiển thị hình chiếu (Ghost Piece) vị trí tiếp đất của khối đang rơi và vùng ảnh hưởng nổ
/// (Blast Zone Preview) khi kích hoạt kỹ năng đặc biệt (SRP & Zero-GC).
/// </summary>
public class Ghost : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("tile")] private Tile _tile;
    [SerializeField, FormerlySerializedAs("board")] private Board _board;
    [SerializeField, FormerlySerializedAs("trackingPiece")] private Piece _trackingPiece;
    [SerializeField] private Tilemap _tilemap;

    [Header("Skill Blast Zone Preview")]
    [Tooltip("Tilemap riêng để tô sáng vùng sắp nổ (nếu để trống sẽ vẽ trên cùng Tilemap Ghost).")]
    [SerializeField] private Tilemap _blastPreviewTilemap;
    [SerializeField] private TileBase _defaultBlastPreviewTile;
    [SerializeField] private Color _blastPreviewColor = new Color(1f, 0.32f, 0.08f, 0.42f);

    private Vector3Int[] _cells;
    private Vector3Int _position;
    private bool _isManagedByContainer;
    private bool _isSubscribedToPiece;
    private bool _isSubscribedToGameManager;

    // Reusable buffer cho Blast Zone Preview (0 bytes GC mỗi lần di chuyển/xoay)
    private readonly List<Vector3Int> _blastPreviewCellsBuffer = new List<Vector3Int>(40);
    private ISkillEffect _activeSkillEffect;
    private TileBase _activeBlastPreviewTile;

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
        ResolveLocalComponents();
    }

    private void Start()
    {
        ResolveLocalComponents();
        SubscribeToPiece();

        if (!_isManagedByContainer)
        {
            SubscribeToGameManager();
        }

        if (_trackingPiece != null && _trackingPiece.Cells != null && _trackingPiece.Cells.Length > 0)
        {
            HandlePieceUpdated();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromPiece();
        UnsubscribeFromGameManager();
    }

    /// <summary>
    /// Tiêm phụ thuộc trực tiếp từ PlayfieldContainer (DIP), loại bỏ hoàn toàn FindFirstObjectByType.
    /// </summary>
    public void Initialize(Board board, Piece trackingPiece, Tilemap tilemap = null)
    {
        UnsubscribeFromPiece();
        UnsubscribeFromGameManager();

        _isManagedByContainer = true;
        _board = board;
        _trackingPiece = trackingPiece;

        if (tilemap != null)
        {
            _tilemap = tilemap;
        }
        else if (_tilemap == null)
        {
            _tilemap = GetComponentInChildren<Tilemap>();
        }

        SubscribeToPiece();
        ClearGhostTilemap();
    }

    /// <summary>
    /// Kích hoạt hiển thị Blast Zone Preview cho Skill đang dùng và vẽ lại ngay lập tức.
    /// </summary>
    public void SetActiveSkillPreview(ISkillEffect skillEffect, TileBase previewTile = null)
    {
        _activeSkillEffect = skillEffect;
        _activeBlastPreviewTile = previewTile != null
            ? previewTile
            : (_defaultBlastPreviewTile != null ? _defaultBlastPreviewTile : _tile);

        if (_trackingPiece != null && _trackingPiece.Cells != null && _trackingPiece.Cells.Length > 0)
        {
            HandlePieceUpdated();
        }
    }

    public void ClearBlastPreview()
    {
        _activeSkillEffect = null;
        _activeBlastPreviewTile = null;
        _blastPreviewCellsBuffer.Clear();

        if (_blastPreviewTilemap != null)
        {
            _blastPreviewTilemap.ClearAllTiles();
        }
    }

    public void ClearGhostTilemap()
    {
        if (_tilemap != null)
        {
            _tilemap.ClearAllTiles();
        }
        if (_blastPreviewTilemap != null)
        {
            _blastPreviewTilemap.ClearAllTiles();
        }
    }

    private void ResolveLocalComponents()
    {
        if (_tilemap == null)
        {
            _tilemap = GetComponentInChildren<Tilemap>();
        }

        if (_trackingPiece == null && _board != null)
        {
            _trackingPiece = _board.GetComponentInChildren<Piece>();
        }

        if (_tile == null)
        {
#if UNITY_EDITOR
            _tile = UnityEditor.AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles/Ghost.asset");
#endif
        }
    }

    private void SubscribeToPiece()
    {
        if (_trackingPiece != null && !_isSubscribedToPiece)
        {
            _trackingPiece.OnPieceUpdated += HandlePieceUpdated;
            _isSubscribedToPiece = true;
        }
    }

    private void UnsubscribeFromPiece()
    {
        if (_trackingPiece != null && _isSubscribedToPiece)
        {
            _trackingPiece.OnPieceUpdated -= HandlePieceUpdated;
            _isSubscribedToPiece = false;
        }
    }

    private void SubscribeToGameManager()
    {
        if (GameManager.Instance != null && !_isSubscribedToGameManager)
        {
            GameManager.Instance.OnGameOver += HandleGameOver;
            GameManager.Instance.OnShowStartMenu += HandleShowStartMenu;
            _isSubscribedToGameManager = true;
        }
    }

    private void UnsubscribeFromGameManager()
    {
        if (GameManager.Instance != null && _isSubscribedToGameManager)
        {
            GameManager.Instance.OnGameOver -= HandleGameOver;
            GameManager.Instance.OnShowStartMenu -= HandleShowStartMenu;
            _isSubscribedToGameManager = false;
        }
    }

    private void HandlePieceUpdated()
    {
        Clear();
        Copy();
        Drop();
        RenderBlastZonePreview();
        Set();
    }

    private void HandleGameOver()
    {
        ClearBlastPreview();
        ClearGhostTilemap();
    }

    private void HandleShowStartMenu()
    {
        ClearBlastPreview();
        ClearGhostTilemap();
    }

    private void Clear()
    {
        if (_tilemap != null)
        {
            _tilemap.ClearAllTiles();
        }
        if (_blastPreviewTilemap != null)
        {
            _blastPreviewTilemap.ClearAllTiles();
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

    private void RenderBlastZonePreview()
    {
        if (_activeSkillEffect == null || _board == null || _cells == null || _cells.Length == 0) return;

        Tilemap targetMap = _blastPreviewTilemap != null ? _blastPreviewTilemap : _tilemap;
        TileBase previewTile = _activeBlastPreviewTile != null ? _activeBlastPreviewTile : _tile;
        if (targetMap == null || previewTile == null) return;

        _activeSkillEffect.GetPreviewCells(_board, _cells, _position, _blastPreviewCellsBuffer);

        int count = _blastPreviewCellsBuffer.Count;
        for (int i = 0; i < count; i++)
        {
            Vector3Int cellPos = _blastPreviewCellsBuffer[i];
            targetMap.SetTile(cellPos, previewTile);
            targetMap.SetTileFlags(cellPos, TileFlags.None);
            targetMap.SetColor(cellPos, _blastPreviewColor);
        }
    }

    private void Set()
    {
        if (_tilemap == null || _cells == null || _cells.Length == 0 || _tile == null) return;

        for (int i = 0; i < _cells.Length; i++)
        {
            Vector3Int tilePosition = _cells[i] + _position;
            _tilemap.SetTile(tilePosition, _tile);
            _tilemap.SetTileFlags(tilePosition, TileFlags.None);
            _tilemap.SetColor(tilePosition, Color.white);
        }
    }
}
