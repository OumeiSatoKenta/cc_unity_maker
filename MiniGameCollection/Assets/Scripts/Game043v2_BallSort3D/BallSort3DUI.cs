using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game043v2_BallSort3D
{
    public class BallSort3DUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _moveCountText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _timerText;
        [SerializeField] GameObject _timerPanel;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;
        [SerializeField] Button _retryButtonFinal;
        [SerializeField] Button _menuButtonFinal;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButtonGameOver;
        [SerializeField] Button _menuButtonGameOver;

        [SerializeField] BallSort3DGameManager _gameManager;
        [SerializeField] BallSort3DMechanic _mechanic;

        public void UpdateStage(int stage)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / 5";
            if (_timerPanel != null) _timerPanel.SetActive(stage == 5);
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateMoveCount(int moves)
        {
            if (_moveCountText != null) _moveCountText.text = $"Moves: {moves}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo >= 2)
            {
                _comboText.text = $"Combo x{combo}!";
                _comboText.gameObject.SetActive(true);
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        public void UpdateTimer(float seconds)
        {
            if (_timerText != null)
            {
                int s = Mathf.CeilToInt(Mathf.Max(0f, seconds));
                _timerText.text = $"Time: {s}";
                _timerText.color = seconds < 20f ? Color.red : Color.white;
            }
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_finalClearPanel != null) _finalClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void ShowStageClearPanel(int score)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score}";
            }
        }

        public void ShowFinalClearPanel(int score)
        {
            if (_finalClearPanel != null)
            {
                _finalClearPanel.SetActive(true);
                if (_finalScoreText != null) _finalScoreText.text = $"Final Score: {score}";
            }
        }

        public void ShowGameOverPanel(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
            }
        }

        public void OnUndoButtonClicked()
        {
            if (_mechanic != null) _mechanic.UndoLastMove();
        }

        public void OnResetButtonClicked()
        {
            if (_gameManager != null) _gameManager.RetryGame();
        }

        public void OnNextStageClicked()
        {
            if (_gameManager != null) _gameManager.AdvanceToNextStage();
        }

        public void OnRetryClicked()
        {
            if (_gameManager != null) _gameManager.RetryGame();
        }

        public void OnMenuClicked()
        {
            if (_gameManager != null) _gameManager.ReturnToMenu();
        }
    }
}
