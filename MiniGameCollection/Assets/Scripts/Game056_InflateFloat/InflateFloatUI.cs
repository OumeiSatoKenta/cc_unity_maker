using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game056_InflateFloat
{
    public class InflateFloatUI : MonoBehaviour
    {
        [SerializeField, Tooltip("サイズゲージ")] private Slider _sizeSlider;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアタイム")] private TextMeshProUGUI _clearTimeText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateSize(float ratio) { if (_sizeSlider != null) _sizeSlider.value = ratio; }
        public void ShowClear(float time) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearTimeText != null) _clearTimeText.text = $"{time:F1}秒"; }
        public void ShowGameOver() { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); }
    }
}
