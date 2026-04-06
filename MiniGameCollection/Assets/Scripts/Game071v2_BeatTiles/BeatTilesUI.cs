using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game071v2_BeatTiles
{
    public class BeatTilesUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _judgementText;
        [SerializeField] Slider _lifeSlider;
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        Coroutine _judgeFade;

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = score.ToString("N0");
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            _comboText.text = combo > 1 ? $"x{combo} COMBO" : "";
            if (combo > 1)
                StartCoroutine(ComboAnim());
        }

        IEnumerator ComboAnim()
        {
            if (_comboText == null) yield break;
            Vector3 orig = _comboText.transform.localScale;
            _comboText.transform.localScale = orig * 1.3f;
            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float s = 1.3f - 0.3f * (t / 0.15f);
                _comboText.transform.localScale = orig * s;
                yield return null;
            }
            _comboText.transform.localScale = orig;
        }

        public void UpdateLife(float ratio)
        {
            if (_lifeSlider != null) _lifeSlider.value = ratio;
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void ShowJudgement(string text, Color color)
        {
            if (_judgementText == null) return;
            if (_judgeFade != null) StopCoroutine(_judgeFade);
            _judgeFade = StartCoroutine(FadeJudgement(text, color));
        }

        IEnumerator FadeJudgement(string text, Color color)
        {
            _judgementText.text = text;
            _judgementText.color = color;
            _judgementText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                color.a = 1f - t / 0.3f;
                _judgementText.color = color;
                yield return null;
            }
            _judgementText.gameObject.SetActive(false);
        }

        public void ShowStageClear(int nextStage)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearText != null) _stageClearText.text = $"ステージ{nextStage - 1}クリア！";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int finalScore)
        {
            if (_allClearPanel != null) _allClearPanel.SetActive(true);
            if (_allClearScoreText != null) _allClearScoreText.text = $"スコア: {finalScore:N0}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"スコア: {score:N0}";
        }
    }
}
