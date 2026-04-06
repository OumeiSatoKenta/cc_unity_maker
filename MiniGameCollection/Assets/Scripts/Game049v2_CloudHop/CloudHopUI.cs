using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game049v2_CloudHop
{
    public class CloudHopUI : MonoBehaviour
    {
        [SerializeField] TMP_Text _altitudeText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] TMP_Text _stageText;
        [SerializeField] TMP_Text _comboText;
        [SerializeField] TMP_Text _bonusText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TMP_Text _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TMP_Text _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TMP_Text _gameOverScoreText;

        [SerializeField] CloudHopGameManager _gameManager;

        private Coroutine _bonusTextCoroutine;

        void Awake()
        {
            if (_bonusText != null) _bonusText.gameObject.SetActive(false);
            HideStageClear();
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
        }

        public void UpdateAltitude(float current, float target)
        {
            if (_altitudeText != null)
                _altitudeText.text = $"{Mathf.RoundToInt(current)}m / {Mathf.RoundToInt(target)}m";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = score.ToString("N0");
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo < 3)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            int mult = combo >= 10 ? 5 : combo >= 5 ? 3 : 2;
            _comboText.text = $"COMBO x{mult}!";
            StartCoroutine(ComboPulse());
        }

        public void ShowBonusText(string text, Color color)
        {
            if (_bonusText == null) return;
            if (_bonusTextCoroutine != null) StopCoroutine(_bonusTextCoroutine);
            _bonusTextCoroutine = StartCoroutine(BonusTextAnim(text, color));
        }

        IEnumerator BonusTextAnim(string text, Color color)
        {
            _bonusText.text = text;
            _bonusText.color = color;
            _bonusText.gameObject.SetActive(true);

            Vector3 startPos = _bonusText.transform.localPosition;
            float t = 0f;
            while (t < 0.8f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.8f;
                _bonusText.transform.localPosition = startPos + Vector3.up * ratio * 60f;
                Color c = color;
                c.a = 1f - ratio;
                _bonusText.color = c;
                yield return null;
            }
            _bonusText.transform.localPosition = startPos;
            _bonusText.gameObject.SetActive(false);
        }

        IEnumerator ComboPulse()
        {
            if (_comboText == null) yield break;
            Vector3 orig = _comboText.transform.localScale;
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.25f;
                float s = ratio < 0.5f ? Mathf.Lerp(1f, 1.3f, ratio * 2f) : Mathf.Lerp(1.3f, 1f, (ratio - 0.5f) * 2f);
                _comboText.transform.localScale = orig * s;
                yield return null;
            }
            _comboText.transform.localScale = orig;
        }

        public void ShowStageClear(int score)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score:N0}";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null) _allClearPanel.SetActive(true);
            if (_allClearScoreText != null) _allClearScoreText.text = $"Total Score: {score:N0}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score:N0}";
        }

        public void OnNextStageClicked() => _gameManager?.GoNextStage();
        public void OnRestartClicked() => _gameManager?.RestartGame();
        public void OnMenuClicked() => _gameManager?.GoToMenu();
    }
}
