using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game072_DrumKit
{
    public class DrumKitUI : MonoBehaviour
    {
        [SerializeField, Tooltip("スコア")] private TextMeshProUGUI _scoreText;
        [SerializeField, Tooltip("ステップ")] private TextMeshProUGUI _stepText;
        [SerializeField, Tooltip("ミス")] private TextMeshProUGUI _missText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateScore(int s) { if (_scoreText != null) _scoreText.text = $"{s}"; }
        public void UpdateStep(int step, int total) { if (_stepText != null) _stepText.text = $"{step}/{total}"; }
        public void UpdateMisses(int m, int max) { if (_missText != null) _missText.text = $"Miss: {m}/{max}"; }
        public void ShowClear(int score) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"スコア: {score}"; }
        public void ShowGameOver(int score) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"スコア: {score}"; }
    }
}
