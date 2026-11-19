using UnityEngine;

[CreateAssetMenu(fileName = "TetrisScoringSettings", menuName = "Tetris/Scoring Settings")]
public class TetrisScoringSettingsSO : ScriptableObject
{
    [Header("Guideline Base Points")]
    [Tooltip("Điểm gốc tương ứng với số hàng xóa [0, 1(Single), 2(Double), 3(Triple), 4(Tetris)].")]
    [SerializeField] private int[] _basePoints = { 0, 100, 300, 500, 800 };

    [Header("Level Progression")]
    [Tooltip("Số hàng cần xóa để thăng 1 level.")]
    [SerializeField] private int _linesPerLevel = 10;

    [Header("Multipliers")]
    [Tooltip("Hệ số tăng thêm cho mỗi lần combo liên tiếp (Combo 2 = +0.5x, Combo 3 = +1.0x,...).")]
    [SerializeField] private float _comboStepMultiplier = 0.5f;

    [Tooltip("Hệ số nhân khi đạt Back-to-Back Tetris.")]
    [SerializeField] private float _backToBackMultiplier = 1.5f;

    public int LinesPerLevel => _linesPerLevel;
    public float BackToBackMultiplier => _backToBackMultiplier;
    public float ComboStepMultiplier => _comboStepMultiplier;

    public int GetBasePoint(int lines)
    {
        if (lines <= 0) return 0;
        if (lines < _basePoints.Length) return _basePoints[lines];
        return _basePoints[_basePoints.Length - 1];
    }

    public int CalculateScore(int lines, int currentLevel, int comboCount, bool isBackToBack)
    {
        if (lines <= 0) return 0;

        int basePoint = GetBasePoint(lines);
        int rawPoint = basePoint * (currentLevel + 1);

        float comboMultiplier = 1f + Mathf.Max(0, comboCount - 1) * _comboStepMultiplier;
        float b2bMultiplier = isBackToBack ? _backToBackMultiplier : 1f;

        return Mathf.RoundToInt(rawPoint * comboMultiplier * b2bMultiplier);
    }
}
