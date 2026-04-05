using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game015v2_TileTurn
{
    public class TileTurnGameManager : MonoBehaviour
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
        [SerializeField] TileManager _tileManager;
        [SerializeField] TileTurnUI _ui;

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
                "015v2",
                "TileTurn",
                "タイルをタップして回転させ、1枚の絵を完成させよう",
                "タイルをタップで90度回転",
                "少ない回転数で全タイルを正しい向きにしよう"
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
            _tileManager.SetupStage(config, stage + 1);
            _ui.UpdateStage(stage + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.HideAllPanels();
        }

        void OnAllStagesCleared()
        {
            _state = GameState.Clear;
            _ui.ShowClearPanel(_totalScore);
        }

        public void OnStageClear(int remainingRotations, int maxRotations, bool previewUsed)
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;

            float remainingRatio = maxRotations > 0 ? (float)remainingRotations / maxRotations : 0f;
            int baseScore = 1000 * (_currentStage + 1);
            float stageScore = baseScore * (1f + remainingRatio);
            if (!previewUsed) stageScore *= 1.5f;

            _combo++;
            float comboMultiplier = _combo >= 5 ? 2.0f : _combo >= 3 ? 1.5f : _combo >= 2 ? 1.2f : 1.0f;
            stageScore *= comboMultiplier;

            int score = Mathf.RoundToInt(stageScore);
            _totalScore += score;

            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClearPanel(score, _combo);
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
            _tileManager.ResetStage();
            _ui.HideAllPanels();
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }

        public void ShowInstructions()
        {
            _instructionPanel.Show(
                "015v2",
                "TileTurn",
                "タイルをタップして回転させ、1枚の絵を完成させよう",
                "タイルをタップで90度回転",
                "少ない回転数で全タイルを正しい向きにしよう"
            );
        }
    }
}
