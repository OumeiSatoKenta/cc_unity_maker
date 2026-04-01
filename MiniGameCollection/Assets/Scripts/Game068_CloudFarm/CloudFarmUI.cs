using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game068_CloudFarm
{
    public class CloudFarmUI : MonoBehaviour
    {
        [SerializeField, Tooltip("コイン")] private TextMeshProUGUI _coinText;
        [SerializeField, Tooltip("コレクション")] private TextMeshProUGUI _collectionText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateCoins(int c) { if (_coinText != null) _coinText.text = $"コイン: {c}"; }
        public void UpdateCollection(int found, int total) { if (_collectionText != null) _collectionText.text = $"作物: {found}/{total}種"; }
        public void ShowClear(int types, int coins) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{types}種コンプ！ {coins}コイン"; }
    }
}
