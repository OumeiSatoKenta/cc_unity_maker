using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game041v2_StackJump
{
    public class StackJumpUI : MonoBehaviour
    {
        [SerializeField] StackJumpGameManager _gameManager;

        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] Slider _progressSlider;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _perfectText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        public void UpdateStage(int stage)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / 5";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateStackCount(int count, int target)
        {
            if (_progressSlider != null)
            {
                _progressSlider.maxValue = target;
                _progressSlider.value = count;
            }
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo >= 2)
            {
                _comboText.gameObject.SetActive(true);
                _comboText.text = $"x{combo} COMBO!";
                StopCoroutine(nameof(ComboScalePulse));
                StartCoroutine(ComboScalePulse());
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        public void ShowPerfect()
        {
            if (_perfectText == null) return;
            StopCoroutine(nameof(PerfectFade));
            StartCoroutine(PerfectFade());
        }

        IEnumerator PerfectFade()
        {
            _perfectText.gameObject.SetActive(true);
            _perfectText.alpha = 1f;
            _perfectText.transform.localScale = Vector3.one * 1.5f;
            float elapsed = 0f;
            while (elapsed < 0.8f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.8f;
                _perfectText.alpha = 1f - t;
                _perfectText.transform.localScale = Vector3.one * (1.5f + t * 0.5f);
                yield return null;
            }
            _perfectText.gameObject.SetActive(false);
        }

        IEnumerator ComboScalePulse()
        {
            if (_comboText == null) yield break;
            float elapsed = 0f;
            while (elapsed < 0.25f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.25f;
                float scale = 1f + 0.3f * Mathf.Sin(t * Mathf.PI);
                _comboText.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            if (_comboText != null) _comboText.transform.localScale = Vector3.one;
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
                if (_finalScoreText != null) _finalScoreText.text = $"最終スコア: {score}";
            }
        }

        public void ShowGameOverPanel(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null) _gameOverScoreText.text = $"スコア: {score}";
            }
        }

        public void OnNextStageButton()
        {
            _gameManager?.AdvanceToNextStage();
        }

        public void OnRetryButton()
        {
            _gameManager?.RetryGame();
        }

        public void OnReturnToMenuButton()
        {
            _gameManager?.ReturnToMenu();
        }
    }
}
