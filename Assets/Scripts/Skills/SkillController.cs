using System;
using UnityEngine;

/// <summary>
/// Quản lý thanh năng lượng Skill (Skill Energy Gauge) và vòng đời kích hoạt kỹ năng của từng bàn chơi (SRP).
/// Gắn trực tiếp trên PlayfieldContainer để hoạt động độc lập giữa Player 1 và Player 2 (không dùng Singleton).
/// </summary>
public class SkillController : MonoBehaviour
{
    [Header("Equipped Skill")]
    [SerializeField] private SkillDefinitionSO _equippedSkill;

    [Header("Energy Gain Rules (0 - 100)")]
    [SerializeField] private float _maxEnergy = 100f;
    [SerializeField] private float _singleLineEnergy = 8f;
    [SerializeField] private float _doubleLineEnergy = 18f;
    [SerializeField] private float _tripleLineEnergy = 30f;
    [SerializeField] private float _tetrisLineEnergy = 50f;
    [SerializeField] private float _comboBonusPerStack = 5f;
    [SerializeField] private float _backToBackBonus = 15f;

    [Header("Fever Passive Multiplier")]
    [Tooltip("Hệ số tăng tốc độ nạp năng lượng khi đang đạt chuỗi Combo >= 3 (Fever Flame).")]
    [SerializeField] private float _feverEnergyMultiplier = 1.25f;
    [SerializeField] private int _feverComboThreshold = 3;

    private float _currentEnergy;
    private bool _wasReadyNotified;
    private bool _isSkillActive;
    private ISkillEffect _cachedEffectInstance;
    private SkillDefinitionSO _cachedEffectSource;

    // PascalCase Properties
    public SkillDefinitionSO EquippedSkill => _equippedSkill;
    public float CurrentEnergy => _currentEnergy;
    public float MaxEnergy => _equippedSkill != null ? _equippedSkill.EnergyCost : _maxEnergy;
    public float NormalizedEnergy => MaxEnergy > 0f ? Mathf.Clamp01(_currentEnergy / MaxEnergy) : 0f;
    public bool IsSkillReady => _equippedSkill != null && !_isSkillActive && _currentEnergy >= MaxEnergy;
    public bool IsSkillActive => _isSkillActive;
    public ISkillEffect ActiveEffect => _isSkillActive ? _cachedEffectInstance : null;

    // Events theo chuẩn DIP cho UI Gauge, Board, Ghost và BoardIgnitionVFXController
    public event Action<float, float> OnEnergyChanged;
    public event Action<SkillDefinitionSO> OnSkillReady;
    public event Action<SkillDefinitionSO, ISkillEffect> OnSkillActivated;
    public event Action<SkillDefinitionSO> OnSkillEnded;

    public void EquipSkill(SkillDefinitionSO skillDefinition)
    {
        _equippedSkill = skillDefinition;
        RebuildCachedEffect();
        ResetEnergy();
    }

    public void ResetEnergy()
    {
        if (_isSkillActive)
        {
            CompleteActiveSkill();
        }

        _currentEnergy = 0f;
        _wasReadyNotified = false;
        OnEnergyChanged?.Invoke(_currentEnergy, MaxEnergy);
    }

    /// <summary>
    /// Cộng năng lượng dựa trên kết quả phá dòng, Combo và Back-to-Back (Zero-GC).
    /// </summary>
    public void AddEnergyFromLineClear(int linesCleared, int comboCount, bool isBackToBack)
    {
        if (_equippedSkill == null || linesCleared <= 0 || _isSkillActive) return;

        float gainedEnergy = GetBaseLineEnergy(linesCleared);

        if (comboCount >= 2)
        {
            gainedEnergy += _comboBonusPerStack * comboCount;
        }

        if (isBackToBack)
        {
            gainedEnergy += _backToBackBonus;
        }

        if (comboCount >= _feverComboThreshold)
        {
            gainedEnergy *= _feverEnergyMultiplier;
        }

        AddEnergy(gainedEnergy);
    }

    public void AddEnergy(float amount)
    {
        if (_equippedSkill == null || amount <= 0f) return;

        float max = MaxEnergy;
        float previous = _currentEnergy;
        _currentEnergy = Mathf.Clamp(_currentEnergy + amount, 0f, max);

        if (!Mathf.Approximately(previous, _currentEnergy))
        {
            OnEnergyChanged?.Invoke(_currentEnergy, max);
        }

        if (!_wasReadyNotified && _currentEnergy >= max)
        {
            _wasReadyNotified = true;
            OnSkillReady?.Invoke(_equippedSkill);
        }
    }

    /// <summary>
    /// Người chơi chủ động bấm phím kích hoạt Skill khi thanh năng lượng đã đạt 100%.
    /// </summary>
    public bool TryActivateSkill()
    {
        if (!IsSkillReady) return false;

        RebuildCachedEffect();
        if (_cachedEffectInstance == null) return false;

        _currentEnergy = 0f;
        _wasReadyNotified = false;
        _isSkillActive = true;

        OnEnergyChanged?.Invoke(_currentEnergy, MaxEnergy);
        OnSkillActivated?.Invoke(_equippedSkill, _cachedEffectInstance);
        return true;
    }

    /// <summary>
    /// Gọi sau khi khối gạch đặc biệt hoàn tất thực thi hiệu ứng trên Board.
    /// </summary>
    public void CompleteActiveSkill()
    {
        if (!_isSkillActive) return;

        _isSkillActive = false;
        OnSkillEnded?.Invoke(_equippedSkill);
    }

    private void RebuildCachedEffect()
    {
        if (_equippedSkill == null)
        {
            _cachedEffectInstance = null;
            _cachedEffectSource = null;
            return;
        }

        // Chỉ tạo instance 1 lần cho mỗi SkillDefinitionSO để tránh cấp phát lặp lại (Zero-GC)
        if (_cachedEffectSource != _equippedSkill || _cachedEffectInstance == null)
        {
            _cachedEffectInstance = _equippedSkill.CreateEffectInstance();
            _cachedEffectSource = _equippedSkill;
        }
    }

    private float GetBaseLineEnergy(int linesCleared)
    {
        switch (linesCleared)
        {
            case 1: return _singleLineEnergy;
            case 2: return _doubleLineEnergy;
            case 3: return _tripleLineEnergy;
            default: return _tetrisLineEnergy;
        }
    }
}
