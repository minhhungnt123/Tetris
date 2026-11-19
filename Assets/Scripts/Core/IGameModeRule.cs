/// <summary>
/// Interface cho quy tắc của các chế độ chơi khác nhau (OCP).
/// </summary>
public interface IGameModeRule
{
    GameMode Mode { get; }
    void OnGameStarted();
    void OnLinesCleared(int totalLinesCleared, int linesJustCleared, out bool shouldLevelUp, out bool isGoalCompleted);
}
