using UnityEngine;
using UnityEngine.UI;

namespace Game009_ColorMix
{
    /// <summary>
    /// ColorMix のUI表示を担当。
    /// スコア・レベル・クリアパネル・色プレビューの更新を行う。
    /// </summary>
    public class ColorMixUI : MonoBehaviour
    {
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _targetColorNameText;
        [SerializeField] private Image _targetColorPreview;
        [SerializeField] private Image _mixedColorPreview;
        [SerializeField] private Text _feedbackText;
        [SerializeField] private Slider _redSlider;
        [SerializeField] private Slider _greenSlider;
        [SerializeField] private Slider _blueSlider;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private Text _clearScoreText;
        [SerializeField] private ColorMixGameManager _gameManager;

        public void SetLevelText(string text)
        {
            if (_levelText != null) _levelText.text = text;
        }

        public void SetTargetColorName(string name)
        {
            if (_targetColorNameText != null)
                _targetColorNameText.text = $"Target: {name}";
        }

        public void UpdateTargetPreview(Color color)
        {
            if (_targetColorPreview != null)
                _targetColorPreview.color = color;
        }

        public void UpdateMixPreview(Color color)
        {
            if (_mixedColorPreview != null)
                _mixedColorPreview.color = color;

            if (_feedbackText != null)
                _feedbackText.text = "";
        }

        public void ShowFeedback(float diff)
        {
            if (_feedbackText != null)
            {
                float pct = diff * 100f;
                _feedbackText.text = pct < 20f
                    ? $"Close! Diff: {pct:F0}%"
                    : $"Try again! Diff: {pct:F0}%";
                _feedbackText.color = pct < 20f
                    ? new Color(1f, 0.8f, 0.2f)
                    : new Color(1f, 0.4f, 0.4f);
            }
        }

        public void ResetSliders()
        {
            if (_redSlider != null)   _redSlider.value = 0f;
            if (_greenSlider != null) _greenSlider.value = 0f;
            if (_blueSlider != null)  _blueSlider.value = 0f;
        }

        public void ShowClearPanel(int level, int score)
        {
            if (_clearPanel != null)
            {
                _clearPanel.SetActive(true);
                if (_clearScoreText != null)
                    _clearScoreText.text = $"Score: {score}";
            }
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null)
                _clearPanel.SetActive(false);
        }

        // ── Button handlers (UnityEvent targets) ──────────────────────

        public void OnRestartClicked()
        {
            _gameManager?.ResetLevel();
        }

        public void OnNextClicked()
        {
            _gameManager?.LoadNextLevel();
        }

        public void OnMenuClicked()
        {
            _gameManager?.LoadMenu();
        }
    }
}
