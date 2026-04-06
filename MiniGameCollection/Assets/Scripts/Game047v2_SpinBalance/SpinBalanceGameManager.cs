using UnityEngine;

namespace Game047v2_SpinBalance
{
    public enum GameState { Idle, Playing, StageClear, AllClear, GameOver }

    public class SpinBalanceGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BalanceManager _balanceManager;
        [SerializeField] SpinBalanceUI _ui;

        public GameState State { get; private set; } = GameState.Idle;
        public int Score { get; private set; }
        public float ScoreMultiplier { get; private set; } = 1f;
        public bool BrakeUsed { get; private set; }

        private float _scoreTimer;

        void Start()
        {
            _instructionPanel.Show(
                "047v2",
                "SpinBalance",
                "コマが落ちないように盤面をドラッグして回転させよう",
                "左右にドラッグで回転 / ダブルクリックで緊急ブレーキ",
                "制限時間が終わるまでコマを盤面上に保持し続けよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void OnDestroy()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
        }

        void StartGame()
        {
            State = GameState.Playing;
            Score = 0;
            BrakeUsed = false;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            State = GameState.Playing;
            int stageNumber = stageIndex + 1;
            _balanceManager.SetupStage(_stageManager.GetCurrentStageConfig(), stageNumber);
            _ui.UpdateStage(stageNumber, 5);
        }

        void OnAllStagesCleared()
        {
            State = GameState.AllClear;
            _balanceManager.StopGame();
            _ui.ShowAllClear(Score);
        }

        void Update()
        {
            if (State != GameState.Playing) return;

            _scoreTimer += Time.deltaTime;
            if (_scoreTimer >= 1f)
            {
                _scoreTimer -= 1f;
                AddScore(10); // base points; AddScore applies ScoreMultiplier internally
            }

            _ui.UpdateScore(Score, ScoreMultiplier);
        }

        public void AddScore(int points)
        {
            Score += Mathf.RoundToInt(points * ScoreMultiplier);
            _ui.UpdateScore(Score, ScoreMultiplier);
        }

        public void UpdateCoinCount(int current, int max)
        {
            if (State != GameState.Playing) return;

            // Update multiplier based on coin count
            if (current >= 8)
                ScoreMultiplier = 3f;
            else if (current >= 5)
                ScoreMultiplier = 2f;
            else
                ScoreMultiplier = 1f;

            _ui.UpdateCoinCount(current, max);
            _ui.UpdateMultiplier(ScoreMultiplier);

            // Coin added bonus (flat, not multiplied)
            if (current > 0)
                Score += current * 100;
        }

        public void NotifyBrakeUsed()
        {
            BrakeUsed = true;
        }

        public void TriggerGameOver()
        {
            if (State != GameState.Playing) return;
            State = GameState.GameOver;
            _balanceManager.StopGame();
            _ui.ShowGameOver(Score);
        }

        public void TriggerStageClear()
        {
            if (State != GameState.Playing) return;
            State = GameState.StageClear;

            int bonus = _stageManager.CurrentStage * 500;
            if (!BrakeUsed) bonus += 500;
            AddScore(bonus);

            _balanceManager.StopGame();
            _ui.ShowStageClear(Score);
        }

        public void GoNextStage()
        {
            BrakeUsed = false;
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
