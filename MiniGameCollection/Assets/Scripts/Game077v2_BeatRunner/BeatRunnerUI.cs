using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game077v2_BeatRunner
{
    public class BeatRunnerUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _lifeText;
        [SerializeField] TextMeshProUGUI _progressText;
        [SerializeField] TextMeshProUGUI _judgementText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearTitle;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        Coroutine _judgementRoutine;
        Coroutine _comboScaleRoutine;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score\n{score:N0}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText)
            {
                if (combo >= 3)
                {
                    _comboText.text = $"{combo} COMBO";
                    _comboText.gameObject.SetActive(true);
                    if (_comboScaleRoutine != null) StopCoroutine(_comboScaleRoutine);
                    _comboScaleRoutine = StartCoroutine(ScalePulse(_comboText.transform, 1.0f, 1.3f, 0.15f));
                }
                else
                {
                    _comboText.gameObject.SetActive(false);
                }
            }
        }

        public void UpdateLife(int life)
        {
            if (_lifeText)
            {
                string hearts = "";
                for (int i = 0; i < 3; i++)
                    hearts += (i < life) ? "♥ " : "♡ ";
                _lifeText.text = hearts.TrimEnd();
                _lifeText.color = life <= 1 ? new Color(1f, 0.3f, 0.3f) : Color.white;
            }
        }

        public void UpdateProgress(int current, int total)
        {
            if (_progressText) _progressText.text = $"{current}/{total}";
        }

        public void ShowJudgement(string text, Color color)
        {
            if (_judgementText == null) return;
            _judgementText.text = text;
            _judgementText.color = color;
            _judgementText.gameObject.SetActive(true);
            if (_judgementRoutine != null) StopCoroutine(_judgementRoutine);
            _judgementRoutine = StartCoroutine(FadeJudgement());
        }

        IEnumerator FadeJudgement()
        {
            _judgementText.transform.localScale = Vector3.one * 1.3f;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                float t = elapsed / 0.2f;
                _judgementText.transform.localScale = Vector3.one * Mathf.Lerp(1.3f, 1.0f, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _judgementText.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.4f);
            elapsed = 0f;
            Color c = _judgementText.color;
            while (elapsed < 0.3f)
            {
                float t = elapsed / 0.3f;
                _judgementText.color = new Color(c.r, c.g, c.b, 1f - t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _judgementText.gameObject.SetActive(false);
            _judgementText.color = new Color(c.r, c.g, c.b, 1f);
        }

        IEnumerator ScalePulse(Transform t, float from, float to, float duration)
        {
            float e = 0f;
            while (e < duration * 0.5f)
            {
                if (t == null) yield break;
                t.localScale = Vector3.one * Mathf.Lerp(from, to, e / (duration * 0.5f));
                e += Time.deltaTime;
                yield return null;
            }
            e = 0f;
            while (e < duration * 0.5f)
            {
                if (t == null) yield break;
                t.localScale = Vector3.one * Mathf.Lerp(to, from, e / (duration * 0.5f));
                e += Time.deltaTime;
                yield return null;
            }
            if (t != null) t.localScale = Vector3.one;
        }

        public void ShowStageClear(int stageNum)
        {
            if (_stageClearPanel == null) return;
            if (_stageClearTitle) _stageClearTitle.text = $"ステージ {stageNum} クリア！";
            _stageClearPanel.SetActive(true);
        }

        public void HideStageClear()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel == null) return;
            if (_allClearScoreText) _allClearScoreText.text = $"Total Score\n{score:N0}";
            _allClearPanel.SetActive(true);
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel == null) return;
            if (_gameOverScoreText) _gameOverScoreText.text = $"Score\n{score:N0}";
            _gameOverPanel.SetActive(true);
        }
    }
}
