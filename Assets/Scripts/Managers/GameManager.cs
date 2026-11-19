using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý vòng đời và trạng thái toàn cục của game: Start, Pause, Resume, Game Over và Game Mode (SRP).
/// Các nghiệp vụ tính điểm và lưu trữ được ủy quyền cho ScoreManager (IScoreService).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private ScoreManager _scoreManager;

    public int Score => _scoreManager != null ? _scoreManager.CurrentScore : 0;
    public int Level { get; private set; } = 1;
    public int LinesCleared { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public int ComboCount => _scoreManager != null ? _scoreManager.ComboCount : 0;
    public GameMode CurrentGameMode { get; private set; } = GameMode.Marathon;

    public List<int> ScoreHistory => _scoreManager != null 
        ? new List<int>(_scoreManager.ScoreHistory) 
        : new List<int> { 5000, 4000, 3000, 2000, 1000 };

    private IGameModeRule _currentGameModeRule;

    // Các sự kiện vòng đời
    public event Action OnGameStarted;
    public event Action OnGameOver;
    public event Action<bool> OnPauseToggled;
    public event Action<int> OnLevelChanged;
    public event Action OnShowStartMenu;

    // Chuyển tiếp các sự kiện từ ScoreService để giữ tương thích ngược
    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboUpdated;
    public event Action OnBackToBack;
    public event Action<int, List<int>> OnLineClearScore;
    public event Action<List<int>> OnScoreHistoryUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (_scoreManager == null)
        {
            _scoreManager = GetComponent<ScoreManager>();
            if (_scoreManager == null)
            {
                _scoreManager = gameObject.AddComponent<ScoreManager>();
            }
        }

        // Đăng ký chuyển tiếp sự kiện từ ScoreManager
        _scoreManager.OnScoreChanged += (score) => OnScoreChanged?.Invoke(score);
        _scoreManager.OnComboUpdated += (combo) => OnComboUpdated?.Invoke(combo);
        _scoreManager.OnBackToBack += () => OnBackToBack?.Invoke();
        _scoreManager.OnLineClearScore += (pts, rows) => OnLineClearScore?.Invoke(pts, rows);
        _scoreManager.OnScoreHistoryUpdated += (hist) => OnScoreHistoryUpdated?.Invoke(new List<int>(hist));
    }

    private void Start()
    {
        ShowStartMenu();
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPause += HandlePauseInput;
        }
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPause -= HandlePauseInput;
        }
    }

    private void HandlePauseInput()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void ShowStartMenu()
    {
        OnShowStartMenu?.Invoke();
        if (_scoreManager != null)
        {
            OnScoreHistoryUpdated?.Invoke(new List<int>(_scoreManager.ScoreHistory));
        }
    }

    public void StartGame()
    {
        StartGame(GameMode.Marathon);
    }

    public void StartGame(GameMode mode)
    {
        CurrentGameMode = mode;
        IsGameOver = false;
        Level = 1;
        LinesCleared = 0;
        IsPaused = false;
        Time.timeScale = 1;

        // Khởi tạo quy tắc cho GameMode tương ứng (Strategy Pattern - OCP)
        switch (mode)
        {
            case GameMode.Sprint:
                _currentGameModeRule = new SprintModeRule(40);
                break;
            default:
                _currentGameModeRule = new MarathonModeRule(10);
                break;
        }
        _currentGameModeRule.OnGameStarted();

        if (_scoreManager != null)
        {
            _scoreManager.ResetScore();
        }

        OnLevelChanged?.Invoke(Level);
        OnGameStarted?.Invoke();
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        if (_scoreManager != null)
        {
            _scoreManager.SaveCurrentScore();
        }

        OnGameOver?.Invoke();
    }

    public void PauseGame()
    {
        if (IsGameOver) return;
        IsPaused = true;
        Time.timeScale = 0;
        OnPauseToggled?.Invoke(IsPaused);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1;
        OnPauseToggled?.Invoke(IsPaused);
    }

    public void AddScore(int lines, List<int> clearedRows = null)
    {
        if (IsGameOver) return;

        int pointsEarned = 0;
        if (_scoreManager != null)
        {
            pointsEarned = _scoreManager.AddLineClearScore(lines, Level, clearedRows);
        }

        if (lines > 0)
        {
            LinesCleared += lines;

            if (_currentGameModeRule != null)
            {
                _currentGameModeRule.OnLinesCleared(LinesCleared, lines, out bool shouldLevelUp, out bool isGoalCompleted);

                if (shouldLevelUp)
                {
                    LevelUp();
                }

                if (isGoalCompleted)
                {
                    // Đạt mục tiêu (ví dụ xóa xong 40 lines ở chế độ Sprint)
                    GameOver();
                }
            }
        }
    }

    private void LevelUp()
    {
        Level++;
        OnLevelChanged?.Invoke(Level);
    }
}
