using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game019v2_PathCut
{
    public class PathCutGameManager : MonoBehaviour
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
        [SerializeField] PathCutManager _pathCutManager;
        [SerializeField] PathCutUI _ui;

        GameState _state = GameState.WaitingInstruction;
        int _totalScore;
        int _combo;
        int _currentStage;

        public GameState State => _state;
        public int TotalScore => _totalScore;
        public int Combo => _combo;

        void Start()
        {
            _instructionPanel.OnDismissed += StartGame;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;

            _instructionPanel.Show(
                "019v2",
                "PathCut",
                "ロープをカットしてボールを星に当てよう",
                "ロープをスワイプでカット",
                "少ないカット数で全ての星にボールを当てよう"
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
            var config = _stageManager.GetCurrentStageConfig();
            _pathCutManager.SetupStage(config, stage + 1);
            _ui.UpdateStage(stage + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.HideAllPanels();
        }

        void OnAllStagesCleared()
        {
            _state = GameState.Clear;
            _ui.ShowClearPanel(_totalScore);
        }

        public void OnStageClear(int cutsUsed, int cutsAllowed, int starsCollected)
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;

            int baseScore = 1000 * (_currentStage + 1);
            float remainingBonus = (cutsAllowed - cutsUsed) * 200f;
            float minCutBonus = (cutsUsed <= 1) ? baseScore * 0.5f : 0f;

            _combo++;
            float comboMul = _combo >= 3 ? 1.5f : _combo >= 2 ? 1.2f : 1.0f;
            int stageScore = Mathf.RoundToInt((baseScore + remainingBonus + minCutBonus) * comboMul);
            _totalScore += stageScore;

            _ui.UpdateScore(_totalScore);

            int stars;
            if (cutsUsed <= 1) stars = 3;
            else if (cutsUsed <= Mathf.CeilToInt(cutsAllowed * 0.6f)) stars = 2;
            else stars = 1;

            _ui.ShowStageClearPanel(stageScore, _combo, stars);
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
            if (_state != GameState.GameOver && _state != GameState.StageClear) return;
            _combo = 0;
            _state = GameState.Playing;
            var config = _stageManager.GetCurrentStageConfig();
            _pathCutManager.SetupStage(config, _currentStage + 1);
            _ui.HideAllPanels();
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }

        public void ShowInstructions()
        {
            _instructionPanel.Show(
                "019v2",
                "PathCut",
                "ロープをカットしてボールを星に当てよう",
                "ロープをスワイプでカット",
                "少ないカット数で全ての星にボールを当てよう"
            );
        }
    }
}
