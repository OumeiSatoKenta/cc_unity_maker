using UnityEngine;

namespace Game077v2_BeatRunner
{
    public class BeatRunnerGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BeatManager _beatManager;
        [SerializeField] BeatRunnerUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "077",
                "BeatRunner",
                "ビートに乗って走り抜けろ！リズムゲームランナー",
                "画面上半分タップでジャンプ、下半分タップでスライド。ビートに合わせてアクション！",
                "楽曲終了まで走り切ろう。Perfect連続でスピードアップ！コンボでハイスコアを目指せ！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _beatManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _beatManager.SetActive(false);
            _ui.ShowAllClear(_beatManager.TotalScore);
        }

        public void OnStageClear()
        {
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void OnGameOver()
        {
            _beatManager.SetActive(false);
            _ui.ShowGameOver(_beatManager.TotalScore);
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void UpdateLifeDisplay(int life) => _ui.UpdateLife(life);
        public void UpdateProgressDisplay(int current, int total) => _ui.UpdateProgress(current, total);
        public void ShowJudgement(string text, Color color) => _ui.ShowJudgement(text, color);

        void OnDestroy()
        {
            if (_instructionPanel != null) _instructionPanel.OnDismissed -= StartGame;
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
        }
    }
}
