using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game060v2_MeltIce
{
    public class MeltIceUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _mirrorCountText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] Button _retryButton;
        [SerializeField] Button _retryButtonGO;
        [SerializeField] Button _menuButton;

        MeltIceGameManager _manager;

        public void Init(MeltIceGameManager manager)
        {
            _manager = manager;

            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);

            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(() => _manager.OnNextStage());
            if (_retryButton != null)
                _retryButton.onClick.AddListener(() => _manager.OnRetry());
            if (_retryButtonGO != null)
                _retryButtonGO.onClick.AddListener(() => _manager.OnRetry());
            if (_menuButton != null)
                _menuButton.onClick.AddListener(() => _manager.OnBackToMenu());
        }

        public void OnStageChanged(int stageNum, int mirrorCount)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {stageNum} / 5";
            UpdateMirrorCount(mirrorCount);
            UpdateScore(0);

            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void UpdateMirrorCount(int count)
        {
            if (_mirrorCountText != null)
                _mirrorCountText.text = $"鏡: {count}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        public void ShowStageClear(int score)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText != null)
                    _stageClearScoreText.text = $"Score: {score}";
            }
        }

        public void ShowGameClear(int totalScore)
        {
            if (_gameClearPanel != null)
            {
                _gameClearPanel.SetActive(true);
                if (_gameClearScoreText != null)
                    _gameClearScoreText.text = $"Total Score: {totalScore}";
            }
        }

        public void ShowGameOver()
        {
            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(true);
        }
    }
}
