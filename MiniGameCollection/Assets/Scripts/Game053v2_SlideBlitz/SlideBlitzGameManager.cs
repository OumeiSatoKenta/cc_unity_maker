using UnityEngine;

namespace Game053v2_SlideBlitz
{
    /// <summary>
    /// SlideBlitz ゲーム全体の状態管理。
    /// StageManager・InstructionPanel と連携する。
    /// </summary>
    public class SlideBlitzGameManager : MonoBehaviour
    {
        [SerializeField] private StageManager _stageManager;
        [SerializeField] private InstructionPanel _instructionPanel;
        [SerializeField] private SlideManager _slideManager;
        [SerializeField] private SlideBlitzUI _ui;

        // ステージごとのパラメータ
        private static readonly float[] TimeLimits = { 60f, 45f, 90f, 90f, 120f };
        private static readonly int[] GridSizes = { 3, 3, 4, 4, 5 };
        private static readonly int[] ShuffleCounts = { 20, 40, 50, 50, 60 };
        private static readonly float[] FrozenFactors = { 0f, 0f, 0f, 0.13f, 0f };

        private float _timeRemaining;
        private bool _isPlaying;
        private int _totalScore;
        private int _currentStageIndex;

        private void Start()
        {
            // カスタムステージ設定
            var configs = new StageManager.StageConfig[5];
            for (int i = 0; i < 5; i++)
            {
                configs[i] = new StageManager.StageConfig
                {
                    speedMultiplier = ShuffleCounts[i] / 20f,
                    countMultiplier = GridSizes[i],
                    complexityFactor = FrozenFactors[i],
                    stageName = $"Stage {i + 1}"
                };
            }
            _stageManager.SetConfigs(configs);
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;

            _slideManager.OnPuzzleSolved += OnPuzzleSolved;
            _slideManager.OnMoveCountChanged += moves => _ui.UpdateMoves(moves);
            _slideManager.OnComboChanged += combo => _ui.UpdateCombo(combo);

            _instructionPanel.Show(
                "053",
                "SlideBlitz",
                "タイルをスライドさせて数字を順番に並べよう！",
                "タイルをタップして空きマスへスワイプ。数字1から順に並べよう",
                "全タイルを正しい位置に並べてステージクリア！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        private void StartGame()
        {
            _totalScore = 0;
            _stageManager.StartFromBeginning();
        }

        private void OnStageChanged(int stage)
        {
            _currentStageIndex = stage;
            var config = _stageManager.GetCurrentStageConfig();
            _slideManager.SetupStage(config);
            _timeRemaining = TimeLimits[Mathf.Clamp(stage, 0, TimeLimits.Length - 1)];
            _isPlaying = true;
            _ui.UpdateStage(stage + 1, _stageManager.TotalStages);
            _ui.UpdateTimer(_timeRemaining);
            _ui.UpdateMoves(0);
            _ui.HideStageClear();
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _timeRemaining -= Time.deltaTime;
            _ui.UpdateTimer(Mathf.Max(0f, _timeRemaining));

            if (_timeRemaining <= 0f)
            {
                _isPlaying = false;
                _slideManager.SetActive(false);
                _ui.ShowGameOver(_totalScore);
            }
        }

        private void OnPuzzleSolved()
        {
            _isPlaying = false;
            _slideManager.SetActive(false);

            // スコア計算
            float remaining = Mathf.Max(0f, _timeRemaining);
            float combo = _slideManager.GetComboMultiplier();
            float timeLimit = TimeLimits[Mathf.Clamp(_currentStageIndex, 0, TimeLimits.Length - 1)];

            float score = remaining * 100f * combo;
            // 効率ボーナス: 手数が最適の1.5倍以内（概算: gridSize^2）
            int optimal = _slideManager.GridSize * _slideManager.GridSize;
            if (_slideManager.MoveCount <= optimal * 1.5f) score *= 2f;
            // スピードボーナス
            if (remaining >= timeLimit * 0.5f) score *= 1.5f;

            int stageScore = Mathf.RoundToInt(score);
            _totalScore += stageScore;

            _ui.ShowStageClear(stageScore, _totalScore);
        }

        public void NextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void RetryStage()
        {
            var config = _stageManager.GetCurrentStageConfig();
            _slideManager.SetupStage(config);
            _timeRemaining = TimeLimits[Mathf.Clamp(_currentStageIndex, 0, TimeLimits.Length - 1)];
            _isPlaying = true;
            _ui.UpdateTimer(_timeRemaining);
            _ui.UpdateMoves(0);
            _ui.HideGameOver();
        }

        private void OnAllStagesCleared()
        {
            _ui.ShowAllClear(_totalScore);
        }

        public void ShowInstruction()
        {
            _isPlaying = false;
            _slideManager.SetActive(false);
            _instructionPanel.OnDismissed -= ResumeAfterInstruction;
            _instructionPanel.Show(
                "053",
                "SlideBlitz",
                "タイルをスライドさせて数字を順番に並べよう！",
                "タイルをタップして空きマスへスワイプ。数字1から順に並べよう",
                "全タイルを正しい位置に並べてステージクリア！"
            );
            _instructionPanel.OnDismissed += ResumeAfterInstruction;
        }

        private void ResumeAfterInstruction()
        {
            _instructionPanel.OnDismissed -= ResumeAfterInstruction;
            if (!_isPlaying)
            {
                _isPlaying = true;
                _slideManager.SetActive(true);
            }
        }
    }
}
