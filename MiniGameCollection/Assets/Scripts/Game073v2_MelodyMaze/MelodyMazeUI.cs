using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game073v2_MelodyMaze
{
    public class MelodyMazeUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _timerText;
        [SerializeField] TextMeshProUGUI _previewText;
        [SerializeField] TextMeshProUGUI _judgementText;
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearStageText;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        Coroutine _judgementCoroutine;
        Coroutine _comboCoroutine;

        void Start()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_judgementText != null) _judgementText.gameObject.SetActive(false);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = score.ToString();
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo <= 0)
            {
                _comboText.text = "";
                return;
            }
            _comboText.text = $"x{combo} COMBO";
            if (_comboCoroutine != null) StopCoroutine(_comboCoroutine);
            _comboCoroutine = StartCoroutine(ComboPopAnim());
        }

        public void UpdateTimer(float remaining)
        {
            if (_timerText == null) return;
            remaining = Mathf.Max(0f, remaining);
            _timerText.text = Mathf.CeilToInt(remaining).ToString();
            _timerText.color = remaining <= 10f ? new Color(1f, 0.3f, 0.3f) : Color.white;
        }

        public void UpdatePreviewPlays(int remaining, int max)
        {
            if (_previewText == null) return;
            if (max < 0)
                _previewText.text = "♪ お手本";
            else
                _previewText.text = $"♪ 残り{remaining}回";
        }

        public void ShowJudgement(string text, Color color)
        {
            if (_judgementText == null) return;
            if (_judgementCoroutine != null) StopCoroutine(_judgementCoroutine);
            _judgementCoroutine = StartCoroutine(JudgementAnim(text, color));
        }

        IEnumerator JudgementAnim(string text, Color color)
        {
            _judgementText.gameObject.SetActive(true);
            _judgementText.text = text;
            _judgementText.color = color;
            _judgementText.transform.localScale = Vector3.one * 1.3f;

            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1.3f, 1.0f, t / 0.15f);
                _judgementText.transform.localScale = Vector3.one * Mathf.Clamp(s, 1.0f, 1.3f);
                float alpha = t < 0.4f ? 1f : 1f - (t - 0.4f) / 0.2f;
                _judgementText.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }
            _judgementText.gameObject.SetActive(false);
        }

        IEnumerator ComboPopAnim()
        {
            if (_comboText == null) yield break;
            _comboText.transform.localScale = Vector3.one * 1.4f;
            _comboText.color = new Color(0f, 1f, 1f);

            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1.4f, 1.0f, t / 0.2f);
                _comboText.transform.localScale = Vector3.one * s;
                yield return null;
            }
            _comboText.transform.localScale = Vector3.one;
            _comboText.color = new Color(0.9f, 0.5f, 1f);
        }

        public void ShowStageClear(int stageNum)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearStageText != null)
                    _stageClearStageText.text = $"Stage {stageNum} クリア！";
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText != null)
                    _allClearScoreText.text = $"スコア: {score}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null)
                    _gameOverScoreText.text = $"スコア: {score}";
            }
        }
    }
}
