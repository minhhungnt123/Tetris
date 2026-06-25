using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile tile;
    public Board board;
    public Piece trackingPiece;

    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.cells = new Vector3Int[4];
    }

    private void Start()
    {
        if (this.trackingPiece != null)
        {
            this.trackingPiece.OnPieceUpdated += HandlePieceUpdated;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += HandleGameOver;
            GameManager.Instance.OnShowStartMenu += HandleShowStartMenu;
        }
    }

    private void OnDestroy()
    {
        if (this.trackingPiece != null)
        {
            this.trackingPiece.OnPieceUpdated -= HandlePieceUpdated;
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

    // Game Over hoặc quay về màn Home: Piece không còn bắn OnPieceUpdated nữa
    // nên ghost của khối cuối cùng sẽ bị kẹt lại trên board nếu không xóa thủ công.
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
        if (this.tilemap != null) this.tilemap.ClearAllTiles();
    }

    private void Clear()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = this.trackingPiece.cells[i];
        }
    }

    private void Drop()
    {
        Vector3Int position = this.trackingPiece.position;

        int current = position.y;
        int bottom = -this.board.boardSize.y / 2 - 1;

        this.board.Clear(this.trackingPiece);

        for(int row = current; row >= bottom; row--)
        {
            position.y = row;
            if(this.board.IsValidPosition(this.trackingPiece, position))
            {
                this.position = position;
            }
            else
            {
                break;
            }
        }
        this.board.Set(this.trackingPiece);
    }

    private void Set()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, this.tile);
        }
    }
}
