using UnityEngine;

namespace Game098v2_InfiniteLoop
{
    public class InfiniteLoopGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] LoopManager _loopManager;
        [SerializeField] InfiniteLoopUI _ui;

        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;
        int _consecutiveDiscoveries;
        bool _memoUsed;

        public bool IsPlaying => _isPlaying;
        public int GetTotalScore() => _totalScore;

        void Start()
        {
            if (_stageManager == null || _instructionPanel == null || _loopManager == null || _ui == null)
            {
                Debug.LogError("[InfiniteLoopGameManager] Required fields not assigned.");
                enabled = false;
                return;
            }
            _instructionPanel.Show(
                "098",
                "InfiniteLoop",
                "ループする世界の法則を見つけ出そう",
                "タップでオブジェクト調査・アクション実行、ボタンでメモ確認",
                "ループの法則を発見して世界から脱出しよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _totalScore = 0;
            _currentStageIndex = 0;
            _isPlaying = true;
            _consecutiveDiscoveries = 0;
            _memoUsed = false;

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _isPlaying = true;
            _consecutiveDiscoveries = 0;
            _memoUsed = false;
            var config = _stageManager.GetCurrentStageConfig();
            _loopManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
            _ui.UpdateScore(_totalScore);
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _loopManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        /// <summary>Called by LoopManager when a change element is discovered.</summary>
        public void OnChangeDiscovered(bool isReal)
        {
            if (!_isPlaying) return;

            int baseScore = 50 * (_currentStageIndex + 1);
            if (isReal)
            {
                _consecutiveDiscoveries++;
                if (_consecutiveDiscoveries >= 2)
                    baseScore = Mathf.RoundToInt(baseScore * 1.3f);
            }
            else
            {
                _consecutiveDiscoveries = 0;
            }

            _totalScore += baseScore;
            _ui.UpdateScore(_totalScore);
            _ui.ShowComboIfNeeded(_consecutiveDiscoveries);
        }

        /// <summary>Called by LoopManager when memo is opened.</summary>
        public void OnMemoUsed()
        {
            _memoUsed = true;
        }

        /// <summary>Called by LoopManager when stage is successfully escaped.</summary>
        public void OnEscapeSuccess(int loopsUsed, int loopLimit)
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _loopManager.SetActive(false);

            // Early clear bonus
            int remaining = loopLimit - loopsUsed;
            _totalScore += remaining * 20;

            // Speed clear bonus
            if (remaining >= 5)
                _totalScore *= 2;

            // Memo-free bonus
            if (!_memoUsed)
                _totalScore += 150;

            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClear(_currentStageIndex + 1, _totalScore);
        }

        /// <summary>Called by LoopManager when loop limit exceeded.</summary>
        public void OnLoopLimitReached()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _loopManager.SetActive(false);
            _ui.ShowGameOver(_totalScore);
        }

        /// <summary>Called by LoopManager when escape attempt fails (wrong law).</summary>
        public void OnEscapeFailed()
        {
            if (!_isPlaying) return;
            _consecutiveDiscoveries = 0;
            _ui.ShowEscapeFailed();
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
            _consecutiveDiscoveries = 0;
            _memoUsed = false;
            // Re-subscribe to avoid missed events if previously unsubscribed
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
