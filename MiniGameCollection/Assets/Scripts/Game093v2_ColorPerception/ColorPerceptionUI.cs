using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game093v2_ColorPerception
{
    public class ColorPerceptionUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _movesText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _cycleText;

        [SerializeField] Button[] _viewButtons;
        [SerializeField] TextMeshProUGUI[] _viewButtonTexts;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        static readonly string[] ViewNames = { "通常", "色覚A", "色覚B" };
        static readonly Color[] ViewActiveColors = {
            new Color(0.46f, 1f, 0.01f),
            new Color(0.83f, 0f, 0.98f),
            new Color(0.01f, 0.86f, 1f),
        };

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        public void UpdateMoves(int used, int limit)
        {
            if (_movesText == null) return;
            _movesText.text = limit > 0 ? $"手数: {used} / {limit}" : $"手数: {used}";
            if (limit > 0)
            {
                float ratio = (float)used / limit;
                _movesText.color = ratio >= 0.8f ? new Color(1f, 0.3f, 0.3f) :
                                   ratio >= 0.6f ? new Color(1f, 0.8f, 0.2f) :
                                   new Color(0.8f, 0.85f, 0.9f);
            }
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            if (combo >= 2)
            {
                _comboText.text = $"Combo x{combo}  ×{multiplier:F1}";
                _comboText.color = new Color(1f, 0.85f, 0.2f);
            }
            else
            {
                _comboText.text = "";
            }
        }

        public void UpdateViewButtons(int currentView, int viewCount)
        {
            if (_viewButtons == null) return;
            for (int i = 0; i < _viewButtons.Length; i++)
            {
                if (_viewButtons[i] == null) continue;
                bool active = i == currentView;
                bool inRange = i < viewCount;
                _viewButtons[i].gameObject.SetActive(inRange);
                var img = _viewButtons[i].GetComponent<Image>();
                if (img != null)
                    img.color = active ? (i < ViewActiveColors.Length ? ViewActiveColors[i] : Color.white) : new Color(0.2f, 0.2f, 0.2f);
                if (_viewButtonTexts != null && i < _viewButtonTexts.Length && _viewButtonTexts[i] != null)
                {
                    _viewButtonTexts[i].text = i < ViewNames.Length ? ViewNames[i] : $"視点{i}";
                    _viewButtonTexts[i].color = active ? Color.black : Color.white;
                }
            }
        }

        public void UpdateCycleCountdown(int turnsLeft)
        {
            if (_cycleText == null) return;
            _cycleText.text = $"変化まで: {turnsLeft}";
            _cycleText.color = turnsLeft <= 1 ? new Color(1f, 0.3f, 0.3f) : new Color(0.8f, 0.9f, 0.6f);
        }

        public void ShowStageClear(int stage, int score)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score}";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null) _allClearPanel.SetActive(true);
            if (_allClearScoreText != null) _allClearScoreText.text = $"Final Score: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
        }

        public void HideGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
