using UnityEngine;

namespace Game048v2_GlassBall
{
    public enum GameState { Idle, Drawing, Rolling, StageClear, AllClear, GameOver }

    public class GlassBallGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] RailManager _railManager;
        [SerializeField] GlassBallController _ballController;
        [SerializeField] GlassBallUI _ui;

        public GameState State { get; private set; } = GameState.Idle;
        public int Score { get; private set; }

        // Stage config cache
        private int _currentStageIndex;
        private int _coinCount;
        private int _totalCoins;
        private bool _allCoinsCollected;

        void Start()
        {
            _instructionPanel.Show(
                "048v2",
                "GlassBall",
                "ガラスのボールをゴールまで誘導しよう",
                "ドラッグでレールを描いて「発射」ボタンを押そう / ダブルクリックでレールリセット",
                "衝撃を与えずにガラスボールをゴールまで届けよう"
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
            State = GameState.Drawing;
            Score = 0;
            _coinCount = 0;
            _allCoinsCollected = false;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            State = GameState.Drawing;
            _currentStageIndex = stageIndex;
            _coinCount = 0;
            _allCoinsCollected = false;

            int stageNumber = stageIndex + 1;
            var config = _stageManager.GetCurrentStageConfig();
            _totalCoins = GetCoinCountForStage(stageNumber);

            _railManager.SetupStage(config, stageNumber);
            _ballController.SetupStage(config, stageNumber);
            _ui.UpdateStage(stageNumber, 5);
            _ui.UpdateCoinCount(_coinCount, _totalCoins);
            _ui.UpdateScore(Score);
        }

        void OnAllStagesCleared()
        {
            State = GameState.AllClear;
            _ui.ShowAllClear(Score);
        }

        int GetCoinCountForStage(int stage)
        {
            switch (stage)
            {
                case 1: return 0;
                case 2: return 3;
                case 3: return 4;
                case 4: return 5;
                case 5: return 6;
                default: return 0;
            }
        }

        public void OnBallLaunched()
        {
            if (State != GameState.Drawing) return;
            State = GameState.Rolling;
        }

        public void OnCoinCollected()
        {
            if (State != GameState.Rolling) return;
            _coinCount++;
            Score += 200;
            _allCoinsCollected = (_totalCoins > 0 && _coinCount >= _totalCoins);
            _ui.UpdateCoinCount(_coinCount, _totalCoins);
            _ui.UpdateScore(Score);
        }

        public void TriggerStageClear(float impactPercent, float inkPercent)
        {
            if (State != GameState.Rolling) return;
            State = GameState.StageClear;

            int bonus = 500;
            bonus += Mathf.RoundToInt((100f - impactPercent) * 30f);
            bonus += Mathf.RoundToInt(inkPercent * 20f);
            if (impactPercent <= 0f) bonus += 1500;
            if (_allCoinsCollected) bonus *= 2;

            Score += bonus;
            _ui.UpdateScore(Score);
            _ui.ShowStageClear(Score);
        }

        public void TriggerGameOver()
        {
            if (State != GameState.Rolling && State != GameState.Drawing) return;
            State = GameState.GameOver;
            _ui.ShowGameOver(Score);
        }

        public void GoNextStage()
        {
            if (State != GameState.StageClear) return;
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
