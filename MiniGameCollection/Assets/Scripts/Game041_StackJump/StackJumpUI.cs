using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game041_StackJump
{
    public class StackJumpUI : MonoBehaviour
    {
        [SerializeField, Tooltip("高さテキスト")] private TextMeshProUGUI _heightText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateHeight(int h, int g) { if (_heightText != null) _heightText.text = $"{h}/{g}"; }
        public void ShowClear(int h) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"{h}段達成！"; }
        public void ShowGameOver(int h) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"最高: {h}段"; }
    }
}
