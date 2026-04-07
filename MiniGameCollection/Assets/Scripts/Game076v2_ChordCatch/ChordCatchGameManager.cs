using UnityEngine;

namespace Game076v2_ChordCatch
{
    public class ChordCatchGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] ChordController _chordController;
        [SerializeField] ChordCatchUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "076",
                "ChordCatch",
                "和音を聴いて正しいコードをタップしよう！",
                "鳴った和音に対応するコードボタンをタップ。リプレイボタンでもう一度聴けるよ",
                "全問回答して正解率50%以上を達成しよう。コンボを繋げば高スコア！"
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
            _chordController.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _chordController.SetActive(false);
            _ui.ShowAllClear(_chordController.TotalScore);
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
            _chordController.SetActive(false);
            _ui.ShowGameOver(_chordController.TotalScore);
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void UpdateMissDisplay(int miss) => _ui.UpdateMiss(miss);
        public void UpdateProgressDisplay(int current, int total) => _ui.UpdateProgress(current, total);
        public void ShowJudgement(string text, Color color) => _ui.ShowJudgement(text, color);
        public void SetBeatActive(bool active) => _ui.SetBeatActive(active);

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
