using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game037_ZapChain
{
    public class ZapChainUI : MonoBehaviour
    {
        [SerializeField, Tooltip("接続数テキスト")]
        private TextMeshProUGUI _connectedText;

        [SerializeField, Tooltip("残りザップ数テキスト")]
        private TextMeshProUGUI _zapsText;

        [SerializeField, Tooltip("クリアパネル")]
        private GameObject _clearPanel;

        [SerializeField, Tooltip("クリア★テキスト")]
        private TextMeshProUGUI _clearStarText;

        [SerializeField, Tooltip("クリアリトライボタン")]
        private Button _clearRetryButton;

        [SerializeField, Tooltip("ゲームオーバーパネル")]
        private GameObject _gameOverPanel;

        [SerializeField, Tooltip("GOリトライボタン")]
        private Button _gameOverRetryButton;

        [SerializeField, Tooltip("メニューボタン")]
        private Button _menuButton;

        public void UpdateConnected(int count, int total)
        {
            if (_connectedText != null) _connectedText.text = $"接続: {count}/{total}";
        }

        public void UpdateZaps(int remaining)
        {
            if (_zapsText != null) _zapsText.text = $"ザップ: {remaining}";
        }

        public void ShowClear(int stars)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearStarText != null)
                _clearStarText.text = new string('\u2605', stars) + new string('\u2606', 3 - stars);
        }

        public void ShowGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }
    }
}
