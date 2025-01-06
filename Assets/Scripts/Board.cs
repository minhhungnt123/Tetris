using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Board : MonoBehaviour
{
    public TetrominoData[] tetrominoes;
    public Tilemap tilemap {  get; private set; }
    public Piece activePiece { get; private set; }
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public int score { get; private set; }
    public TMP_Text scoreText;
    public Tilemap nextPieceTilemap;
    private TetrominoData nextTetromino;
    private List<Tetromino> spawnHistory = new List<Tetromino>();
    private const int maxConsecutiveCount = 2;
    public TMP_Text levelText;
    public int level { get; private set; } = 1;
    private const int linesPerLevel = 10;
    private int linesCleared = 0;
    private bool isGameOver = false;
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public Button startButton;
    public Button resetButton;
    public Button backToHomeButton;
    public Tilemap holdPieceTilemap;
    private TetrominoData holdPieceData;
    private bool hasUsedHold = false;
    public TMP_Text historyText;
    private List<int> scoreHistory = new List<int> { 5000, 4000, 3000, 2000, 1000};
    public TMP_Text gameOverHistoryText;
    private bool isPaused = false;
    public GameObject pausePanel;
    public Button resumeButton;
    public Button quitButton;
    public Button restartButton;
    public bool IsPaused { get; private set; }

    public bool IsGameOver
    {
        get { return isGameOver; }
    }

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }
    //Khởi tạo
    private void Awake()
    {
        this.activePiece = GetComponentInChildren<Piece>();
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.nextPieceTilemap = GameObject.Find("NextPieceTilemap").GetComponent<Tilemap>();

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
        startButton.onClick.AddListener(StartGame);
        resetButton.onClick.AddListener(ResetGame);
        backToHomeButton.onClick.AddListener(ShowStartMenu);
        resumeButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(ShowStartMenu);
        restartButton.onClick.AddListener(ResetGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    //Quản lý giao diện
    public void Start()
    {
        ShowStartMenu();
    }

    private void ShowStartMenu()
    {
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        tilemap.ClearAllTiles();

        UpdateHistoryText();
    }

    public void StartGame()
    {
        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        ResetGame();
    }

    private void GameOver()
    {
        this.tilemap.ClearAllTiles();
        this.nextPieceTilemap.ClearAllTiles();
        this.holdPieceTilemap.ClearAllTiles();

        isGameOver = true;
        scoreHistory.Add(this.score);
        UpdateHistoryText();
        gameOverPanel.SetActive(true);
        UpdateGameOverHistoryText();
    }

    public void BackToStartMenu()
    {
        gameOverPanel.SetActive(false);
        startPanel.SetActive(true);
        pausePanel.SetActive(false);
        ResetGame();
    }


    private void ResetGame()
    {
        this.isGameOver = false;
        this.score = 0;
        this.level = 1;
        scoreText.text = "Score: 0";
        levelText.text = "Level: 1";

        this.tilemap.ClearAllTiles();
        this.nextPieceTilemap.ClearAllTiles();
        this.spawnHistory.Clear();
        this.holdPieceTilemap.ClearAllTiles();

        if (activePiece != null)
        {
            this.activePiece.stepDelay = 1.0f;
        }

        nextTetromino = GetNextTetromino();
        SpawnPiece();

        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    public void PauseGame()
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
               spawnHistory.FindAll(t => t == nextData.tetromino).Count >= maxConsecutiveCount);

        spawnHistory.Add(nextData.tetromino);

        if (spawnHistory.Count > maxConsecutiveCount)
        {
            spawnHistory.RemoveAt(0);
        }

        return nextData;
    }
    //Sinh khối
    public void SpawnPiece(TetrominoData? specificData = null)
    {
        if (isGameOver) return;

        TetrominoData data = specificData ?? nextTetromino;

        activePiece.Initialized(this, spawnPosition, data);

        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
        }
        else
        {
            GameOver();
            return;
        }

        if (specificData == null)
        {
            nextTetromino = GetNextTetromino();
        }

        DisplayNextPiece(nextTetromino);
        hasUsedHold = false;
    }

    //Ô hiển thị khối tiếp theo

    private void DisplayNextPiece(TetrominoData data)
    {
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
    //Xử lý hàng
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
        AddScore(linesCleared);
    }
    //Kiểm tra hàng đã đầy hay chưa
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
    //Tăng cấp
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
    //Giữ khối
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
    //Lịch sử thành tựu
    private void UpdateHistoryText()
    {
        if (historyText != null)
        {
            List<int> sortedScores = new List<int>(scoreHistory);
            sortedScores.Sort((a, b) => b.CompareTo(a));

            int max = Mathf.Min(sortedScores.Count, 5);

            historyText.text = "HIGH SCORE\n";
            for (int i = 0; i < max; i++)
            {
                historyText.text += $"{i + 1}. {sortedScores[i]} points\n";
            }
        }
    }
    private void UpdateGameOverHistoryText()
    {
        if (gameOverHistoryText != null)
        {
            List<int> sortedScores = new List<int>(scoreHistory);
            sortedScores.Sort((a, b) => b.CompareTo(a));

            int max = Mathf.Min(sortedScores.Count, 5);

            gameOverHistoryText.text = "HIGH SCORE\n";
            for (int i = 0; i < max; i++)
            {
                gameOverHistoryText.text += $"{i + 1}. {sortedScores[i]} points\n";
            }
        }
    }
    //Tạm dừng trò chơi
    public void TogglePause()
    {
        IsPaused = !IsPaused;

        Time.timeScale = IsPaused ? 0 : 1;
    }
}
