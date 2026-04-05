using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game017v2_CrystalSort
{
    public class CrystalSortUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _movesText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _bottleCountText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearComboText;
        [SerializeField] GameObject _clearPanel;
        [SerializeField] TextMeshProUGUI _clearScoreText;

        [SerializeField] GameObject _gameOverPanel;

        Coroutine _comboCoroutine;

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateMoves(int moves, int maxMoves)
        {
            if (_movesText != null)
            {
                int remaining = maxMoves - moves;
                _movesText.text = $"手数: {remaining}/{maxMoves}";
                _movesText.color = remaining <= 5 ? Color.red : new Color(1f, 0.85f, 0.3f);
            }
        }

        public void UpdateBottleCount(int completed, int total)
        {
            if (_bottleCountText != null) _bottleCountText.text = $"瓶: {completed}/{total}";
        }

        public void ShowCombo(int comboCount)
        {
            if (_comboCoroutine != null) StopCoroutine(_comboCoroutine);
            _comboCoroutine = StartCoroutine(ShowComboCoroutine(comboCount));
        }

        IEnumerator ShowComboCoroutine(int count)
        {
            if (_comboText == null) yield break;
            _comboText.text = $"コンボ×{count}!";
            _comboText.gameObject.SetActive(true);
            float t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float alpha = 1f - (t / 0.5f) * 0.5f;
                _comboText.alpha = alpha;
                yield return null;
            }
            _comboText.gameObject.SetActive(false);
        }

        public void ShowStageClearPanel(int score, int combo)
        {
            HideAllPanels();
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"+{score}pt";
            if (_stageClearComboText != null && combo > 1)
                _stageClearComboText.text = $"コンボ×{combo}!";
            else if (_stageClearComboText != null)
                _stageClearComboText.text = "";
        }

        public void ShowClearPanel(int totalScore)
        {
            HideAllPanels();
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearScoreText != null) _clearScoreText.text = $"Total: {totalScore}pt";
        }

        public void ShowGameOverPanel()
        {
            HideAllPanels();
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_clearPanel != null) _clearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
