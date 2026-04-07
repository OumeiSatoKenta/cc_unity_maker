using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game026v2_SliceNinja
{
    public class SliceNinjaUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _missText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] TextMeshProUGUI _gameOverMaxComboText;

        SliceNinjaGameManager _gameManager;
        Coroutine _pulseCoroutine;

        public void Initialize(SliceNinjaGameManager gm)
        {
            _gameManager = gm;
            UpdateScore(0);
            UpdateCombo(1f);
            UpdateMiss(0, 3);
            HideStageClear();
            if (_finalClearPanel) _finalClearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
            StartPulse(_scoreText);
        }

        public void UpdateCombo(float multiplier)
        {
            if (_comboText)
            {
                if (multiplier <= 1f)
                    _comboText.text = "";
                else
                    _comboText.text = $"x{multiplier:F1} COMBO!";
            }
        }

        public void UpdateMiss(int count, int max)
        {
            if (_missText)
            {
                string hearts = "";
                for (int i = 0; i < max; i++)
                    hearts += (i < max - count) ? "● " : "× ";
                _missText.text = hearts.TrimEnd();
            }
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void ShowStageClear(int stage)
        {
            if (_stageClearPanel)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearText)
                    _stageClearText.text = $"Stage {stage} Clear!";
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowFinalClear(int score)
        {
            if (_finalClearPanel)
            {
                _finalClearPanel.SetActive(true);
                if (_finalScoreText) _finalScoreText.text = $"Score: {score}";
            }
        }

        public void ShowGameOver(int score, int maxCombo)
        {
            if (_gameOverPanel)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText) _gameOverScoreText.text = $"Score: {score}";
                if (_gameOverMaxComboText) _gameOverMaxComboText.text = $"Max Combo: x{maxCombo / 10f:F1}";
            }
        }

        void StartPulse(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;
            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = StartCoroutine(PulseText(tmp));
        }

        IEnumerator PulseText(TextMeshProUGUI tmp)
        {
            if (tmp == null) yield break;
            Vector3 orig = tmp.transform.localScale;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float s = 1f + 0.3f * Mathf.Sin(t / 0.2f * Mathf.PI);
                tmp.transform.localScale = orig * s;
                yield return null;
            }
            tmp.transform.localScale = orig;
            _pulseCoroutine = null;
        }

        public void OnRestartButton()
        {
            if (_gameManager) _gameManager.RestartGame();
        }

        public void OnReturnToMenuButton()
        {
            if (_gameManager) _gameManager.ReturnToMenu();
        }
    }
}
