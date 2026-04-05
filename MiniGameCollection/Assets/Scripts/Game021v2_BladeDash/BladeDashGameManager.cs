using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game021v2_BladeDash
{
    public enum BladeDashGameState { WaitingInstruction, Playing, StageClear, Clear, GameOver }

    public class BladeDashGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BladeRunner _bladeRunner;
        [SerializeField] BladeDashUI _ui;

        public BladeDashGameState State { get; private set; } = BladeDashGameState.WaitingInstruction;

        int _totalScore;
        int _comboCount;
        float _comboMultiplier = 1f;
        int _currentTargetScore;

        // Target scores per stage (0-based index)
        static readonly int[] StageTargetScores = { 500, 1500, 3000, 5000, 8000 };

        void Start()
        {
            ShowInstructions();
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _totalScore = 0;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        // stage is 0-based from StageManager
        void OnStageChanged(int stage)
        {
            State = BladeDashGameState.Playing;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _currentTargetScore = GetTargetScore(stage);
            var config = _stageManager.GetCurrentStageConfig();
            _bladeRunner.SetupStage(config, _currentTargetScore);
            _ui.UpdateStageDisplay(stage + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore, _currentTargetScore);
            _ui.UpdateCombo(_comboMultiplier);
        }

        void OnAllStagesCleared()
        {
            State = BladeDashGameState.Clear;
            _ui.ShowClearPanel(_totalScore);
        }

        int GetTargetScore(int stage0Based)
        {
            int idx = Mathf.Clamp(stage0Based, 0, StageTargetScores.Length - 1);
            return StageTargetScores[idx];
        }

        public void OnCoinCollected()
        {
            if (State != BladeDashGameState.Playing) return;
            _comboCount++;
            UpdateComboMultiplier();
            int points = Mathf.RoundToInt(10 * _comboMultiplier);
            _totalScore += points;
            _ui.UpdateScore(_totalScore, _currentTargetScore);
            _ui.UpdateCombo(_comboMultiplier);

            if (_totalScore >= _currentTargetScore)
                StageClear();
        }

        public void OnNearMiss()
        {
            if (State != BladeDashGameState.Playing) return;
            int bonus = Mathf.RoundToInt(30 * _comboMultiplier);
            _totalScore += bonus;
            _ui.ShowNearMissBonus(bonus);
            _ui.UpdateScore(_totalScore, _currentTargetScore);

            if (_totalScore >= _currentTargetScore)
                StageClear();
        }

        public void OnGameOver()
        {
            if (State != BladeDashGameState.Playing) return;
            State = BladeDashGameState.GameOver;
            _bladeRunner.StopGame();
            _ui.ShowGameOverPanel(_totalScore);
        }

        void StageClear()
        {
            State = BladeDashGameState.StageClear;
            _bladeRunner.StopGame();
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
            if (State != BladeDashGameState.StageClear) return;
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
                    "021v2",
                    "BladeDash",
                    "迫りくる刃を避けながらコインを集めよう",
                    "左右スワイプでレーン切替、上スワイプでジャンプ、下スワイプでスライディング",
                    "刃を避けてコインを集め、目標スコアに到達しよう"
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
