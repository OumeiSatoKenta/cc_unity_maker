using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game032_SpinCutter
{
    public class SpinCutterUI : MonoBehaviour
    {
        [SerializeField, Tooltip("発射残数テキスト")]
        private TextMeshProUGUI _launchesText;

        [SerializeField, Tooltip("撃破数テキスト")]
        private TextMeshProUGUI _killsText;

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

        public void UpdateLaunches(int remaining)
        {
            if (_launchesText != null)
                _launchesText.text = $"発射: {remaining}";
        }

        public void UpdateKills(int killed, int total)
        {
            if (_killsText != null)
                _killsText.text = $"撃破: {killed}/{total}";
        }

        public void ShowClear(int stars)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearStarText != null)
                _clearStarText.text = new string('★', stars) + new string('☆', 3 - stars);
        }

        public void ShowGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }

        public Button ClearRetryButton => _clearRetryButton;
        public Button GameOverRetryButton => _gameOverRetryButton;
        public Button MenuButton => _menuButton;
    }
}
