using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game035_WaveRider
{
    public class WaveRiderUI : MonoBehaviour
    {
        [SerializeField, Tooltip("スコアテキスト")]
        private TextMeshProUGUI _scoreText;

        [SerializeField, Tooltip("距離テキスト")]
        private TextMeshProUGUI _distanceText;

        [SerializeField, Tooltip("バランススライダー")]
        private Slider _balanceSlider;

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

        private WaveManager _waveManager;

        private void Start()
        {
            _waveManager = GetComponentInParent<WaveManager>();
            if (_waveManager == null)
                _waveManager = Object.FindFirstObjectByType<WaveManager>();
        }

        private void Update()
        {
            if (_balanceSlider != null && _waveManager != null)
                _balanceSlider.value = _waveManager.Balance / 100f;
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
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
