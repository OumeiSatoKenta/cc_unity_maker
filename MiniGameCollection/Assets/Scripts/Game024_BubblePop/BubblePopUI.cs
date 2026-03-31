using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game024_BubblePop
{
    public class BubblePopUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private BubblePopGameManager _gameManager;

        private void Awake()
        {
            if (_retryButton != null)
                _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"スコア: {score}";
        }

        public void UpdateLives(int lives)
        {
            if (_livesText != null) _livesText.text = $"ライフ: {"♥".PadRight(lives, '♥').Substring(0, Mathf.Max(lives, 0))}";
        }

        public void ShowGameOverPanel(int finalScore)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverText != null) _gameOverText.text = $"ゲームオーバー\nスコア: {finalScore}";
        }

        public void HideGameOverPanel()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
