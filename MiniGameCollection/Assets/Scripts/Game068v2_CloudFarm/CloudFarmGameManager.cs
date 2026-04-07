using UnityEngine;

namespace Game068v2_CloudFarm
{
    public class CloudFarmGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] FarmManager _farmManager;
        [SerializeField] CloudFarmUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "068",
                "CloudFarm",
                "雲の上の農場で作物を育てて出荷しよう",
                "タップで種まき・収穫、ボタンで出荷",
                "出荷目標を達成してステージクリア"
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
            _farmManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _farmManager.SetActive(false);
            _ui.ShowAllClear(_farmManager.TotalEarned);
        }

        public void OnStageClear()
        {
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void NextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void UpdateCoinsDisplay(long coins)
        {
            _ui.UpdateCoins(coins);
        }

        public void UpdateInventoryDisplay(long value)
        {
            _ui.UpdateInventory(value);
        }

        public void UpdateWeatherDisplay(int weatherIndex)
        {
            _ui.UpdateWeather(weatherIndex);
        }

        public void UpdateMarketDisplay(float multiplier)
        {
            _ui.UpdateMarketPrice(multiplier);
        }

        public void UpdateProgressDisplay(long earned, long target)
        {
            _ui.UpdateProgress(earned, target);
        }

        public void UpdateComboDisplay(int combo)
        {
            _ui.UpdateCombo(combo);
        }

        public void UpdateAutoRateDisplay(float perSec)
        {
            _ui.UpdateAutoRate(perSec);
        }

        public void UpdateShopButtons(bool autoUnlocked, bool companionUnlocked,
            bool pestUnlocked, bool premiumUnlocked,
            long coins, long autoUpgradeCost, long growthUpgradeCost)
        {
            _ui.UpdateShopButtons(autoUnlocked, companionUnlocked, pestUnlocked, premiumUnlocked,
                coins, autoUpgradeCost, growthUpgradeCost);
        }

        public void UpdateSelectedCrop(string cropName)
        {
            _ui.UpdateSelectedCrop(cropName);
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
