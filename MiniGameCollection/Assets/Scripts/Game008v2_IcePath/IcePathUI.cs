using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game008v2_IcePath
{
    public class IcePathUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _moveCountText;
        [SerializeField] private TextMeshProUGUI _remainingText;

        [SerializeField] private GameObject _stageClearPanel;
        [SerializeField] private TextMeshProUGUI _stageClearTitleText;
        [SerializeField] private TextMeshProUGUI _stageClearScoreText;
        [SerializeField] private TextMeshProUGUI _starsText;
        [SerializeField] private Button _nextStageButton;

        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearScoreText;

        [SerializeField] private GameObject _gameOverPanel;

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateMoveCount(int count)
        {
            if (_moveCountText != null) _moveCountText.text = $"手数: {count}";
        }

        public void UpdateRemaining(int remaining)
        {
            if (_remainingText != null) _remainingText.text = $"残り: {remaining}";
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_clearPanel != null) _clearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void ShowStageClearPanel(int stage, int totalScore, int stars)
        {
            HideAllPanels();
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearTitleText != null)
                    _stageClearTitleText.text = $"ステージ {stage} クリア！";
                if (_stageClearScoreText != null)
                    _stageClearScoreText.text = $"スコア: {totalScore}";
                if (_starsText != null)
                    _starsText.text = stars >= 3 ? "★★★" : stars >= 2 ? "★★☆" : "★☆☆";
            }
        }

        public void ShowClearPanel(int totalScore)
        {
            HideAllPanels();
            if (_clearPanel != null)
            {
                _clearPanel.SetActive(true);
                if (_clearScoreText != null)
                    _clearScoreText.text = $"全ステージクリア！\nスコア: {totalScore}";
            }
        }

        public void ShowGameOverPanel()
        {
            HideAllPanels();
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }
    }
}
