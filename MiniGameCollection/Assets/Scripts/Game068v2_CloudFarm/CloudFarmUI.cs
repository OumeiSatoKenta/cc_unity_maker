using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game068v2_CloudFarm
{
    public class CloudFarmUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _coinsText;
        [SerializeField] TextMeshProUGUI _inventoryText;
        [SerializeField] TextMeshProUGUI _weatherText;
        [SerializeField] TextMeshProUGUI _marketText;
        [SerializeField] TextMeshProUGUI _progressText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _autoRateText;
        [SerializeField] TextMeshProUGUI _selectedCropText;

        // Stage clear panel
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;

        // All clear panel
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        // Shop buttons
        [SerializeField] Button _autoUpgradeBtn;
        [SerializeField] TextMeshProUGUI _autoUpgradeCostText;
        [SerializeField] Button _growthUpgradeBtn;
        [SerializeField] TextMeshProUGUI _growthUpgradeCostText;
        [SerializeField] Button _sellBtn;
        [SerializeField] TextMeshProUGUI _sellBtnText;

        static readonly string[] WeatherNames = { "☀晴れ", "🌧雨", "⛈嵐" };

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateCoins(long coins)
        {
            if (_coinsText != null) _coinsText.text = $"コイン: {coins:N0}G";
        }

        public void UpdateInventory(long value)
        {
            if (_inventoryText != null) _inventoryText.text = $"在庫: {value:N0}G";
            if (_sellBtnText != null) _sellBtnText.text = value > 0 ? $"出荷 ({value:N0}G)" : "出荷";
        }

        public void UpdateWeather(int weatherIndex)
        {
            if (_weatherText != null)
                _weatherText.text = weatherIndex >= 0 && weatherIndex < WeatherNames.Length
                    ? WeatherNames[weatherIndex]
                    : "";
        }

        public void UpdateMarketPrice(float multiplier)
        {
            if (_marketText != null)
            {
                string color = multiplier >= 1.5f ? "#FFD700" : multiplier <= 0.7f ? "#FF6060" : "#FFFFFF";
                _marketText.text = $"市場: <color={color}>{multiplier:F1}x</color>";
            }
            if (_sellBtn != null)
            {
                var colors = _sellBtn.colors;
                colors.normalColor = multiplier >= 1.5f ? new Color(1f, 0.85f, 0f) : Color.white;
                _sellBtn.colors = colors;
            }
        }

        public void UpdateProgress(long earned, long target)
        {
            if (_progressText != null)
                _progressText.text = $"出荷: {earned:N0} / {target:N0}G";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText != null)
            {
                _comboText.text = combo > 1 ? $"コンボ x{combo}!" : "";
            }
        }

        public void UpdateAutoRate(float perSec)
        {
            if (_autoRateText != null)
                _autoRateText.text = perSec > 0 ? $"自動: {perSec:F1}/秒" : "";
        }

        public void UpdateSelectedCrop(string cropName)
        {
            if (_selectedCropText != null) _selectedCropText.text = $"種: {cropName}";
        }

        public void UpdateShopButtons(bool autoUnlocked, bool companionUnlocked,
            bool pestUnlocked, bool premiumUnlocked,
            long coins, long autoUpgradeCost, long growthUpgradeCost)
        {
            if (_autoUpgradeBtn != null)
            {
                _autoUpgradeBtn.gameObject.SetActive(autoUnlocked);
                _autoUpgradeBtn.interactable = coins >= autoUpgradeCost;
                if (_autoUpgradeCostText != null) _autoUpgradeCostText.text = $"自動強化\n{autoUpgradeCost:N0}G";
            }
            if (_growthUpgradeBtn != null)
            {
                _growthUpgradeBtn.interactable = coins >= growthUpgradeCost;
                if (_growthUpgradeCostText != null) _growthUpgradeCostText.text = $"成長強化\n{growthUpgradeCost:N0}G";
            }
        }

        public void ShowStageClear(int clearedStage)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearText != null) _stageClearText.text = $"ステージ {clearedStage} クリア！";
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(long totalEarned)
        {
            if (_allClearPanel != null)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText != null)
                    _allClearScoreText.text = $"全ステージクリア！\n総出荷額: {totalEarned:N0}G";
            }
        }
    }
}
