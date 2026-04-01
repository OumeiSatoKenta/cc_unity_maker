using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game057_CandyDrop
{
    public class CandyDropUI : MonoBehaviour
    {
        [SerializeField, Tooltip("高さテキスト")] private TextMeshProUGUI _heightText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateHeight(float h, float target) { if (_heightText != null) _heightText.text = $"{h:F1}/{target:F0}m"; }
        public void ShowClear(int h) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"{h}mタワー完成！"; }
        public void ShowGameOver(float h) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"最高{h:F1}m"; }
    }
}
