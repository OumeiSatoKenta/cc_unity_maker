using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game053v2_SlideBlitz
{
    /// <summary>
    /// SlideBlitz の UI管理。タイマー・手数・ステージ・スコア・各種パネルを制御する。
    /// </summary>
    public class SlideBlitzUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _movesText;
        [SerializeField] private TextMeshProUGUI _comboText;

        [Header("Stage Clear Panel")]
        [SerializeField] private GameObject _stageClearPanel;
        [SerializeField] private TextMeshProUGUI _stageClearScoreText;
        [SerializeField] private TextMeshProUGUI _totalScoreInClearText;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverScoreText;

        [Header("All Clear Panel")]
        [SerializeField] private GameObject _allClearPanel;
        [SerializeField] private TextMeshProUGUI _allClearScoreText;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateTimer(float seconds)
        {
            if (_timerText == null) return;
            int s = Mathf.CeilToInt(seconds);
            _timerText.text = $"{s}";
            _timerText.color = seconds <= 10f ? new Color(1f, 0.3f, 0.3f) : Color.white;
        }

        public void UpdateMoves(int moves)
        {
            if (_movesText) _movesText.text = $"手数: {moves}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo >= 2)
            {
                _comboText.text = $"COMBO ×{combo}";
                _comboText.gameObject.SetActive(true);
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        public void ShowStageClear(int stageScore, int totalScore)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText) _stageClearScoreText.text = $"+{stageScore}";
            if (_totalScoreInClearText) _totalScoreInClearText.text = $"合計: {totalScore}";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText) _gameOverScoreText.text = $"スコア: {score}";
        }

        public void HideGameOver()
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
        }

        public void ShowAllClear(int totalScore)
        {
            if (_allClearPanel) _allClearPanel.SetActive(true);
            if (_allClearScoreText) _allClearScoreText.text = $"最終スコア: {totalScore}";
        }
    }
}
