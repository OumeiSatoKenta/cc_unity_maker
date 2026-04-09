using UnityEngine;

namespace Game099v2_TouchMemory
{
    public class TouchMemoryGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] TouchMemoryManager _touchMemoryManager;
        [SerializeField] TouchMemoryUI _ui;

        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;
        int _comboCount;

        public bool IsPlaying => _isPlaying;
        public int GetTotalScore() => _totalScore;

        void Start()
        {
            if (_stageManager == null || _instructionPanel == null || _touchMemoryManager == null || _ui == null)
            {
                Debug.LogError("[TouchMemoryGameManager] Required fields not assigned.");
                enabled = false;
                return;
            }
            _instructionPanel.Show(
                "099",
                "TouchMemory",
                "光るパターンを記憶して再現しよう",
                "光った順番にパネルをタップ",
                "できるだけ多くのラウンドをクリアしよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _totalScore = 0;
            _currentStageIndex = 0;
            _isPlaying = true;
            _comboCount = 0;

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _isPlaying = true;
            _comboCount = 0;
            var config = _stageManager.GetCurrentStageConfig();
            _touchMemoryManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateRound(1);
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _touchMemoryManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        /// <summary>Called by TouchMemoryManager when a round is completed successfully.</summary>
        public void OnRoundCleared(int round, int patternLength, float inputTime)
        {
            if (!_isPlaying) return;

            _comboCount++;
            float comboMultiplier = Mathf.Min(1.0f + _comboCount * 0.1f, 2.0f);
            int baseScore = 100 * (_currentStageIndex + 1) * patternLength;
            int roundScore = Mathf.RoundToInt(baseScore * comboMultiplier);

            // Instant-answer bonus (within 2 seconds)
            if (inputTime <= 2.0f)
                roundScore = Mathf.RoundToInt(roundScore * 1.5f);

            _totalScore += roundScore;
            _ui.UpdateScore(_totalScore);
            _ui.ShowComboIfNeeded(_comboCount);
        }

        /// <summary>Called by TouchMemoryManager when a stage's all rounds are cleared.</summary>
        public void OnStageClear(int stage)
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _touchMemoryManager.SetActive(false);
            _ui.ShowStageClear(stage, _totalScore);
        }

        /// <summary>Called by TouchMemoryManager when player misses.</summary>
        public void OnMissed()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _comboCount = 0;
            _touchMemoryManager.SetActive(false);
            _ui.ShowGameOver(_totalScore);
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void RestartGame()
        {
            _ui.HideGameOver();
            _totalScore = 0;
            _currentStageIndex = 0;
            _isPlaying = true;
            _comboCount = 0;
            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
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
