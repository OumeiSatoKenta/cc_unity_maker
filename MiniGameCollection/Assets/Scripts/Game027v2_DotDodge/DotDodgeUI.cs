using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game027v2_DotDodge
{
    public class DotDodgeUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _timeText;
        [SerializeField] TextMeshProUGUI _comboText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] TextMeshProUGUI _stageBonusText;

        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearScoreText;
        [SerializeField] TextMeshProUGUI _gameClearTimeText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] TextMeshProUGUI _gameOverTimeText;

        DotDodgeGameManager _gm;
        Coroutine _nearMissCoroutine;

        public void Initialize(DotDodgeGameManager gm)
        {
            _gm = gm;
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateTime(float time)
        {
            if (_timeText != null) _timeText.text = $"{time:F1}s";
        }

        public void UpdateCombo(float mult)
        {
            if (_comboText == null) return;
            if (mult > 1f)
            {
                _comboText.gameObject.SetActive(true);
                _comboText.text = $"x{mult:F1} COMBO!";
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        public void ShowNearMissEffect()
        {
            if (_nearMissCoroutine != null) StopCoroutine(_nearMissCoroutine);
            if (_comboText != null)
                _nearMissCoroutine = StartCoroutine(NearMissPulse());
        }

        IEnumerator NearMissPulse()
        {
            if (_comboText == null) yield break;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float scale = 1f + 0.3f * Mathf.Sin(Mathf.PI * t / 0.3f);
                _comboText.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            _comboText.transform.localScale = Vector3.one;
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowStageClear(int stage, int bonus)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearText != null) _stageClearText.text = $"Stage {stage} Clear!";
            if (_stageBonusText != null) _stageBonusText.text = $"+{bonus} Bonus!";
        }

        public void ShowFinalClear(int score, float time)
        {
            if (_gameClearPanel == null) return;
            _gameClearPanel.SetActive(true);
            if (_gameClearScoreText != null) _gameClearScoreText.text = $"Score: {score}";
            if (_gameClearTimeText != null) _gameClearTimeText.text = $"Time: {time:F1}s";
        }

        public void ShowGameOver(int score, float time)
        {
            if (_gameOverPanel == null) return;
            _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
            if (_gameOverTimeText != null) _gameOverTimeText.text = $"Time: {time:F1}s";
        }

        public void OnRestartButton()
        {
            _gm?.RestartGame();
        }

        public void OnReturnMenuButton()
        {
            _gm?.ReturnToMenu();
        }
    }
}
