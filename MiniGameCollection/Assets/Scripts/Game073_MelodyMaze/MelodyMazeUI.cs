using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game073_MelodyMaze
{
    public class MelodyMazeUI : MonoBehaviour
    {
        [SerializeField, Tooltip("ノート数")] private TextMeshProUGUI _noteText;
        [SerializeField, Tooltip("手数")] private TextMeshProUGUI _moveText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateNotes(int n, int target) { if (_noteText != null) _noteText.text = $"♪ {n}/{target}"; }
        public void UpdateMoves(int m) { if (_moveText != null) _moveText.text = $"{m}歩"; }
        public void ShowClear(int moves, int notes) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"{moves}歩 / {notes}音符！"; }
        public void ShowGameOver(int notes, int target) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"{notes}/{target}音符"; }
    }
}
