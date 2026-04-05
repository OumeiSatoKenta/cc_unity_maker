using UnityEngine;
using System.Collections;

namespace Game036v2_CoinStack
{
    public enum CoinStackState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class CoinStackGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] CoinMechanic _mechanic;
        [SerializeField] CoinStackUI _ui;

        public CoinStackState State { get; private set; } = CoinStackState.WaitingInstruction;

        int _score;
        int _combo;
        int _currentStage;

        void Start()
        {
            _instructionPanel.Show(
                "036v2",
                "CoinStack",
                "コインをタイミングよく積み上げてタワーを作ろう",
                "タップでコインをドロップ",
                "崩さずに目標の高さまで積み上げよう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _score = 0;
            _combo = 0;
            State = CoinStackState.Playing;

            _ui.Initialize(this);
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(0);

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 5,  complexityFactor = 0.0f, stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 8,  complexityFactor = 0.2f, stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 10, complexityFactor = 0.5f, stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 2.5f, countMultiplier = 12, complexityFactor = 0.7f, stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 3.0f, countMultiplier = 15, complexityFactor = 1.0f, stageName = "Stage 5" },
            });

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            if (State == CoinStackState.Clear) return;
            _currentStage = stageIndex + 1;
            State = CoinStackState.Playing;
            _combo = 0;

            var config = _stageManager.GetCurrentStageConfig();
            _mechanic.SetupStage(config, stageIndex);

            _ui.UpdateStage(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(0);
            _ui.HideStageClear();
        }

        void OnAllStagesCleared()
        {
            State = CoinStackState.Clear;
            _mechanic.Deactivate();
            _ui.ShowFinalClear(_score);
        }

        public void OnCoinPlaced(float offset, int remainingCoins)
        {
            if (State != CoinStackState.Playing) return;

            bool isPerfect = offset < 0.1f;
            bool isGood = offset < 0.3f;

            if (isPerfect)
            {
                _combo++;
                int multiplier = _combo >= 5 ? 3 : (_combo >= 3 ? 2 : 1);
                _score += 30 * multiplier;
            }
            else if (isGood)
            {
                _combo = 0;
                _score += 10;
            }
            else
            {
                _combo = 0;
            }

            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_combo);
            _ui.UpdateCoinCount(remainingCoins);
        }

        public void OnStageGoalReached()
        {
            if (State != CoinStackState.Playing) return;
            State = CoinStackState.StageClear;
            _mechanic.Deactivate();
            _ui.ShowStageClear(_currentStage, _stageManager.TotalStages);
        }

        public void OnTowerCollapsed()
        {
            if (State != CoinStackState.Playing) return;
            State = CoinStackState.GameOver;
            _mechanic.Deactivate();
            _ui.ShowGameOver(_score);
        }

        public void AdvanceToNextStage()
        {
            if (State != CoinStackState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void RetryGame()
        {
            StartGame();
        }

        public void ReturnToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
