using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game044_TiltMaze
{
    public class TiltMazeUI : MonoBehaviour
    {
        [SerializeField, Tooltip("タイマー")] private TextMeshProUGUI _timerText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアタイム")] private TextMeshProUGUI _clearTimeText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateTimer(float t) { if (_timerText != null) _timerText.text = $"{t:F1}s"; }
        public void ShowClear(float remaining) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearTimeText != null) _clearTimeText.text = $"残り {remaining:F1}秒！"; }
        public void ShowGameOver() { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); }
    }
}
