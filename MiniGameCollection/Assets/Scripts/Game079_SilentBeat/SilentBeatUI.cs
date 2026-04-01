using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game079_SilentBeat
{
    public class SilentBeatUI : MonoBehaviour
    {
        [SerializeField, Tooltip("タイマー")] private TextMeshProUGUI _timerText;
        [SerializeField, Tooltip("精度")] private TextMeshProUGUI _accuracyText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateTimer(float t) { if (_timerText != null) _timerText.text = $"{t:F1}s"; }
        public void UpdateAccuracy(float a) { if (_accuracyText != null) _accuracyText.text = $"精度: {a*100:F0}%"; }
        public void ShowClear(int accuracy, int taps) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"精度{accuracy}% / {taps}タップ"; }
        public void ShowGameOver(int accuracy, int taps) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"精度{accuracy}% / {taps}タップ"; }
    }
}
