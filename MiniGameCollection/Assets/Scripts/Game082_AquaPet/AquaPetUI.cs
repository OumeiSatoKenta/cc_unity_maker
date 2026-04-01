using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game082_AquaPet
{
    public class AquaPetUI : MonoBehaviour
    {
        [SerializeField, Tooltip("コイン")] private TextMeshProUGUI _coinText;
        [SerializeField, Tooltip("魚数")] private TextMeshProUGUI _fishText;
        [SerializeField, Tooltip("コレクション")] private TextMeshProUGUI _collectionText;
        [SerializeField, Tooltip("魚追加ボタン")] private Button _addFishButton;
        [SerializeField, Tooltip("魚追加テキスト")] private TextMeshProUGUI _addFishButtonText;
        [SerializeField, Tooltip("TankManager")] private TankManager _tankManager;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        private void Update()
        {
            if (_addFishButtonText != null && _tankManager != null)
                _addFishButtonText.text = $"魚追加\n{_tankManager.NextFishCost}";
        }

        public void UpdateCoins(int c) { if (_coinText != null) _coinText.text = $"コイン: {c}"; }
        public void UpdateFish(int f) { if (_fishText != null) _fishText.text = $"魚: {f}匹"; }
        public void UpdateCollection(int s, int total) { if (_collectionText != null) _collectionText.text = $"種: {s}/{total}"; }
        public void ShowClear(int species, int fish) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{species}種 / {fish}匹！"; }
    }
}
