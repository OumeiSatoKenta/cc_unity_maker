using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game009v2_ColorMix
{
    public class ColorMixUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private TextMeshProUGUI _judgeCountText;

        [Header("Sliders")]
        [SerializeField] private GameObject _brightnessSliderGroup;

        [Header("Stage Clear Panel")]
        [SerializeField] private GameObject _stageClearPanel;
        [SerializeField] private TextMeshProUGUI _stageClearTitle;
        [SerializeField] private TextMeshProUGUI _stageClearScore;
        [SerializeField] private TextMeshProUGUI _stageClearStars;
        [SerializeField] private TextMeshProUGUI _stageClearDeltaE;

        [Header("Game Clear Panel")]
        [SerializeField] private GameObject _gameClearPanel;
        [SerializeField] private TextMeshProUGUI _gameClearScore;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject _gameOverPanel;

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score:N0}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText != null)
            {
                _comboText.gameObject.SetActive(combo > 1);
                _comboText.text = combo > 1 ? $"Combo ×{combo}" : "";
            }
        }

        public void ShowJudgeCount(bool show, int max, int left)
        {
            if (_judgeCountText == null) return;
            _judgeCountText.gameObject.SetActive(show && max > 0);
            if (show && max > 0)
                _judgeCountText.text = $"残り判定: {left}回";
        }

        public void SetupSliders(bool showBrightness)
        {
            if (_brightnessSliderGroup != null)
                _brightnessSliderGroup.SetActive(showBrightness);
        }

        public void ShowStageClearPanel(int stage, int score, int stars, int deltaE)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearTitle != null) _stageClearTitle.text = $"Stage {stage} クリア！";
            if (_stageClearScore != null) _stageClearScore.text = $"Score: {score:N0}";
            if (_stageClearDeltaE != null) _stageClearDeltaE.text = $"色差: {deltaE}";
            if (_stageClearStars != null)
                _stageClearStars.text = stars == 3 ? "★★★" : stars == 2 ? "★★☆" : "★☆☆";
        }

        public void ShowClearPanel(int score)
        {
            if (_gameClearPanel != null) _gameClearPanel.SetActive(true);
            if (_gameClearScore != null) _gameClearScore.text = $"Total Score: {score:N0}";
        }

        public void ShowGameOverPanel()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
