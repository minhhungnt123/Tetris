using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public int Level { get; private set; } = 1;
    public int LinesCleared { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public int ComboCount { get; private set; }

    private const int LinesPerLevel = 10;

    // Bảng điểm chuẩn Tetris Guideline (điểm gốc, nhân với Level+1)
    private static readonly int[] BasePoints = { 0, 100, 300, 500, 800 };

    // Theo dõi trạng thái xóa để tính Back-to-Back
    private bool _lastClearWasTetris;
    
    public List<int> ScoreHistory { get; private set; } = new List<int> { 5000, 4000, 3000, 2000, 1000 };

    public event Action OnGameStarted;
    public event Action OnGameOver;
    public event Action<bool> OnPauseToggled;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnLevelChanged;
    public event Action<List<int>> OnScoreHistoryUpdated;
    public event Action OnShowStartMenu;

    /// <summary>Kích hoạt khi combo thay đổi. Tham số: số combo hiện tại (0 = reset).</summary>
    public event Action<int> OnComboUpdated;

    /// <summary>Kích hoạt khi xảy ra Back-to-Back Tetris.</summary>
    public event Action OnBackToBack;

    /// <summary>
    /// Kích hoạt sau khi xóa dòng thành công.
    /// Tham số: (int points) điểm vừa nhận được, (List&lt;int&gt; clearedRows) chỉ số các hàng đã xóa.
    /// </summary>
    public event Action<int, List<int>> OnLineClearScore;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
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
        OnScoreHistoryUpdated?.Invoke(ScoreHistory);
    }

    public void StartGame()
    {
        IsGameOver = false;
        Score = 0;
        Level = 1;
        LinesCleared = 0;
        IsPaused = false;
        ComboCount = 0;
        _lastClearWasTetris = false;
        Time.timeScale = 1;

        OnScoreChanged?.Invoke(Score);
        OnLevelChanged?.Invoke(Level);
        OnGameStarted?.Invoke();
    }

    public void GameOver()
    {
        IsGameOver = true;
        ScoreHistory.Add(Score);
        OnScoreHistoryUpdated?.Invoke(ScoreHistory);
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

    /// <summary>
    /// Cập nhật điểm khi xóa dòng thành công.
    /// </summary>
    /// <param name="lines">Số dòng đã xóa.</param>
    /// <param name="clearedRows">Danh sách chỉ số hàng đã xóa (dùng để spawn floating text).</param>
    public void AddScore(int lines, List<int> clearedRows = null)
    {
        if (lines <= 0)
        {
            // Không xóa được dòng nào: reset combo
            ComboCount = 0;
            _lastClearWasTetris = false;
            OnComboUpdated?.Invoke(ComboCount);
            return;
        }

        // --- Tính Combo ---
        ComboCount++;
        OnComboUpdated?.Invoke(ComboCount);

        // Combo multiplier: x1.0 lần đầu, +0.5 mỗi combo tiếp theo
        float comboMultiplier = 1f + Mathf.Max(0, ComboCount - 1) * 0.5f;

        // --- Tính Back-to-Back (Tetris liên tiếp) ---
        bool isTetris = lines == 4;
        bool isBackToBack = isTetris && _lastClearWasTetris;
        float b2bMultiplier = isBackToBack ? 1.5f : 1f;

        if (isBackToBack)
        {
            OnBackToBack?.Invoke();
        }

        _lastClearWasTetris = isTetris;

        // --- Tính điểm theo Tetris Guideline ---
        int basePoint = lines < BasePoints.Length ? BasePoints[lines] : BasePoints[BasePoints.Length - 1];
        int rawPoint = basePoint * (Level + 1);
        int finalPoint = Mathf.RoundToInt(rawPoint * comboMultiplier * b2bMultiplier);

        Score += finalPoint;
        OnScoreChanged?.Invoke(Score);

        // Phát sự kiện floating score text
        OnLineClearScore?.Invoke(finalPoint, clearedRows);

        LinesCleared += lines;
        if (LinesCleared >= Level * LinesPerLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        OnLevelChanged?.Invoke(Level);
    }
}
