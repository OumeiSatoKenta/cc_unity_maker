using UnityEngine;

namespace Game096v2_DualControl
{
    public class DualControlGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] ControlManager _controlManager;
        [SerializeField] DualControlUI _ui;

        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;
        bool _noDamageThisStage;

        public bool IsPlaying => _isPlaying;
        public int GetCurrentScore() => _totalScore;

        void Start()
        {
            if (_stageManager == null || _instructionPanel == null || _controlManager == null || _ui == null)
            {
                Debug.LogError("[DualControlGameManager] Required fields not assigned.");
                enabled = false;
                return;
            }
            _instructionPanel.Show(
                "096",
                "DualControl",
                "左右の親指で2キャラ同時操作！",
                "左ドラッグで左キャラ、右ドラッグで右キャラを操作",
                "2人同時にゴールさせよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _totalScore = 0;
            _currentStageIndex = 0;
            _isPlaying = true;
            _noDamageThisStage = true;

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _isPlaying = true;
            _noDamageThisStage = true;
            var config = _stageManager.GetCurrentStageConfig();
            _controlManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
            _ui.UpdateScore(_totalScore);
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _controlManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        /// <summary>Called by ControlManager when both characters reach the goal.</summary>
        public void OnStageClear(bool isSynchro)
        {
            if (!_isPlaying) return;

            var config = _stageManager.GetCurrentStageConfig();
            int baseScore = Mathf.RoundToInt(100f * (_currentStageIndex + 1) * config.speedMultiplier);
            if (isSynchro) baseScore = Mathf.RoundToInt(baseScore * 2.0f);
            if (_noDamageThisStage) baseScore += 50;
            _totalScore += baseScore;

            _isPlaying = false;
            _controlManager.SetActive(false);
            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClear(_currentStageIndex + 1, _totalScore, isSynchro);
        }

        /// <summary>Called by ControlManager when a character hits a trap.</summary>
        public void OnTrapHit()
        {
            if (!_isPlaying) return;

            _noDamageThisStage = false;
            _isPlaying = false;
            _controlManager.SetActive(false);
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
            _noDamageThisStage = true;
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
