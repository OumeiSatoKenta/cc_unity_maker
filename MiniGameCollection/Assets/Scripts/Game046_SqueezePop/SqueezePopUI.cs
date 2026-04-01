using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game046_SqueezePop
{
    public class SqueezePopUI : MonoBehaviour
    {
        [SerializeField, Tooltip("残りテキスト")] private TextMeshProUGUI _remainingText;
        [SerializeField, Tooltip("タイマー")] private TextMeshProUGUI _timerText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアテキスト")] private TextMeshProUGUI _clearTimeText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateRemaining(int r) { if (_remainingText != null) _remainingText.text = $"残り: {r}"; }
        public void UpdateTimer(float t) { if (_timerText != null) _timerText.text = $"{t:F1}s"; }
        public void ShowClear(float remaining) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearTimeText != null) _clearTimeText.text = $"残り{remaining:F1}秒！"; }
        public void ShowGameOver(int popped) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); }
    }
}
