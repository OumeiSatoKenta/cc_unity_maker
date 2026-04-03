using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game007v2_NumberFlow
{
    public class NumberFlowUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _progressText;
        [SerializeField] TextMeshProUGUI _timerText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearStarsText;

        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearScoreText;

        float _timerStart;
        bool _timerRunning;

        public void UpdateStage(int current, int total)
        {
            if (_stageText) _stageText.text = $"Stage {current} / {total}";
            _timerStart = Time.time;
            _timerRunning = true;
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText) _comboText.text = combo >= 2 ? $"Combo x{combo}!" : "";
        }

        public void UpdateProgress(int filled, int total)
        {
            if (_progressText) _progressText.text = $"{filled}/{total}マス";
        }

        void Update()
        {
            if (_timerRunning && _timerText)
            {
                float elapsed = Time.time - _timerStart;
                int min = (int)(elapsed / 60);
                int sec = (int)(elapsed % 60);
                _timerText.text = $"{min}:{sec:00}";
            }
        }

        public void ShowStageClearPanel(bool show, int score = 0, int stars = 0)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(show);
            if (show)
            {
                _timerRunning = false;
                if (_stageClearScoreText) _stageClearScoreText.text = $"+{score}";
                if (_stageClearStarsText) _stageClearStarsText.text = stars >= 3 ? "★★★" : stars >= 2 ? "★★☆" : "★☆☆";
            }
        }

        public void ShowGameClearPanel(int totalScore)
        {
            _timerRunning = false;
            if (_gameClearPanel) _gameClearPanel.SetActive(true);
            if (_gameClearScoreText) _gameClearScoreText.text = $"Total: {totalScore}";
        }
    }
}
