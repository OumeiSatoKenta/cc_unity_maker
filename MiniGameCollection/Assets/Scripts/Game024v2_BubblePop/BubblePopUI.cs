using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game024v2_BubblePop
{
    public class BubblePopUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _livesText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _bonusText;
        [SerializeField] Slider _feverSlider;
        [SerializeField] TextMeshProUGUI _feverLabel;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] GameObject _clearPanel;
        [SerializeField] TextMeshProUGUI _clearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        Camera _mainCam;

        void Awake()
        {
            _mainCam = Camera.main;
            if (_bonusText != null) _bonusText.gameObject.SetActive(false);
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_clearPanel != null) _clearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_feverLabel != null) _feverLabel.gameObject.SetActive(false);
        }

        public void UpdateStageDisplay(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = score.ToString("N0");
        }

        public void UpdateLives(int lives)
        {
            if (_livesText != null)
            {
                string hearts = "";
                for (int i = 0; i < 3; i++)
                    hearts += (i < lives) ? "♥ " : "♡ ";
                _livesText.text = hearts.Trim();
            }
        }

        public void UpdateCombo(float multiplier, int streak)
        {
            if (_comboText == null) return;
            if (streak >= 2)
            {
                _comboText.text = $"x{multiplier:0.0} COMBO {streak}";
                StartCoroutine(ComboPulse());
            }
            else
            {
                _comboText.text = "";
            }
        }

        public void UpdateFever(bool active, float ratio)
        {
            if (_feverSlider != null)
            {
                _feverSlider.gameObject.SetActive(true);
                _feverSlider.value = active ? ratio : 0f;
            }
            if (_feverLabel != null)
                _feverLabel.gameObject.SetActive(active);
        }

        public void ShowFeverStart()
        {
            StartCoroutine(FeverFlash());
        }

        public void ShowBubbleBonus(int score, int comboStreak, bool fever)
        {
            if (_bonusText == null) return;
            string text = $"+{score}";
            if (fever) text += " FEVER!";
            else if (comboStreak >= 2) text += $" x{comboStreak}CHAIN";
            StartCoroutine(ShowFloatingBonus(text));
        }

        public void ShowLifeLost()
        {
            StartCoroutine(LifeLostFlash());
        }

        public void ShowStageClearPanel(int score)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null)
                _stageClearScoreText.text = $"Score: {score:N0}";
        }

        public void ShowClearPanel(int score)
        {
            if (_clearPanel == null) return;
            _clearPanel.SetActive(true);
            if (_clearScoreText != null)
                _clearScoreText.text = $"Final Score: {score:N0}";
        }

        public void ShowGameOverPanel(int score, int maxCombo)
        {
            if (_gameOverPanel == null) return;
            _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null)
                _gameOverScoreText.text = $"Score: {score:N0}\nMax Combo: {maxCombo}";
        }

        IEnumerator ShowFloatingBonus(string text)
        {
            if (_bonusText == null) yield break;
            _bonusText.text = text;
            _bonusText.gameObject.SetActive(true);
            _bonusText.transform.localScale = Vector3.one;
            _bonusText.color = new Color(1f, 0.95f, 0.3f, 1f);

            float t = 0f;
            Vector3 startPos = _bonusText.transform.localPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f;
                _bonusText.transform.localPosition = startPos + Vector3.up * (t * 60f);
                var c = _bonusText.color;
                c.a = 1f - t;
                _bonusText.color = c;
                yield return null;
            }
            _bonusText.transform.localPosition = startPos;
            _bonusText.gameObject.SetActive(false);
        }

        IEnumerator ComboPulse()
        {
            if (_comboText == null) yield break;
            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float r = t / 0.15f;
                float s = r < 0.5f ? Mathf.Lerp(1f, 1.3f, r * 2f) : Mathf.Lerp(1.3f, 1f, (r - 0.5f) * 2f);
                if (_comboText != null)
                    _comboText.transform.localScale = Vector3.one * s;
                yield return null;
            }
            if (_comboText != null)
                _comboText.transform.localScale = Vector3.one;
        }

        IEnumerator FeverFlash()
        {
            if (_feverLabel == null) yield break;
            for (int i = 0; i < 6; i++)
            {
                _feverLabel.color = i % 2 == 0 ? new Color(1f, 0.9f, 0.1f) : new Color(1f, 0.3f, 0.1f);
                _feverLabel.transform.localScale = i % 2 == 0 ? Vector3.one * 1.2f : Vector3.one;
                yield return new WaitForSeconds(0.15f);
            }
            _feverLabel.transform.localScale = Vector3.one;
        }

        IEnumerator LifeLostFlash()
        {
            if (_livesText == null) yield break;
            Color orig = _livesText.color;
            for (int i = 0; i < 4; i++)
            {
                _livesText.color = i % 2 == 0 ? Color.red : orig;
                yield return new WaitForSeconds(0.1f);
            }
            _livesText.color = orig;
        }
    }
}
