using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game050v2_BubbleSort
{
    public class BubbleSortUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TMP_Text _stageText;
        [SerializeField] TMP_Text _movesText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] TMP_Text _comboText;

        [Header("Panels")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TMP_Text _stageClearScoreText;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TMP_Text _allClearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TMP_Text _gameOverScoreText;

        [Header("Bonus")]
        [SerializeField] TMP_Text _bonusText;

        private Coroutine _comboHideCoroutine;
        private Coroutine _bonusHideCoroutine;

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateMoves(int remaining, int total)
        {
            if (_movesText != null)
            {
                _movesText.text = $"手数: {remaining}";
                _movesText.color = remaining <= total / 4 ? Color.red : Color.white;
            }
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo <= 1)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            _comboText.text = $"×{combo} COMBO!";
            if (_comboHideCoroutine != null) StopCoroutine(_comboHideCoroutine);
            _comboHideCoroutine = StartCoroutine(HideAfter(_comboText.gameObject, 1.5f));
        }

        public void ShowBonusText(string text, Color color)
        {
            if (_bonusText == null) return;
            _bonusText.text = text;
            _bonusText.color = color;
            _bonusText.gameObject.SetActive(true);
            if (_bonusHideCoroutine != null) StopCoroutine(_bonusHideCoroutine);
            _bonusHideCoroutine = StartCoroutine(HideAfter(_bonusText.gameObject, 1.5f));
        }

        public void ShowStageClear(int score)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score}";
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText != null) _allClearScoreText.text = $"Final Score: {score}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
            }
        }

        IEnumerator HideAfter(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null) go.SetActive(false);
        }
    }
}
