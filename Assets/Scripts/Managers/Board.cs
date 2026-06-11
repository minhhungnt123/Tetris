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
    private List<Tetromino> spawnHistory = new List<Tetromino>();
    private const int maxConsecutiveCount = 2;
    
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
        // Hardcode fallback removed; prefer Inspector assignment but retain old behavior if null
        if (this.nextPieceTilemap == null)
        {
            GameObject nextObj = GameObject.Find("NextPieceTilemap");
            if (nextObj != null) this.nextPieceTilemap = nextObj.GetComponent<Tilemap>();
        }

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
        this.spawnHistory.Clear();

        if (activePiece != null)
        {
            this.activePiece.stepDelay = 1.0f;
        }

        nextTetromino = GetNextTetromino();
        SpawnPiece();
    }

    private void HandleLevelChanged(int level)
    {
        if (isGameOver) return;

        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1;
    }

    //Xử lý khối
    private TetrominoData GetNextTetromino()
    {
        TetrominoData nextData;
        do
        {
            int random = Random.Range(0, tetrominoes.Length);
            nextData = tetrominoes[random];
        }
        while (spawnHistory.Count >= maxConsecutiveCount &&
               spawnHistory.FindAll(t => t.Equals(nextData.tetromino)).Count >= maxConsecutiveCount);
        spawnHistory.Add(nextData.tetromino);
        if (spawnHistory.Count > maxConsecutiveCount)
        {
            spawnHistory.RemoveAt(0);
        }
        return nextData;
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
            nextTetromino = GetNextTetromino();
        }
        DisplayNextPiece(nextTetromino);
        hasUsedHold = false;
    }



    private void DisplayNextPiece(TetrominoData data)
    {
        if (this.nextPieceTilemap == null) return;
        this.nextPieceTilemap.ClearAllTiles();
        Vector3Int startPosition = new Vector3Int(-1, 1, 0);

        foreach (Vector2Int cell in data.cells)
        {
            Vector3Int tilePosition = new Vector3Int(cell.x * 2, cell.y * 2, 0) + startPosition;
            this.nextPieceTilemap.SetTile(tilePosition, data.tile);
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
        
        if (linesCleared > 0 && GameManager.Instance != null)
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
    //Xử lý điểm số
    private void AddScore(int linesCleared)
    {
        int point = 0;

        switch (linesCleared)
        {
            case 1: point = 50 * (level + 1); break;
            case 2: point = 100 * (level + 1); break;
            case 3: point = 300 * (level + 1); break;
            case 4: point = 1200 * (level + 1); break;
            default: point = 0; break;
        }
        this.score += point;

        if (scoreText != null)
        {
            scoreText.text = $"Score: {this.score}";
        }
        this.linesCleared++;
        if (this.linesCleared >= this.level * linesPerLevel)
        {
            LevelUp();
        }

        Debug.Log($"Score: {this.score}");
    }

    private void LevelUp()
    {
        this.level++;

        float newStepDelay = Mathf.Max(0.1f, this.activePiece.stepDelay * 0.9f);
        this.activePiece.UpdateStepDelay(newStepDelay);

        if (levelText != null)
        {
            levelText.text = $"Level: {this.level}";
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
            nextTetromino = GetNextTetromino();
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
