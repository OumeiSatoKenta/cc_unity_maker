using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game042_ColorDrop
{
    public class ColorDropUI : MonoBehaviour
    {
        [SerializeField, Tooltip("スコア")] private TextMeshProUGUI _scoreText;
        [SerializeField, Tooltip("ミス")] private TextMeshProUGUI _missesText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateScore(int s) { if (_scoreText != null) _scoreText.text = $"Score: {s}"; }
        public void UpdateMisses(int m, int max) { if (_missesText != null) _missesText.text = $"Miss: {m}/{max}"; }
        public void ShowClear(int s) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"Score: {s}"; }
        public void ShowGameOver(int s) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {s}"; }
    }
}
