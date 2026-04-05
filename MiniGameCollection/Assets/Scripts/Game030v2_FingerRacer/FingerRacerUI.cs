using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game030v2_FingerRacer
{
    public class FingerRacerUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _timeText;
        [SerializeField] TextMeshProUGUI _courseOutText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _boostText;

        [SerializeField] GameObject _drawingUI;
        [SerializeField] Button _startRaceButton;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        FingerRacerGameManager _gameManager;
        Coroutine _comboCo;

        public void Initialize(FingerRacerGameManager gm)
        {
            _gameManager = gm;
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_finalClearPanel != null) _finalClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_comboText != null) _comboText.text = "";
            UpdateScore(0);
            UpdateTime(0f);
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateTime(float elapsed)
        {
            if (_timeText != null) _timeText.text = $"{elapsed:F1}s";
        }

        public void UpdateCourseOut(int count, int max)
        {
            if (_courseOutText != null)
                _courseOutText.text = $"OUT: {count}/{max}";
        }

        public void UpdateBoost(int count, int max)
        {
            if (_boostText != null)
                _boostText.text = $"⚡{count}/{max}";
        }

        public void ShowCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            if (_comboCo != null) StopCoroutine(_comboCo);
            if (combo <= 0)
            {
                _comboText.text = "";
                return;
            }
            _comboText.text = $"Boost x{combo} ({multiplier:F1}x)";
            _comboCo = StartCoroutine(FadeCombo());
        }

        IEnumerator FadeCombo()
        {
            yield return new WaitForSeconds(1.5f);
            float elapsed = 0f;
            Color orig = _comboText.color;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                _comboText.color = new Color(orig.r, orig.g, orig.b, 1f - elapsed / 0.5f);
                yield return null;
            }
            _comboText.text = "";
            _comboText.color = orig;
        }

        public void ShowDrawingUI(bool show)
        {
            if (_drawingUI != null) _drawingUI.SetActive(show);
        }

        public void ShowStageClear(int stage, int bonus, bool perfect)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearText != null)
            {
                string extra = perfect ? "\n⭐ パーフェクト！" : "";
                _stageClearText.text = $"ステージ {stage} クリア！\n+{bonus} pt{extra}";
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowFinalClear(int score)
        {
            if (_finalClearPanel != null) _finalClearPanel.SetActive(true);
            if (_finalClearScoreText != null)
                _finalClearScoreText.text = $"Total Score\n{score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null)
                _gameOverScoreText.text = $"Score: {score}";
        }
    }
}
