using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game021_BladeDash
{
    public class BladeDashUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private BladeDashGameManager _gameManager;

        private void Awake()
        {
            if (_retryButton != null)
                _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"スコア: {score}";
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
