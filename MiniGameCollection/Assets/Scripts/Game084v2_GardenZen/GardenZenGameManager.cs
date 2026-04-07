using UnityEngine;

namespace Game084v2_GardenZen
{
    public class GardenZenGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] GardenManager _gardenManager;
        [SerializeField] GardenZenUI _ui;

        int _consecutiveStar3Count;

        void Start()
        {
            _instructionPanel.Show(
                "084",
                "GardenZen",
                "禅の庭をデザインして心を整えよう",
                "石・植物を選んでグリッドに配置\n砂をなぞって砂紋を描こう",
                "依頼通りの庭をデザインして★3評価を獲得しよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _consecutiveStar3Count = 0;
            _gardenManager.ResetScore();
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _gardenManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _gardenManager.SetActive(false);
            _ui.ShowAllClear(_gardenManager.TotalScore);
        }

        public void OnStageClear(int stars)
        {
            if (stars == 3)
                _consecutiveStar3Count++;
            else
                _consecutiveStar3Count = 0;

            _gardenManager.SetComboMultiplier(_consecutiveStar3Count >= 2 ? 1.5f : 1.0f);
            _ui.ShowStageClear(_stageManager.CurrentStage + 1, stars);
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateMatchRateDisplay(float rate) => _ui.UpdateMatchRate(rate);

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
