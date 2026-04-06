using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game046v2_SqueezePop
{
    public class SqueezePopUI : MonoBehaviour
    {
        [SerializeField] TMP_Text _timerText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] TMP_Text _comboText;
        [SerializeField] TMP_Text _remainingText;
        [SerializeField] TMP_Text _stageText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TMP_Text _stageClearScoreText;
        [SerializeField] TMP_Text _perfectBonusText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TMP_Text _gameOverScoreText;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TMP_Text _allClearScoreText;

        [SerializeField] SqueezePopGameManager _gameManager;

        private Coroutine _comboRoutine;

        private void Start()
        {
            _stageClearPanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
            _allClearPanel?.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
        }

        public void SetupStage(int stage, int total, int targetCount, float timeLimit)
        {
            _stageClearPanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
            _allClearPanel?.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);

            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
            if (_remainingText != null) _remainingText.text = $"残り: {targetCount}";
            if (_scoreText != null) _scoreText.text = "0";
            if (_timerText != null) _timerText.text = $"{timeLimit:F0}";
        }

        public void UpdateHUD(float timeLeft, int score, int combo, int remaining)
        {
            if (_timerText != null)
            {
                _timerText.text = $"{Mathf.Max(timeLeft, 0f):F1}";
                _timerText.color = timeLeft < 10f ? Color.red : Color.white;
            }
            if (_scoreText != null) _scoreText.text = score.ToString();
            if (_remainingText != null) _remainingText.text = $"残り: {remaining}";
        }

        public void ShowComboEffect(int combo, float multiplier)
        {
            if (_comboText == null || combo < 2) return;

            if (_comboRoutine != null) StopCoroutine(_comboRoutine);
            _comboRoutine = StartCoroutine(ComboAnimation(combo, multiplier));
        }

        private IEnumerator ComboAnimation(int combo, float multiplier)
        {
            _comboText.gameObject.SetActive(true);
            _comboText.text = multiplier > 1.5f ? $"×{multiplier:F1} CHAIN!" : $"×{multiplier:F1}";
            _comboText.color = multiplier >= 2.0f ? Color.yellow : new Color(1f, 0.8f, 0.2f);

            float dur = 0.12f;
            float elapsed = 0f;
            Vector3 startScale = Vector3.one * 0.5f;
            Vector3 endScale = Vector3.one * 1.2f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                _comboText.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / dur);
                yield return null;
            }

            yield return new WaitForSeconds(0.6f);

            dur = 0.2f;
            elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(1f, 0f, elapsed / dur);
                _comboText.color = new Color(_comboText.color.r, _comboText.color.g, _comboText.color.b, a);
                yield return null;
            }
            _comboText.gameObject.SetActive(false);
        }

        public void ShowStageClear(int score, bool allPerfect, int timeBonus)
        {
            _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"スコア: {score}";
            if (_perfectBonusText != null)
            {
                _perfectBonusText.gameObject.SetActive(allPerfect);
                if (allPerfect) _perfectBonusText.text = "ALL PERFECT! ×3";
            }
        }

        public void ShowGameOver(int score)
        {
            _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"スコア: {score}";
        }

        public void ShowAllClear(int score)
        {
            _allClearPanel.SetActive(true);
            if (_allClearScoreText != null) _allClearScoreText.text = $"最終スコア: {score}";
        }

        public void OnNextStageButton()
        {
            _gameManager.OnNextStageButton();
        }
    }
}
