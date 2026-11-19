using System;
using System.Collections.Generic;

/// <summary>
/// Interface cho dịch vụ tính điểm và lưu trữ kỷ lục điểm số (DIP & ISP).
/// </summary>
public interface IScoreService
{
    int CurrentScore { get; }
    int ComboCount { get; }
    IReadOnlyList<int> ScoreHistory { get; }

    event Action<int> OnScoreChanged;
    event Action<int> OnComboUpdated;
    event Action OnBackToBack;
    event Action<int, List<int>> OnLineClearScore;
    event Action<List<int>> OnScoreHistoryUpdated;

    void ResetScore();
    int AddLineClearScore(int lines, int currentLevel, List<int> clearedRows = null);
    void SaveCurrentScore();
}
