using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game081v2_PetBonsai
{
    public class PetBonsaiUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _beautyText;
        [SerializeField] Slider _growthSlider;
        [SerializeField] TextMeshProUGUI _waterText;
        [SerializeField] TextMeshProUGUI _seasonText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _feedbackText;
        [SerializeField] TextMeshProUGUI _rivalText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        Coroutine _feedbackCoroutine;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateBeautyScore(int score)
        {
            if (_beautyText != null)
            {
                _beautyText.text = $"美しさ: {score}";
                _beautyText.color = score >= 85 ? Color.green : Color.white;
                StartCoroutine(PulseText(_beautyText.transform));
            }
        }

        public void UpdateGrowth(float ratio)
        {
            if (_growthSlider != null) _growthSlider.value = ratio;
        }

        public void UpdateWater(int current, int max)
        {
            if (_waterText != null) _waterText.text = $"水やり: {current}/{max}";
        }

        public void UpdateSeason(string season)
        {
            if (_seasonText != null)
            {
                _seasonText.text = $"季節: {season}";
                Color col = season switch
                {
                    "春" => new Color(1f, 0.7f, 0.8f),
                    "夏" => new Color(0.3f, 0.9f, 0.4f),
                    "秋" => new Color(1f, 0.6f, 0.2f),
                    "冬" => new Color(0.7f, 0.9f, 1f),
                    _ => Color.white
                };
                _seasonText.color = col;
            }
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText != null)
            {
                if (combo >= 3)
                {
                    _comboText.text = $"COMBO x{combo}!";
                    _comboText.color = new Color(1f, 0.9f, 0.2f);
                    _comboText.gameObject.SetActive(true);
                    StartCoroutine(PulseText(_comboText.transform));
                }
                else
                {
                    _comboText.gameObject.SetActive(false);
                }
            }
        }

        public void ShowFeedback(string text, Color color)
        {
            if (_feedbackText == null) return;
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(ShowFeedbackCoroutine(text, color));
        }

        IEnumerator ShowFeedbackCoroutine(string text, Color color)
        {
            _feedbackText.text = text;
            _feedbackText.color = color;
            _feedbackText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.2f);
            _feedbackText.gameObject.SetActive(false);
        }

        public void ShowRivalScore(int rival)
        {
            if (_rivalText != null)
            {
                _rivalText.gameObject.SetActive(true);
                _rivalText.text = $"ライバル: {rival}pt";
            }
        }

        public void ShowStageClear(int stage)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearText != null)
                    _stageClearText.text = $"Stage {stage} クリア！";
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
                    _allClearScoreText.text = $"最終スコア: {score}";
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

        IEnumerator PulseText(Transform t)
        {
            Vector3 orig = t.localScale;
            float elapsed = 0f;
            while (elapsed < 0.1f)
            {
                elapsed += Time.deltaTime;
                float s = 1f + (elapsed / 0.1f) * 0.15f;
                t.localScale = orig * s;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < 0.1f)
            {
                elapsed += Time.deltaTime;
                float s = 1.15f - (elapsed / 0.1f) * 0.15f;
                t.localScale = orig * s;
                yield return null;
            }
            t.localScale = orig;
        }
    }
}
