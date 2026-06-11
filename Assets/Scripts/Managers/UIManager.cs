using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject pausePanel;

    public TMP_Text scoreText;
    public TMP_Text levelText;
    public TMP_Text historyText;
    public TMP_Text gameOverHistoryText;

    public Button startButton;
    public Button resetButton;
    public Button backToHomeButton;
    public Button resumeButton;
    public Button quitButton;
    public Button restartButton;

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

            startButton.onClick.AddListener(GameManager.Instance.StartGame);
            resetButton.onClick.AddListener(GameManager.Instance.StartGame);
            restartButton.onClick.AddListener(GameManager.Instance.StartGame);
            resumeButton.onClick.AddListener(GameManager.Instance.ResumeGame);
            
            backToHomeButton.onClick.AddListener(GameManager.Instance.ShowStartMenu);
            quitButton.onClick.AddListener(GameManager.Instance.ShowStartMenu);
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
        }
    }

    private void HandleShowStartMenu()
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void HandleGameStarted()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
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
