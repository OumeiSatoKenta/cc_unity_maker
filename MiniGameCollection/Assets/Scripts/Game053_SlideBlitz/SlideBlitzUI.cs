using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game053_SlideBlitz
{
    public class SlideBlitzUI : MonoBehaviour
    {
        [SerializeField, Tooltip("タイマー")] private TextMeshProUGUI _timerText;
        [SerializeField, Tooltip("手数")] private TextMeshProUGUI _movesText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateTimer(float t) { if (_timerText != null) _timerText.text = $"{t:F1}s"; }
        public void UpdateMoves(int m) { if (_movesText != null) _movesText.text = $"{m}手"; }
        public void ShowClear(int moves, float remaining) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"{moves}手 / 残り{remaining:F1}秒"; }
        public void ShowGameOver(int moves) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); }
    }
}
