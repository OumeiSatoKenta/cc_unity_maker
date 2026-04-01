using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game063_StarMiner
{
    public class StarMinerUI : MonoBehaviour
    {
        [SerializeField, Tooltip("鉱石数")] private TextMeshProUGUI _oreText;
        [SerializeField, Tooltip("ドリルボタン")] private Button _drillButton;
        [SerializeField, Tooltip("ドリルテキスト")] private TextMeshProUGUI _drillButtonText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateOre(int ore, int target) { if (_oreText != null) _oreText.text = $"鉱石: {ore}/{target}"; }
        public void UpdateDrill(int lv, int cost) { if (_drillButtonText != null) _drillButtonText.text = $"ドリルLv{lv}\n{cost}個"; }
        public void ShowClear(int ore) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"鉱石{ore}個収集！"; }
    }
}
