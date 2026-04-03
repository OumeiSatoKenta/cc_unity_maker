using System.Collections;
using TMPro;
using UnityEngine;

namespace Game005v2_PipeConnect
{
    public class PipeConnectUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _timerText;

        [SerializeField] private GameObject _stageClearPanel;
        [SerializeField] private TextMeshProUGUI _stageClearStageText;
        [SerializeField] private TextMeshProUGUI _stageClearScoreText;
        [SerializeField] private TextMeshProUGUI _stageClearStarsText;

        [SerializeField] private GameObject _gameClearPanel;
        [SerializeField] private TextMeshProUGUI _gameClearScoreText;

        [SerializeField] private GameObject _gameOverPanel;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = score.ToString("N0");
        }

        public void UpdateTimer(float t)
        {
            if (_timerText != null)
            {
                _timerText.text = Mathf.CeilToInt(t).ToString();
                _timerText.color = t <= 10f ? new Color(1f, 0.3f, 0.3f) : new Color(0.3f, 1f, 0.7f);
            }
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void ShowStageClearPanel(int stage, int score, int stars)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearStageText != null) _stageClearStageText.text = $"Stage {stage} クリア！";
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"スコア: {score:N0}";
            if (_stageClearStarsText != null)
                _stageClearStarsText.text = stars >= 3 ? "★★★" : stars >= 2 ? "★★☆" : "★☆☆";
        }

        public void ShowClearPanel(int score)
        {
            if (_gameClearPanel == null) return;
            _gameClearPanel.SetActive(true);
            if (_gameClearScoreText != null) _gameClearScoreText.text = $"Total Score: {score:N0}";
        }

        public void ShowGameOverPanel()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }
    }
}
