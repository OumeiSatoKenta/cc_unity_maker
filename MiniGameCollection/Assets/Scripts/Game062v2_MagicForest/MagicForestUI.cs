using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game062v2_MagicForest
{
    public class MagicForestUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _manaText;
        [SerializeField] TextMeshProUGUI _areaText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _worldTreeText;

        [SerializeField] Button _autoGrowBtn;
        [SerializeField] TextMeshProUGUI _autoGrowCostText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageBtn;

        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearText;
        [SerializeField] Button _retryBtn;

        [SerializeField] GameObject _stormWarning;

        [SerializeField] MagicForestGameManager _gameManager;
        [SerializeField] ForestManager _forestManager;

        void Start()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_stormWarning != null) _stormWarning.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
            if (_worldTreeText != null) _worldTreeText.gameObject.SetActive(false);
            if (_autoGrowBtn != null) _autoGrowBtn.gameObject.SetActive(false);
        }

        public void UpdateStageDisplay(int stage)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / 5";
        }

        public void UpdateMana(int mana)
        {
            if (_manaText != null) _manaText.text = $"✨ {mana}";
        }

        public void UpdateArea(int current, int target)
        {
            if (_areaText != null) _areaText.text = $"🌳 {current} / {target}";
        }

        public void UpdateCombo(int combo, float mult)
        {
            if (_comboText == null) return;
            if (combo >= 3)
            {
                _comboText.gameObject.SetActive(true);
                _comboText.text = $"Combo x{combo} ({mult:F1}x)";
                _comboText.color = combo >= 10 ? new Color(1f, 0.5f, 0f) : new Color(0.2f, 0.8f, 0.2f);
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        public void SetAutoGrowButtonVisible(bool visible)
        {
            if (_autoGrowBtn != null) _autoGrowBtn.gameObject.SetActive(visible);
        }

        public void UpdateAutoGrowCost(int cost)
        {
            if (_autoGrowCostText != null) _autoGrowCostText.text = $"自動成長\n✨{cost}";
        }

        public void ShowStorm()
        {
            if (_stormWarning != null) _stormWarning.SetActive(true);
        }

        public void HideStorm()
        {
            if (_stormWarning != null) _stormWarning.SetActive(false);
        }

        public void ShowWorldTreeGrowing()
        {
            if (_worldTreeText != null)
            {
                _worldTreeText.gameObject.SetActive(true);
                _worldTreeText.text = "🌳 世界樹が成長中...";
            }
        }

        public void UpdateWorldTreeProgress(float progress)
        {
            if (_worldTreeText != null)
                _worldTreeText.text = $"🌳 世界樹 {(int)(progress * 100)}%";
        }

        public void ShowStageClear(int trees)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearText != null) _stageClearText.text = $"ステージクリア！\n🌳 {trees}本";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowGameClear(int trees)
        {
            if (_gameClearPanel != null) _gameClearPanel.SetActive(true);
            if (_gameClearText != null) _gameClearText.text = $"魔法の森 完成！\n🌳 {trees}本の木が育った！";
        }

        public void HideGameClear()
        {
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
        }

        // Button callbacks
        public void OnAutoGrowButtonClicked()
        {
            if (_forestManager != null) _forestManager.PurchaseAutoGrow();
        }

        public void OnNextStageButtonClicked()
        {
            if (_gameManager != null) _gameManager.OnNextStage();
        }

        public void OnRetryButtonClicked()
        {
            if (_gameManager != null) _gameManager.OnRetry();
        }
    }
}
