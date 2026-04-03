using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game002v2_MirrorMaze
{
    public class MirrorMazeUI : MonoBehaviour
    {
        [SerializeField] private MirrorMazeGameManager _gameManager;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Button _fireButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private GameObject _stageClearPanel;
        [SerializeField] private TextMeshProUGUI _stageClearText;
        [SerializeField] private TextMeshProUGUI _stageScoreText;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private Button _clearMenuButton;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private Button _retryButton;

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        public void ShowStageClearPanel(int stageNum, int stageScore)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearText != null)
                    _stageClearText.text = $"ステージ {stageNum} クリア！";
                if (_stageScoreText != null)
                    _stageScoreText.text = $"+{stageScore}";
            }
        }

        public void ShowClearPanel(int totalScore)
        {
            if (_clearPanel != null)
            {
                _clearPanel.SetActive(true);
                if (_finalScoreText != null)
                    _finalScoreText.text = $"最終スコア: {totalScore}";
            }
        }

        public void ShowGameOverPanel()
        {
            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(true);
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_clearPanel != null) _clearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
