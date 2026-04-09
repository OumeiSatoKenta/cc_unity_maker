using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game096v2_DualControl
{
    public class DualControlUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _synchroBonusText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _synchroText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;

        [SerializeField] Button _menuButton;
        [SerializeField] DualControlGameManager _gameManager;

        void Start()
        {
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(() => _gameManager.NextStage());
            if (_retryButton != null)
                _retryButton.onClick.AddListener(() => _gameManager.RestartGame());

            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_synchroBonusText != null) _synchroBonusText.gameObject.SetActive(false);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        public void ShowStageClear(int stageNum, int score, bool isSynchro)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null)
                _stageClearScoreText.text = $"Score: {score}";
            if (_synchroText != null)
            {
                _synchroText.text = isSynchro ? "SYNCHRO BONUS! x2.0" : "ステージクリア！";
                _synchroText.color = isSynchro ? new Color(1f, 0.85f, 0.2f) : Color.white;
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null) _allClearPanel.SetActive(true);
            if (_allClearScoreText != null)
                _allClearScoreText.text = $"Final Score: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null)
                _gameOverScoreText.text = $"Score: {score}";
        }

        public void HideGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
