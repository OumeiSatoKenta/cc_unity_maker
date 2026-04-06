using UnityEngine;
using System;

namespace Game046v2_SqueezePop
{
    public class SqueezePopGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BalloonManager _balloonManager;
        [SerializeField] SqueezePopUI _ui;

        public enum GameState { Idle, Playing, StageClear, Clear, GameOver }

        public GameState State { get; private set; } = GameState.Idle;
        public int Score { get; private set; }
        public int ComboCount { get; private set; }
        public bool AllPerfect { get; private set; }

        private static readonly int[] TargetCounts = { 8, 12, 16, 20, 25 };
        private static readonly float[] TimeLimits = { 40f, 40f, 35f, 35f, 30f };
        private static readonly float[] InflateSpeeds = { 0.7f, 0.8f, 0.9f, 1.0f, 1.1f };
        private static readonly float[] ExplodeTimes = { 1.5f, 1.3f, 1.2f, 1.1f, 1.0f };
        private static readonly int[] BombCounts = { 0, 0, 0, 3, 3 };

        private int _currentStageIndex;
        private float _timeLeft;
        private int _totalPops;
        private int _perfectPops;

        private void Start()
        {
            _instructionPanel.Show(
                "046v2_SqueezePop",
                "SqueezePop",
                "風船を長押しで膨らませてポップさせよう",
                "長押しで膨らませ、指を離してポップ！膨らませすぎると破裂！",
                "全ての風船をポップさせてステージクリア！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        private void StartGame()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        private void OnStageChanged(int stageIndex)
        {
            _currentStageIndex = stageIndex;
            State = GameState.Playing;
            _totalPops = 0;
            _perfectPops = 0;
            AllPerfect = false;

            int count = stageIndex < TargetCounts.Length ? TargetCounts[stageIndex] : 25;
            float timeLimit = stageIndex < TimeLimits.Length ? TimeLimits[stageIndex] : 30f;
            float inflateSpeed = stageIndex < InflateSpeeds.Length ? InflateSpeeds[stageIndex] : 1.1f;
            float explodeTime = stageIndex < ExplodeTimes.Length ? ExplodeTimes[stageIndex] : 1.0f;
            int bombCount = stageIndex < BombCounts.Length ? BombCounts[stageIndex] : 3;
            bool hasMoving = stageIndex >= 4;
            bool hasSizeVariety = stageIndex >= 1;
            bool hasChain = stageIndex >= 2;

            _timeLeft = timeLimit;
            _balloonManager.SetupStage(count, inflateSpeed, explodeTime, bombCount, hasMoving, hasSizeVariety, hasChain);
            _ui.SetupStage(stageIndex + 1, _stageManager.TotalStages, count, timeLimit);
        }

        private void Update()
        {
            if (State != GameState.Playing) return;

            _timeLeft = Mathf.Max(_timeLeft - Time.deltaTime, 0f);
            _ui.UpdateHUD(_timeLeft, Score, ComboCount, _balloonManager.RemainingCount);

            if (_timeLeft <= 0f)
            {
                if (_balloonManager.RemainingCount <= 0)
                    StageClear();
                else
                    TriggerGameOver();
            }
        }

        public void OnBalloonPopped(bool isPerfect, bool isBomb)
        {
            if (State != GameState.Playing) return;

            _totalPops++;
            if (isPerfect) _perfectPops++;

            if (isBomb)
            {
                // 爆弾起因のポップはコンボ・スコアに影響しない（残数チェックのみ）
                if (_balloonManager.RemainingCount <= 0 && State == GameState.Playing) StageClear();
                return;
            }

            if (!isPerfect) ComboCount = 0;
            else ComboCount++;

            float multiplier = ComboCount >= 4 ? 2.0f : (ComboCount >= 2 ? 1.5f : 1.0f);
            int baseScore = isPerfect ? 300 : 100;
            Score += Mathf.RoundToInt(baseScore * multiplier);

            _ui.ShowComboEffect(ComboCount, multiplier);

            if (_balloonManager.RemainingCount <= 0 && State == GameState.Playing)
                StageClear();
        }

        public void OnBalloonFailed()
        {
            if (State != GameState.Playing) return;
            ComboCount = 0;
        }

        private void StageClear()
        {
            if (State != GameState.Playing) return;
            State = GameState.StageClear;

            int timeBonus = Mathf.RoundToInt(Mathf.Max(_timeLeft, 0f) * 50f);
            Score += timeBonus;

            AllPerfect = _totalPops > 0 && _perfectPops == _totalPops;
            if (AllPerfect) Score = Mathf.RoundToInt(Score * 3f);

            _ui.ShowStageClear(Score, AllPerfect, timeBonus);
        }

        public void OnNextStageButton()
        {
            if (State != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        private void OnDestroy()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
        }

        private void OnAllStagesCleared()
        {
            State = GameState.Clear;
            _ui.ShowAllClear(Score);
        }

        private void TriggerGameOver()
        {
            if (State != GameState.Playing) return;
            State = GameState.GameOver;
            _balloonManager.StopAll();
            _ui.ShowGameOver(Score);
        }
    }
}
