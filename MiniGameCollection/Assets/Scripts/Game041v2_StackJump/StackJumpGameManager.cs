using UnityEngine;

namespace Game041v2_StackJump
{
    public enum StackJumpState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class StackJumpGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] StackJumpMechanic _mechanic;
        [SerializeField] StackJumpUI _ui;

        StackJumpState _state;
        int _totalScore;
        int _currentStage;

        public StackJumpState State => _state;
        public int TotalScore => _totalScore;
        public int CurrentStage => _currentStage;

        void Start()
        {
            _state = StackJumpState.WaitingInstruction;
            _instructionPanel.Show(
                "041v2",
                "StackJump",
                "タイミングよくタップしてブロックを積み上げよう",
                "画面タップでブロックを止める",
                "目標段数まで積み上げてステージクリア！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
            if (_instructionPanel != null)
                _instructionPanel.OnDismissed -= StartGame;
        }

        void StartGame()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            _currentStage = stage;
            _ui.HideAllPanels();
            _state = StackJumpState.Playing;
            if (stage == 0) _totalScore = 0;

            _mechanic.SetupStage(stage);
            _ui.UpdateStage(stage + 1);
            _ui.UpdateScore(_totalScore);
        }

        void OnAllStagesCleared()
        {
            _state = StackJumpState.Clear;
            _mechanic.SetActive(false);
            _ui.ShowFinalClearPanel(_totalScore);
        }

        public void OnStageClear(int stageBonusScore)
        {
            if (_state != StackJumpState.Playing) return;
            _state = StackJumpState.StageClear;
            _totalScore += stageBonusScore;
            _mechanic.SetActive(false);
            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClearPanel(_totalScore);
        }

        public void OnGameOver()
        {
            if (_state != StackJumpState.Playing) return;
            _state = StackJumpState.GameOver;
            _mechanic.SetActive(false);
            _ui.ShowGameOverPanel(_totalScore);
        }

        public void OnScoreAdded(int score)
        {
            _totalScore += score;
            _ui.UpdateScore(_totalScore);
        }

        public void OnStackCountChanged(int count, int target)
        {
            _ui.UpdateStackCount(count, target);
        }

        public void OnComboChanged(int combo)
        {
            _ui.UpdateCombo(combo);
        }

        public void OnPerfect()
        {
            _ui.ShowPerfect();
        }

        public void AdvanceToNextStage()
        {
            if (_state != StackJumpState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void RetryGame()
        {
            if (_state != StackJumpState.GameOver && _state != StackJumpState.Clear) return;
            _stageManager.StartFromBeginning();
        }

        public void ReturnToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
