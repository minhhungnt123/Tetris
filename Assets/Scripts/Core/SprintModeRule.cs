public class SprintModeRule : IGameModeRule
{
    private readonly int _targetLines;

    public GameMode Mode => GameMode.Sprint;

    public SprintModeRule(int targetLines = 40)
    {
        _targetLines = targetLines;
    }

    public void OnGameStarted()
    {
    }

    public void OnLinesCleared(int totalLinesCleared, int linesJustCleared, out bool shouldLevelUp, out bool isGoalCompleted)
    {
        shouldLevelUp = false;
        isGoalCompleted = (totalLinesCleared >= _targetLines);
    }
}
