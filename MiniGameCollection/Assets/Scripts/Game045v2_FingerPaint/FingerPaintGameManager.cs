using UnityEngine;
using System;

namespace Game045v2_FingerPaint
{
    public class FingerPaintGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] FingerPaintCanvas _canvas;
        [SerializeField] FingerPaintUI _ui;

        public enum GameState { Idle, Playing, StageClear, Clear, GameOver }

        public GameState State { get; private set; } = GameState.Idle;
        public int Score { get; private set; }
        public int ComboCount { get; private set; }
        public float ComboMultiplier => ComboCount >= 3 ? 1.5f : 1.0f;

        // Stage timing
        private static readonly float[] StageTimes = { 60f, 60f, 55f, 50f, 45f };
        private static readonly float[] StageInkAmounts = { 1.0f, 0.9f, 0.85f, 0.8f, 0.75f };
        private static readonly float[] StageTargetMatch = { 0.50f, 0.55f, 0.60f, 0.65f, 0.70f };

        private float _timeLeft;
        private int _currentStageIndex;

        private void Start()
        {
            _instructionPanel.Show(
                "045v2_FingerPaint",
                "FingerPaint",
                "お手本に合わせて指でキャンバスに絵を描こう",
                "ドラッグで描く・パレットで色を選ぶ・ダブルタップでお手本表示切替",
                "制限時間内にお手本との一致率を目標値以上にしてクリア！"
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
            Score = 0;
            ComboCount = 0;

            float timeLimit = stageIndex < StageTimes.Length ? StageTimes[stageIndex] : 45f;
            float inkAmount = stageIndex < StageInkAmounts.Length ? StageInkAmounts[stageIndex] : 0.75f;
            float targetMatch = stageIndex < StageTargetMatch.Length ? StageTargetMatch[stageIndex] : 0.70f;

            _canvas.SetupStage(_stageManager.GetCurrentStageConfig(), stageIndex, inkAmount);
            _ui.SetupStage(stageIndex + 1, _stageManager.TotalStages, targetMatch, timeLimit);
            _timeLeft = timeLimit;
        }

        private void Update()
        {
            if (State != GameState.Playing) return;

            _timeLeft -= Time.deltaTime;
            float matchRate = _canvas.GetMatchRate();
            float ink = _canvas.GetInkAmount();

            _ui.UpdateHUD(matchRate, ink, _timeLeft);

            float target = _currentStageIndex < StageTargetMatch.Length ? StageTargetMatch[_currentStageIndex] : 0.70f;

            if (ink <= 0f || _timeLeft <= 0f)
            {
                if (matchRate >= target)
                    StageClear(matchRate, ink, Mathf.Max(_timeLeft, 0f));
                else
                    TriggerGameOver();
            }
        }

        public void AddCombo()
        {
            ComboCount++;
            _ui.ShowCombo(ComboCount);
        }

        public void ResetCombo()
        {
            ComboCount = 0;
            _ui.ShowCombo(0);
        }

        public void OnStageClearButtonPressed()
        {
            if (State != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        private void StageClear(float matchRate, float inkRemaining, float timeRemaining)
        {
            State = GameState.StageClear;
            int baseScore = Mathf.RoundToInt(matchRate * 100f * 100f);
            int inkBonus = Mathf.RoundToInt(inkRemaining * 100f * 50f);
            int timeBonus = Mathf.RoundToInt(timeRemaining * 30f);
            int comboBonus = Mathf.RoundToInt(baseScore * (ComboMultiplier - 1f));
            int perfectBonus = matchRate >= 0.95f ? 2000 : 0;
            Score = baseScore + inkBonus + timeBonus + comboBonus + perfectBonus;

            int stars = matchRate >= 0.9f ? 3 : matchRate >= 0.7f ? 2 : 1;
            _ui.ShowStageClear(Score, stars);
        }

        private void OnAllStagesCleared()
        {
            State = GameState.Clear;
            if (_canvas != null) _canvas.SetActive(false);
            _ui.ShowFinalClear(Score);
        }

        private void TriggerGameOver()
        {
            State = GameState.GameOver;
            if (_canvas != null) _canvas.SetActive(false);
            _ui.ShowGameOver(_canvas.GetMatchRate());
        }

        private void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
            if (_instructionPanel != null)
                _instructionPanel.OnDismissed -= StartGame;
        }
    }
}
