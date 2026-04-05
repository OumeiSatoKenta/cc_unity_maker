using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game015v2_TileTurn
{
    public class TileTurnUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _rotationsText;
        [SerializeField] TextMeshProUGUI _comboText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearComboText;

        [SerializeField] GameObject _gameOverPanel;

        [SerializeField] GameObject _clearPanel;
        [SerializeField] TextMeshProUGUI _clearTotalScoreText;

        [SerializeField] TileTurnGameManager _gameManager;
        [SerializeField] TileManager _tileManager;

        public void UpdateStage(int current, int total)
        {
            if (_stageText) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateRotations(int remaining, int max)
        {
            if (_rotationsText) _rotationsText.text = $"残り: {remaining}/{max}";
        }

        public void ShowStageClearPanel(int stageScore, int combo)
        {
            HideAllPanels();
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText) _stageClearScoreText.text = $"+{stageScore}";
            if (_stageClearComboText) _stageClearComboText.text = combo >= 2 ? $"コンボ x{combo}!" : "";
            ShowComboEffect(combo);
        }

        public void ShowGameOverPanel()
        {
            HideAllPanels();
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
        }

        public void ShowClearPanel(int totalScore)
        {
            HideAllPanels();
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearTotalScoreText) _clearTotalScoreText.text = $"Total: {totalScore}";
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
            if (_clearPanel) _clearPanel.SetActive(false);
            if (_comboText) _comboText.text = "";
        }

        void ShowComboEffect(int combo)
        {
            if (combo < 2 || _comboText == null) return;
            _comboText.text = $"COMBO x{combo}!";
            StartCoroutine(ComboAnimation());
        }

        IEnumerator ComboAnimation()
        {
            if (_comboText == null) yield break;
            _comboText.transform.localScale = Vector3.zero;
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.15f;
                float scale = t < 0.7f ? Mathf.Lerp(0f, 1.2f, t / 0.7f) : Mathf.Lerp(1.2f, 1f, (t - 0.7f) / 0.3f);
                _comboText.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            _comboText.transform.localScale = Vector3.one;
        }

        // Button callbacks
        public void OnPreviewDown()
        {
            // Called on pointer down
            _tileManager?.SetPreviewUsed();
        }

        public void OnRetryButton()
        {
            _gameManager?.OnRetry();
        }

        public void OnNextStageButton()
        {
            _gameManager?.OnNextStage();
        }
    }
}
