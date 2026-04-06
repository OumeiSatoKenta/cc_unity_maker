using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game058v2_ThreadNeedle
{
    public class ThreadNeedleUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _roundText;
        [SerializeField] TextMeshProUGUI _judgementText;
        [SerializeField] Image[] _missIndicators;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;

        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        ThreadNeedleGameManager _gameManager;
        Coroutine _judgementCoroutine;

        public void Init(ThreadNeedleGameManager gm)
        {
            _gameManager = gm;
            if (_judgementText != null) _judgementText.gameObject.SetActive(false);
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
            UpdateMiss(0);
        }

        public void OnStageChanged(int stageNumber)
        {
            if (_stageText != null) _stageText.text = $"Stage {stageNumber} / 5";
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            if (combo < 2)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            _comboText.text = $"Combo x{combo}  ×{multiplier:F1}";
        }

        public void UpdateMiss(int missCount)
        {
            if (_missIndicators == null) return;
            for (int i = 0; i < _missIndicators.Length; i++)
            {
                if (_missIndicators[i] != null)
                    _missIndicators[i].color = i < missCount
                        ? new Color(0.9f, 0.2f, 0.2f, 1f)
                        : new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
        }

        public void UpdateRound(int current, int total)
        {
            if (_roundText != null) _roundText.text = $"Round {current} / {total}";
        }

        public void ShowJudgement(string text, Color color)
        {
            if (_judgementText == null) return;
            if (_judgementCoroutine != null) StopCoroutine(_judgementCoroutine);
            _judgementCoroutine = StartCoroutine(ShowJudgementRoutine(text, color));
        }

        IEnumerator ShowJudgementRoutine(string text, Color color)
        {
            _judgementText.gameObject.SetActive(true);
            _judgementText.text = text;
            _judgementText.color = color;
            _judgementText.transform.localScale = Vector3.one * 1.3f;

            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                _judgementText.transform.localScale = Vector3.Lerp(
                    Vector3.one * 1.3f, Vector3.one, elapsed / 0.15f);
                yield return null;
            }
            _judgementText.transform.localScale = Vector3.one;

            yield return new WaitForSeconds(0.7f);

            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                Color c = _judgementText.color;
                c.a = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                _judgementText.color = c;
                yield return null;
            }
            _judgementText.gameObject.SetActive(false);
        }

        public void ShowStageClear(int score)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText != null) _stageClearScoreText.text = $"Stage Score: {score}";
            }
        }

        public void ShowGameClear(int totalScore)
        {
            if (_gameClearPanel != null)
            {
                _gameClearPanel.SetActive(true);
                if (_gameClearScoreText != null) _gameClearScoreText.text = $"Total Score: {totalScore}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
            }
        }

        // Button callbacks
        public void OnNextStageClicked() => _gameManager?.OnNextStage();
        public void OnRetryClicked() => _gameManager?.OnRetry();
        public void OnBackToMenuClicked() => _gameManager?.OnBackToMenu();
    }
}
