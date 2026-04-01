using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game049_CloudHop
{
    public class CloudHopUI : MonoBehaviour
    {
        [SerializeField, Tooltip("高度テキスト")] private TextMeshProUGUI _heightText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateHeight(float h) { if (_heightText != null) _heightText.text = $"{h:F0}m"; }
        public void ShowClear(int score) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"到達: {score}m"; }
        public void ShowGameOver(int score) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"最高: {score}m"; }
    }
}
