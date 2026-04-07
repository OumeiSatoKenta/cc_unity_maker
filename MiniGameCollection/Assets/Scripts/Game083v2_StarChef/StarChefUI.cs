using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game083v2_StarChef
{
    public class StarChefUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _failText;

        [Header("Stage Clear Panel")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearStageText;
        [SerializeField] Button _nextStageButton;

        [Header("All Clear Panel")]
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [Header("Game Over Panel")]
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo >= 3)
            {
                _comboText.text = $"Combo x{combo}!";
                _comboText.color = combo >= 8 ? new Color(1f, 0.4f, 0f) :
                                   combo >= 5 ? new Color(1f, 0.9f, 0f) :
                                                new Color(0.5f, 1f, 0.5f);
                _comboText.gameObject.SetActive(true);
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        public void UpdateFails(int fails)
        {
            if (_failText != null)
                _failText.text = $"失敗: {fails}/3";
        }

        public void ShowStageClear(int stageNum)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearStageText != null)
                    _stageClearStageText.text = $"Stage {stageNum} クリア！";
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
                if (_allClearScoreText != null)
                    _allClearScoreText.text = $"最終スコア: {score}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null)
                    _gameOverScoreText.text = $"スコア: {score}";
            }
        }
    }
}
