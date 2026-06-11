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

    private const int LinesPerLevel = 10;
    
    public List<int> ScoreHistory { get; private set; } = new List<int> { 5000, 4000, 3000, 2000, 1000 };

    public event Action OnGameStarted;
    public event Action OnGameOver;
    public event Action<bool> OnPauseToggled;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnLevelChanged;
    public event Action<List<int>> OnScoreHistoryUpdated;
    public event Action OnShowStartMenu;

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

    public void AddScore(int lines)
    {
        int point = 0;
        switch (lines)
        {
            case 1: point = 50 * (Level + 1); break;
            case 2: point = 100 * (Level + 1); break;
            case 3: point = 300 * (Level + 1); break;
            case 4: point = 1200 * (Level + 1); break;
        }
        Score += point;
        OnScoreChanged?.Invoke(Score);

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
