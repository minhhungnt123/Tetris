using System.Collections;
using UnityEngine;

/// <summary>
/// Điều khiển hiệu ứng Ngọn Lửa Bùng Từ Chân Lên Đỉnh Bảng (Board Ignition Flame VFX)
/// theo đúng 3 giai đoạn: Idle -> IgniteSweep (0.4s) -> SustainedBurn -> Extinguish (0.35s).
/// Hỗ trợ Dual-Trigger (Passive Combo >= 3 Fever & Active Skill Overdrive) và tối ưu Zero-GC
/// thông qua Shader.PropertyToID, MaterialPropertyBlock và Coroutine yield return null.
/// </summary>
public class BoardIgnitionVFXController : MonoBehaviour
{
    public enum IgnitionState
    {
        Idle,
        IgniteSweep,
        SustainedBurn,
        Extinguish
    }

    [Header("Flame Frame Renderer (Shader Tetris/CartoonBoardIgnitionFlame)")]
    [SerializeField] private SpriteRenderer _flameFrameRenderer;
    [SerializeField] private SpriteRenderer _borderRenderer;

    [Header("2D Particle Systems (Optional)")]
    [SerializeField] private ParticleSystem _baseBurstParticles;
    [SerializeField] private ParticleSystem _sideSparksParticles;
    [SerializeField] private ParticleSystem _topCrestParticles;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource _oneShotAudioSource;
    [SerializeField] private AudioClip _igniteWhooshClip;
    [SerializeField] private AudioSource _burnLoopAudioSource;

    [Header("Timing Settings")]
    [SerializeField] private float _igniteSweepDuration = 0.4f;
    [SerializeField] private float _extinguishDuration = 0.35f;
    [SerializeField] private int _feverComboThreshold = 3;

    [Header("Default Fever Flame Palette (Cartoon Fantasy Arcade)")]
    [SerializeField] private Color _feverCoreColor = new Color(1.0f, 0.965f, 0.702f, 1.0f);  // #FFF6B3
    [SerializeField] private Color _feverMidColor = new Color(1.0f, 0.690f, 0.125f, 1.0f);   // #FFB020
    [SerializeField] private Color _feverEdgeColor = new Color(1.0f, 0.271f, 0.094f, 1.0f);  // #FF4518
    [SerializeField] private Color _normalBorderTint = Color.white;
    [SerializeField] private Color _burningBorderTint = new Color(1.0f, 0.85f, 0.55f, 1.0f);

    // Cache toàn bộ Shader Property ID một lần duy nhất (Zero-GC)
    private static readonly int RevealHeightId = Shader.PropertyToID("_RevealHeight");
    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    private static readonly int FlameCoreColorId = Shader.PropertyToID("_FlameCoreColor");
    private static readonly int FlameMidColorId = Shader.PropertyToID("_FlameMidColor");
    private static readonly int FlameEdgeColorId = Shader.PropertyToID("_FlameEdgeColor");

    private MaterialPropertyBlock _propertyBlock;
    private Coroutine _transitionCoroutine;

    private IScoreService _scoreService;
    private SkillController _skillController;
    private bool _isSubscribed;

    private bool _isFeverActive;
    private bool _isSkillOverdriveActive;
    private float _currentRevealHeight;
    private float _currentIntensity;

    private Color _activeCoreColor;
    private Color _activeMidColor;
    private Color _activeEdgeColor;

    public IgnitionState CurrentState { get; private set; } = IgnitionState.Idle;
    public bool IsBurning => CurrentState == IgnitionState.IgniteSweep || CurrentState == IgnitionState.SustainedBurn;

    private void Awake()
    {
        if (_flameFrameRenderer == null)
        {
            _flameFrameRenderer = GetComponent<SpriteRenderer>();
        }

        _propertyBlock = new MaterialPropertyBlock();
        _activeCoreColor = _feverCoreColor;
        _activeMidColor = _feverMidColor;
        _activeEdgeColor = _feverEdgeColor;

        ApplyShaderValues(revealHeight: 0f, intensity: 0f);
        SetFlameRendererActive(false);
    }

    private void OnDestroy()
    {
        UnsubscribeSources();
    }

    /// <summary>
    /// Kết nối Dual-Trigger với ScoreService và SkillController cục bộ của PlayfieldContainer (DIP).
    /// </summary>
    public void Initialize(IScoreService scoreService, SkillController skillController)
    {
        UnsubscribeSources();

        _scoreService = scoreService;
        _skillController = skillController;

        SubscribeSources();
        ResetImmediate();
    }

    /// <summary>
    /// Tắt lập tức toàn bộ lửa và hạt (gọi khi bắt đầu trận mới hoặc quay về Menu).
    /// </summary>
    public void ResetImmediate()
    {
        StopTransitionCoroutine();

        _isFeverActive = false;
        _isSkillOverdriveActive = false;
        CurrentState = IgnitionState.Idle;

        _activeCoreColor = _feverCoreColor;
        _activeMidColor = _feverMidColor;
        _activeEdgeColor = _feverEdgeColor;

        ApplyShaderValues(0f, 0f);
        SetFlameRendererActive(false);
        SetBorderTint(_normalBorderTint);

        StopParticlesImmediate(_baseBurstParticles);
        StopParticlesImmediate(_sideSparksParticles);
        StopParticlesImmediate(_topCrestParticles);

        if (_burnLoopAudioSource != null && _burnLoopAudioSource.isPlaying)
        {
            _burnLoopAudioSource.Stop();
        }
    }

    /// <summary>
    /// Kích hoạt bùng lửa thủ công (dùng cho testing hoặc hiệu ứng đặc biệt).
    /// </summary>
    public void TriggerIgnite(Color coreColor, Color midColor, Color edgeColor)
    {
        _activeCoreColor = coreColor;
        _activeMidColor = midColor;
        _activeEdgeColor = edgeColor;

        if (CurrentState == IgnitionState.SustainedBurn)
        {
            // Đang cháy sẵn -> chỉ cập nhật bảng màu lửa tức thì không cần quét lại từ đáy
            ApplyShaderValues(1f, 1f);
            return;
        }

        StopTransitionCoroutine();
        _transitionCoroutine = StartCoroutine(IgniteSweepRoutine());
    }

    public void TriggerExtinguish()
    {
        if (CurrentState == IgnitionState.Idle || CurrentState == IgnitionState.Extinguish) return;

        StopTransitionCoroutine();
        _transitionCoroutine = StartCoroutine(ExtinguishRoutine());
    }

    private void SubscribeSources()
    {
        if (_isSubscribed) return;

        if (_scoreService != null)
        {
            _scoreService.OnComboUpdated += HandleComboUpdated;
        }

        if (_skillController != null)
        {
            _skillController.OnSkillActivated += HandleSkillActivated;
            _skillController.OnSkillEnded += HandleSkillEnded;
        }

        _isSubscribed = true;
    }

    private void UnsubscribeSources()
    {
        if (!_isSubscribed) return;

        if (_scoreService != null)
        {
            _scoreService.OnComboUpdated -= HandleComboUpdated;
        }

        if (_skillController != null)
        {
            _skillController.OnSkillActivated -= HandleSkillActivated;
            _skillController.OnSkillEnded -= HandleSkillEnded;
        }

        _isSubscribed = false;
    }

    private void HandleComboUpdated(int comboCount)
    {
        bool shouldFever = comboCount >= _feverComboThreshold;
        if (shouldFever == _isFeverActive) return;

        _isFeverActive = shouldFever;
        EvaluateDualTriggerState();
    }

    private void HandleSkillActivated(SkillDefinitionSO definition, ISkillEffect effect)
    {
        _isSkillOverdriveActive = true;
        if (definition != null)
        {
            TriggerIgnite(definition.FlameCoreColor, definition.FlameMidColor, definition.FlameEdgeColor);
        }
        else
        {
            TriggerIgnite(_feverCoreColor, _feverMidColor, _feverEdgeColor);
        }
    }

    private void HandleSkillEnded(SkillDefinitionSO definition)
    {
        _isSkillOverdriveActive = false;
        EvaluateDualTriggerState();
    }

    private void EvaluateDualTriggerState()
    {
        if (_isSkillOverdriveActive) return;

        if (_isFeverActive)
        {
            TriggerIgnite(_feverCoreColor, _feverMidColor, _feverEdgeColor);
        }
        else
        {
            TriggerExtinguish();
        }
    }

    private IEnumerator IgniteSweepRoutine()
    {
        CurrentState = IgnitionState.IgniteSweep;
        SetFlameRendererActive(true);

        if (_baseBurstParticles != null) _baseBurstParticles.Play();
        if (_sideSparksParticles != null) _sideSparksParticles.Play();

        if (_oneShotAudioSource != null && _igniteWhooshClip != null)
        {
            _oneShotAudioSource.PlayOneShot(_igniteWhooshClip);
        }

        if (_burnLoopAudioSource != null && !_burnLoopAudioSource.isPlaying)
        {
            _burnLoopAudioSource.Play();
        }

        float startReveal = _currentRevealHeight;
        float startIntensity = _currentIntensity;
        float duration = Mathf.Max(0.05f, _igniteSweepDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease-out nhanh để tạo lực bùng mạnh từ chân lên đỉnh bảng
            float eased = 1f - (1f - t) * (1f - t);

            float reveal = Mathf.Lerp(startReveal, 1f, eased);
            float intensity = Mathf.Lerp(startIntensity, 1f, Mathf.Clamp01(t * 2.2f));

            ApplyShaderValues(reveal, intensity);
            SetBorderTint(Color.Lerp(_normalBorderTint, _burningBorderTint, eased));

            // yield return null không cấp phát bộ nhớ (0 bytes GC)
            yield return null;
        }

        ApplyShaderValues(1f, 1f);
        SetBorderTint(_burningBorderTint);

        CurrentState = IgnitionState.SustainedBurn;
        if (_topCrestParticles != null) _topCrestParticles.Play();

        _transitionCoroutine = null;
    }

    private IEnumerator ExtinguishRoutine()
    {
        CurrentState = IgnitionState.Extinguish;

        StopParticlesGracefully(_baseBurstParticles);
        StopParticlesGracefully(_sideSparksParticles);
        StopParticlesGracefully(_topCrestParticles);

        float startIntensity = _currentIntensity;
        float duration = Mathf.Max(0.05f, _extinguishDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float intensity = Mathf.Lerp(startIntensity, 0f, t);

            ApplyShaderValues(_currentRevealHeight, intensity);
            SetBorderTint(Color.Lerp(_burningBorderTint, _normalBorderTint, t));

            yield return null;
        }

        ApplyShaderValues(0f, 0f);
        SetBorderTint(_normalBorderTint);
        SetFlameRendererActive(false);

        if (_burnLoopAudioSource != null && _burnLoopAudioSource.isPlaying)
        {
            _burnLoopAudioSource.Stop();
        }

        CurrentState = IgnitionState.Idle;
        _transitionCoroutine = null;
    }

    private void ApplyShaderValues(float revealHeight, float intensity)
    {
        _currentRevealHeight = revealHeight;
        _currentIntensity = intensity;

        if (_flameFrameRenderer == null || _propertyBlock == null) return;

        _flameFrameRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetFloat(RevealHeightId, _currentRevealHeight);
        _propertyBlock.SetFloat(IntensityId, _currentIntensity);
        _propertyBlock.SetColor(FlameCoreColorId, _activeCoreColor);
        _propertyBlock.SetColor(FlameMidColorId, _activeMidColor);
        _propertyBlock.SetColor(FlameEdgeColorId, _activeEdgeColor);
        _flameFrameRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void SetFlameRendererActive(bool isActive)
    {
        if (_flameFrameRenderer != null && _flameFrameRenderer.enabled != isActive)
        {
            _flameFrameRenderer.enabled = isActive;
        }
    }

    private void SetBorderTint(Color tint)
    {
        if (_borderRenderer != null)
        {
            _borderRenderer.color = tint;
        }
    }

    private void StopTransitionCoroutine()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
    }

    private static void StopParticlesGracefully(ParticleSystem ps)
    {
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private static void StopParticlesImmediate(ParticleSystem ps)
    {
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
