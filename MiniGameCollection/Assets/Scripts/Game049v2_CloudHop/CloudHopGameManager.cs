using UnityEngine;

namespace Game049v2_CloudHop
{
    public enum GameState { Idle, Playing, StageClear, AllClear, GameOver }

    public class CloudHopGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] CloudHopController _controller;
        [SerializeField] CloudSpawner _spawner;
        [SerializeField] CloudHopUI _ui;

        public GameState State { get; private set; } = GameState.Idle;
        public int Score { get; private set; }
        public float CurrentAltitude { get; private set; }
        public float TargetAltitude { get; private set; }

        private int _comboCount;
        private int _comboMultiplier = 1;
        private int _coinCount;

        void Start()
        {
            _instructionPanel.Show(
                "049v2",
                "CloudHop",
                "消える雲を踏み台にして空高く跳ね上がろう",
                "タップでジャンプ！左右ドラッグで方向調整。下スワイプで急降下できるよ",
                "目標高度に到達してステージクリア！"
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
            State = GameState.Playing;
            Score = 0;
            CurrentAltitude = 0f;
            _comboCount = 0;
            _comboMultiplier = 1;
            _coinCount = 0;

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            State = GameState.Playing;
            _comboCount = 0;
            _comboMultiplier = 1;
            _coinCount = 0;
            CurrentAltitude = 0f;

            int stageNumber = stageIndex + 1;
            var config = _stageManager.GetCurrentStageConfig();
            TargetAltitude = GetTargetAltitude(stageNumber);

            _spawner.SetupStage(config, stageNumber);
            _controller.SetupStage(config, stageNumber);
            _ui.UpdateStage(stageNumber, 5);
            _ui.UpdateAltitude(0f, TargetAltitude);
            _ui.UpdateScore(Score);
            _ui.UpdateCombo(0);
            _ui.HideStageClear();
        }

        void OnAllStagesCleared()
        {
            State = GameState.AllClear;
            _ui.ShowAllClear(Score);
        }

        float GetTargetAltitude(int stage)
        {
            switch (stage)
            {
                case 1: return 100f;
                case 2: return 200f;
                case 3: return 350f;
                case 4: return 500f;
                case 5: return 700f;
                default: return 100f;
            }
        }

        void Update()
        {
            if (State != GameState.Playing) return;

            float prev = CurrentAltitude;
            CurrentAltitude = _controller.GetAltitude();
            float delta = CurrentAltitude - prev;
            if (delta > 0f)
            {
                int altScore = Mathf.RoundToInt(delta * 10f * _comboMultiplier);
                Score += altScore;
                _ui.UpdateScore(Score);
                _ui.UpdateAltitude(CurrentAltitude, TargetAltitude);
            }

            if (CurrentAltitude >= TargetAltitude)
            {
                TriggerStageClear();
            }
        }

        public void OnCloudLanded(CloudType cloudType)
        {
            if (State != GameState.Playing) return;

            if (cloudType == CloudType.Thunder)
            {
                // Thunder cloud: stun, reset combo
                _comboCount = 0;
                _comboMultiplier = 1;
                _ui.UpdateCombo(0);
                _controller.TriggerStun();
                return;
            }

            _comboCount++;
            UpdateComboMultiplier();
            _ui.UpdateCombo(_comboCount);

            if (cloudType == CloudType.Spring)
            {
                Score += Mathf.RoundToInt(300 * _comboMultiplier);
                _ui.UpdateScore(Score);
                _ui.ShowBonusText("+300 SPRING!", Color.yellow);
            }
        }

        public void OnQuickDrop()
        {
            // Quick drop resets combo
            _comboCount = 0;
            _comboMultiplier = 1;
            _ui.UpdateCombo(0);
        }

        public void OnCoinCollected()
        {
            if (State != GameState.Playing) return;
            _coinCount++;
            int pts = Mathf.RoundToInt(100 * _comboMultiplier);
            Score += pts;
            _ui.UpdateScore(Score);
            _ui.ShowBonusText("+" + pts + " COIN!", Color.yellow);
        }

        void UpdateComboMultiplier()
        {
            if (_comboCount >= 10) _comboMultiplier = 5;
            else if (_comboCount >= 5) _comboMultiplier = 3;
            else if (_comboCount >= 3) _comboMultiplier = 2;
            else _comboMultiplier = 1;
        }

        void TriggerStageClear()
        {
            if (State != GameState.Playing) return;
            State = GameState.StageClear;

            int bonus = 500;
            float excess = Mathf.Max(0f, CurrentAltitude - TargetAltitude);
            bonus += Mathf.RoundToInt(excess * 20f);
            bonus += _coinCount * 50;
            Score += bonus;

            _ui.UpdateScore(Score);
            _ui.ShowStageClear(Score);
        }

        public void TriggerGameOver()
        {
            if (State != GameState.Playing) return;
            State = GameState.GameOver;
            _controller.SetActive(false);
            _spawner.SetActive(false);
            _ui.ShowGameOver(Score);
        }

        public void GoNextStage()
        {
            if (State != GameState.StageClear) return;
            _controller.ResetForNewStage();
            _spawner.ClearClouds();
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
