using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game011v2_FoldPaper
{
    public class FoldPaperUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _movesText;
        [SerializeField] TextMeshProUGUI _undoText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _timerText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearStarsText;

        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearScoreText;

        [SerializeField] TextMeshProUGUI _retryMessageText;
        [SerializeField] Button _undoButton;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage + 1} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateMoves(int movesLeft, int maxMoves)
        {
            if (_movesText)
            {
                _movesText.text = $"手数: {movesLeft}";
                _movesText.color = movesLeft <= 3 ? new Color(1f, 0.3f, 0.3f) : new Color(0.8f, 1f, 0.8f);
            }
        }

        public void UpdateUndo(int undoLeft)
        {
            if (_undoText) _undoText.text = $"Undo: {undoLeft}";
            if (_undoButton) _undoButton.interactable = undoLeft > 0;
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText)
            {
                _comboText.text = combo >= 2 ? $"COMBO ×{combo}" : "";
            }
        }

        public void UpdateTimer(float seconds)
        {
            if (_timerText)
            {
                int s = Mathf.CeilToInt(seconds);
                _timerText.text = seconds > 0 ? $"⏱ {s}s" : "";
                _timerText.color = seconds <= 10f ? new Color(1f, 0.3f, 0.3f) : Color.white;
            }
        }

        public void ShowStageClearPanel(bool show, int score = 0, int stars = 0)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(show);
            if (show)
            {
                if (_stageClearScoreText) _stageClearScoreText.text = $"+{score}";
                if (_stageClearStarsText)
                {
                    string starStr = "";
                    for (int i = 0; i < 3; i++)
                        starStr += i < stars ? "★" : "☆";
                    _stageClearStarsText.text = starStr;
                }
            }
        }

        public void ShowGameClearPanel(int totalScore)
        {
            if (_gameClearPanel) _gameClearPanel.SetActive(true);
            if (_gameClearScoreText) _gameClearScoreText.text = $"Total: {totalScore}";
        }

        public void ShowRetryMessage()
        {
            if (_retryMessageText)
            {
                _retryMessageText.text = "手数オーバー！　リセットして再挑戦";
                _retryMessageText.gameObject.SetActive(true);
            }
        }
    }
}
