using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game042v2_ColorDrop
{
    public class ColorDropUI : MonoBehaviour
    {
        [SerializeField] ColorDropGameManager _gameManager;

        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] Slider _progressSlider;
        [SerializeField] TextMeshProUGUI _livesText;
        [SerializeField] TextMeshProUGUI _comboText;

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

        public void UpdateLives(int lives)
        {
            if (_livesText != null)
            {
                string hearts = "";
                for (int i = 0; i < 3; i++)
                    hearts += i < lives ? "★" : "☆";
                _livesText.text = hearts;
            }
        }

        public void UpdateProgress(int processed, int target)
        {
            if (_progressSlider != null)
            {
                _progressSlider.maxValue = target;
                _progressSlider.value = processed;
            }
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo >= 2)
            {
                _comboText.gameObject.SetActive(true);
                float multiplier = combo >= 20 ? 5f : combo >= 10 ? 3f : combo >= 5 ? 2f : 1f;
                _comboText.text = multiplier > 1f ? $"x{combo} COMBO! (×{multiplier})" : $"x{combo} COMBO!";
                StopCoroutine(nameof(ComboScalePulse));
                StartCoroutine(ComboScalePulse());
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        IEnumerator ComboScalePulse()
        {
            if (_comboText == null) yield break;
            float elapsed = 0f;
            while (elapsed < 0.25f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.25f;
                float scale = 1f + 0.35f * Mathf.Sin(t * Mathf.PI);
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
