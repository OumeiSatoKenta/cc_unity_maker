using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game020v2_EchoMaze
{
    public enum EchoMazeGameState { WaitingInstruction, Playing, StageClear, Clear, GameOver }

    public class EchoMazeGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] MazeController _mazeController;
        [SerializeField] EchoMazeUI _ui;

        public EchoMazeGameState State { get; private set; } = EchoMazeGameState.WaitingInstruction;

        int _totalScore;
        int _comboCount;
        bool _stageClearWithoutEcho;
        bool _stageClearWithoutMap;

        void Start()
        {
            _instructionPanel.Show(
                "020v2",
                "EchoMaze",
                "音のエコーだけを頼りに見えない迷路を進もう",
                "方向ボタンで移動、中央ボタンでエコー発信",
                "エコーを聞いて壁を避け、ゴールに到達しよう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _totalScore = 0;
            _comboCount = 0;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            State = EchoMazeGameState.Playing;
            _stageClearWithoutEcho = true;
            _stageClearWithoutMap = true;
            _comboCount = 0;
            _mazeController.SetupStage(stage, _stageManager.GetCurrentStageConfig());
            _ui.UpdateStageDisplay(stage, _stageManager.TotalStages);
        }

        void OnAllStagesCleared()
        {
            State = EchoMazeGameState.Clear;
            _ui.ShowClearPanel(_totalScore);
        }

        public void OnGoalReached(int movesRemaining, int moveLimit)
        {
            if (State != EchoMazeGameState.Playing) return;
            State = EchoMazeGameState.StageClear;

            int baseScore = 1000;
            float moveBonus = 1f + (float)movesRemaining / Mathf.Max(moveLimit, 1);
            float echoBonus = _stageClearWithoutEcho ? 1.5f : 1f;
            float mapBonus = _stageClearWithoutMap ? 2.0f : 1f;
            float comboMult = _comboCount >= 3 ? 1.5f : _comboCount >= 2 ? 1.2f : 1.0f;

            int stageScore = Mathf.RoundToInt(baseScore * moveBonus * echoBonus * mapBonus * comboMult);
            _totalScore += stageScore;

            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClearPanel(stageScore, _comboCount, echoBonus > 1f, mapBonus > 1f);
        }

        public void OnGameOver()
        {
            if (State != EchoMazeGameState.Playing) return;
            State = EchoMazeGameState.GameOver;
            _ui.ShowGameOverPanel();
        }

        public void OnEchoUsed()
        {
            _stageClearWithoutEcho = false;
        }

        public void OnMapUsed()
        {
            _stageClearWithoutMap = false;
        }

        public void OnExploredNewCell()
        {
            _comboCount++;
        }

        public void OnNextStage()
        {
            if (State != EchoMazeGameState.StageClear) return;
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
            _instructionPanel.Show(
                "020v2",
                "EchoMaze",
                "音のエコーだけを頼りに見えない迷路を進もう",
                "方向ボタンで移動、中央ボタンでエコー発信",
                "エコーを聞いて壁を避け、ゴールに到達しよう"
            );
        }
    }
}
