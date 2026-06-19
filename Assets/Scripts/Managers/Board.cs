using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class Board : MonoBehaviour
{
    public TetrominoData[] tetrominoes;
    public Tilemap tilemap {  get; private set; }
    public Piece activePiece { get; private set; }
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    
    public Tilemap nextPieceTilemap;
    private TetrominoData nextTetromino;

    // 7-Bag Randomizer: đảm bảo mỗi 7 loại tetromino xuất hiện đúng 1 lần
    private readonly Queue<TetrominoData> _pieceQueue = new Queue<TetrominoData>();
    
    public Tilemap holdPieceTilemap;
    private TetrominoData holdPieceData;
    private bool hasUsedHold = false;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        this.activePiece = GetComponentInChildren<Piece>();
        this.tilemap = GetComponentInChildren<Tilemap>();
        // nextPieceTilemap không còn được auto-find:
        // NextPieceQueueDisplay.cs chịu trách nhiệm hiển thị preview queue

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += ResetGame;
            GameManager.Instance.OnGameOver += HandleGameOver;
            GameManager.Instance.OnLevelChanged += HandleLevelChanged;
            GameManager.Instance.OnShowStartMenu += ClearAll;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= ResetGame;
            GameManager.Instance.OnGameOver -= HandleGameOver;
            GameManager.Instance.OnLevelChanged -= HandleLevelChanged;
            GameManager.Instance.OnShowStartMenu -= ClearAll;
        }
    }

    private void ClearAll()
    {
        if (this.tilemap != null) this.tilemap.ClearAllTiles();
        if (this.nextPieceTilemap != null) this.nextPieceTilemap.ClearAllTiles();
        if (this.holdPieceTilemap != null) this.holdPieceTilemap.ClearAllTiles();
    }

    private void HandleGameOver()
    {
        ClearAll();
    }

    private void ResetGame()
    {
        ClearAll();
        _pieceQueue.Clear();

        if (activePiece != null)
        {
            this.activePiece.stepDelay = 1.0f;
        }

        nextTetromino = DequeueNextTetromino();
        SpawnPiece();
    }

    private void HandleLevelChanged(int level)
    {
        if (this.activePiece != null)
        {
            float newStepDelay = Mathf.Max(0.1f, 1.0f * Mathf.Pow(0.9f, level - 1));
            this.activePiece.UpdateStepDelay(newStepDelay);
        }
    }

    // === 7-Bag Randomizer ===

    /// <summary>
    /// Lấy khối tiếp theo từ queue. Nếu queue rỗng, tự động nạp thêm 1 bag mới.
    /// </summary>
    private TetrominoData DequeueNextTetromino()
    {
        if (_pieceQueue.Count == 0)
        {
            RefillBag();
        }
        return _pieceQueue.Dequeue();
    }

    /// <summary>
    /// Tạo một bag mới gồm đủ 7 loại tetromino, trộn ngẫu nhiên bằng
    /// thuật toán Fisher-Yates (không thiên vị), rồi đẩy vào _pieceQueue.
    /// </summary>
    private void RefillBag()
    {
        // Sao chép toàn bộ mảng tetrominoes vào list tạm
        var bag = new List<TetrominoData>(tetrominoes);

        // Fisher-Yates shuffle: O(n), phân phối đều
        for (int i = bag.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }

        foreach (TetrominoData piece in bag)
        {
            _pieceQueue.Enqueue(piece);
        }
    }
    
    public void SpawnPiece(TetrominoData? specificData = null)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        
        TetrominoData data = specificData ?? nextTetromino;
        activePiece.Initialized(this, spawnPosition, data);
        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
            this.activePiece.TriggerPieceUpdated();
        }
        else
        {
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
            return;
        }
        
        if (specificData == null)
        {
            nextTetromino = DequeueNextTetromino();
        }
        DisplayNextPiece(nextTetromino);
        hasUsedHold = false;
    }



    private void DisplayNextPiece(TetrominoData data)
    {
        if (this.nextPieceTilemap == null) return;
        this.nextPieceTilemap.ClearAllTiles();
        DrawPieceOnTilemap(this.nextPieceTilemap, data, new Vector3Int(-1, 1, 0));
    }

    /// <summary>
    /// Vẽ một tetromino lên Tilemap tại vị trí offset cho trước.
    /// </summary>
    public static void DrawPieceOnTilemap(Tilemap tilemap, TetrominoData data, Vector3Int offset)
    {
        foreach (Vector2Int cell in data.cells)
        {
            Vector3Int tilePos = new Vector3Int(cell.x * 2, cell.y * 2, 0) + offset;
            tilemap.SetTile(tilePos, data.tile);
        }
    }

    public void Set(Piece piece)
    {
        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }
    
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }
    //Kiểm tra tính hợp lệ của vị trí
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++;
            }
        }
        
        // Luôn gọi AddScore để GameManager cập nhật trạng thái Combo
        // (kể cả khi linesCleared == 0 để reset combo)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(linesCleared);
        }
    }
    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;
        for(int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!this.tilemap.HasTile(position))
            {
                return false ;
            }
        }
        return true ;
    }
    //Xóa hàng
    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;
        for(int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }
        while(row < bounds.yMax)
        {
            for(int col = bounds.xMin; col <bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }
            row++;
        }
    }

    //Xử lý chức năng giữ khối
    private void DisplayHoldPiece()
    {
        if (this.holdPieceTilemap == null) return;
        this.holdPieceTilemap.ClearAllTiles();

        if (holdPieceData.cells != null && holdPieceData.cells.Length > 0)
        {
            Vector3Int startPosition = new Vector3Int(-1, 1, 0);

            foreach (Vector2Int cell in holdPieceData.cells)
            {
                Vector3Int tilePosition = new Vector3Int(cell.x * 2, cell.y * 2, 0) + startPosition;
                this.holdPieceTilemap.SetTile(tilePosition, holdPieceData.tile);
            }
        }
    }
    public void HoldPiece()
    {
        if (hasUsedHold) return;

        Clear(this.activePiece);

        if (holdPieceData.cells == null || holdPieceData.cells.Length == 0)
        {
            holdPieceData = this.activePiece.data;
            nextTetromino = DequeueNextTetromino();
            SpawnPiece();
        }
        else
        {
            TetrominoData temp = holdPieceData;
            holdPieceData = this.activePiece.data;
            SpawnPiece(temp);
        }

        DisplayHoldPiece();
        hasUsedHold = true;
    }
}
