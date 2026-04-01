using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game038_FlyBird
{
    public class FlyBirdUI : MonoBehaviour
    {
        [SerializeField, Tooltip("スコアテキスト")]
        private TextMeshProUGUI _scoreText;

        [SerializeField, Tooltip("距離テキスト")]
        private TextMeshProUGUI _distanceText;

        [SerializeField, Tooltip("クリアパネル")]
        private GameObject _clearPanel;

        [SerializeField, Tooltip("クリアスコアテキスト")]
        private TextMeshProUGUI _clearScoreText;

        [SerializeField, Tooltip("クリアリトライ")]
        private Button _clearRetryButton;

        [SerializeField, Tooltip("GOパネル")]
        private GameObject _gameOverPanel;

        [SerializeField, Tooltip("GOスコアテキスト")]
        private TextMeshProUGUI _gameOverScoreText;

        [SerializeField, Tooltip("GOリトライ")]
        private Button _gameOverRetryButton;

        [SerializeField, Tooltip("メニューボタン")]
        private Button _menuButton;

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"{score}";
        }

        public void UpdateDistance(float dist, float goal)
        {
            if (_distanceText != null) _distanceText.text = $"{dist:F0}m / {goal:F0}m";
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
