using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game022v2_GravityBall
{
    public class GravityBallUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _distanceText;
        [SerializeField] TextMeshProUGUI _bonusText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearComboText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _clearPanel;
        [SerializeField] TextMeshProUGUI _clearScoreText;
        [SerializeField] Button _clearRetryButton;
        [SerializeField] Button _clearMenuButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] TextMeshProUGUI _gameOverDistanceText;
        [SerializeField] Button _retryButton;
        [SerializeField] Button _menuButton;

        Coroutine _bonusCoroutine;

        public void UpdateStageDisplay(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = score.ToString();
        }

        public void UpdateCombo(float multiplier, int count)
        {
            if (_comboText)
            {
                if (count >= 3)
                    _comboText.text = $"x{multiplier:F1} COMBO {count}";
                else
                    _comboText.text = "";
            }
        }

        public void UpdateDistance(float dist, float target)
        {
            if (_distanceText)
                _distanceText.text = $"{Mathf.FloorToInt(dist)}m / {Mathf.FloorToInt(target)}m";
        }

        public void ShowBonus(int bonus, bool isPerfect)
        {
            if (_bonusText == null) return;
            if (_bonusCoroutine != null) StopCoroutine(_bonusCoroutine);
            _bonusCoroutine = StartCoroutine(BonusPopRoutine(bonus, isPerfect));
        }

        IEnumerator BonusPopRoutine(int bonus, bool isPerfect)
        {
            _bonusText.text = isPerfect ? $"PERFECT +{bonus}" : $"+{bonus}";
            _bonusText.color = isPerfect ? new Color(1f, 0.9f, 0.1f) : new Color(0.5f, 1f, 0.6f);
            _bonusText.gameObject.SetActive(true);

            float dur = 0.15f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(0f, 1.2f, elapsed / dur);
                _bonusText.transform.localScale = Vector3.one * s;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < 0.05f)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(1.2f, 1f, elapsed / 0.05f);
                _bonusText.transform.localScale = Vector3.one * s;
                yield return null;
            }
            yield return new WaitForSeconds(0.8f);
            _bonusText.gameObject.SetActive(false);
        }

        public void ShowStageClearPanel(int combo)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearComboText) _stageClearComboText.text = $"コンボ: {combo}";
        }

        public void ShowClearPanel(int totalScore)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearScoreText) _clearScoreText.text = $"Total: {totalScore}pt";
        }

        public void ShowGameOverPanel(int score, int distance)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText) _gameOverScoreText.text = $"Score: {score}pt";
            if (_gameOverDistanceText) _gameOverDistanceText.text = $"距離: {distance}m";
        }
    }
}
