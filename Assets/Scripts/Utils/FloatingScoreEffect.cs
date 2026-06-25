using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Điều khiển một floating score text duy nhất: nổi lên, phóng to rồi fade out.
/// Gắn script này vào Canvas gốc (World Space). TMP_Text là child của Canvas.
/// Khi animation kết thúc sẽ gọi callback onComplete để Spawner trả object về pool.
/// </summary>
public class FloatingScoreEffect : MonoBehaviour
{
    // ─── Inspector config ───────────────────────────────────────────────────────
    [Header("Animation Settings")]
    [Tooltip("Tổng thời gian hiệu ứng (giây).")]
    [SerializeField] private float _lifetime = 1.2f;

    [Tooltip("Khoảng cách di chuyển lên phía trên (đơn vị thế giới).")]
    [SerializeField] private float _riseDistance = 1.5f;

    [Tooltip("Scale bình thường khi hiển thị (1.0 = đúng kích thước Prefab).")]
    [SerializeField] private float _normalScale = 1f;

    [Tooltip("Tỉ lệ scale lớn nhất trong pha zoom-in ban đầu (so với _normalScale).")]
    [SerializeField] private float _peakScale = 1.3f;

    [Tooltip("Tỉ lệ phần đầu của lifetime dành cho pha zoom-in.")]
    [Range(0.1f, 0.5f)]
    [SerializeField] private float _zoomInRatio = 0.2f;

    // ─── Private state ───────────────────────────────────────────────────────────
    private TMP_Text _label;
    private Coroutine _animationCoroutine;
    private Action<FloatingScoreEffect> _onComplete;

    // ─── Unity callbacks ─────────────────────────────────────────────────────────
    private void Awake()
    {
        _label = GetComponentInChildren<TMP_Text>();
        if (_label == null)
        {
            Debug.LogError($"[FloatingScoreEffect] Không tìm thấy TMP_Text trong '{gameObject.name}'.");
        }
    }

    // ─── Public API ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Khởi tạo và phát hiệu ứng.
    /// </summary>
    /// <param name="points">Số điểm cần hiển thị (ví dụ: 1200).</param>
    /// <param name="worldPosition">Vị trí thế giới tại trung tâm dòng bị xóa.</param>
    /// <param name="onComplete">Callback khi animation kết thúc — dùng để trả về pool.</param>
    public void Play(int points, Vector3 worldPosition, Action<FloatingScoreEffect> onComplete = null)
    {
        // Dừng animation cũ nếu đang chạy (trường hợp tái sử dụng pool)
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }

        _onComplete = onComplete;
        transform.position = worldPosition;

        if (_label != null)
        {
            _label.text = $"+{points}";
        }

        _animationCoroutine = StartCoroutine(AnimateRoutine());
    }

    // ─── Private helpers ─────────────────────────────────────────────────────────

    private IEnumerator AnimateRoutine()
    {
        float elapsed = 0f;
        float zoomInDuration = _lifetime * _zoomInRatio;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * _riseDistance;

        // Scale = 0 lúc bắt đầu để tạo hiệu ứng xuất hiện
        transform.localScale = Vector3.zero;
        SetAlpha(1f);

        while (elapsed < _lifetime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / _lifetime;

            // --- Di chuyển lên (Ease Out) ---
            transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));

            // --- Scale animation: 0 → peak → normalScale ---
            float scaleValue;
            float zoomT = elapsed / zoomInDuration;
            if (zoomT < 1f)
            {
                // Pha zoom-in: 0 → _peakScale
                scaleValue = Mathf.Lerp(0f, _peakScale, Mathf.SmoothStep(0f, 1f, zoomT));
            }
            else
            {
                // Pha zoom-out: _peakScale → _normalScale
                float normalizeT = (t - _zoomInRatio) / (1f - _zoomInRatio);
                scaleValue = Mathf.Lerp(_peakScale, _normalScale, Mathf.SmoothStep(0f, 1f, normalizeT));
            }
            transform.localScale = Vector3.one * scaleValue;

            // --- Fade out ở nửa sau ---
            float fadeAlpha = t < 0.5f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
            SetAlpha(fadeAlpha);

            yield return null;
        }

        // Kết thúc: gọi callback để Spawner xử lý (trả về pool)
        _animationCoroutine = null;
        _onComplete?.Invoke(this);
    }

    private void SetAlpha(float alpha)
    {
        if (_label == null) return;
        Color c = _label.color;
        c.a = alpha;
        _label.color = c;
    }
}
