using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Container đóng gói toàn bộ thành phần của 1 bàn chơi độc lập (1P hoặc mỗi người chơi trong 2P Versus).
/// Điều phối kết nối giữa Board, Ghost, ScoreManager, SkillController và FloatingScoreSpawner (DIP & Zero-GC).
/// </summary>
public class PlayfieldContainer : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private int _playerId = 1;

    [Header("Settings (ScriptableObjects)")]
    [SerializeField] private TetrisGameSettingsSO _gameSettings;
    [SerializeField] private TetrisScoringSettingsSO _scoringSettings;

    [Header("Core Components")]
    [SerializeField] private Board _board;
    [SerializeField] private Ghost _ghost;
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private SkillController _skillController;
    [SerializeField] private PlayerInputController _playerInputController;
    [SerializeField] private FloatingScoreSpawner _floatingScoreSpawner;
    [SerializeField] private BoardIgnitionVFXController _ignitionVfxController;

    [Header("Slot Tilemaps")]
    [SerializeField] private Tilemap _nextPieceTilemap;
    [SerializeField] private Tilemap _holdPieceTilemap;
    [SerializeField] private int _nextQueuePreviewCount = 4;

    private IGameModeRule _modeRule;
    private bool _isInitialized;
    private bool _isEventsSubscribed;
    private bool _wasLastClearBackToBack;

    public int PlayerId => _playerId;
    public int CurrentLevel { get; private set; } = 1;
    public int LinesCleared { get; private set; }
    public bool IsDefeated { get; private set; }
    public bool IsGoalCompleted { get; private set; }

    public Board Board => _board;
    public Ghost Ghost => _ghost;
    public IScoreService ScoreService => _scoreManager;
    public SkillController SkillController => _skillController;
    public BoardIgnitionVFXController IgnitionVfxController => _ignitionVfxController;

    public event Action<PlayfieldContainer, int> OnLevelChanged;
    public event Action<PlayfieldContainer, int> OnLinesTotalChanged;
    public event Action<PlayfieldContainer> OnPlayerDefeated;
    public event Action<PlayfieldContainer> OnGoalCompleted;

    private void Awake()
    {
        ResolveLocalComponents();
    }

    private void OnDestroy()
    {
        UnsubscribeInternalEvents();
    }

    /// <summary>
    /// Khởi tạo toàn bộ kết nối nội bộ cho PlayfieldContainer (có thể gọi từ LayoutTransitionController hoặc GameManager).
    /// </summary>
    public void Initialize(int playerId, TetrisGameSettingsSO gameSettings = null, TetrisScoringSettingsSO scoringSettings = null)
    {
        _playerId = playerId;

        if (gameSettings != null) _gameSettings = gameSettings;
        if (scoringSettings != null) _scoringSettings = scoringSettings;

        ResolveLocalComponents();

        if (_scoreManager != null && _scoringSettings != null)
        {
            _scoreManager.Initialize(_scoringSettings);
        }

        if (_board != null)
        {
            _board.Initialize(_gameSettings, _nextPieceTilemap, _holdPieceTilemap, _nextQueuePreviewCount);

            if (_ghost != null)
            {
                _ghost.Initialize(_board, _board.ActivePiece);
            }

            if (_floatingScoreSpawner != null)
            {
                _floatingScoreSpawner.Initialize(_board, _board.Tilemap, _scoreManager);
            }
        }

        if (_playerInputController != null)
        {
            if (_gameSettings != null) _playerInputController.InitializeSettings(_gameSettings);
            if (_skillController != null) _playerInputController.InitializeSkillController(_skillController);
        }

        if (_ignitionVfxController != null)
        {
            _ignitionVfxController.Initialize(_scoreManager, _skillController);
        }

        SubscribeInternalEvents();
        _isInitialized = true;
    }

    public void StartMatch(IGameModeRule modeRule)
    {
        if (!_isInitialized)
        {
            Initialize(_playerId);
        }

        _modeRule = modeRule;
        CurrentLevel = 1;
        LinesCleared = 0;
        IsDefeated = false;
        IsGoalCompleted = false;
        _wasLastClearBackToBack = false;

        _modeRule?.OnGameStarted();

        if (_scoreManager != null)
        {
            _scoreManager.ResetScore();
        }

        if (_skillController != null)
        {
            _skillController.ResetEnergy();
        }

        if (_ghost != null)
        {
            _ghost.ClearBlastPreview();
        }

        if (_ignitionVfxController != null)
        {
            _ignitionVfxController.ResetImmediate();
        }

        OnLevelChanged?.Invoke(this, CurrentLevel);
        OnLinesTotalChanged?.Invoke(this, LinesCleared);

        if (_board != null)
        {
            _board.ResetBoard();
            _board.ApplyLevelSpeed(CurrentLevel);
        }
    }

    public void SetPaused(bool isPaused)
    {
        if (_board != null)
        {
            _board.SetSimulationPaused(isPaused);
        }
    }

    public void StopMatch(bool saveHighScore = false)
    {
        if (saveHighScore && _scoreManager != null)
        {
            _scoreManager.SaveCurrentScore();
        }

        if (_skillController != null)
        {
            _skillController.CompleteActiveSkill();
        }

        if (_board != null)
        {
            _board.StopBoard();
        }

        if (_ghost != null)
        {
            _ghost.ClearBlastPreview();
            _ghost.ClearGhostTilemap();
        }

        if (_ignitionVfxController != null)
        {
            _ignitionVfxController.ResetImmediate();
        }
    }

    public void ClearVisuals()
    {
        if (_board != null) _board.ClearAll();
        if (_ghost != null)
        {
            _ghost.ClearBlastPreview();
            _ghost.ClearGhostTilemap();
        }
        if (_ignitionVfxController != null)
        {
            _ignitionVfxController.ResetImmediate();
        }
    }

    private void ResolveLocalComponents()
    {
        if (_board == null) _board = GetComponentInChildren<Board>(true);
        if (_ghost == null) _ghost = GetComponentInChildren<Ghost>(true);
        if (_floatingScoreSpawner == null) _floatingScoreSpawner = GetComponentInChildren<FloatingScoreSpawner>(true);
        if (_playerInputController == null) _playerInputController = GetComponentInChildren<PlayerInputController>(true);
        if (_ignitionVfxController == null) _ignitionVfxController = GetComponentInChildren<BoardIgnitionVFXController>(true);

        if (_scoreManager == null)
        {
            _scoreManager = GetComponent<ScoreManager>();
            if (_scoreManager == null)
            {
                _scoreManager = gameObject.AddComponent<ScoreManager>();
            }
        }

        if (_skillController == null)
        {
            _skillController = GetComponent<SkillController>();
        }
    }

    private void SubscribeInternalEvents()
    {
        if (_isEventsSubscribed) return;

        if (_board != null)
        {
            _board.OnLinesCleared += HandleBoardLinesCleared;
            _board.OnTopOut += HandleBoardTopOut;
            _board.OnBoardCleared += HandleBoardCleared;
            _board.OnActiveSkillExecuted += HandleBoardActiveSkillExecuted;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnBackToBack += HandleScoreBackToBack;
        }

        if (_skillController != null)
        {
            _skillController.OnSkillActivated += HandleSkillActivated;
            _skillController.OnSkillEnded += HandleSkillEnded;
        }

        _isEventsSubscribed = true;
    }

    private void UnsubscribeInternalEvents()
    {
        if (!_isEventsSubscribed) return;

        if (_board != null)
        {
            _board.OnLinesCleared -= HandleBoardLinesCleared;
            _board.OnTopOut -= HandleBoardTopOut;
            _board.OnBoardCleared -= HandleBoardCleared;
            _board.OnActiveSkillExecuted -= HandleBoardActiveSkillExecuted;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnBackToBack -= HandleScoreBackToBack;
        }

        if (_skillController != null)
        {
            _skillController.OnSkillActivated -= HandleSkillActivated;
            _skillController.OnSkillEnded -= HandleSkillEnded;
        }

        _isEventsSubscribed = false;
    }

    private void HandleScoreBackToBack()
    {
        _wasLastClearBackToBack = true;
    }

    private void HandleSkillActivated(SkillDefinitionSO definition, ISkillEffect effect)
    {
        if (definition == null || effect == null) return;

        if (_ghost != null)
        {
            _ghost.SetActiveSkillPreview(effect, definition.BlastPreviewTile);
        }

        if (_board != null)
        {
            _board.SetActiveSkillModifier(effect, definition.SpecialBlockTile);
        }
    }

    private void HandleSkillEnded(SkillDefinitionSO definition)
    {
        if (_board != null)
        {
            _board.ClearActiveSkillModifier();
        }

        if (_ghost != null)
        {
            _ghost.ClearBlastPreview();
        }
    }

    private void HandleBoardActiveSkillExecuted()
    {
        if (_skillController != null)
        {
            _skillController.CompleteActiveSkill();
        }
    }

    private void HandleBoardLinesCleared(int lines, List<int> clearedRows)
    {
        if (IsDefeated || IsGoalCompleted) return;

        _wasLastClearBackToBack = false;

        if (_scoreManager != null)
        {
            _scoreManager.AddLineClearScore(lines, CurrentLevel, clearedRows);
        }

        if (lines <= 0) return;

        if (_skillController != null)
        {
            int currentCombo = _scoreManager != null ? _scoreManager.ComboCount : 0;
            _skillController.AddEnergyFromLineClear(lines, currentCombo, _wasLastClearBackToBack);
        }

        LinesCleared += lines;
        OnLinesTotalChanged?.Invoke(this, LinesCleared);

        if (_modeRule != null)
        {
            _modeRule.OnLinesCleared(LinesCleared, lines, out bool shouldLevelUp, out bool isGoalCompleted);

            if (shouldLevelUp)
            {
                CurrentLevel++;
                if (_board != null)
                {
                    _board.ApplyLevelSpeed(CurrentLevel);
                }
                OnLevelChanged?.Invoke(this, CurrentLevel);
            }

            if (isGoalCompleted)
            {
                IsGoalCompleted = true;
                StopMatch(saveHighScore: _playerId == 1);
                OnGoalCompleted?.Invoke(this);
            }
        }
    }

    private void HandleBoardTopOut()
    {
        if (IsDefeated) return;

        IsDefeated = true;
        StopMatch(saveHighScore: _playerId == 1);
        OnPlayerDefeated?.Invoke(this);
    }

    private void HandleBoardCleared()
    {
        if (_ghost != null)
        {
            _ghost.ClearBlastPreview();
            _ghost.ClearGhostTilemap();
        }
    }
}
