using UnityEngine;

namespace Game050v2_BubbleSort
{
    public enum GameState { Idle, Playing, StageClear, AllClear, GameOver }

    public class BubbleSortGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BubbleGridManager _gridManager;
        [SerializeField] BubbleSortUI _ui;

        public GameState State { get; private set; } = GameState.Idle;
        public int Score { get; private set; }

        private int _comboCount;
        private int _comboMultiplier = 1;

        void Start()
        {
            _instructionPanel.Show(
                "050v2",
                "BubbleSort",
                "色バブルを並び替えてソートを完成させよう",
                "隣り合う2つのバブルを順にタップして入れ替えよう。同じ色を3つ以上揃えると消えてボーナス！",
                "全バブルを色ごとにまとめて並び替えたらクリア！手数が少ないほど高得点"
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
            _comboMultiplier = 1;

            // Custom stage configs for BubbleSort difficulty curve
            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { stageName = "Stage 1", speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f },
                new StageManager.StageConfig { stageName = "Stage 2", speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f },
                new StageManager.StageConfig { stageName = "Stage 3", speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.2f },
                new StageManager.StageConfig { stageName = "Stage 4", speedMultiplier = 1.5f, countMultiplier = 1, complexityFactor = 0.2f },
                new StageManager.StageConfig { stageName = "Stage 5", speedMultiplier = 1.5f, countMultiplier = 1, complexityFactor = 0.3f },
            });

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            State = GameState.Playing;
            _comboCount = 0;
            _comboMultiplier = 1;

            int stageNumber = stageIndex + 1;
            var config = _stageManager.GetCurrentStageConfig();
            _gridManager.SetupStage(config, stageNumber);
            _ui.UpdateStage(stageNumber, 5);
            _ui.UpdateScore(Score);
            _ui.UpdateCombo(0);
            _ui.HideStageClear();
        }

        void OnAllStagesCleared()
        {
            State = GameState.AllClear;
            _ui.ShowAllClear(Score);
        }

        // Called by BubbleGridManager when a swap is performed
        public void OnSwapPerformed(int movesRemaining, int totalMoves)
        {
            if (State != GameState.Playing) return;
            _ui.UpdateMoves(movesRemaining, totalMoves);

            if (movesRemaining <= 0)
            {
                // Check if sort is complete (will be checked by grid manager)
            }
        }

        // Called by BubbleGridManager on match (3+ consecutive same color)
        public void OnMatchCleared(int matchSize, bool isChain)
        {
            if (State != GameState.Playing) return;

            int pts = 0;
            if (matchSize == 3) pts = 500;
            else if (matchSize == 4) pts = 800;
            else if (matchSize >= 5) pts = 1500;

            if (isChain)
            {
                _comboCount++;
                _comboMultiplier = _comboCount >= 3 ? 3 : _comboCount >= 2 ? 2 : 1;
                pts += _comboCount * 300 * _comboMultiplier;
                _ui.UpdateCombo(_comboCount);
            }
            else
            {
                _comboCount = 1;
                _comboMultiplier = 1;
            }

            Score += pts;
            _ui.UpdateScore(Score);
            _ui.ShowBonusText("+" + pts + " COMBO!", new Color(1f, 0.9f, 0.2f));
        }

        // Called when sort is complete
        public void OnSortComplete(int movesRemaining, int minimumMoves)
        {
            if (State != GameState.Playing) return;
            State = GameState.StageClear;
            _gridManager.SetActive(false);

            int bonus = 1000 + movesRemaining * 200;
            if (movesRemaining >= minimumMoves - 1)
            {
                // Perfect clear
                bonus *= 3;
                _ui.ShowBonusText("PERFECT! ×3", Color.yellow);
            }
            Score += bonus;
            _ui.UpdateScore(Score);
            _ui.ShowStageClear(Score);
        }

        // Called when out of moves without sorting
        public void OnMovesExhausted()
        {
            if (State != GameState.Playing) return;
            State = GameState.GameOver;
            _gridManager.SetActive(false);
            _ui.ShowGameOver(Score);
        }

        public void ResetCombo()
        {
            _comboCount = 0;
            _comboMultiplier = 1;
            _ui.UpdateCombo(0);
        }

        public void GoNextStage()
        {
            if (State != GameState.StageClear) return;
            _gridManager.ClearGrid();
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
