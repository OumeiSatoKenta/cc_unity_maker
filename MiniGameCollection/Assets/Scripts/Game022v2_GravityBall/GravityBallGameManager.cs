using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game022v2_GravityBall
{
    public enum GravityBallGameState { WaitingInstruction, Playing, StageClear, Clear, GameOver }

    public class GravityBallGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] GravityBallController _controller;
        [SerializeField] GravityBallUI _ui;

        public GravityBallGameState State { get; private set; } = GravityBallGameState.WaitingInstruction;

        int _totalScore;
        int _comboCount;
        float _comboMultiplier = 1f;
        float _distanceTraveled;

        // Target distances per stage (meters)
        static readonly float[] StageTargetDistances = { 100f, 200f, 300f, 400f, 500f };

        void Start()
        {
            _instructionPanel.Show(
                "022v2",
                "GravityBall",
                "重力を反転させながら障害物をよけよう",
                "画面をタップして重力を上下に反転させる",
                "各ステージの目標距離に到達しよう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _totalScore = 0;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _distanceTraveled = 0f;
            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            State = GravityBallGameState.Playing;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _distanceTraveled = 0f;
            float targetDist = GetTargetDistance(stage);
            var config = _stageManager.GetCurrentStageConfig();
            _controller.SetupStage(config, targetDist);
            _ui.UpdateStageDisplay(stage + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_comboMultiplier, _comboCount);
            _ui.UpdateDistance(0f, targetDist);
        }

        void OnAllStagesCleared()
        {
            State = GravityBallGameState.Clear;
            _ui.ShowClearPanel(_totalScore);
        }

        float GetTargetDistance(int stage0Based)
        {
            int idx = Mathf.Clamp(stage0Based, 0, StageTargetDistances.Length - 1);
            return StageTargetDistances[idx];
        }

        public void OnDistanceUpdated(float distance, float targetDist)
        {
            if (State != GravityBallGameState.Playing) return;
            _distanceTraveled = distance;

            // Distance score
            int distScore = Mathf.FloorToInt(distance);
            // Update total only incrementally (keep base + bonuses)
            _ui.UpdateDistance(distance, targetDist);

            if (distance >= targetDist)
                StageClear();
        }

        public void OnObstaclePassed(bool isNarrow, bool isPerfect)
        {
            if (State != GravityBallGameState.Playing) return;
            _comboCount++;
            UpdateComboMultiplier();

            int bonus = 0;
            if (isPerfect)
                bonus += Mathf.RoundToInt(100 * _comboMultiplier);
            else if (isNarrow)
                bonus += Mathf.RoundToInt(50 * _comboMultiplier);

            if (bonus > 0)
            {
                _totalScore += bonus;
                _ui.ShowBonus(bonus, isPerfect);
            }

            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_comboMultiplier, _comboCount);
        }

        public void OnGameOver()
        {
            if (State != GravityBallGameState.Playing) return;
            State = GravityBallGameState.GameOver;
            _controller.StopGame();
            _ui.ShowGameOverPanel(_totalScore, Mathf.FloorToInt(_distanceTraveled));
        }

        void StageClear()
        {
            if (State != GravityBallGameState.Playing) return;
            State = GravityBallGameState.StageClear;
            _controller.StopGame();
            _ui.ShowStageClearPanel(_comboCount);
        }

        void UpdateComboMultiplier()
        {
            if (_comboCount >= 10) _comboMultiplier = 3.0f;
            else if (_comboCount >= 6) _comboMultiplier = 2.0f;
            else if (_comboCount >= 3) _comboMultiplier = 1.5f;
            else _comboMultiplier = 1.0f;
        }

        public void OnNextStage()
        {
            if (State != GravityBallGameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("CollectionSelect");
        }

        public void ShowInstructions()
        {
            if (_instructionPanel != null)
                _instructionPanel.Show(
                    "022v2",
                    "GravityBall",
                    "重力を反転させながら障害物をよけよう",
                    "画面をタップして重力を上下に反転させる",
                    "各ステージの目標距離に到達しよう"
                );
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
    }
}
