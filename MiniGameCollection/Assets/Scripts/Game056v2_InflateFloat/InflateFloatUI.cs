using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game056v2_InflateFloat
{
    public class InflateFloatUI : MonoBehaviour
    {
        [SerializeField] InflateFloatGameManager _gm;
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] Slider _inflateGauge;
        [SerializeField] Image _inflateGaugeFill;
        [SerializeField] Slider _distanceSlider;
        [SerializeField] TextMeshProUGUI _comboText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageBtn;

        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] Button _retryBtn;
        [SerializeField] Button _retryBtn2;
        [SerializeField] Button _menuBtn;
        [SerializeField] Button _menuBtn2;

        void Start()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);

            _nextStageBtn?.onClick.AddListener(() => _gm.OnNextStage());
            _retryBtn?.onClick.AddListener(() => _gm.OnRetry());
            _retryBtn2?.onClick.AddListener(() => _gm.OnRetry());
            _menuBtn?.onClick.AddListener(() => _gm.OnBackToMenu());
            _menuBtn2?.onClick.AddListener(() => _gm.OnBackToMenu());
        }

        public void OnStageChanged(int stage)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / 5";
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
            UpdateScore(0);
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateInflateGauge(float ratio)
        {
            if (_inflateGauge != null) _inflateGauge.value = ratio;
            if (_inflateGaugeFill != null)
                _inflateGaugeFill.color = ratio > 0.8f ? Color.red : ratio > 0.6f ? Color.yellow : new Color(0.3f, 0.9f, 0.3f);
        }

        public void UpdateDistance(float ratio)
        {
            if (_distanceSlider != null) _distanceSlider.value = ratio;
        }

        public void ShowCombo(int count)
        {
            if (_comboText == null) return;
            if (count >= 3)
            {
                _comboText.gameObject.SetActive(true);
                _comboText.text = $"COMBO x{count}!";
                StopCoroutine("HideCombo");
                StartCoroutine("HideCombo");
            }
        }

        IEnumerator HideCombo()
        {
            yield return new WaitForSeconds(1.5f);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
        }

        public void ShowStageClear(int score)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score}";
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
