using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game021v2_BladeDash
{
    public class BladeDashUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _targetScoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _nearMissText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearComboText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _clearPanel;
        [SerializeField] TextMeshProUGUI _clearScoreText;
        [SerializeField] Button _clearRetryButton;
        [SerializeField] Button _clearMenuButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;
        [SerializeField] Button _menuButton;

        Coroutine _nearMissCoroutine;

        public void UpdateStageDisplay(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score, int target)
        {
            if (_scoreText) _scoreText.text = $"{score}";
            if (_targetScoreText) _targetScoreText.text = $"/ {target}";
        }

        public void UpdateCombo(float multiplier)
        {
            if (_comboText)
            {
                _comboText.text = multiplier > 1f ? $"x{multiplier:F1}" : "";
                _comboText.color = multiplier >= 3f ? Color.red : multiplier >= 2f ? Color.yellow : Color.white;
            }
        }

        public void ShowNearMissBonus(int bonus)
        {
            if (_nearMissCoroutine != null) StopCoroutine(_nearMissCoroutine);
            _nearMissCoroutine = StartCoroutine(NearMissAnim(bonus));
        }

        IEnumerator NearMissAnim(int bonus)
        {
            if (_nearMissText == null) yield break;
            _nearMissText.text = $"NEAR MISS! +{bonus}";
            _nearMissText.color = new Color(1f, 1f, 0f, 1f);
            _nearMissText.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < 1.2f)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / 1.2f);
                _nearMissText.color = new Color(1f, 1f, 0f, alpha);
                yield return null;
            }
            _nearMissText.gameObject.SetActive(false);
        }

        public void ShowStageClearPanel(int comboCount)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearComboText) _stageClearComboText.text = $"コンボ: {comboCount}";
        }

        public void ShowClearPanel(int totalScore)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearScoreText) _clearScoreText.text = $"Total: {totalScore}pt";
        }

        public void ShowGameOverPanel(int score)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText) _gameOverScoreText.text = $"Score: {score}pt";
        }
    }
}
