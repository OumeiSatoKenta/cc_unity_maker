using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game052_HammerNail
{
    public class HammerNailUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _missText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private HammerNailGameManager _gameManager;

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = "スコア: " + score;
        }

        public void UpdateMisses(int misses, int max)
        {
            if (_missText != null) _missText.text = "ミス: " + misses + "/" + max;
        }

        public void ShowGameOverPanel(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_finalScoreText != null) _finalScoreText.text = "スコア: " + score;
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
