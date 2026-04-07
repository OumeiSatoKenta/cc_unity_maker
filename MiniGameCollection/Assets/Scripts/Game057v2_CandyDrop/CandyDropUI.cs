using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game057v2_CandyDrop
{
    public class CandyDropUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] Slider _heightGauge;
        [SerializeField] Image _nextCandyImage;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;
        [SerializeField] Button _backToMenuButton;
        [SerializeField] Button _backToMenuButton2;

        public void Init(CandyDropGameManager gm)
        {
            if (_nextStageButton != null) _nextStageButton.onClick.AddListener(gm.OnNextStage);
            if (_retryButton != null) _retryButton.onClick.AddListener(gm.OnRetry);
            if (_backToMenuButton != null) _backToMenuButton.onClick.AddListener(gm.OnBackToMenu);
            if (_backToMenuButton2 != null) _backToMenuButton2.onClick.AddListener(gm.OnBackToMenu);
        }

        public void OnStageChanged(int stageNumber)
        {
            if (_stageText != null) _stageText.text = $"Stage {stageNumber} / 5";
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_comboText != null) _comboText.text = "";
            if (_heightGauge != null) _heightGauge.value = 0f;
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            if (combo <= 0)
            {
                _comboText.text = "";
                return;
            }
            _comboText.text = combo >= 5 ? $"Combo x{combo}! (x{multiplier:F1})" : $"Combo x{combo}";
            StartCoroutine(ComboPopAnimation());
        }

        IEnumerator ComboPopAnimation()
        {
            if (_comboText == null) yield break;
            Vector3 orig = _comboText.transform.localScale;
            _comboText.transform.localScale = orig * 1.5f;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                _comboText.transform.localScale = Vector3.Lerp(orig * 1.5f, orig, elapsed / 0.2f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _comboText.transform.localScale = orig;
        }

        public void UpdateHeightGauge(float ratio)
        {
            if (_heightGauge != null) _heightGauge.value = Mathf.Clamp01(ratio);
        }

        public void UpdateNextPreview(Sprite sprite, Color color)
        {
            if (_nextCandyImage == null) return;
            _nextCandyImage.sprite = sprite;
            _nextCandyImage.color = sprite != null ? color : Color.clear;
        }

        public void ShowStageClear(int score)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score}";
        }

        public void ShowGameClear(int score)
        {
            if (_gameClearPanel != null) _gameClearPanel.SetActive(true);
            if (_gameClearScoreText != null) _gameClearScoreText.text = $"Final Score: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
        }
    }
}
