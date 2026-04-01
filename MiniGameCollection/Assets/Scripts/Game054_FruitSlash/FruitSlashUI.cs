using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game054_FruitSlash
{
    public class FruitSlashUI : MonoBehaviour
    {
        [SerializeField, Tooltip("スコア")] private TextMeshProUGUI _scoreText;
        [SerializeField, Tooltip("コンボ")] private TextMeshProUGUI _comboText;
        [SerializeField, Tooltip("ミス")] private TextMeshProUGUI _missText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateScore(int s, int target) { if (_scoreText != null) _scoreText.text = $"{s}/{target}"; }
        public void UpdateCombo(int c) { if (_comboText != null) _comboText.text = c > 1 ? $"x{c}" : ""; }
        public void UpdateMisses(int m, int max) { if (_missText != null) _missText.text = $"ミス: {m}/{max}"; }
        public void ShowClear(int score) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"スコア: {score}"; }
        public void ShowGameOver(int score) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"スコア: {score}"; }
    }
}
