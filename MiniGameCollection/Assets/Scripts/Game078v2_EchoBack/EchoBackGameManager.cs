using UnityEngine;

namespace Game078v2_EchoBack
{
    public class EchoBackGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] EchoManager _echoManager;
        [SerializeField] EchoBackUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "078",
                "EchoBack",
                "鳴り響くメロディを記憶して、同じパターンを鍵盤で再現する音楽記憶ゲーム",
                "メロディを聴いて鍵盤をタップ！同じ音を同じリズムで入力しよう。リプレイボタンでもう一度聴ける",
                "5ステージのパターンを完璧に再現してマスターエコーを目指せ！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _echoManager.ResetScore();
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _echoManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _echoManager.SetActive(false);
            _ui.ShowAllClear(_echoManager.TotalScore);
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
            _echoManager.SetActive(false);
            _ui.ShowGameOver(_echoManager.TotalScore);
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void ShowJudgement(string text, Color color) => _ui.ShowJudgement(text, color);
        public void UpdatePhase(string phase) => _ui.UpdatePhase(phase);
        public void UpdateReplayCount(int remaining) => _ui.UpdateReplayCount(remaining);
        public void UpdateProgressDots(int current, int total) => _ui.UpdateProgressDots(current, total);

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
