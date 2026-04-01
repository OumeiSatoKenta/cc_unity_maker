using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game074_NoteRain
{
    public class NoteRainUI : MonoBehaviour
    {
        [SerializeField, Tooltip("スコア")] private TextMeshProUGUI _scoreText;
        [SerializeField, Tooltip("コンボ")] private TextMeshProUGUI _comboText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateScore(int caught, int total) { if (_scoreText != null) _scoreText.text = $"♪ {caught}/{total}"; }
        public void UpdateCombo(int c) { if (_comboText != null) _comboText.text = c > 1 ? $"x{c}" : ""; }
        public void ShowClear(int caught, int maxCombo) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"{caught}音符 / 最大コンボ{maxCombo}"; }
        public void ShowGameOver(int caught, int total) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"{caught}/{total}音符"; }
    }
}
