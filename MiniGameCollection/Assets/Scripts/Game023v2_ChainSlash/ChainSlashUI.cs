using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game023v2_ChainSlash
{
    public class ChainSlashUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _timerText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _bonusText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _clearPanel;
        [SerializeField] TextMeshProUGUI _clearScoreText;
        [SerializeField] Button _clearRetryButton;
        [SerializeField] Button _clearMenuButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;
        [SerializeField] Button _menuButton;

        Coroutine _bonusCoroutine;

        public void UpdateStageDisplay(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = score.ToString();
        }

        public void UpdateTimer(float remaining, float total)
        {
            if (_timerText == null) return;
            remaining = Mathf.Max(0f, remaining);
            _timerText.text = Mathf.CeilToInt(remaining).ToString();
            // Red when <= 10s
            _timerText.color = remaining <= 10f ? new Color(1f, 0.3f, 0.3f) : new Color(0.5f, 1f, 0.7f);
        }

        public void UpdateCombo(float multiplier, int streak)
        {
            if (_comboText == null) return;
            if (streak <= 1)
                _comboText.text = "";
            else
                _comboText.text = $"COMBO x{multiplier:F1}";
        }

        public void ShowSlashBonus(int score, int chainCount, bool sameColor)
        {
            if (_bonusCoroutine != null) StopCoroutine(_bonusCoroutine);
            _bonusCoroutine = StartCoroutine(AnimateBonusText(score, chainCount, sameColor));
        }

        IEnumerator AnimateBonusText(int score, int chainCount, bool sameColor)
        {
            if (_bonusText == null) yield break;
            string label = sameColor ? $"+{score}\n同色 x1.5!" : $"+{score}\n{chainCount} Chain!";
            _bonusText.text = label;
            _bonusText.gameObject.SetActive(true);
            _bonusText.transform.localScale = Vector3.one * 1.5f;

            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.6f;
                float scale = Mathf.Lerp(1.5f, 0.8f, ratio);
                _bonusText.transform.localScale = Vector3.one * scale;
                float alpha = 1f - Mathf.Pow(ratio, 2f);
                _bonusText.color = new Color(1f, 0.9f, 0.3f, alpha);
                yield return null;
            }
            _bonusText.gameObject.SetActive(false);
        }

        public void ShowStageClearPanel(int score, int maxChain)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText != null)
                    _stageClearScoreText.text = $"Score: {score}  MaxChain: {maxChain}";
            }
        }

        public void ShowClearPanel(int totalScore)
        {
            if (_clearPanel != null)
            {
                _clearPanel.SetActive(true);
                if (_clearScoreText != null)
                    _clearScoreText.text = $"Total: {totalScore}pt";
            }
        }

        public void ShowGameOverPanel(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null)
                    _gameOverScoreText.text = $"Score: {score}pt";
            }
        }
    }
}
