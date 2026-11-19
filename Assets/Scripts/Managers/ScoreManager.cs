using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý việc tính toán điểm số theo chuẩn Tetris Guideline, xử lý combo,
/// Back-to-Back và lưu trữ kỷ lục điểm số vào PlayerPrefs (SRP & DIP).
/// </summary>
public class ScoreManager : MonoBehaviour, IScoreService
{
    private const string HighScorePrefsKey = "Tetris_HighScores";
    private const int MaxHistoryCount = 5;

    [SerializeField] private TetrisScoringSettingsSO _scoringSettings;

    public int CurrentScore { get; private set; }
    public int ComboCount { get; private set; }

    private bool _lastClearWasTetris;
    private readonly List<int> _scoreHistory = new List<int>();

    public IReadOnlyList<int> ScoreHistory => _scoreHistory;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboUpdated;
    public event Action OnBackToBack;
    public event Action<int, List<int>> OnLineClearScore;
    public event Action<List<int>> OnScoreHistoryUpdated;

    private void Awake()
    {
        LoadScoreHistory();
    }

    public void Initialize(TetrisScoringSettingsSO settings)
    {
        _scoringSettings = settings;
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        ComboCount = 0;
        _lastClearWasTetris = false;

        OnScoreChanged?.Invoke(CurrentScore);
        OnComboUpdated?.Invoke(ComboCount);
    }

    public int AddLineClearScore(int lines, int currentLevel, List<int> clearedRows = null)
    {
        if (lines <= 0)
        {
            // Reset combo khi không xóa được dòng nào
            ComboCount = 0;
            _lastClearWasTetris = false;
            OnComboUpdated?.Invoke(ComboCount);
            return 0;
        }

        ComboCount++;
        OnComboUpdated?.Invoke(ComboCount);

        bool isTetris = (lines == 4);
        bool isBackToBack = isTetris && _lastClearWasTetris;

        if (isBackToBack)
        {
            OnBackToBack?.Invoke();
        }

        _lastClearWasTetris = isTetris;

        int finalPoint;
        if (_scoringSettings != null)
        {
            finalPoint = _scoringSettings.CalculateScore(lines, currentLevel, ComboCount, isBackToBack);
        }
        else
        {
            // Fallback nếu chưa gắn ScriptableObject
            int[] basePoints = { 0, 100, 300, 500, 800 };
            int basePoint = lines < basePoints.Length ? basePoints[lines] : basePoints[basePoints.Length - 1];
            float comboMul = 1f + Mathf.Max(0, ComboCount - 1) * 0.5f;
            float b2bMul = isBackToBack ? 1.5f : 1f;
            finalPoint = Mathf.RoundToInt(basePoint * (currentLevel + 1) * comboMul * b2bMul);
        }

        CurrentScore += finalPoint;
        OnScoreChanged?.Invoke(CurrentScore);
        OnLineClearScore?.Invoke(finalPoint, clearedRows);

        return finalPoint;
    }

    public void SaveCurrentScore()
    {
        _scoreHistory.Add(CurrentScore);
        _scoreHistory.Sort((a, b) => b.CompareTo(a));

        if (_scoreHistory.Count > MaxHistoryCount)
        {
            _scoreHistory.RemoveRange(MaxHistoryCount, _scoreHistory.Count - MaxHistoryCount);
        }

        SaveScoreHistoryToPrefs();
        OnScoreHistoryUpdated?.Invoke(_scoreHistory);
    }

    private void LoadScoreHistory()
    {
        _scoreHistory.Clear();
        string raw = PlayerPrefs.GetString(HighScorePrefsKey, string.Empty);

        if (!string.IsNullOrEmpty(raw))
        {
            string[] items = raw.Split(',');
            foreach (string item in items)
            {
                if (int.TryParse(item, out int s))
                {
                    _scoreHistory.Add(s);
                }
            }
        }

        // Nếu chưa có lịch sử, khởi tạo mặc định ban đầu
        if (_scoreHistory.Count == 0)
        {
            _scoreHistory.AddRange(new[] { 5000, 4000, 3000, 2000, 1000 });
        }

        _scoreHistory.Sort((a, b) => b.CompareTo(a));
    }

    private void SaveScoreHistoryToPrefs()
    {
        string raw = string.Join(",", _scoreHistory);
        PlayerPrefs.SetString(HighScorePrefsKey, raw);
        PlayerPrefs.Save();
    }
}
