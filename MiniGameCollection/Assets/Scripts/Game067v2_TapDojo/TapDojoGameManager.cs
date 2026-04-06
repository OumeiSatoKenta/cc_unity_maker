using UnityEngine;
using Common;

namespace Game067v2_TapDojo
{
    public class TapDojoGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] DojoManager _dojoManager;
        [SerializeField] TapDojoUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "067",
                "TapDojo",
                "タップで修行して最強の武道家を目指そう",
                "道場をタップして修行、ボタンで技習得・大会参加",
                "段位目標の修行ポイントを達成してステージクリア"
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
            var config = _stageManager.GetCurrentConfig();
            _dojoManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _dojoManager.SetActive(false);
            _ui.ShowAllClear(_dojoManager.TotalScore);
        }

        public void OnStageClear()
        {
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void NextStage()
        {
            _stageManager.AdvanceStage();
        }

        public void UpdateMPDisplay(long mp, long target)
        {
            _ui.UpdateMP(mp, target);
        }

        public void UpdateComboDisplay(int combo, float multiplier)
        {
            _ui.UpdateCombo(combo, multiplier);
        }

        public void UpdateAutoRateDisplay(float rate)
        {
            _ui.UpdateAutoRate(rate);
        }

        public void UpdateRankDisplay(string rankName)
        {
            _ui.UpdateRank(rankName);
        }

        public void UpdateTechButtons(bool[] unlocked, bool[] affordable, bool autoUnlocked, bool tournamentUnlocked, bool trainingUnlocked, bool shihanTestUnlocked)
        {
            _ui.UpdateTechButtons(unlocked, affordable, autoUnlocked, tournamentUnlocked, trainingUnlocked, shihanTestUnlocked);
        }

        public void UpdateTrainingTimer(bool active, float remaining, int taps, int goal)
        {
            _ui.UpdateTrainingTimer(active, remaining, taps, goal);
        }

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
