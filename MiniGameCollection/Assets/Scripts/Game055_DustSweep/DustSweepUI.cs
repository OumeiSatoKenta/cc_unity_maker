using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game055_DustSweep
{
    public class DustSweepUI : MonoBehaviour
    {
        [SerializeField, Tooltip("タイマー")] private TextMeshProUGUI _timerText;
        [SerializeField, Tooltip("進捗スライダー")] private Slider _progressSlider;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアタイム")] private TextMeshProUGUI _clearTimeText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GO進捗")] private TextMeshProUGUI _gameOverProgressText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateTimer(float t) { if (_timerText != null) _timerText.text = $"{t:F1}s"; }
        public void UpdateProgress(float ratio) { if (_progressSlider != null) _progressSlider.value = ratio; }
        public void ShowClear(float time) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearTimeText != null) _clearTimeText.text = $"{time:F1}秒でクリア！"; }
        public void ShowGameOver(float ratio) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverProgressText != null) _gameOverProgressText.text = $"清潔度: {ratio*100:F0}%"; }
    }
}
