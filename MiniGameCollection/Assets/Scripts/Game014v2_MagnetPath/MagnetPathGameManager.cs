using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game014v2_MagnetPath
{
    public class MagnetPathGameManager : MonoBehaviour
    {
        public enum GameState
        {
            WaitingInstruction,
            Playing,
            BallMoving,
            StageClear,
            Clear,
            GameOver
        }

        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] MagnetManager _magnetManager;
        [SerializeField] MagnetPathUI _ui;

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
                "014v2",
                "MagnetPath",
                "磁石の極性を切り替えて鉄球をゴールに導こう",
                "磁石をタップでN/S切替 → スタートで鉄球発射",
                "少ない切替回数で鉄球をゴールに到達させよう"
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
            _magnetManager.SetupStage(config, stage + 1);
            _ui.UpdateStage(stage + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.HideAllPanels();
        }

        void OnAllStagesCleared()
        {
            _state = GameState.Clear;
            _ui.ShowClearPanel(_totalScore);
        }

        public void OnBallLaunched()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.BallMoving;
        }

        public void OnBallReset()
        {
            if (_state != GameState.BallMoving && _state != GameState.Playing) return;
            _state = GameState.Playing;
        }

        public void OnGoalReached(int remainingSwitches, int maxSwitches)
        {
            if (_state != GameState.BallMoving) return;
            _state = GameState.StageClear;

            // Score calculation
            float ratio = maxSwitches > 0 ? (float)remainingSwitches / maxSwitches : 0f;
            int baseScore = 1000 * (_currentStage + 1);
            float efficiencyBonus = 1f + ratio;
            bool perfectClear = remainingSwitches == maxSwitches; // no switches used beyond reset
            float perfectMultiplier = perfectClear ? 2.0f : 1.0f;

            _combo++;
            float comboMultiplier = _combo >= 5 ? 2.0f : _combo >= 3 ? 1.5f : _combo >= 2 ? 1.2f : 1.0f;

            int stageScore = Mathf.RoundToInt(baseScore * efficiencyBonus * perfectMultiplier * comboMultiplier);
            _totalScore += stageScore;

            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClearPanel(stageScore, _combo);
        }

        public void OnBallOutOfBounds()
        {
            if (_state != GameState.BallMoving) return;
            _state = GameState.GameOver;
            _combo = 0;
            _ui.ShowGameOverPanel();
        }

        public void OnSwitchLimitExceeded()
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
            _magnetManager.ResetStage();
            _ui.HideAllPanels();
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }

        public void ShowInstructions()
        {
            _instructionPanel.Show(
                "014v2",
                "MagnetPath",
                "磁石の極性を切り替えて鉄球をゴールに導こう",
                "磁石をタップでN/S切替 → スタートで鉄球発射",
                "少ない切替回数で鉄球をゴールに到達させよう"
            );
        }
    }
}
