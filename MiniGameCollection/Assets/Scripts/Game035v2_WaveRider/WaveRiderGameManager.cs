using UnityEngine;
using System.Collections;

namespace Game035v2_WaveRider
{
    public enum WaveRiderState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class WaveRiderGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] WaveMechanic _mechanic;
        [SerializeField] WaveRiderUI _ui;

        public WaveRiderState State { get; private set; } = WaveRiderState.WaitingInstruction;

        int _score;
        int _combo;
        int _currentStage;
        bool _shieldActive;

        void Start()
        {
            _instructionPanel.Show(
                "035v2",
                "WaveRider",
                "波に乗ってトリックを決めながらゴールを目指そう",
                "左右タップでレーン移動、タップでジャンプ・トリック",
                "岩を避けてゴールまで走破しよう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _score = 0;
            _combo = 0;
            _shieldActive = false;
            State = WaveRiderState.Playing;

            _ui.Initialize(this);
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(0);

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 5,  complexityFactor = 0.0f, stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 1.3f, countMultiplier = 6,  complexityFactor = 0.2f, stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 1.6f, countMultiplier = 8,  complexityFactor = 0.5f, stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 10, complexityFactor = 0.7f, stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 2.5f, countMultiplier = 13, complexityFactor = 1.0f, stageName = "Stage 5" },
            });

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            if (State == WaveRiderState.Clear) return;
            _currentStage = stageIndex + 1;
            State = WaveRiderState.Playing;
            _combo = 0;
            _shieldActive = false;

            var config = _stageManager.GetCurrentStageConfig();
            _mechanic.SetupStage(config, stageIndex);

            _ui.UpdateStage(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(0);
            _ui.HideStageClear();
        }

        void OnAllStagesCleared()
        {
            State = WaveRiderState.Clear;
            _mechanic.Deactivate();
            _ui.ShowFinalClear(_score);
        }

        public void OnTrickSuccess(bool isPerfect)
        {
            if (State != WaveRiderState.Playing) return;

            _combo++;
            int multiplier = Mathf.Clamp(_combo, 1, 5);
            int pts = isPerfect ? 100 * multiplier : 50 * multiplier;
            _score += pts;

            // Shield acquisition at combo 3+ in stage 4+
            if (_currentStage >= 4 && _combo >= 3 && !_shieldActive)
            {
                _shieldActive = true;
                _mechanic.ActivateShield();
                _ui.ShowShield(true);
            }

            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_combo);
        }

        public void OnTrickFailed()
        {
            if (State != WaveRiderState.Playing) return;
            _combo = 0;
            _ui.UpdateCombo(0);
        }

        public void OnHitObstacle()
        {
            if (State != WaveRiderState.Playing) return;

            if (_shieldActive)
            {
                _shieldActive = false;
                _mechanic.DeactivateShield();
                _ui.ShowShield(false);
                _combo = 0;
                _ui.UpdateCombo(0);
                return;
            }

            State = WaveRiderState.GameOver;
            _mechanic.Deactivate();
            _ui.ShowGameOver(_score);
        }

        public void OnStageGoalReached()
        {
            if (State != WaveRiderState.Playing) return;
            State = WaveRiderState.StageClear;
            _mechanic.Deactivate();
            _ui.ShowStageClear(_currentStage, _stageManager.TotalStages);
        }

        public void OnDistanceUpdate(float distanceTraveled, float goalDistance)
        {
            if (State != WaveRiderState.Playing) return;
            _score += 1; // distance bonus per call
            _ui.UpdateDistance(distanceTraveled, goalDistance);
            _ui.UpdateScore(_score);
        }

        public void AdvanceToNextStage()
        {
            if (State != WaveRiderState.StageClear) return;
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
