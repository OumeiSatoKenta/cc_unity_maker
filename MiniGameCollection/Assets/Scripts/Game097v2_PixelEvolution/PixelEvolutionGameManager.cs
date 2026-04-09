using UnityEngine;

namespace Game097v2_PixelEvolution
{
    public class PixelEvolutionGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] EvolutionManager _evolutionManager;
        [SerializeField] PixelEvolutionUI _ui;

        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;
        int _consecutiveOptimalCount;
        bool _hiddenBranchFound;

        public bool IsPlaying => _isPlaying;
        public int GetCurrentScore() => _totalScore;

        void Start()
        {
            if (_stageManager == null || _instructionPanel == null || _evolutionManager == null || _ui == null)
            {
                Debug.LogError("[PixelEvolutionGameManager] Required fields not assigned.");
                enabled = false;
                return;
            }
            _instructionPanel.Show(
                "097",
                "PixelEvolution",
                "ピクセル生命体を進化させよう",
                "ボタンで環境変更・世代交代、タップで進化方向を選択",
                "世代制限内に目標の最終形態まで進化させよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _totalScore = 0;
            _currentStageIndex = 0;
            _isPlaying = true;
            _consecutiveOptimalCount = 0;
            _hiddenBranchFound = false;

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _isPlaying = true;
            _consecutiveOptimalCount = 0;
            _hiddenBranchFound = false;
            var config = _stageManager.GetCurrentStageConfig();
            _evolutionManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
            _ui.UpdateScore(_totalScore);
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _evolutionManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        /// <summary>Called by EvolutionManager when evolution advances.</summary>
        public void OnEvolutionAdvanced(bool isOptimal, bool isHiddenBranch)
        {
            if (!_isPlaying) return;

            var config = _stageManager.GetCurrentStageConfig();
            int baseScore = Mathf.RoundToInt(50f * (_currentStageIndex + 1) * config.speedMultiplier);

            if (isOptimal)
            {
                _consecutiveOptimalCount++;
                if (_consecutiveOptimalCount >= 3)
                    baseScore = Mathf.RoundToInt(baseScore * 1.5f);
            }
            else
            {
                _consecutiveOptimalCount = 0;
            }

            if (isHiddenBranch && !_hiddenBranchFound)
            {
                _hiddenBranchFound = true;
                baseScore += 200;
            }

            _totalScore += baseScore;
            _ui.UpdateScore(_totalScore);
            _ui.ShowComboIfNeeded(_consecutiveOptimalCount);
        }

        /// <summary>Called by EvolutionManager when evolution level reaches 5 (final form).</summary>
        public void OnEvolutionComplete(int generationsUsed, int generationLimit)
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _evolutionManager.SetActive(false);

            // Bonus for speed clear
            if (generationsUsed <= generationLimit / 2)
                _totalScore *= 2;

            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClear(_currentStageIndex + 1, _totalScore);
        }

        /// <summary>Called by EvolutionManager when generation limit is reached without clearing.</summary>
        public void OnGenerationLimitReached()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _evolutionManager.SetActive(false);
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
            _consecutiveOptimalCount = 0;
            _hiddenBranchFound = false;
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
