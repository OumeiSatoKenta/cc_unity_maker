using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game031_BounceKing
{
    public class BounceKingUI : MonoBehaviour
    {
        [SerializeField, Tooltip("スコア表示テキスト")]
        private TextMeshProUGUI _scoreText;

        [SerializeField, Tooltip("ライフ表示テキスト")]
        private TextMeshProUGUI _livesText;

        [SerializeField, Tooltip("クリアパネル")]
        private GameObject _clearPanel;

        [SerializeField, Tooltip("クリア時スコアテキスト")]
        private TextMeshProUGUI _clearScoreText;

        [SerializeField, Tooltip("ゲームオーバーパネル")]
        private GameObject _gameOverPanel;

        [SerializeField, Tooltip("ゲームオーバー時スコアテキスト")]
        private TextMeshProUGUI _gameOverScoreText;

        [SerializeField, Tooltip("クリア画面のリトライボタン")]
        private Button _clearRetryButton;

        [SerializeField, Tooltip("ゲームオーバー画面のリトライボタン")]
        private Button _gameOverRetryButton;

        [SerializeField, Tooltip("メニューへ戻るボタン")]
        private Button _menuButton;

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateLives(int lives)
        {
            if (_livesText != null) _livesText.text = $"Lives: {lives}";
        }

        public void ShowClearPanel(int score)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearScoreText != null) _clearScoreText.text = $"Score: {score}";
        }

        public void ShowGameOverPanel(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
        }

        public void HidePanels()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
