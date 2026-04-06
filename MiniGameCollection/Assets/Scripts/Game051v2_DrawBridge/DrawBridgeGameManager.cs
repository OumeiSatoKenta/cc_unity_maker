using UnityEngine;

namespace Game051v2_DrawBridge
{
    public enum GameState { Idle, Drawing, Rolling, StageClear, AllClear, GameOver }

    public class DrawBridgeGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] DrawingManager _drawingManager;
        [SerializeField] BallController _ballController;
        [SerializeField] DrawBridgeUI _ui;

        public GameState State { get; private set; } = GameState.Idle;
        public int Score { get; private set; }

        private int _comboCount;
        private float _comboMultiplier = 1.0f;
        private float _stageStartTime;

        void Start()
        {
            _instructionPanel.Show(
                "051v2",
                "DrawBridge",
                "橋を描いてボールをゴールへ届けよう",
                "画面をドラッグして橋を描き、GOボタンでボールを転がそう。消しゴムで描き直しもできるよ",
                "ボールが対岸のゴールに到達したらクリア！残りインクが多いほど高得点"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void OnDestroy()
        {
            _instructionPanel.OnDismissed -= StartGame;
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
        }

        void StartGame()
        {
            Score = 0;
            _comboCount = 0;
            _comboMultiplier = 1.0f;

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { stageName = "Stage 1", speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f },
                new StageManager.StageConfig { stageName = "Stage 2", speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.2f },
                new StageManager.StageConfig { stageName = "Stage 3", speedMultiplier = 1.2f, countMultiplier = 1, complexityFactor = 0.4f },
                new StageManager.StageConfig { stageName = "Stage 4", speedMultiplier = 1.2f, countMultiplier = 2, complexityFactor = 0.6f },
                new StageManager.StageConfig { stageName = "Stage 5", speedMultiplier = 1.5f, countMultiplier = 2, complexityFactor = 0.8f },
            });

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            State = GameState.Drawing;
            _comboCount = Mathf.Max(0, _comboCount);

            var config = _stageManager.GetCurrentStageConfig();
            int stageNumber = stageIndex + 1;
            _drawingManager.SetupStage(config, stageNumber);
            _ballController.ResetBall();
            _ui.UpdateStage(stageNumber, 5);
            _ui.UpdateScore(Score);
            _ui.HideStageClear();
            _ui.HideGameOver();
            _ui.SetGoButtonEnabled(true);
            _stageStartTime = Time.time;
        }

        void OnAllStagesCleared()
        {
            State = GameState.AllClear;
            _ui.ShowAllClear(Score);
        }

        public void OnGoPressed()
        {
            if (State != GameState.Drawing) return;
            State = GameState.Rolling;
            _drawingManager.SetDrawingEnabled(false);
            _ui.SetGoButtonEnabled(false);
            _ballController.Launch();
        }

        public void OnErasePressed()
        {
            if (State != GameState.Drawing) return;
            _drawingManager.ClearLines();
            _ui.UpdateInk(_drawingManager.InkRemaining);
        }

        public void OnBallReachedGoal()
        {
            if (State != GameState.Rolling) return;
            State = GameState.StageClear;
            _drawingManager.SetActive(false);

            float ink = _drawingManager.InkRemaining;
            int baseScore = Mathf.RoundToInt(ink * 100f);

            // Speed bonus
            float elapsed = Time.time - _stageStartTime;
            float speedBonus = elapsed <= 5f ? 1.5f : 1.0f;

            // Efficiency bonus
            float efficiency = _drawingManager.GetEfficiencyRatio();
            float efficiencyBonus = efficiency <= 1.5f ? 2.0f : 1.0f;

            _comboCount++;
            _comboMultiplier = 1.0f + (_comboCount - 1) * 0.1f;
            _comboMultiplier = Mathf.Min(_comboMultiplier, 1.5f);

            int stageScore = Mathf.RoundToInt(baseScore * speedBonus * efficiencyBonus * _comboMultiplier);
            Score += stageScore;
            _ui.UpdateScore(Score);
            _ui.ShowStageClear(Score);
        }

        public void OnBallFell()
        {
            if (State != GameState.Rolling) return;
            State = GameState.GameOver;
            _drawingManager.SetActive(false);
            _comboCount = 0;
            _comboMultiplier = 1.0f;
            _ui.ShowGameOver(Score);
        }

        public void GoNextStage()
        {
            if (State != GameState.StageClear) return;
            _drawingManager.ClearLines();
            _stageManager.CompleteCurrentStage();
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void GoToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
