public class MarathonModeRule : IGameModeRule
{
    private readonly int _linesPerLevel;
    private int _currentLevel = 1;

    public GameMode Mode => GameMode.Marathon;

    public MarathonModeRule(int linesPerLevel = 10)
    {
        _linesPerLevel = linesPerLevel;
    }

    public void OnGameStarted()
    {
        _currentLevel = 1;
    }

    public void OnLinesCleared(int totalLinesCleared, int linesJustCleared, out bool shouldLevelUp, out bool isGoalCompleted)
    {
        isGoalCompleted = false;
        if (totalLinesCleared >= _currentLevel * _linesPerLevel)
        {
            _currentLevel++;
            shouldLevelUp = true;
        }
        else
        {
            shouldLevelUp = false;
        }
    }
}
