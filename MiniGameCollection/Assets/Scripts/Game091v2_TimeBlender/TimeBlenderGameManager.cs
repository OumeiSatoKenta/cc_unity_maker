using UnityEngine;

namespace Game091v2_TimeBlender
{
    public class TimeBlenderGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] PuzzleManager _puzzleManager;
        [SerializeField] TimeBlenderUI _ui;

        int _combo;
        float _scoreMultiplier;
        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;
        bool _eraSwitchedThisStage;

        public bool IsPlaying => _isPlaying;
        public int GetCurrentScore() => _totalScore;
        public int GetCurrentStageIndex() => _currentStageIndex;

        void Start()
        {
            _instructionPanel.Show(
                "091",
                "TimeBlender",
                "過去と未来を切り替えて謎を解こう",
                "ボタンで時代切替、タップで移動",
                "時代の変化を利用してゴールに到達しよう"
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
            _eraSwitchedThisStage = false;

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _isPlaying = true;
            _eraSwitchedThisStage = false;

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

        /// <summary>Called by PuzzleManager when player successfully moves.</summary>
        public void OnPlayerMoved(bool paradoxFree)
        {
            if (!_isPlaying) return;

            if (paradoxFree)
            {
                _combo++;
                _scoreMultiplier = _combo >= 5 ? 2.0f
                                 : _combo >= 3 ? 1.5f
                                 : 1.0f;
            }
            else
            {
                _combo = 0;
                _scoreMultiplier = 1.0f;
            }

            _ui.UpdateCombo(_combo, _scoreMultiplier);
        }

        /// <summary>Called by PuzzleManager when era is switched.</summary>
        public void OnEraSwitched()
        {
            if (!_isPlaying) return;
            _eraSwitchedThisStage = true;
        }

        /// <summary>Called by PuzzleManager when a paradox occurs.</summary>
        public void OnParadoxOccurred(int remainingParadoxCount)
        {
            if (!_isPlaying) return;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.ShowParadox(remainingParadoxCount);

            if (remainingParadoxCount <= 0)
            {
                _isPlaying = false;
                _puzzleManager.SetActive(false);
                _ui.ShowGameOver(_totalScore);
            }
        }

        /// <summary>Called by PuzzleManager when player reaches goal.</summary>
        public void OnGoalReached(int movesUsed, int movesLimit)
        {
            if (!_isPlaying) return;

            int baseScore = 100 * (_currentStageIndex + 1);
            int movesBonus = movesLimit > 0 ? Mathf.Max(0, movesLimit - movesUsed) * 10 : 20;
            int noSwitchBonus = !_eraSwitchedThisStage ? 50 : 0;
            int earned = Mathf.RoundToInt((baseScore + movesBonus + noSwitchBonus) * _scoreMultiplier);
            _totalScore += earned;

            _isPlaying = false;
            _puzzleManager.SetActive(false);
            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClear(_currentStageIndex + 1, _totalScore);
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
            _eraSwitchedThisStage = false;
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
