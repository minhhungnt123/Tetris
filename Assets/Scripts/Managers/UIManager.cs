using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject pausePanel;

    [Header("Mode Select & Placeholder Screens")]
    public GameObject modeSelectPanel;
    public GameObject versusAIPanel;
    public GameObject lobbyPanel;

    public TMP_Text scoreText;
    public TMP_Text levelText;
    public TMP_Text historyText;
    public TMP_Text gameOverHistoryText;

    [Header("Combo & B2B Display")]
    [Tooltip("Hiển thị số combo hiện tại. Để trống nếu không có UI.")]
    public TMP_Text comboText;
    [Tooltip("Hiển thị nhãn B2B khi Tetris liên tiếp. Để trống nếu không có UI.")]
    public TMP_Text backToBackText;

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

    [Header("Versus AI Placeholder Buttons")]
    public Button versusAIBackButton;

    [Header("Lobby Placeholder Buttons")]
    [Tooltip("Nút Back trong LobbyPanel. Nút Create Room/Join Room do OnlineLobbyStub tự quản lý.")]
    public Button lobbyBackButton;

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

            startButton.onClick.AddListener(HandleStartButtonClicked);
            resetButton.onClick.AddListener(GameManager.Instance.StartGame);
            restartButton.onClick.AddListener(GameManager.Instance.StartGame);
            resumeButton.onClick.AddListener(GameManager.Instance.ResumeGame);

            backToHomeButton.onClick.AddListener(GameManager.Instance.ShowStartMenu);
            quitButton.onClick.AddListener(GameManager.Instance.ShowStartMenu);

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

    /// <summary>
    /// Ẩn tất cả các panel màn hình (start/mode select/versus AI/lobby/game over/pause)
    /// rồi chỉ hiện panel được chỉ định. Truyền null để ẩn hết (vào gameplay).
    /// </summary>
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
        // Tự ẩn sau 2 giây
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
        List<int> sortedScores = new List<int>(scoreHistory);
        sortedScores.Sort((a, b) => b.CompareTo(a));
        int max = Mathf.Min(sortedScores.Count, 5);
        string historyStr = "HIGH SCORE\n";
        for (int i = 0; i < max; i++)
        {
            historyStr += $"{i + 1}. {sortedScores[i]} points\n";
        }

        if (historyText != null) historyText.text = historyStr;
        if (gameOverHistoryText != null) gameOverHistoryText.text = historyStr;
    }
}
