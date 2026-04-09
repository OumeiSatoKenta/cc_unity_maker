using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game094v2_GravityPainter
{
    public class GravityPainterUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _matchRateText;
        [SerializeField] TextMeshProUGUI _paintCountText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _timeText;

        [Header("Color Palette")]
        [SerializeField] Button[] _colorButtons;       // 4 buttons
        [SerializeField] Image[] _colorButtonImages;   // highlight frames

        [Header("Panels")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        int _currentColorIndex = 0;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateMatchRate(float rate)
        {
            if (_matchRateText == null) return;
            int pct = Mathf.RoundToInt(rate * 100f);
            _matchRateText.text = $"一致率: {pct}%";
            _matchRateText.color = rate >= 0.5f ? new Color(1f, 0.9f, 0.1f) : new Color(0.8f, 0.9f, 1f);
        }

        public void UpdatePaintCount(int count)
        {
            if (_paintCountText != null) _paintCountText.text = $"絵の具: {count}";
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            _comboText.text = combo >= 2 ? $"Combo x{combo} ({multiplier:F1}x)" : "";
        }

        public void UpdateTimeRemaining(float time)
        {
            if (_timeText != null)
            {
                _timeText.text = time > 0f ? $"残り: {Mathf.CeilToInt(time)}秒" : "";
                _timeText.color = time < 10f ? new Color(1f, 0.3f, 0.3f) : new Color(0.9f, 0.9f, 0.9f);
            }
        }

        public void SetColorCount(int count)
        {
            if (_colorButtons == null) return;
            for (int i = 0; i < _colorButtons.Length; i++)
            {
                if (_colorButtons[i] != null)
                    _colorButtons[i].gameObject.SetActive(i < count);
            }
            HighlightColor(0);
        }

        public void HighlightColor(int index)
        {
            _currentColorIndex = index;
            if (_colorButtonImages == null) return;
            for (int i = 0; i < _colorButtonImages.Length; i++)
            {
                if (_colorButtonImages[i] == null) continue;
                _colorButtonImages[i].color = (i == index)
                    ? new Color(1f, 1f, 0.3f, 1f)
                    : new Color(1f, 1f, 1f, 0.3f);
            }
        }

        public void ShowStageClear(int stage, int score)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearText != null) _stageClearText.text = $"ステージ{stage} クリア！\nScore: {score}";
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText != null) _allClearScoreText.text = $"全ステージクリア！\nFinal Score: {score}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null) _gameOverScoreText.text = $"ゲームオーバー\nScore: {score}";
            }
        }

        public void HideGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
