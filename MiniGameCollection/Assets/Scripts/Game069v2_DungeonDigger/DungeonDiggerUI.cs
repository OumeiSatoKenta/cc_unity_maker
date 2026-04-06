using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game069v2_DungeonDigger
{
    public class DungeonDiggerUI : MonoBehaviour
    {
        [SerializeField] TMP_Text _stageText;
        [SerializeField] TMP_Text _depthText;
        [SerializeField] TMP_Text _goldText;
        [SerializeField] TMP_Text _inventoryText;
        [SerializeField] TMP_Text _drillLevelText;
        [SerializeField] TMP_Text _autoRateText;
        [SerializeField] TMP_Text _comboText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TMP_Text _stageClearText;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TMP_Text _allClearText;

        [SerializeField] Button _drillUpgradeButton;
        [SerializeField] TMP_Text _drillUpgradeCostText;
        [SerializeField] Button _heatShieldButton;
        [SerializeField] TMP_Text _heatShieldButtonText;
        [SerializeField] Button _lanternButton;
        [SerializeField] TMP_Text _lanternButtonText;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateDepth(int depth, int target)
        {
            if (_depthText != null) _depthText.text = $"深度: {depth} / {target}m";
        }

        public void UpdateGold(long gold)
        {
            if (_goldText != null) _goldText.text = $"G: {gold}";
        }

        public void UpdateInventory(int count)
        {
            if (_inventoryText != null) _inventoryText.text = $"アイテム: {count}";
        }

        public void UpdateDrillLevel(int level)
        {
            if (_drillLevelText != null) _drillLevelText.text = $"ドリル Lv.{level}";
        }

        public void UpdateAutoRate(float rate)
        {
            if (_autoRateText != null)
                _autoRateText.text = rate > 0f ? $"自動: {rate:F1}/秒" : "自動: なし";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo <= 0)
            {
                _comboText.text = "";
                _comboText.transform.localScale = Vector3.one;
                return;
            }
            _comboText.text = $"COMBO x{combo}";
            float scale = Mathf.Min(1f + combo * 0.02f, 1.5f);
            _comboText.transform.localScale = new Vector3(scale, scale, 1f);
        }

        public void ShowStageClear(int nextStage)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearText != null)
                _stageClearText.text = nextStage <= 5
                    ? $"ステージ {nextStage - 1} クリア！"
                    : "全ステージクリア！";
        }

        public void ShowAllClear(long totalGold)
        {
            if (_allClearPanel != null) _allClearPanel.SetActive(true);
            if (_allClearText != null)
                _allClearText.text = $"全クリア！\n総ゴールド: {totalGold}G";
        }

        public void UpdateUpgradeButtons(long gold, long drillCost, bool heatShieldOwned,
            bool lanternOwned, long heatShieldCost, long lanternCost)
        {
            if (_drillUpgradeButton != null)
            {
                bool canAfford = gold >= drillCost && drillCost < 9999;
                _drillUpgradeButton.interactable = canAfford;
                if (_drillUpgradeCostText != null)
                    _drillUpgradeCostText.text = drillCost < 9999 ? $"強化 {drillCost}G" : "MAX";
            }

            if (_heatShieldButton != null)
            {
                _heatShieldButton.interactable = !heatShieldOwned && gold >= heatShieldCost;
                if (_heatShieldButtonText != null)
                    _heatShieldButtonText.text = heatShieldOwned ? "耐熱 ✓" : $"耐熱 {heatShieldCost}G";
            }

            if (_lanternButton != null)
            {
                _lanternButton.interactable = !lanternOwned && gold >= lanternCost;
                if (_lanternButtonText != null)
                    _lanternButtonText.text = lanternOwned ? "照明 ✓" : $"照明 {lanternCost}G";
            }
        }
    }
}
