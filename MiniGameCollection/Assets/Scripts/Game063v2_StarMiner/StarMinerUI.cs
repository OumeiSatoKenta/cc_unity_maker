using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game063v2_StarMiner
{
    public class StarMinerUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _oreText;
        [SerializeField] TextMeshProUGUI _fundText;
        [SerializeField] TextMeshProUGUI _drillLevelText;
        [SerializeField] TextMeshProUGUI _droneCountText;
        [SerializeField] TextMeshProUGUI _autoRateText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] Button _drillUpgradeBtn;
        [SerializeField] TextMeshProUGUI _drillBtnText;
        [SerializeField] Button _droneBtn;
        [SerializeField] TextMeshProUGUI _droneBtnText;
        [SerializeField] Button _legendaryBtn;
        [SerializeField] TextMeshProUGUI _legendaryBtnText;
        [SerializeField] Button _sellBtn;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageBtn;

        [SerializeField] GameObject _allClearPanel;

        [SerializeField] GameObject _stormWarningPanel;

        // Star sprites for each stage
        [SerializeField] SpriteRenderer _starRenderer;
        [SerializeField] Sprite[] _starSprites; // 5 sprites

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateOre(long ore, long target)
        {
            if (_oreText) _oreText.text = $"鉱石: {ore:N0} / {target:N0}";
        }

        public void UpdateFunds(long funds)
        {
            if (_fundText) _fundText.text = $"資金: {funds:N0}G";
        }

        public void UpdateDrillLevel(int level)
        {
            if (_drillLevelText) _drillLevelText.text = $"ドリル Lv.{level}";
        }

        public void UpdateDroneCount(int count)
        {
            if (_droneCountText) _droneCountText.text = $"ドローン: {count}機";
        }

        public void UpdateAutoRate(int droneCount, float speed)
        {
            if (_autoRateText)
            {
                float rate = droneCount * speed;
                _autoRateText.text = droneCount > 0 ? $"自動採掘: {rate:F1}/秒" : "自動採掘: なし";
            }
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo <= 1)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            float mult = combo >= 5 ? 5f : combo >= 4 ? 3f : combo >= 3 ? 2f : 1.5f;
            _comboText.text = $"Combo x{mult:F1}!";
        }

        public void UpdateUpgradeCosts(long drillCost, long droneCost, long legendaryCost, bool droneUnlocked, bool legendaryUnlocked)
        {
            if (_drillBtnText) _drillBtnText.text = $"ドリル強化\n{drillCost}G";
            if (_droneBtnText)
            {
                _droneBtnText.text = droneUnlocked
                    ? $"ドローン追加\n{droneCost}G"
                    : $"ドローン解放\n{droneCost}G";
            }
            if (_legendaryBtnText && legendaryUnlocked)
                _legendaryBtnText.text = $"伝説採掘\n{legendaryCost}G";
        }

        public void ShowDroneButton(bool show)
        {
            if (_droneBtn) _droneBtn.gameObject.SetActive(show);
        }

        public void ShowDroneUnlocked()
        {
            if (_droneBtn) _droneBtn.gameObject.SetActive(true);
        }

        public void ShowLegendaryButton(bool show)
        {
            if (_legendaryBtn) _legendaryBtn.gameObject.SetActive(show);
        }

        public void ShowStormWarning(bool show)
        {
            if (_stormWarningPanel) _stormWarningPanel.SetActive(show);
        }

        public void SetUpgradeButtonsInteractable(bool interactable)
        {
            if (_drillUpgradeBtn) _drillUpgradeBtn.interactable = interactable;
            if (_droneBtn) _droneBtn.interactable = interactable;
            if (_sellBtn) _sellBtn.interactable = interactable;
        }

        public void ShowStageClear(int stage, int total)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearText) _stageClearText.text = stage < total ? $"Stage {stage} クリア！" : "全ステージクリア！";
            if (_nextStageBtn) _nextStageBtn.gameObject.SetActive(stage < total);
        }

        public void HideStageClear()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear()
        {
            if (_allClearPanel) _allClearPanel.SetActive(true);
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void UpdateStarForStage(int stage)
        {
            if (_starRenderer == null || _starSprites == null) return;
            int idx = Mathf.Clamp(stage - 1, 0, _starSprites.Length - 1);
            if (_starSprites[idx] != null)
                _starRenderer.sprite = _starSprites[idx];
        }
    }
}
