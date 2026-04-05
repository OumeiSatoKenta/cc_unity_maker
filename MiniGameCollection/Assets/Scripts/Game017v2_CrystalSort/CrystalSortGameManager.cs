using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game017v2_CrystalSort
{
    public class CrystalSortGameManager : MonoBehaviour
    {
        public enum GameState
        {
            WaitingInstruction,
            Playing,
            StageClear,
            Clear,
            GameOver
        }

        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BottleManager _bottleManager;
        [SerializeField] CrystalSortUI _ui;

        GameState _state = GameState.WaitingInstruction;
        int _totalScore;
        int _combo;
        int _currentStage;
        int _completedBottles;

        public GameState State => _state;
        public int TotalScore => _totalScore;
        public int Combo => _combo;

        void Start()
        {
            _instructionPanel.OnDismissed += StartGame;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;

            _instructionPanel.Show(
                "017v2",
                "CrystalSort",
                "同じ色のクリスタルを同じ瓶に集めよう",
                "瓶タップで選択 → 移動先の瓶タップで移動",
                "少ない手数で全瓶を単色に揃えよう"
            );
        }

        void StartGame()
        {
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            _currentStage = stage;
            _state = GameState.Playing;
            _completedBottles = 0;
            var config = _stageManager.GetCurrentStageConfig();
            _bottleManager.SetupStage(config, stage + 1);
            _ui.UpdateStage(stage + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.HideAllPanels();
        }

        void OnAllStagesCleared()
        {
            _state = GameState.Clear;
            _ui.ShowClearPanel(_totalScore);
        }

        public void OnStageClear(int remainingMoves, int maxMoves)
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;

            float remainingRatio = maxMoves > 0 ? (float)remainingMoves / maxMoves : 0f;
            int baseScore = 1000 * (_currentStage + 1);
            float stageScore = baseScore * (1f + remainingRatio);

            _combo++;
            float comboMultiplier = _combo >= 5 ? 2.0f : _combo >= 3 ? 1.6f : _combo >= 2 ? 1.3f : 1.0f;
            float bottleMultiplier = Mathf.Pow(1.1f, _completedBottles);
            stageScore *= comboMultiplier * bottleMultiplier;

            int score = Mathf.RoundToInt(stageScore);
            _totalScore += score;

            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClearPanel(score, _combo);
        }

        public void OnBottleCompleted()
        {
            _completedBottles++;
        }

        public void OnComboMove(int comboCount)
        {
            int bonus = comboCount >= 5 ? 500 : comboCount >= 3 ? 150 : 50;
            _totalScore += bonus;
            _ui.UpdateScore(_totalScore);
            _ui.ShowCombo(comboCount);
        }

        public void OnGameOver()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.GameOver;
            _combo = 0;
            _ui.ShowGameOverPanel();
        }

        public void OnNextStage()
        {
            if (_state != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            if (_state != GameState.GameOver) return;
            _state = GameState.Playing;
            _completedBottles = 0;
            var config = _stageManager.GetCurrentStageConfig();
            _bottleManager.SetupStage(config, _currentStage + 1);
            _ui.HideAllPanels();
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }

        public void ShowInstructions()
        {
            _instructionPanel.Show(
                "017v2",
                "CrystalSort",
                "同じ色のクリスタルを同じ瓶に集めよう",
                "瓶タップで選択 → 移動先の瓶タップで移動",
                "少ない手数で全瓶を単色に揃えよう"
            );
        }
    }
}
