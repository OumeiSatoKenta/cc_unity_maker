using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game034_DropZone
{
    public class DropZoneUI : MonoBehaviour
    {
        [SerializeField, Tooltip("スコアテキスト")]
        private TextMeshProUGUI _scoreText;

        [SerializeField, Tooltip("ミス数テキスト")]
        private TextMeshProUGUI _missesText;

        [SerializeField, Tooltip("残りアイテムテキスト")]
        private TextMeshProUGUI _remainingText;

        [SerializeField, Tooltip("クリアパネル")]
        private GameObject _clearPanel;

        [SerializeField, Tooltip("クリアスコアテキスト")]
        private TextMeshProUGUI _clearScoreText;

        [SerializeField, Tooltip("クリアリトライボタン")]
        private Button _clearRetryButton;

        [SerializeField, Tooltip("ゲームオーバーパネル")]
        private GameObject _gameOverPanel;

        [SerializeField, Tooltip("ゲームオーバースコアテキスト")]
        private TextMeshProUGUI _gameOverScoreText;

        [SerializeField, Tooltip("ゲームオーバーリトライボタン")]
        private Button _gameOverRetryButton;

        [SerializeField, Tooltip("メニューボタン")]
        private Button _menuButton;

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateMisses(int misses, int max)
        {
            if (_missesText != null) _missesText.text = $"Miss: {misses}/{max}";
        }

        public void UpdateRemaining(int remaining)
        {
            if (_remainingText != null) _remainingText.text = $"残り: {remaining}";
        }

        public void ShowClear(int score)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearScoreText != null) _clearScoreText.text = $"Score: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
        }
    }
}
