using UnityEngine;

namespace Game044v2_TiltMaze
{
    public enum TiltMazeState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class TiltMazeGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] TiltMazeMechanic _mechanic;
        [SerializeField] TiltMazeUI _ui;

        TiltMazeState _state;
        int _totalScore;
        int _currentStage;
        int _life;
        int _coinsCollected;
        int _totalCoins;
        bool _noMiss;
        float _comboMultiplier;

        static readonly float[] StageComboMultipliers = { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f };
        static readonly int[] StageTimes = { 60, 50, 45, 40, 35 };
        static readonly int[] StageMisses = { 3, 3, 3, 3, 3 };

        public TiltMazeState State => _state;
        public int TotalScore => _totalScore;
        public int CurrentStage => _currentStage;
        public int Life => _life;
        public int CoinsCollected => _coinsCollected;
        public int TotalCoins => _totalCoins;

        void Start()
        {
            _state = TiltMazeState.WaitingInstruction;
            _instructionPanel.Show(
                "044v2",
                "TiltMaze",
                "迷路を傾けてボールをゴールへ転がそう",
                "画面をドラッグして迷路を傾ける。長押しでブレーキ",
                "穴に落ちずにボールをゴールまで届けよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
            if (_instructionPanel != null)
                _instructionPanel.OnDismissed -= StartGame;
        }

        void StartGame()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _totalScore = 0;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            _currentStage = stage;
            int clampedStage = Mathf.Clamp(stage, 0, StageMisses.Length - 1);
            _life = StageMisses[clampedStage];
            _noMiss = true;
            _coinsCollected = 0;
            _totalCoins = 0;
            _comboMultiplier = StageComboMultipliers[Mathf.Clamp(stage, 0, StageComboMultipliers.Length - 1)];
            _ui.HideAllPanels();
            _state = TiltMazeState.Playing;

            int stageTime = StageTimes[clampedStage];
            _mechanic.SetupStage(stage, stageTime);
            _ui.UpdateStage(stage + 1);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateLife(_life);
            _ui.UpdateTimer(stageTime);
            _ui.UpdateCoins(0, 0);
            _ui.UpdateBrakeGauge(1f);
        }

        void OnAllStagesCleared()
        {
            _state = TiltMazeState.Clear;
            _mechanic.SetActive(false);
            _ui.ShowFinalClearPanel(_totalScore);
        }

        public void SetTotalCoins(int total)
        {
            _totalCoins = total;
            _ui.UpdateCoins(_coinsCollected, _totalCoins);
        }

        public void OnCoinCollected()
        {
            if (_state != TiltMazeState.Playing) return;
            _coinsCollected++;
            _ui.UpdateCoins(_coinsCollected, _totalCoins);
        }

        public void OnTimerUpdated(float remaining)
        {
            if (_state != TiltMazeState.Playing) return;
            _ui.UpdateTimer(remaining);
        }

        public void OnBrakeGaugeUpdated(float gauge)
        {
            if (_state != TiltMazeState.Playing) return;
            _ui.UpdateBrakeGauge(gauge);
        }

        public void OnBallFell()
        {
            if (_state != TiltMazeState.Playing) return;
            _noMiss = false;
            _life--;
            _ui.UpdateLife(_life);
            if (_life <= 0)
            {
                OnGameOver();
            }
        }

        public void OnGoalReached(float remainingTime)
        {
            if (_state != TiltMazeState.Playing) return;
            _state = TiltMazeState.StageClear;
            _mechanic.SetActive(false);

            int baseScore = 500;
            int timeBonus = Mathf.RoundToInt(remainingTime * 100f);
            int coinBonus = _coinsCollected * 200;
            int allCoinBonus = (_coinsCollected == _totalCoins && _totalCoins > 0) ? 1 : 0;
            int noMissBonus = _noMiss ? 1000 : 0;
            int stageTotalScore = Mathf.RoundToInt((baseScore + timeBonus + coinBonus + noMissBonus) * _comboMultiplier);
            if (allCoinBonus > 0) stageTotalScore *= 2;
            _totalScore += stageTotalScore;

            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClearPanel(_totalScore, _noMiss, _coinsCollected == _totalCoins && _totalCoins > 0);
        }

        public void OnTimeUp()
        {
            if (_state != TiltMazeState.Playing) return;
            OnGameOver();
        }

        void OnGameOver()
        {
            _state = TiltMazeState.GameOver;
            _mechanic.SetActive(false);
            _ui.ShowGameOverPanel(_totalScore);
        }

        public void AdvanceToNextStage()
        {
            if (_state != TiltMazeState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void RetryGame()
        {
            if (_state != TiltMazeState.GameOver && _state != TiltMazeState.Clear) return;
            _totalScore = 0;
            _stageManager.StartFromBeginning();
        }

        public void ReturnToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
