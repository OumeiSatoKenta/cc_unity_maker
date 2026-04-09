using UnityEngine;

namespace Game092v2_MirrorWorld
{
    public class MirrorWorldGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] MirrorPuzzleManager _puzzleManager;
        [SerializeField] MirrorWorldUI _ui;

        int _combo;
        float _scoreMultiplier;
        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;

        public bool IsPlaying => _isPlaying;
        public int GetCurrentScore() => _totalScore;

        void Start()
        {
            _instructionPanel.Show(
                "092",
                "MirrorWorld",
                "鏡の世界で2人同時にゴールさせよう",
                "スワイプで移動（上下キャラが鏡像で連動）",
                "両方のキャラをゴールに導こう"
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
            _isPlaying = true;

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _isPlaying = true;

            var config = _stageManager.GetCurrentStageConfig();
            _puzzleManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _puzzleManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        /// <summary>Called by MirrorPuzzleManager when both characters reach their goals.</summary>
        public void OnBothReachedGoal(int movesUsed, int movesLimit, int bounceCount)
        {
            if (!_isPlaying) return;

            int baseScore = 100 * (_currentStageIndex + 1);
            int movesBonus = movesLimit > 0 ? Mathf.Max(0, movesLimit - movesUsed) * 10 : 20;
            int bounceBonus = bounceCount * 20;
            _combo++;
            _scoreMultiplier = _combo >= 3 ? 1.5f : _combo >= 2 ? 1.2f : 1.0f;
            int earned = Mathf.RoundToInt((baseScore + movesBonus + bounceBonus) * _scoreMultiplier);
            _totalScore += earned;

            _isPlaying = false;
            _puzzleManager.SetActive(false);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.ShowStageClear(_currentStageIndex + 1, _totalScore);
        }

        /// <summary>Called by MirrorPuzzleManager when a character hits a trap.</summary>
        public void OnTrapHit()
        {
            if (!_isPlaying) return;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _isPlaying = false;
            _puzzleManager.SetActive(false);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.ShowGameOver(_totalScore);
        }

        /// <summary>Called by MirrorPuzzleManager when moves exceed limit.</summary>
        public void OnMovesExceeded()
        {
            if (!_isPlaying) return;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _isPlaying = false;
            _puzzleManager.SetActive(false);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
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
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _totalScore = 0;
            _currentStageIndex = 0;
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
