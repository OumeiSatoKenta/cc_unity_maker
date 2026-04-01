using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game033_AimSniper
{
    public class AimSniperUI : MonoBehaviour
    {
        [SerializeField, Tooltip("弾数テキスト")]
        private TextMeshProUGUI _bulletsText;

        [SerializeField, Tooltip("命中率テキスト")]
        private TextMeshProUGUI _accuracyText;

        [SerializeField, Tooltip("クリアパネル")]
        private GameObject _clearPanel;

        [SerializeField, Tooltip("クリア★テキスト")]
        private TextMeshProUGUI _clearStarText;

        [SerializeField, Tooltip("クリアリトライボタン")]
        private Button _clearRetryButton;

        [SerializeField, Tooltip("ゲームオーバーパネル")]
        private GameObject _gameOverPanel;

        [SerializeField, Tooltip("ゲームオーバーリトライボタン")]
        private Button _gameOverRetryButton;

        [SerializeField, Tooltip("メニューボタン")]
        private Button _menuButton;

        public void UpdateBullets(int remaining)
        {
            if (_bulletsText != null)
                _bulletsText.text = $"弾: {remaining}";
        }

        public void UpdateAccuracy(float accuracy)
        {
            if (_accuracyText != null)
                _accuracyText.text = $"命中率: {accuracy:F0}%";
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
