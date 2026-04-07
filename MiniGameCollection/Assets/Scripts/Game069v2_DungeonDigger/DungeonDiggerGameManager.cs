using UnityEngine;

namespace Game069v2_DungeonDigger
{
    public class DungeonDiggerGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] DigManager _digManager;
        [SerializeField] DungeonDiggerUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "069",
                "DungeonDigger",
                "地下を掘り進めてお宝を見つけよう",
                "タップで掘削、ボタンでアップグレード",
                "深度目標を達成してステージクリア"
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
            _digManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _digManager.SetActive(false);
            _ui.ShowAllClear(_digManager.TotalGold);
        }

        public void OnStageClear()
        {
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void NextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void UpdateDepthDisplay(int depth, int target)
        {
            _ui.UpdateDepth(depth, target);
        }

        public void UpdateGoldDisplay(long gold)
        {
            _ui.UpdateGold(gold);
        }

        public void UpdateInventoryDisplay(int count)
        {
            _ui.UpdateInventory(count);
        }

        public void UpdateDrillLevelDisplay(int level)
        {
            _ui.UpdateDrillLevel(level);
        }

        public void UpdateComboDisplay(int combo)
        {
            _ui.UpdateCombo(combo);
        }

        public void UpdateAutoRateDisplay(float rate)
        {
            _ui.UpdateAutoRate(rate);
        }

        public void UpdateUpgradeButtons(long gold, long drillCost, bool heatShieldOwned, bool lanternOwned,
            long heatShieldCost, long lanternCost)
        {
            _ui.UpdateUpgradeButtons(gold, drillCost, heatShieldOwned, lanternOwned, heatShieldCost, lanternCost);
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
