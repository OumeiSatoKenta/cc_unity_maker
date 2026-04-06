using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game055v2_DustSweep
{
    public class DustSweepUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _timerText;
        [SerializeField] Slider _cleanlinessSlider;
        [SerializeField] TextMeshProUGUI _cleanlinessText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _penaltyText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearScoreText;
        [SerializeField] Button _gameClearRetryButton;
        [SerializeField] Button _gameClearMenuButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] Button _retryButton;
        [SerializeField] Button _menuButton;

        [SerializeField] DustSweepGameManager _gm;

        void Awake()
        {
            _stageClearPanel?.SetActive(false);
            _gameClearPanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
            if (_penaltyText != null) _penaltyText.gameObject.SetActive(false);

            // Button listeners are wired via SceneSetup (AddPersistentListener) to avoid duplicates
        }

        public void OnStageChanged(int stage)
        {
            _stageClearPanel?.SetActive(false);
            if (_stageText != null) _stageText.text = $"Stage {stage} / 5";
            UpdateCleanliness(0f);
        }

        public void UpdateCleanliness(float ratio)
        {
            if (_cleanlinessSlider != null) _cleanlinessSlider.value = ratio;
            if (_cleanlinessText != null) _cleanlinessText.text = $"{Mathf.RoundToInt(ratio * 100)}%";
        }

        public void UpdateTimer(float remaining)
        {
            if (_timerText != null)
            {
                int sec = Mathf.CeilToInt(remaining);
                _timerText.text = sec.ToString();
                _timerText.color = remaining <= 10f ? Color.red : Color.white;
            }
        }

        public void ShowCombo(int combo)
        {
            if (_comboText == null) return;
            StopCoroutine(nameof(HideCombo));
            _comboText.gameObject.SetActive(true);
            _comboText.text = combo >= 3 ? $"COMBO x{combo}!" : $"Item Found!";
            StartCoroutine(nameof(HideCombo));
        }

        IEnumerator HideCombo()
        {
            yield return new WaitForSeconds(1.5f);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
        }

        public void ShowPenalty()
        {
            if (_penaltyText == null) return;
            StopCoroutine(nameof(HidePenalty));
            _penaltyText.gameObject.SetActive(true);
            _penaltyText.text = "-5秒！";
            StartCoroutine(nameof(HidePenalty));
        }

        IEnumerator HidePenalty()
        {
            yield return new WaitForSeconds(1.5f);
            if (_penaltyText != null) _penaltyText.gameObject.SetActive(false);
        }

        public void ShowStageClear(int score)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score}";
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void ShowGameClear(int score)
        {
            if (_gameClearPanel != null) _gameClearPanel.SetActive(true);
            if (_gameClearScoreText != null) _gameClearScoreText.text = $"Total Score: {score}";
        }

        public void ShowGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }
    }
}
