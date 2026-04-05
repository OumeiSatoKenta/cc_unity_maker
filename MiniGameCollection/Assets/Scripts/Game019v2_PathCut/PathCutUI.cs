using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game019v2_PathCut
{
    public class PathCutUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _cutCountText;
        [SerializeField] TextMeshProUGUI _starCountText;

        [Header("Stage Clear Panel")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearComboText;
        [SerializeField] TextMeshProUGUI _stageClearStarsText;
        [SerializeField] Button _nextStageButton;

        [Header("Clear Panel")]
        [SerializeField] GameObject _clearPanel;
        [SerializeField] TextMeshProUGUI _clearScoreText;
        [SerializeField] Button _clearMenuButton;

        [Header("Game Over Panel")]
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] Button _retryButton;
        [SerializeField] Button _gameOverMenuButton;

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCutCount(int remaining, int max)
        {
            if (_cutCountText != null)
            {
                _cutCountText.text = $"✂ {remaining}/{max}";
                _cutCountText.color = remaining <= 1 ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.9f, 0.4f);
            }
        }

        public void UpdateStarCount(int collected, int total)
        {
            if (_starCountText != null)
                _starCountText.text = $"★ {collected}/{total}";
        }

        public void ShowStageClearPanel(int stageScore, int combo, int stars)
        {
            HideAllPanels();
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"+{stageScore}";
            if (_stageClearComboText != null)
                _stageClearComboText.text = combo >= 2 ? $"COMBO ×{combo}" : "";
            if (_stageClearStarsText != null)
            {
                string starStr = "";
                for (int i = 0; i < 3; i++) starStr += (i < stars) ? "★" : "☆";
                _stageClearStarsText.text = starStr;
            }
        }

        public void ShowClearPanel(int totalScore)
        {
            HideAllPanels();
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearScoreText != null) _clearScoreText.text = $"Total: {totalScore}";
        }

        public void ShowGameOverPanel()
        {
            HideAllPanels();
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_clearPanel != null) _clearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
