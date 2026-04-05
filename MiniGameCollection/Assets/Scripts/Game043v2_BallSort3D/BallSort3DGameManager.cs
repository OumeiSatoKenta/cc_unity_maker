using UnityEngine;

namespace Game043v2_BallSort3D
{
    public enum BallSort3DState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class BallSort3DGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BallSort3DMechanic _mechanic;
        [SerializeField] BallSort3DUI _ui;

        BallSort3DState _state;
        int _totalScore;
        int _currentStage;
        int _moveCount;
        bool _undoUsed;

        public BallSort3DState State => _state;
        public int TotalScore => _totalScore;
        public int CurrentStage => _currentStage;
        public int MoveCount => _moveCount;

        void Start()
        {
            _state = BallSort3DState.WaitingInstruction;
            _instructionPanel.Show(
                "043v2",
                "BallSort3D",
                "色付きボールを同じ色のチューブに揃えよう",
                "チューブをタップしてボールを移動。同色か空のチューブに入れられる",
                "全チューブを同じ色のボールだけにしたらクリア"
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
            _moveCount = 0;
            _undoUsed = false;
            _ui.HideAllPanels();
            _state = BallSort3DState.Playing;
            if (stage == 0) _totalScore = 0;

            _mechanic.SetupStage(stage);
            _ui.UpdateStage(stage + 1);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateMoveCount(_moveCount);
            _ui.UpdateCombo(0);
        }

        void OnAllStagesCleared()
        {
            _state = BallSort3DState.Clear;
            _mechanic.SetActive(false);
            _ui.ShowFinalClearPanel(_totalScore);
        }

        public void OnStageClear(int minMoves)
        {
            if (_state != BallSort3DState.Playing) return;
            _state = BallSort3DState.StageClear;
            _mechanic.SetActive(false);

            int baseClear = 1000;
            int movesBonus = _moveCount > 0 ? Mathf.RoundToInt((float)minMoves / _moveCount * 2000f) : 2000;
            int undoBonus = _undoUsed ? 0 : 500;
            int stageBonus = (_currentStage + 1) * 200;
            int stageTotalScore = baseClear + movesBonus + undoBonus + stageBonus;
            _totalScore += stageTotalScore;

            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClearPanel(_totalScore);
        }

        public void OnGameOver()
        {
            if (_state != BallSort3DState.Playing) return;
            _state = BallSort3DState.GameOver;
            _mechanic.SetActive(false);
            _ui.ShowGameOverPanel(_totalScore);
        }

        public void OnMoveMade()
        {
            if (_state != BallSort3DState.Playing) return;
            _moveCount++;
            _ui.UpdateMoveCount(_moveCount);
        }

        public void OnUndoUsed()
        {
            _undoUsed = true;
        }

        public void OnComboChanged(int combo)
        {
            _ui.UpdateCombo(combo);
        }

        public void OnScoreAdded(int score)
        {
            _totalScore += score;
            _ui.UpdateScore(_totalScore);
        }

        public void AdvanceToNextStage()
        {
            if (_state != BallSort3DState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void RetryGame()
        {
            if (_state != BallSort3DState.GameOver && _state != BallSort3DState.Clear) return;
            _stageManager.StartFromBeginning();
        }

        public void ReturnToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
