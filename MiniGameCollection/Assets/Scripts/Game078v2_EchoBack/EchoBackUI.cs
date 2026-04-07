using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game078v2_EchoBack
{
    public class EchoBackUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _phaseText;
        [SerializeField] TextMeshProUGUI _judgementText;
        [SerializeField] TextMeshProUGUI _replayCountText;
        [SerializeField] Image[] _progressDots;
        [SerializeField] Sprite _dotFilled;
        [SerializeField] Sprite _dotEmpty;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        Coroutine _judgementCoroutine;
        Coroutine _pulseCoroutine;

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            _comboText.text = combo >= 2 ? $"x{combo} COMBO" : "";
            if (combo >= 10) _comboText.color = new Color(1f, 0.85f, 0.1f);
            else _comboText.color = Color.white;

            if (combo >= 2)
            {
                if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = StartCoroutine(PulseScale(_comboText.transform, 1.3f, 0.15f));
            }
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdatePhase(string phase)
        {
            if (_phaseText != null) _phaseText.text = phase;
        }

        public void UpdateReplayCount(int remaining)
        {
            if (_replayCountText == null) return;
            _replayCountText.text = remaining < 0 ? "Replay: ∞" : $"Replay: {remaining}";
        }

        public void UpdateProgressDots(int current, int total)
        {
            if (_progressDots == null) return;
            for (int i = 0; i < _progressDots.Length; i++)
            {
                if (_progressDots[i] == null) continue;
                bool visible = i < total;
                _progressDots[i].gameObject.SetActive(visible);
                if (visible)
                    _progressDots[i].sprite = i < current ? _dotFilled : _dotEmpty;
            }
        }

        public void ShowJudgement(string text, Color color)
        {
            if (_judgementText == null) return;
            if (_judgementCoroutine != null) StopCoroutine(_judgementCoroutine);
            _judgementCoroutine = StartCoroutine(ShowJudgementCoroutine(text, color));
        }

        IEnumerator ShowJudgementCoroutine(string text, Color color)
        {
            _judgementText.text = text;
            _judgementText.color = color;
            _judgementText.gameObject.SetActive(true);
            _judgementText.transform.localScale = Vector3.one * 1.2f;

            float dur = 0.8f;
            float elapsed = 0f;
            Vector3 startPos = _judgementText.transform.localPosition;
            Vector3 endPos = startPos + new Vector3(0, 30f, 0);
            while (elapsed < dur)
            {
                float ratio = elapsed / dur;
                float alpha = Mathf.Clamp01(1f - ratio * 1.5f);
                var c = color;
                c.a = alpha;
                _judgementText.color = c;
                _judgementText.transform.localPosition = Vector3.Lerp(startPos, endPos, ratio);
                _judgementText.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 0.9f, ratio);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _judgementText.transform.localPosition = startPos;
            _judgementText.gameObject.SetActive(false);
        }

        IEnumerator PulseScale(Transform t, float targetScale, float duration)
        {
            float half = duration * 0.5f;
            float elapsed = 0f;
            while (elapsed < half)
            {
                t.localScale = Vector3.one * Mathf.Lerp(1f, targetScale, elapsed / half);
                elapsed += Time.deltaTime;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < half)
            {
                t.localScale = Vector3.one * Mathf.Lerp(targetScale, 1f, elapsed / half);
                elapsed += Time.deltaTime;
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        public void ShowStageClear(int stageNum)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearText != null) _stageClearText.text = $"Stage {stageNum} Clear!";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null) _allClearPanel.SetActive(true);
            if (_allClearScoreText != null) _allClearScoreText.text = $"Final Score: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
        }
    }
}
