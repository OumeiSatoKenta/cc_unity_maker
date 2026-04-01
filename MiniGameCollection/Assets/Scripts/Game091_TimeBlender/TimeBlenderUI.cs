using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game091_TimeBlender
{
    public class TimeBlenderUI : MonoBehaviour
    {
        [SerializeField, Tooltip("パズル数")] private TextMeshProUGUI _puzzleText;
        [SerializeField, Tooltip("時代テキスト")] private TextMeshProUGUI _eraText;
        [SerializeField, Tooltip("時代切替ボタン")] private Button _toggleButton;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdatePuzzles(int solved, int total) { if (_puzzleText != null) _puzzleText.text = $"パズル: {solved}/{total}"; }
        public void UpdateEra(bool isPresent) { if (_eraText != null) _eraText.text = isPresent ? "現在" : "過去"; }
        public void ShowClear(int solved) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{solved}パズルクリア！"; }
        public void ShowGameOver(int solved) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"{solved}パズルまで"; }
    }
}
