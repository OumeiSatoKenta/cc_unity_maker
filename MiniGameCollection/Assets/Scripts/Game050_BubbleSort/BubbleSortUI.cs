using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game050_BubbleSort
{
    public class BubbleSortUI : MonoBehaviour
    {
        [SerializeField, Tooltip("残り手数")] private TextMeshProUGUI _movesText;
        [SerializeField, Tooltip("進捗")] private TextMeshProUGUI _progressText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateMoves(int remaining) { if (_movesText != null) _movesText.text = $"残り: {remaining}手"; }
        public void UpdateProgress(int correct, int total) { if (_progressText != null) _progressText.text = $"{correct}/{total}"; }
        public void ShowClear(int remaining) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"残り{remaining}手でクリア！"; }
        public void ShowGameOver() { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); }
    }
}
