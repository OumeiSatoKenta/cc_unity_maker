using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game014v2_MagnetPath
{
    public class MagnetPathUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _switchCountText;
        [SerializeField] TextMeshProUGUI _comboText;

        [Header("StageClear Panel")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearComboText;

        [Header("GameOver Panel")]
        [SerializeField] GameObject _gameOverPanel;

        [Header("Clear Panel")]
        [SerializeField] GameObject _clearPanel;
        [SerializeField] TextMeshProUGUI _clearTotalScoreText;

        [SerializeField] MagnetPathGameManager _gameManager;
        [SerializeField] MagnetManager _magnetManager;

        void Update()
        {
            if (_switchCountText != null && _magnetManager != null)
            {
                _switchCountText.text = $"切替: {_magnetManager.SwitchCount}/{_magnetManager.MaxSwitches}";
            }
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void ShowStageClearPanel(int stageScore, int combo)
        {
            HideAllPanels();
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"+{stageScore}";
            if (_stageClearComboText != null)
            {
                if (combo >= 2)
                    _stageClearComboText.text = $"Combo x{combo}!";
                else
                    _stageClearComboText.text = "";
            }
        }

        public void ShowGameOverPanel()
        {
            HideAllPanels();
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }

        public void ShowClearPanel(int totalScore)
        {
            HideAllPanels();
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearTotalScoreText != null) _clearTotalScoreText.text = $"Total: {totalScore}";
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_clearPanel != null) _clearPanel.SetActive(false);
            if (_comboText != null) _comboText.text = "";
        }

        // Button callbacks
        public void OnNextStageButton() => _gameManager.OnNextStage();
        public void OnRetryButton() => _gameManager.OnRetry();
        public void OnReturnToMenuButton() => _gameManager.OnReturnToMenu();
        public void OnShowInstructionsButton() => _gameManager.ShowInstructions();
        public void OnStartButton()
        {
            if (_magnetManager != null) _magnetManager.LaunchBall();
        }
        public void OnResetButton()
        {
            if (_magnetManager != null) _magnetManager.ResetStage();
        }
    }
}
