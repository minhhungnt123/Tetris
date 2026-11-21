using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ScriptableObject định nghĩa dữ liệu cấu hình cho từng kỹ năng (Data-Driven Design).
/// Hỗ trợ tùy chỉnh màu sắc, icon, gạch đặc biệt, màu ngọn lửa (Board Ignition VFX) và hệ số điểm.
/// </summary>
[CreateAssetMenu(fileName = "NewSkillDefinition", menuName = "Tetris/Skill Definition")]
public class SkillDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _skillId = "row_detonator";
    [SerializeField] private string _displayName = "Row Detonator";
    [TextArea(2, 4)]
    [SerializeField] private string _description = "Phát nổ toàn bộ các hàng ngang mà khối gạch chạm vào mà không cần lấp đầy.";
    [SerializeField] private SkillType _skillType = SkillType.RowDetonator;

    [Header("Visuals & Theme (Cartoon Fantasy Arcade)")]
    [SerializeField] private Sprite _skillIcon;
    [SerializeField] private Color _themeColor = new Color(1f, 0.45f, 0.1f, 1f);
    [SerializeField] private Color _flameCoreColor = new Color(1f, 0.965f, 0.702f, 1f);   // #FFF6B3
    [SerializeField] private Color _flameMidColor = new Color(1f, 0.69f, 0.125f, 1f);     // #FFB020
    [SerializeField] private Color _flameEdgeColor = new Color(1f, 0.27f, 0.094f, 1f);    // #FF4518

    [Header("Tile Overrides")]
    [Tooltip("Tile dùng để thay thế diện mạo khối gạch đang rơi khi kích hoạt Skill.")]
    [SerializeField] private TileBase _specialBlockTile;
    [Tooltip("Tile dùng để tô sáng vùng sắp nổ (Blast Zone Preview) trên Ghost.")]
    [SerializeField] private TileBase _blastPreviewTile;

    [Header("Gameplay & Balance")]
    [SerializeField] private float _energyCost = 100f;
    [SerializeField, Range(0.25f, 1.5f)] private float _scoreMultiplier = 0.75f;
    [SerializeField] private int _blastRadius = 1;
    [SerializeField] private float _blastFlashDuration = 0.24f;
    [SerializeField] private int _blastFlashCount = 2;

    // Public Properties (PascalCase)
    public string SkillId => _skillId;
    public string DisplayName => _displayName;
    public string Description => _description;
    public SkillType SkillType => _skillType;
    public Sprite SkillIcon => _skillIcon;
    public Color ThemeColor => _themeColor;
    public Color FlameCoreColor => _flameCoreColor;
    public Color FlameMidColor => _flameMidColor;
    public Color FlameEdgeColor => _flameEdgeColor;
    public TileBase SpecialBlockTile => _specialBlockTile;
    public TileBase BlastPreviewTile => _blastPreviewTile;
    public float EnergyCost => _energyCost;
    public float ScoreMultiplier => _scoreMultiplier;
    public int BlastRadius => _blastRadius;
    public float BlastFlashDuration => _blastFlashDuration;
    public int BlastFlashCount => _blastFlashCount;

    /// <summary>
    /// Factory tạo instance ISkillEffect tương ứng (có thể tái sử dụng để tránh cấp phát lặp lại).
    /// </summary>
    public ISkillEffect CreateEffectInstance()
    {
        switch (_skillType)
        {
            case SkillType.NovaBomb:
                return new NovaBombSkill(_blastRadius, _blastFlashDuration, _blastFlashCount);
            case SkillType.RowDetonator:
            default:
                return new RowDetonatorSkill(_blastFlashDuration, _blastFlashCount);
        }
    }
}
