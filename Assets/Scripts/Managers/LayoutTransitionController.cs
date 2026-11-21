using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Điều phối chuyển đổi bố cục mượt mà giữa chế độ 1P (Solo căn giữa) và 2P (Versus trái/phải).
/// Sử dụng Coroutine không cấp phát bộ nhớ (yield return null - Zero-GC) và tái sử dụng instance P2 (Pooling)
/// để tránh Instantiate/Destroy lặp lại.
/// </summary>
public class LayoutTransitionController : MonoBehaviour
{
    public enum PlayfieldLayoutMode
    {
        Solo,
        Versus
    }

    [Header("Playfield Containers")]
    [SerializeField] private PlayfieldContainer _playerOneContainer;
    [SerializeField] private PlayfieldContainer _playerTwoContainer;
    [Tooltip("Prefab dùng để khởi tạo Player 2 (chỉ Instantiate 1 lần duy nhất nếu chưa gán sẵn).")]
    [SerializeField] private PlayfieldContainer _playfieldPrefab;
    [SerializeField] private Transform _containersRoot;

    [Header("Solo (1P) Transform Targets")]
    [SerializeField] private Vector3 _soloPosition = Vector3.zero;
    [SerializeField] private Vector3 _soloScale = Vector3.one;

    [Header("Versus (2P) Transform Targets")]
    [SerializeField] private Vector3 _versusLeftPosition = new Vector3(-220f, 0f, 0f);
    [SerializeField] private Vector3 _versusRightPosition = new Vector3(220f, 0f, 0f);
    [SerializeField] private Vector3 _versusRightSpawnOffset = new Vector3(120f, 0f, 0f);
    [SerializeField] private Vector3 _versusScale = new Vector3(0.85f, 0.85f, 1f);

    [Header("Transition Animation")]
    [SerializeField] private float _transitionDuration = 0.45f;
    [SerializeField] private AnimationCurve _easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine _transitionCoroutine;

    public PlayfieldLayoutMode CurrentLayoutMode { get; private set; } = PlayfieldLayoutMode.Solo;
    public bool IsTransitioning => _transitionCoroutine != null;
    public PlayfieldContainer PlayerOneContainer => _playerOneContainer;
    public PlayfieldContainer PlayerTwoContainer => _playerTwoContainer;

    public event Action<PlayfieldLayoutMode> OnLayoutTransitionStarted;
    public event Action<PlayfieldLayoutMode> OnLayoutTransitionCompleted;

    /// <summary>
    /// Chuyển bố cục về chế độ Chơi Đơn (1 bảng căn giữa, ẩn bảng P2 mà không Destroy).
    /// </summary>
    public void SetSoloLayout(bool instant = false)
    {
        CurrentLayoutMode = PlayfieldLayoutMode.Solo;
        StopActiveTransition();

        if (instant || _transitionDuration <= 0f)
        {
            ApplySoloInstant();
            OnLayoutTransitionCompleted?.Invoke(CurrentLayoutMode);
            return;
        }

        OnLayoutTransitionStarted?.Invoke(CurrentLayoutMode);
        _transitionCoroutine = StartCoroutine(TransitionToSoloRoutine());
    }

    /// <summary>
    /// Chuyển bố cục sang chế độ Đối Kháng 1v1 (trượt P1 sang trái, đưa P2 xuất hiện bên phải).
    /// </summary>
    public void SetVersusLayout(bool instant = false)
    {
        CurrentLayoutMode = PlayfieldLayoutMode.Versus;
        EnsurePlayerTwoExists();
        StopActiveTransition();

        if (instant || _transitionDuration <= 0f)
        {
            ApplyVersusInstant();
            OnLayoutTransitionCompleted?.Invoke(CurrentLayoutMode);
            return;
        }

        OnLayoutTransitionStarted?.Invoke(CurrentLayoutMode);
        _transitionCoroutine = StartCoroutine(TransitionToVersusRoutine());
    }

    private void EnsurePlayerTwoExists()
    {
        if (_playerTwoContainer != null) return;
        if (_playfieldPrefab == null)
        {
            Debug.LogWarning("[LayoutTransitionController] Chưa gán _playfieldPrefab để tạo Player 2.");
            return;
        }

        Transform parent = _containersRoot != null ? _containersRoot : transform;
        _playerTwoContainer = Instantiate(_playfieldPrefab, parent);
        _playerTwoContainer.name = "PlayfieldContainer_P2";
        _playerTwoContainer.Initialize(playerId: 2);
        _playerTwoContainer.gameObject.SetActive(false);
    }

    private void StopActiveTransition()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
    }

    private void ApplySoloInstant()
    {
        if (_playerOneContainer != null)
        {
            Transform p1 = _playerOneContainer.transform;
            p1.localPosition = _soloPosition;
            p1.localScale = _soloScale;
        }

        if (_playerTwoContainer != null)
        {
            _playerTwoContainer.ClearVisuals();
            _playerTwoContainer.gameObject.SetActive(false);
        }
    }

    private void ApplyVersusInstant()
    {
        if (_playerOneContainer != null)
        {
            Transform p1 = _playerOneContainer.transform;
            p1.localPosition = _versusLeftPosition;
            p1.localScale = _versusScale;
        }

        if (_playerTwoContainer != null)
        {
            _playerTwoContainer.gameObject.SetActive(true);
            Transform p2 = _playerTwoContainer.transform;
            p2.localPosition = _versusRightPosition;
            p2.localScale = _versusScale;
        }
    }

    private IEnumerator TransitionToSoloRoutine()
    {
        Transform p1 = _playerOneContainer != null ? _playerOneContainer.transform : null;
        Transform p2 = _playerTwoContainer != null && _playerTwoContainer.gameObject.activeSelf
            ? _playerTwoContainer.transform
            : null;

        Vector3 p1StartPos = p1 != null ? p1.localPosition : _soloPosition;
        Vector3 p1StartScale = p1 != null ? p1.localScale : _soloScale;

        Vector3 p2StartPos = p2 != null ? p2.localPosition : _versusRightPosition;
        Vector3 p2EndPos = _versusRightPosition + _versusRightSpawnOffset;
        Vector3 p2StartScale = p2 != null ? p2.localScale : _versusScale;

        float elapsed = 0f;
        while (elapsed < _transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _transitionDuration);
            float eased = _easeCurve != null ? _easeCurve.Evaluate(t) : Mathf.SmoothStep(0f, 1f, t);

            if (p1 != null)
            {
                p1.localPosition = Vector3.LerpUnclamped(p1StartPos, _soloPosition, eased);
                p1.localScale = Vector3.LerpUnclamped(p1StartScale, _soloScale, eased);
            }

            if (p2 != null)
            {
                p2.localPosition = Vector3.LerpUnclamped(p2StartPos, p2EndPos, eased);
                p2.localScale = Vector3.LerpUnclamped(p2StartScale, Vector3.zero, eased);
            }

            // yield return null không cấp phát bộ nhớ (0 bytes GC)
            yield return null;
        }

        ApplySoloInstant();
        _transitionCoroutine = null;
        OnLayoutTransitionCompleted?.Invoke(CurrentLayoutMode);
    }

    private IEnumerator TransitionToVersusRoutine()
    {
        Transform p1 = _playerOneContainer != null ? _playerOneContainer.transform : null;
        Transform p2 = null;

        if (_playerTwoContainer != null)
        {
            _playerTwoContainer.gameObject.SetActive(true);
            p2 = _playerTwoContainer.transform;
            p2.localPosition = _versusRightPosition + _versusRightSpawnOffset;
            p2.localScale = _versusScale * 0.85f;
        }

        Vector3 p1StartPos = p1 != null ? p1.localPosition : _soloPosition;
        Vector3 p1StartScale = p1 != null ? p1.localScale : _soloScale;

        Vector3 p2StartPos = p2 != null ? p2.localPosition : _versusRightPosition;
        Vector3 p2StartScale = p2 != null ? p2.localScale : _versusScale;

        float elapsed = 0f;
        while (elapsed < _transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _transitionDuration);
            float eased = _easeCurve != null ? _easeCurve.Evaluate(t) : Mathf.SmoothStep(0f, 1f, t);

            if (p1 != null)
            {
                p1.localPosition = Vector3.LerpUnclamped(p1StartPos, _versusLeftPosition, eased);
                p1.localScale = Vector3.LerpUnclamped(p1StartScale, _versusScale, eased);
            }

            if (p2 != null)
            {
                p2.localPosition = Vector3.LerpUnclamped(p2StartPos, _versusRightPosition, eased);
                p2.localScale = Vector3.LerpUnclamped(p2StartScale, _versusScale, eased);
            }

            yield return null;
        }

        ApplyVersusInstant();
        _transitionCoroutine = null;
        OnLayoutTransitionCompleted?.Invoke(CurrentLayoutMode);
    }
}
