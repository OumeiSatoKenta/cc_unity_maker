using UnityEngine;

namespace Game079v2_SilentBeat
{
    public class SilentBeatGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] RhythmManager _rhythmManager;
        [SerializeField] SilentBeatUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "079",
                "SilentBeat",
                "無音の画面でリズムを感じ取り正確なタイミングでタップ",
                "ガイド拍を聞いて覚え、無音になったら同じリズムでタップし続けよう！Perfect判定でコンボが繋がり高得点！",
                "5ステージのリズムをマスターして完全内部時計を目指せ！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _rhythmManager.ResetScore();
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _rhythmManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _rhythmManager.SetActive(false);
            _ui.ShowAllClear(_rhythmManager.TotalScore);
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
            _rhythmManager.SetActive(false);
            _ui.ShowGameOver(_rhythmManager.TotalScore);
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void ShowJudgement(string text, Color color) => _ui.ShowJudgement(text, color);
        public void UpdateProgress(int current, int total) => _ui.UpdateProgress(current, total);
        public void UpdateBpm(float bpm) => _ui.UpdateBpm(bpm);
        public void ShowGuidePhase() => _ui.ShowGuidePhase();
        public void HideGuidePhase() => _ui.HideGuidePhase();
        public void UpdateAccuracyIndicator(float deviation) => _ui.UpdateAccuracyIndicator(deviation);
        public void FlashTapArea(bool isActive) => _ui.FlashTapArea(isActive);

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
