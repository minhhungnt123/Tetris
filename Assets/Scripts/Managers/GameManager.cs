using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý vòng đời và trạng thái toàn cục của game: Start, Pause, Resume, Game Over và Game Mode (SRP).
/// Hỗ trợ điều phối 1P/2P thông qua LayoutTransitionController & PlayfieldContainer (DIP),
/// đồng thời giữ tương thích ngược nếu Scene chưa gắn PlayfieldContainer.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private LayoutTransitionController _layoutTransitionController;

    private IScoreService ActivePrimaryScoreService =>
        _layoutTransitionController != null && _layoutTransitionController.PlayerOneContainer != null
            ? _layoutTransitionController.PlayerOneContainer.ScoreService
            : _scoreManager;

    public int Score => ActivePrimaryScoreService != null ? ActivePrimaryScoreService.CurrentScore : 0;
    public int Level { get; private set; } = 1;
    public int LinesCleared { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public int ComboCount => ActivePrimaryScoreService != null ? ActivePrimaryScoreService.ComboCount : 0;
    public GameMode CurrentGameMode { get; private set; } = GameMode.Marathon;

    public List<int> ScoreHistory
    {
        get
        {
            _scoreHistoryCache.Clear();
            IScoreService service = ActivePrimaryScoreService;
            if (service != null && service.ScoreHistory != null)
            {
                IReadOnlyList<int> source = service.ScoreHistory;
                for (int i = 0; i < source.Count; i++)
                {
                    _scoreHistoryCache.Add(source[i]);
                }
            }
            return _scoreHistoryCache;
        }
    }

    private readonly List<int> _scoreHistoryCache = new List<int>(5);
    private IGameModeRule _currentGameModeRule;
    private IScoreService _subscribedScoreService;
    private PlayfieldContainer _subscribedPlayerOne;

    // Các sự kiện vòng đời
    public event Action OnGameStarted;
    public event Action OnGameOver;
    public event Action<bool> OnPauseToggled;
    public event Action<int> OnLevelChanged;
    public event Action OnShowStartMenu;

    // Chuyển tiếp các sự kiện từ ScoreService để giữ tương thích ngược với UIManager
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

        if (_layoutTransitionController != null && _layoutTransitionController.PlayerOneContainer != null)
        {
            BindPrimaryPlayfield(_layoutTransitionController.PlayerOneContainer);
        }
        else
        {
            BindScoreService(_scoreManager);
        }
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
        UnbindPrimaryPlayfield();
        UnbindScoreService();

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPause -= HandlePauseInput;
        }
    }

    private void BindPrimaryPlayfield(PlayfieldContainer container)
    {
        if (container == null || _subscribedPlayerOne == container) return;

        UnbindPrimaryPlayfield();
        _subscribedPlayerOne = container;
        _subscribedPlayerOne.Initialize(playerId: 1);

        _subscribedPlayerOne.OnLevelChanged += HandlePrimaryLevelChanged;
        _subscribedPlayerOne.OnLinesTotalChanged += HandlePrimaryLinesChanged;
        _subscribedPlayerOne.OnPlayerDefeated += HandlePrimaryPlayerEnded;
        _subscribedPlayerOne.OnGoalCompleted += HandlePrimaryPlayerEnded;

        BindScoreService(_subscribedPlayerOne.ScoreService);
    }

    private void UnbindPrimaryPlayfield()
    {
        if (_subscribedPlayerOne == null) return;

        _subscribedPlayerOne.OnLevelChanged -= HandlePrimaryLevelChanged;
        _subscribedPlayerOne.OnLinesTotalChanged -= HandlePrimaryLinesChanged;
        _subscribedPlayerOne.OnPlayerDefeated -= HandlePrimaryPlayerEnded;
        _subscribedPlayerOne.OnGoalCompleted -= HandlePrimaryPlayerEnded;
        _subscribedPlayerOne = null;
    }

    private void BindScoreService(IScoreService service)
    {
        if (service == null || _subscribedScoreService == service) return;

        UnbindScoreService();
        _subscribedScoreService = service;

        _subscribedScoreService.OnScoreChanged += ForwardScoreChanged;
        _subscribedScoreService.OnComboUpdated += ForwardComboUpdated;
        _subscribedScoreService.OnBackToBack += ForwardBackToBack;
        _subscribedScoreService.OnLineClearScore += ForwardLineClearScore;
        _subscribedScoreService.OnScoreHistoryUpdated += ForwardScoreHistoryUpdated;
    }

    private void UnbindScoreService()
    {
        if (_subscribedScoreService == null) return;

        _subscribedScoreService.OnScoreChanged -= ForwardScoreChanged;
        _subscribedScoreService.OnComboUpdated -= ForwardComboUpdated;
        _subscribedScoreService.OnBackToBack -= ForwardBackToBack;
        _subscribedScoreService.OnLineClearScore -= ForwardLineClearScore;
        _subscribedScoreService.OnScoreHistoryUpdated -= ForwardScoreHistoryUpdated;
        _subscribedScoreService = null;
    }

    private void ForwardScoreChanged(int score) => OnScoreChanged?.Invoke(score);
    private void ForwardComboUpdated(int combo) => OnComboUpdated?.Invoke(combo);
    private void ForwardBackToBack() => OnBackToBack?.Invoke();
    private void ForwardLineClearScore(int pts, List<int> rows) => OnLineClearScore?.Invoke(pts, rows);
    private void ForwardScoreHistoryUpdated(List<int> history) => OnScoreHistoryUpdated?.Invoke(history);

    private void HandlePrimaryLevelChanged(PlayfieldContainer container, int newLevel)
    {
        Level = newLevel;
        OnLevelChanged?.Invoke(Level);
    }

    private void HandlePrimaryLinesChanged(PlayfieldContainer container, int totalLines)
    {
        LinesCleared = totalLines;
    }

    private void HandlePrimaryPlayerEnded(PlayfieldContainer container)
    {
        GameOver();
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
        IsPaused = false;
        Time.timeScale = 1f;

        if (_layoutTransitionController != null)
        {
            _layoutTransitionController.SetSoloLayout(instant: true);
            if (_layoutTransitionController.PlayerOneContainer != null)
            {
                _layoutTransitionController.PlayerOneContainer.ClearVisuals();
            }
        }

        OnShowStartMenu?.Invoke();
        OnScoreHistoryUpdated?.Invoke(ScoreHistory);
    }

    public void StartGame()
    {
        StartGame(CurrentGameMode);
    }

    public void StartGame(GameMode mode)
    {
        CurrentGameMode = mode;
        IsGameOver = false;
        Level = 1;
        LinesCleared = 0;
        IsPaused = false;
        Time.timeScale = 1f;

        _currentGameModeRule = CreateGameModeRule(mode);

        if (_layoutTransitionController != null && _layoutTransitionController.PlayerOneContainer != null)
        {
            bool isVersusMode = mode == GameMode.VersusAI || mode == GameMode.OnlineMultiplayer;
            if (isVersusMode)
            {
                _layoutTransitionController.SetVersusLayout(instant: false);
            }
            else
            {
                _layoutTransitionController.SetSoloLayout(instant: false);
            }

            BindPrimaryPlayfield(_layoutTransitionController.PlayerOneContainer);
            _layoutTransitionController.PlayerOneContainer.StartMatch(_currentGameModeRule);

            if (isVersusMode && _layoutTransitionController.PlayerTwoContainer != null)
            {
                _layoutTransitionController.PlayerTwoContainer.StartMatch(CreateGameModeRule(mode));
            }
        }
        else
        {
            _currentGameModeRule.OnGameStarted();
            if (_scoreManager != null)
            {
                _scoreManager.ResetScore();
            }
            OnLevelChanged?.Invoke(Level);
        }

        OnGameStarted?.Invoke();
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;

        if (_layoutTransitionController != null)
        {
            if (_layoutTransitionController.PlayerOneContainer != null)
            {
                _layoutTransitionController.PlayerOneContainer.StopMatch(saveHighScore: true);
            }
            if (_layoutTransitionController.PlayerTwoContainer != null && _layoutTransitionController.PlayerTwoContainer.gameObject.activeSelf)
            {
                _layoutTransitionController.PlayerTwoContainer.StopMatch(saveHighScore: false);
            }
        }
        else if (_scoreManager != null)
        {
            _scoreManager.SaveCurrentScore();
        }

        OnGameOver?.Invoke();
    }

    public void PauseGame()
    {
        if (IsGameOver) return;
        IsPaused = true;
        Time.timeScale = 0f;
        ApplyPauseToContainers(true);
        OnPauseToggled?.Invoke(IsPaused);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        ApplyPauseToContainers(false);
        OnPauseToggled?.Invoke(IsPaused);
    }

    public void AddScore(int lines, List<int> clearedRows = null)
    {
        if (IsGameOver) return;

        if (_scoreManager != null)
        {
            _scoreManager.AddLineClearScore(lines, Level, clearedRows);
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
                    GameOver();
                }
            }
        }
    }

    private void ApplyPauseToContainers(bool isPaused)
    {
        if (_layoutTransitionController == null) return;

        if (_layoutTransitionController.PlayerOneContainer != null)
        {
            _layoutTransitionController.PlayerOneContainer.SetPaused(isPaused);
        }
        if (_layoutTransitionController.PlayerTwoContainer != null && _layoutTransitionController.PlayerTwoContainer.gameObject.activeSelf)
        {
            _layoutTransitionController.PlayerTwoContainer.SetPaused(isPaused);
        }
    }

    private IGameModeRule CreateGameModeRule(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Sprint:
                return new SprintModeRule(40);
            default:
                return new MarathonModeRule(10);
        }
    }

    private void LevelUp()
    {
        Level++;
        OnLevelChanged?.Invoke(Level);
    }
}
