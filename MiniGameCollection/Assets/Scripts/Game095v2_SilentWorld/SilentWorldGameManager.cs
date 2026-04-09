using UnityEngine;

namespace Game095v2_SilentWorld
{
    public class SilentWorldGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] WorldManager _worldManager;
        [SerializeField] SilentWorldUI _ui;

        int _combo;
        float _scoreMultiplier;
        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;
        int _lives;
        int _hintsUsedThisStage;

        const int MaxLives = 3;

        public bool IsPlaying => _isPlaying;
        public int GetCurrentScore() => _totalScore;
        public int GetLives() => _lives;
        public int GetHintsUsed() => _hintsUsedThisStage;

        void Start()
        {
            if (_stageManager == null || _instructionPanel == null || _worldManager == null || _ui == null)
            {
                Debug.LogError("[SilentWorldGameManager] Required fields not assigned.");
                enabled = false;
                return;
            }
            _instructionPanel.Show(
                "095",
                "SilentWorld",
                "視覚だけを頼りに無音の世界を進もう",
                "タップで移動、長押しで周囲の手がかりを強調表示",
                "音符を集めて出口を開き、すべてのステージをクリアしよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _totalScore = 0;
            _currentStageIndex = 0;
            _lives = MaxLives;
            _hintsUsedThisStage = 0;
            _isPlaying = true;

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _hintsUsedThisStage = 0;
            _isPlaying = true;
            var config = _stageManager.GetCurrentStageConfig();
            _worldManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateLives(_lives, MaxLives);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.UpdateHints(0, GetMaxHintsForStage(config));
        }

        int GetMaxHintsForStage(StageManager.StageConfig config)
        {
            // Higher complexityFactor = fewer hints allowed
            if (config.complexityFactor >= 0.8f) return 1;
            if (config.complexityFactor >= 0.4f) return 2;
            return 3;
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _worldManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        /// <summary>Called by WorldManager when player collects an item.</summary>
        public void OnItemCollected(bool isComboItem)
        {
            if (!_isPlaying) return;

            _combo++;
            _scoreMultiplier = _combo >= 5 ? 2.0f : _combo >= 3 ? 1.5f : 1.0f;
            int baseScore = 50 * (_currentStageIndex + 1);
            int earned = Mathf.RoundToInt(baseScore * _scoreMultiplier);
            _totalScore += earned;

            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
        }

        /// <summary>Called by WorldManager when player reaches the exit.</summary>
        public void OnStageClear(bool noHintsUsed, bool noDamage)
        {
            if (!_isPlaying) return;

            int bonus = 0;
            if (noHintsUsed) bonus += 100;
            if (noDamage) bonus += 100;
            _totalScore += bonus;

            _isPlaying = false;
            _worldManager.SetActive(false);
            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClear(_currentStageIndex + 1, _totalScore);
        }

        /// <summary>Called by WorldManager when player hits a trap.</summary>
        public void OnTrapHit()
        {
            if (!_isPlaying) return;

            _combo = 0;
            _scoreMultiplier = 1.0f;
            _lives--;
            _ui.UpdateLives(_lives, MaxLives);
            _ui.UpdateCombo(_combo, _scoreMultiplier);

            if (_lives <= 0)
            {
                _isPlaying = false;
                _worldManager.SetActive(false);
                _ui.ShowGameOver(_totalScore);
            }
        }

        /// <summary>Called by WorldManager when observe action is used.</summary>
        public bool TryUseHint(StageManager.StageConfig config)
        {
            int maxHints = GetMaxHintsForStage(config);
            if (_hintsUsedThisStage >= maxHints) return false;
            _hintsUsedThisStage++;
            _ui.UpdateHints(_hintsUsedThisStage, maxHints);
            return true;
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void RestartGame()
        {
            _ui.HideGameOver();
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _totalScore = 0;
            _currentStageIndex = 0;
            _lives = MaxLives;
            _hintsUsedThisStage = 0;
            _isPlaying = true;
            _stageManager.StartFromBeginning();
        }

        void OnDestroy()
        {
            if (_instructionPanel != null) _instructionPanel.OnDismissed -= StartGame;
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
        }
    }
}
