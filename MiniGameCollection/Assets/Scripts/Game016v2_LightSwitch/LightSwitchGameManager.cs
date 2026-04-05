using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game016v2_LightSwitch
{
    public class LightSwitchGameManager : MonoBehaviour
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
        [SerializeField] BulbManager _bulbManager;
        [SerializeField] LightSwitchUI _ui;

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
                "016v2",
                "LightSwitch",
                "電球をタップして目標のパターンを作ろう。隣の電球も連動するよ",
                "電球をタップでオン/オフ切替（隣接も反転）",
                "少ない手数で目標パターンを完成させよう"
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
            _bulbManager.SetupStage(config, stage + 1);
            _ui.UpdateStage(stage + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.HideAllPanels();
        }

        void OnAllStagesCleared()
        {
            _state = GameState.Clear;
            _ui.ShowClearPanel(_totalScore);
        }

        public void OnStageClear(int remainingMoves, int maxMoves, bool undoUsed)
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;

            float remainingRatio = maxMoves > 0 ? (float)remainingMoves / maxMoves : 0f;
            int baseScore = 1000 * (_currentStage + 1);
            float stageScore = baseScore * (1f + remainingRatio);
            if (!undoUsed) stageScore *= 1.5f;

            _combo++;
            float comboMultiplier = _combo >= 5 ? 2.0f : _combo >= 3 ? 1.6f : _combo >= 2 ? 1.3f : 1.0f;
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
            _bulbManager.ResetStage();
            _ui.HideAllPanels();
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }

        public void ShowInstructions()
        {
            _instructionPanel.Show(
                "016v2",
                "LightSwitch",
                "電球をタップして目標のパターンを作ろう。隣の電球も連動するよ",
                "電球をタップでオン/オフ切替（隣接も反転）",
                "少ない手数で目標パターンを完成させよう"
            );
        }
    }
}
