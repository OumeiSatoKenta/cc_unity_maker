using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game036_CoinStack
{
    public class CoinStackUI : MonoBehaviour
    {
        [SerializeField, Tooltip("高さテキスト")]
        private TextMeshProUGUI _heightText;

        [SerializeField, Tooltip("残りテキスト")]
        private TextMeshProUGUI _remainingText;

        [SerializeField, Tooltip("クリアパネル")]
        private GameObject _clearPanel;

        [SerializeField, Tooltip("クリアスコアテキスト")]
        private TextMeshProUGUI _clearScoreText;

        [SerializeField, Tooltip("クリアリトライボタン")]
        private Button _clearRetryButton;

        [SerializeField, Tooltip("ゲームオーバーパネル")]
        private GameObject _gameOverPanel;

        [SerializeField, Tooltip("GOスコアテキスト")]
        private TextMeshProUGUI _gameOverScoreText;

        [SerializeField, Tooltip("GOリトライボタン")]
        private Button _gameOverRetryButton;

        [SerializeField, Tooltip("メニューボタン")]
        private Button _menuButton;

        public void UpdateHeight(int height, int goal)
        {
            if (_heightText != null) _heightText.text = $"高さ: {height}/{goal}";
        }

        public void UpdateRemaining(int remaining)
        {
            if (_remainingText != null) _remainingText.text = $"残り: {remaining}";
        }

        public void ShowClear(int height)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearScoreText != null) _clearScoreText.text = $"{height}段達成！";
        }

        public void ShowGameOver(int height)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"最高: {height}段";
        }
    }
}
