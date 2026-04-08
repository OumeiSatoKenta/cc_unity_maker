using UnityEngine;

namespace Game093v2_ColorPerception
{
    public class ColorPerceptionGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] ColorPuzzleManager _puzzleManager;
        [SerializeField] ColorPerceptionUI _ui;

        int _combo;
        float _scoreMultiplier;
        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;

        public bool IsPlaying => _isPlaying;
        public int GetCurrentScore() => _totalScore;

        void Start()
        {
            if (_instructionPanel == null || _stageManager == null || _puzzleManager == null || _ui == null)
            {
                Debug.LogError("[ColorPerceptionGameManager] Required fields not assigned.");
                enabled = false;
                return;
            }
            _instructionPanel.Show(
                "093",
                "ColorPerception",
                "視点を切り替えて隠れた道を見つけよう",
                "ボタンで視点切替、上下左右で移動",
                "色の見え方を変えてゴールまで辿り着こう"
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

        /// <summary>Called by ColorPuzzleManager when player reaches the goal.</summary>
        public void OnGoalReached(int movesUsed, int movesLimit, int viewSwitchCount)
        {
            if (!_isPlaying) return;

            int baseScore = 100 * (_currentStageIndex + 1);
            int movesBonus = (movesLimit > 0) ? Mathf.Max(0, movesLimit - movesUsed) * 15 : 0;
            int viewBonus = (viewSwitchCount <= 3) ? 50 : 0;
            _combo++;
            _scoreMultiplier = _combo >= 3 ? 1.5f : _combo >= 2 ? 1.2f : 1.0f;
            int earned = Mathf.RoundToInt((baseScore + movesBonus + viewBonus) * _scoreMultiplier);
            _totalScore += earned;

            _isPlaying = false;
            _puzzleManager.SetActive(false);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.ShowStageClear(_currentStageIndex + 1, _totalScore);
        }

        /// <summary>Called by ColorPuzzleManager when moves exceed limit.</summary>
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
