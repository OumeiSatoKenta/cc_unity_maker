using UnityEngine;

namespace Game042v2_ColorDrop
{
    public enum ColorDropState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class ColorDropGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] ColorDropMechanic _mechanic;
        [SerializeField] ColorDropUI _ui;

        ColorDropState _state;
        int _totalScore;
        int _currentStage;
        int _lives;

        public ColorDropState State => _state;
        public int TotalScore => _totalScore;
        public int CurrentStage => _currentStage;
        public int Lives => _lives;

        void Start()
        {
            _state = ColorDropState.WaitingInstruction;
            _instructionPanel.Show(
                "042v2",
                "ColorDrop",
                "色付きの雫を同じ色のバケツに振り分けよう",
                "左右にスワイプして雫を振り分ける",
                "目標数の雫を正しく振り分けてステージクリア！"
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
            _lives = 3;
            _ui.HideAllPanels();
            _state = ColorDropState.Playing;
            if (stage == 0) _totalScore = 0;

            _mechanic.SetupStage(stage);
            _ui.UpdateStage(stage + 1);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateLives(_lives);
        }

        void OnAllStagesCleared()
        {
            _state = ColorDropState.Clear;
            _mechanic.SetActive(false);
            _ui.ShowFinalClearPanel(_totalScore);
        }

        public void OnStageClear(int stageBonusScore)
        {
            if (_state != ColorDropState.Playing) return;
            _state = ColorDropState.StageClear;
            int bonus = _lives * 300 + stageBonusScore;
            _totalScore += bonus;
            _mechanic.SetActive(false);
            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClearPanel(_totalScore);
        }

        public void OnGameOver()
        {
            if (_state != ColorDropState.Playing) return;
            _state = ColorDropState.GameOver;
            _mechanic.SetActive(false);
            _ui.ShowGameOverPanel(_totalScore);
        }

        public void OnScoreAdded(int score)
        {
            _totalScore += score;
            _ui.UpdateScore(_totalScore);
        }

        public void OnLifeLost()
        {
            if (_state != ColorDropState.Playing) return;
            _lives = Mathf.Max(0, _lives - 1);
            _ui.UpdateLives(_lives);
            if (_lives <= 0)
            {
                OnGameOver();
            }
        }

        public void OnProgressChanged(int processed, int target)
        {
            _ui.UpdateProgress(processed, target);
        }

        public void OnComboChanged(int combo)
        {
            _ui.UpdateCombo(combo);
        }

        public void AdvanceToNextStage()
        {
            if (_state != ColorDropState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void RetryGame()
        {
            if (_state != ColorDropState.GameOver && _state != ColorDropState.Clear) return;
            _stageManager.StartFromBeginning();
        }

        public void ReturnToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
