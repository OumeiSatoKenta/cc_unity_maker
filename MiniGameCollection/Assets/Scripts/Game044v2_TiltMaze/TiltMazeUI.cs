using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game044v2_TiltMaze
{
    public class TiltMazeUI : MonoBehaviour
    {
        [SerializeField] TiltMazeGameManager _gameManager;

        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _timerText;
        [SerializeField] TextMeshProUGUI _lifeText;
        [SerializeField] TextMeshProUGUI _coinText;
        [SerializeField] Slider _brakeSlider;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearBonusText;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        public void UpdateStage(int stage)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / 5";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateTimer(float seconds)
        {
            if (_timerText)
            {
                int s = Mathf.CeilToInt(seconds);
                _timerText.text = $"Time: {s}";
                _timerText.color = s <= 10 ? Color.red : Color.white;
            }
        }

        public void UpdateLife(int life)
        {
            if (_lifeText)
            {
                string hearts = "";
                for (int i = 0; i < 3; i++)
                    hearts += i < life ? "♥ " : "♡ ";
                _lifeText.text = hearts.Trim();
            }
        }

        public void UpdateCoins(int collected, int total)
        {
            if (_coinText)
                _coinText.text = total > 0 ? $"Coin: {collected}/{total}" : "";
        }

        public void UpdateBrakeGauge(float ratio)
        {
            if (_brakeSlider) _brakeSlider.value = ratio;
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_finalClearPanel) _finalClearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
        }

        public void ShowStageClearPanel(int score, bool noMiss, bool allCoins)
        {
            if (_stageClearPanel)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText) _stageClearScoreText.text = $"Score: {score}";
                if (_stageClearBonusText)
                {
                    string bonus = "";
                    if (noMiss) bonus += "ノーミス! ";
                    if (allCoins) bonus += "全コイン獲得! ×2倍!";
                    _stageClearBonusText.text = bonus;
                }
            }
        }

        public void ShowFinalClearPanel(int score)
        {
            if (_finalClearPanel)
            {
                _finalClearPanel.SetActive(true);
                if (_finalScoreText) _finalScoreText.text = $"Final Score: {score}";
            }
        }

        public void ShowGameOverPanel(int score)
        {
            if (_gameOverPanel)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText) _gameOverScoreText.text = $"Score: {score}";
            }
        }
    }
}
