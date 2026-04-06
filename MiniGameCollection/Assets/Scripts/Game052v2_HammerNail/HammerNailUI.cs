using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game052v2_HammerNail
{
    public class HammerNailUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _missText;
        [SerializeField] TextMeshProUGUI _remainingNailsText;
        [SerializeField] TextMeshProUGUI _judgmentText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] Button _allClearRetryButton;

        [SerializeField] Button _menuButton;

        Coroutine _judgmentCoroutine;
        Coroutine _scoreScaleCoroutine;

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
            if (_scoreScaleCoroutine != null) StopCoroutine(_scoreScaleCoroutine);
            if (_scoreText != null) _scoreScaleCoroutine = StartCoroutine(ScalePulse(_scoreText.transform));
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

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateMiss(int miss, int maxMiss)
        {
            if (_missText != null) _missText.text = $"Miss: {miss} / {maxMiss}";
        }

        public void UpdateRemainingNails(int remaining)
        {
            if (_remainingNailsText != null) _remainingNailsText.text = $"釘: {remaining}";
        }

        public void ShowJudgment(string text, bool isGood)
        {
            if (_judgmentText == null) return;
            if (_judgmentCoroutine != null) StopCoroutine(_judgmentCoroutine);
            _judgmentCoroutine = StartCoroutine(ShowJudgmentCoroutine(text, isGood));
        }

        IEnumerator ShowJudgmentCoroutine(string text, bool isGood)
        {
            _judgmentText.text = text;
            _judgmentText.color = isGood
                ? (text.Contains("PERFECT") ? new Color(1f, 0.9f, 0.1f) : new Color(0.3f, 1f, 0.3f))
                : new Color(1f, 0.3f, 0.3f);
            _judgmentText.gameObject.SetActive(true);

            var rt = _judgmentText.rectTransform;
            rt.localScale = Vector3.one * 1.4f;
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                rt.localScale = Vector3.Lerp(Vector3.one * 1.4f, Vector3.one, elapsed / 0.15f);
                yield return null;
            }
            rt.localScale = Vector3.one;

            yield return new WaitForSeconds(0.5f);

            // Fade out
            elapsed = 0f;
            Color col = _judgmentText.color;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                col.a = 1f - elapsed / 0.3f;
                _judgmentText.color = col;
                yield return null;
            }
            _judgmentText.gameObject.SetActive(false);
        }

        IEnumerator ScalePulse(Transform t)
        {
            Vector3 orig = t.localScale;
            t.localScale = orig * 1.2f;
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(orig * 1.2f, orig, elapsed / 0.15f);
                yield return null;
            }
            t.localScale = orig;
        }

        public void ShowStageClear(int score)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score}";
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
            }
        }

        public void HideGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText != null) _allClearScoreText.text = $"Final Score: {score}";
            }
        }

        public void HideAllClear()
        {
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
        }
    }
}
