using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game081_PetBonsai
{
    public class PetBonsaiUI : MonoBehaviour
    {
        [SerializeField, Tooltip("美しさ")] private TextMeshProUGUI _beautyText;
        [SerializeField, Tooltip("成長")] private TextMeshProUGUI _growthText;
        [SerializeField, Tooltip("水スライダー")] private Slider _waterSlider;
        [SerializeField, Tooltip("水やりボタン")] private Button _waterButton;
        [SerializeField, Tooltip("剪定ボタン")] private Button _pruneButton;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateBeauty(int b, int target) { if (_beautyText != null) _beautyText.text = $"美: {b}/{target}"; }
        public void UpdateGrowth(int lv) { if (_growthText != null) _growthText.text = $"Lv{lv}"; }
        public void UpdateWater(float w) { if (_waterSlider != null) _waterSlider.value = w; }
        public void ShowClear(int beauty) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"美しさ{beauty}で優勝！"; }
    }
}
