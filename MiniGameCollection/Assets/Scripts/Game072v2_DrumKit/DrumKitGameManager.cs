using UnityEngine;
using Common;

namespace Game072v2_DrumKit
{
    public class DrumKitGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] DrumPadManager _drumPadManager;
        [SerializeField] DrumKitUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "072",
                "DrumKit",
                "ドラムセットをリズムよくタップしよう",
                "リングが縮んでパッドに重なったらタップ！",
                "5ステージを全てクリアしてドラムマスターになろう"
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
            _drumPadManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _drumPadManager.SetActive(false);
            _ui.ShowAllClear(_drumPadManager.TotalScore);
        }

        public void OnStageClear()
        {
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void NextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void OnGameOver()
        {
            _ui.ShowGameOver(_drumPadManager.TotalScore);
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void UpdateMissDisplay(int miss, int maxMiss) => _ui.UpdateMiss(miss, maxMiss);
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
