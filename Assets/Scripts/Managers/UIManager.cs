using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý hiển thị các màn hình và cập nhật HUD (View layer).
/// Lắng nghe các sự kiện từ GameManager và ScoreManager (SRP).
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Primary Panels")]
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject pausePanel;

    [Header("Mode Select & Placeholder Screens")]
    public GameObject modeSelectPanel;
    public GameObject versusAIPanel;
    public GameObject lobbyPanel;

    [Header("HUD Text Elements")]
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public TMP_Text historyText;
    public TMP_Text gameOverHistoryText;

    [Header("Combo & B2B Display")]
    [Tooltip("Hiển thị số combo hiện tại.")]
    public TMP_Text comboText;
    [Tooltip("Hiển thị nhãn B2B khi Tetris liên tiếp.")]
    public TMP_Text backToBackText;

    [Header("Main Menu & Dialog Buttons")]
    public Button startButton;
    public Button resetButton;
    public Button backToHomeButton;
    public Button resumeButton;
    public Button quitButton;
    public Button restartButton;

    [Header("Mode Select Buttons")]
    public Button marathonButton;
    public Button sprintButton;
    public Button versusAIButton;
    public Button onlineMultiplayerButton;
    public Button modeSelectBackButton;

    [Header("Versus AI Buttons")]
    public Button versusAIBackButton;

    [Header("Lobby Buttons")]
    public Button lobbyBackButton;

    private readonly StringBuilder _historyStringBuilder = new StringBuilder();

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += HandleGameStarted;
            GameManager.Instance.OnGameOver += HandleGameOver;
            GameManager.Instance.OnPauseToggled += HandlePauseToggled;
            GameManager.Instance.OnScoreChanged += HandleScoreChanged;
            GameManager.Instance.OnLevelChanged += HandleLevelChanged;
            GameManager.Instance.OnScoreHistoryUpdated += HandleScoreHistoryUpdated;
            GameManager.Instance.OnShowStartMenu += HandleShowStartMenu;
            GameManager.Instance.OnComboUpdated += HandleComboUpdated;
            GameManager.Instance.OnBackToBack += HandleBackToBack;

            if (startButton != null) startButton.onClick.AddListener(HandleStartButtonClicked);
            if (resetButton != null) resetButton.onClick.AddListener(GameManager.Instance.StartGame);
            if (restartButton != null) restartButton.onClick.AddListener(GameManager.Instance.StartGame);
            if (resumeButton != null) resumeButton.onClick.AddListener(GameManager.Instance.ResumeGame);

            if (backToHomeButton != null) backToHomeButton.onClick.AddListener(GameManager.Instance.ShowStartMenu);
            if (quitButton != null) quitButton.onClick.AddListener(GameManager.Instance.ShowStartMenu);

            if (marathonButton != null) marathonButton.onClick.AddListener(HandleMarathonButtonClicked);
            if (sprintButton != null) sprintButton.interactable = false;
            if (versusAIButton != null) versusAIButton.onClick.AddListener(HandleVersusAIButtonClicked);
            if (onlineMultiplayerButton != null) onlineMultiplayerButton.onClick.AddListener(HandleOnlineMultiplayerButtonClicked);
            if (modeSelectBackButton != null) modeSelectBackButton.onClick.AddListener(HandleBackToStartClicked);
            if (versusAIBackButton != null) versusAIBackButton.onClick.AddListener(HandleBackToModeSelectClicked);
            if (lobbyBackButton != null) lobbyBackButton.onClick.AddListener(HandleBackToModeSelectClicked);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= HandleGameStarted;
            GameManager.Instance.OnGameOver -= HandleGameOver;
            GameManager.Instance.OnPauseToggled -= HandlePauseToggled;
            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
            GameManager.Instance.OnLevelChanged -= HandleLevelChanged;
            GameManager.Instance.OnScoreHistoryUpdated -= HandleScoreHistoryUpdated;
            GameManager.Instance.OnShowStartMenu -= HandleShowStartMenu;
            GameManager.Instance.OnComboUpdated -= HandleComboUpdated;
            GameManager.Instance.OnBackToBack -= HandleBackToBack;
        }
    }

    private void HandleShowStartMenu()
    {
        ShowOnlyPanel(startPanel);
    }

    private void HandleGameStarted()
    {
        ShowOnlyPanel(null);
    }

    private void ShowOnlyPanel(GameObject panelToShow)
    {
        if (startPanel != null) startPanel.SetActive(startPanel == panelToShow);
        if (modeSelectPanel != null) modeSelectPanel.SetActive(modeSelectPanel == panelToShow);
        if (versusAIPanel != null) versusAIPanel.SetActive(versusAIPanel == panelToShow);
        if (lobbyPanel != null) lobbyPanel.SetActive(lobbyPanel == panelToShow);
        if (gameOverPanel != null) gameOverPanel.SetActive(gameOverPanel == panelToShow);
        if (pausePanel != null) pausePanel.SetActive(pausePanel == panelToShow);
    }

    private void HandleStartButtonClicked()
    {
        ShowOnlyPanel(modeSelectPanel);
    }

    private void HandleMarathonButtonClicked()
    {
        GameManager.Instance.StartGame(GameMode.Marathon);
    }

    private void HandleVersusAIButtonClicked()
    {
        ShowOnlyPanel(versusAIPanel);
    }

    private void HandleOnlineMultiplayerButtonClicked()
    {
        ShowOnlyPanel(lobbyPanel);
    }

    private void HandleBackToStartClicked()
    {
        ShowOnlyPanel(startPanel);
    }

    private void HandleBackToModeSelectClicked()
    {
        ShowOnlyPanel(modeSelectPanel);
    }

    private void HandleGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private void HandlePauseToggled(bool isPaused)
    {
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    private void HandleScoreChanged(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {newScore}";
        }
    }

    private void HandleComboUpdated(int comboCount)
    {
        if (comboText == null) return;

        if (comboCount >= 2)
        {
            comboText.text = $"COMBO x{comboCount}";
            comboText.gameObject.SetActive(true);
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }

    private void HandleBackToBack()
    {
        if (backToBackText == null) return;

        backToBackText.text = "BACK-TO-BACK!";
        backToBackText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideBackToBackText));
        Invoke(nameof(HideBackToBackText), 2f);
    }

    private void HideBackToBackText()
    {
        if (backToBackText != null)
        {
            backToBackText.gameObject.SetActive(false);
        }
    }

    private void HandleLevelChanged(int newLevel)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {newLevel}";
        }
    }

    private void HandleScoreHistoryUpdated(List<int> scoreHistory)
    {
        if (scoreHistory == null) return;

        List<int> sortedScores = new List<int>(scoreHistory);
        sortedScores.Sort((a, b) => b.CompareTo(a));
        int max = Mathf.Min(sortedScores.Count, 5);

        _historyStringBuilder.Clear();
        _historyStringBuilder.AppendLine("HIGH SCORE");
        for (int i = 0; i < max; i++)
        {
            _historyStringBuilder.Append(i + 1).Append(". ").Append(sortedScores[i]).AppendLine(" points");
        }

        string historyStr = _historyStringBuilder.ToString();
        if (historyText != null) historyText.text = historyStr;
        if (gameOverHistoryText != null) gameOverHistoryText.text = historyStr;
    }
}
