using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game049_CloudHop
{
    public class CloudHopUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private CloudHopGameManager _gameManager;

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = "高さ: " + score;
        }

        public void ShowGameOverPanel(int finalScore)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_finalScoreText != null) _finalScoreText.text = "高さ: " + finalScore;
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
