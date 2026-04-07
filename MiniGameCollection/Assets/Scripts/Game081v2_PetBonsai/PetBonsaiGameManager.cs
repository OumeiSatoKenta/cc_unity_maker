using UnityEngine;

namespace Game081v2_PetBonsai
{
    public class PetBonsaiGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] PetBonsaiManager _bonsaiManager;
        [SerializeField] PetBonsaiUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "081",
                "PetBonsai",
                "盆栽を育てて品評会で優勝しよう",
                "タップで水やり、枝をタップして剪定",
                "美しい盆栽を育てて品評会で★3評価を目指そう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _bonsaiManager.ResetScore();
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _bonsaiManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _bonsaiManager.SetActive(false);
            _ui.ShowAllClear(_bonsaiManager.TotalScore);
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
            _bonsaiManager.SetActive(false);
            _ui.ShowGameOver(_bonsaiManager.TotalScore);
        }

        public void UpdateBeautyDisplay(int beauty) => _ui.UpdateBeautyScore(beauty);
        public void UpdateGrowthDisplay(float ratio) => _ui.UpdateGrowth(ratio);
        public void UpdateWaterDisplay(int current, int max) => _ui.UpdateWater(current, max);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void UpdateSeasonDisplay(string season) => _ui.UpdateSeason(season);
        public void ShowFeedback(string text, Color color) => _ui.ShowFeedback(text, color);
        public void UpdateRivalScore(int rival) => _ui.ShowRivalScore(rival);

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
