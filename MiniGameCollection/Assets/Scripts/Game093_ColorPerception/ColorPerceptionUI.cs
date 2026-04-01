using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game093_ColorPerception
{
    public class ColorPerceptionUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _puzzleText;
        [SerializeField] private TextMeshProUGUI _moveText;
        [SerializeField] private Button _toggleButton;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearScoreText;
        [SerializeField] private Button _clearRetryButton;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField] private Button _gameOverRetryButton;
        [SerializeField] private Button _menuButton;

        public void UpdatePuzzle(int p, int t) { if (_puzzleText) _puzzleText.text = $"Q{p}/{t}"; }
        public void UpdateMoves(int remaining) { if (_moveText) _moveText.text = $"残り{remaining}手"; }
        public void ShowClear(int moves) { if (_clearPanel) _clearPanel.SetActive(true); if (_clearScoreText) _clearScoreText.text = $"{moves}手でクリア！"; }
        public void ShowGameOver(int solved) { if (_gameOverPanel) _gameOverPanel.SetActive(true); if (_gameOverScoreText) _gameOverScoreText.text = $"{solved}問正解"; }
    }
}
