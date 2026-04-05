using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game029v2_MeteorShield
{
    public class MeteorShieldUI : MonoBehaviour
    {
        [SerializeField] Slider _hpBar;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _timeText;
        [SerializeField] TextMeshProUGUI _stageText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        MeteorShieldGameManager _gameManager;
        Coroutine _comboCo;
        Coroutine _bonusCo;

        public void Initialize(MeteorShieldGameManager gm)
        {
            _gameManager = gm;
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_finalClearPanel != null) _finalClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            UpdateScore(0);
            UpdateTime(0f);
            ShowCombo(0, 1f);
        }

        public void UpdateHP(float current, float max)
        {
            if (_hpBar != null) _hpBar.value = current / max;
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateTime(float seconds)
        {
            if (_timeText != null) _timeText.text = $"{Mathf.FloorToInt(seconds)}s";
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage}/{total}";
        }

        public void ShowCombo(int count, float multiplier)
        {
            if (_comboText == null) return;
            if (count <= 0)
            {
                _comboText.text = "";
                return;
            }
            _comboText.text = $"x{multiplier:F1} ({count} combo)";
            if (_comboCo != null) StopCoroutine(_comboCo);
            _comboCo = StartCoroutine(ComboPopEffect());
        }

        public void ShowSurvivalBonus(int bonus)
        {
            if (_bonusCo != null) StopCoroutine(_bonusCo);
            _bonusCo = StartCoroutine(ShowBonusText($"+{bonus} Bonus!"));
        }

        IEnumerator ShowBonusText(string text)
        {
            if (_comboText == null) yield break;
            string prev = _comboText.text;
            _comboText.text = text;
            _comboText.color = Color.yellow;
            yield return new WaitForSeconds(1.2f);
            _comboText.color = Color.white;
            _comboText.text = prev;
        }

        IEnumerator ComboPopEffect()
        {
            if (_comboText == null) yield break;
            Vector3 orig = _comboText.transform.localScale;
            Vector3 big = orig * 1.4f;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.1f;
                _comboText.transform.localScale = ratio <= 1f ? Vector3.Lerp(orig, big, ratio) : Vector3.Lerp(big, orig, ratio - 1f);
                yield return null;
            }
            _comboText.transform.localScale = orig;
        }

        public void ShowStageClear(int stage, int bonus)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearText != null)
                    _stageClearText.text = $"Stage {stage} Clear!\n+{bonus}pt";
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowFinalClear(int score)
        {
            if (_finalClearPanel != null)
            {
                _finalClearPanel.SetActive(true);
                if (_finalClearScoreText != null)
                    _finalClearScoreText.text = $"Score: {score}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null)
                    _gameOverScoreText.text = $"Score: {score}";
            }
        }

        public void OnRestartButton()
        {
            _gameManager?.RestartGame();
        }

        public void OnMenuButton()
        {
            _gameManager?.ReturnToMenu();
        }
    }
}
