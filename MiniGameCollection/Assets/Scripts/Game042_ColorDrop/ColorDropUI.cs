using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game042_ColorDrop
{
    public class ColorDropUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _lifeText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private ColorDropGameManager _gameManager;

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = "スコア: " + score;
        }

        public void UpdateLife(int life)
        {
            if (_lifeText != null)
            {
                string hearts = "";
                for (int i = 0; i < life; i++) hearts += "\u2665 ";
                _lifeText.text = hearts.TrimEnd();
            }
        }

        public void ShowGameOverPanel(int finalScore)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_finalScoreText != null) _finalScoreText.text = "スコア: " + finalScore;
        }

        public void HideGameOverPanel()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void OnRetryButton()
        {
            if (_gameManager != null) _gameManager.StartGame();
        }
    }
}
