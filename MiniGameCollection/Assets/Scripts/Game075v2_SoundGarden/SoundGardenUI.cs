using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game075v2_SoundGarden
{
    public class SoundGardenUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _timerText;
        [SerializeField] TextMeshProUGUI _judgementText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        Coroutine _judgementCoroutine;
        Coroutine _comboCoroutine;

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score\n{score:N0}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo < 2)
            {
                _comboText.text = "";
                return;
            }
            _comboText.text = $"{combo} Combo!";
            if (_comboCoroutine != null) StopCoroutine(_comboCoroutine);
            _comboCoroutine = StartCoroutine(ComboAnim());
        }

        IEnumerator ComboAnim()
        {
            if (_comboText == null) yield break;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1.4f, 1f, t / 0.2f);
                _comboText.transform.localScale = Vector3.one * s;
                yield return null;
            }
            _comboText.transform.localScale = Vector3.one;
        }

        public void UpdateTimer(float seconds)
        {
            if (_timerText == null) return;
            int s = Mathf.CeilToInt(seconds);
            _timerText.text = $"{s}";
            _timerText.color = s <= 10 ? new Color(1f, 0.3f, 0.3f) : Color.white;
        }

        public void ShowJudgement(string text, Color color)
        {
            if (_judgementText == null) return;
            if (_judgementCoroutine != null) StopCoroutine(_judgementCoroutine);
            _judgementCoroutine = StartCoroutine(JudgementAnim(text, color));
        }

        IEnumerator JudgementAnim(string text, Color color)
        {
            _judgementText.text = text;
            _judgementText.color = color;
            _judgementText.gameObject.SetActive(true);
            _judgementText.transform.localScale = Vector3.one * 1.4f;

            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                _judgementText.transform.localScale = Vector3.one * Mathf.Lerp(1.4f, 1f, t / 0.15f);
                yield return null;
            }
            _judgementText.transform.localScale = Vector3.one;

            yield return new WaitForSeconds(0.6f);

            t = 0f;
            Color c = color;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / 0.3f);
                _judgementText.color = c;
                yield return null;
            }
            _judgementText.gameObject.SetActive(false);
        }

        public void ShowStageClear(int stage)
        {
            if (_stageClearPanel == null) return;
            if (_stageClearText != null)
                _stageClearText.text = $"Stage {stage} クリア！";
            _stageClearPanel.SetActive(true);
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null)
                _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel == null) return;
            if (_allClearScoreText != null)
                _allClearScoreText.text = $"スコア: {score:N0}\nハーモニー完成！";
            _allClearPanel.SetActive(true);
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel == null) return;
            if (_gameOverScoreText != null)
                _gameOverScoreText.text = $"スコア: {score:N0}";
            _gameOverPanel.SetActive(true);
        }
    }
}
