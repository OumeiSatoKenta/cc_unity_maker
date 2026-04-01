using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game092_MirrorWorld
{
    public class MirrorWorldUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _moveText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearScoreText;
        [SerializeField] private Button _clearRetryButton;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField] private Button _gameOverRetryButton;
        [SerializeField] private Button _menuButton;

        public void UpdateStage(int s, int t) { if (_stageText) _stageText.text = $"Stage {s}/{t}"; }
        public void UpdateMoves(int m) { if (_moveText) _moveText.text = $"{m}手"; }
        public void ShowClear(int moves) { if (_clearPanel) _clearPanel.SetActive(true); if (_clearScoreText) _clearScoreText.text = $"{moves}手でクリア！"; }
        public void ShowGameOver(int stage) { if (_gameOverPanel) _gameOverPanel.SetActive(true); if (_gameOverScoreText) _gameOverScoreText.text = $"Stage {stage}まで"; }
    }
}
