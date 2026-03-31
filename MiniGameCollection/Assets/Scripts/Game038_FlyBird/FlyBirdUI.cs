using UnityEngine;using UnityEngine.UI;using TMPro;
namespace Game038_FlyBird
{
    public class FlyBirdUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private FlyBirdGameManager _gameManager;
        private void Awake() { if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); }); }
        public void UpdateScore(int s) { if (_scoreText != null) _scoreText.text = $"{s}"; }
        public void ShowGameOverPanel(int s) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverText != null) _gameOverText.text = $"ゲームオーバー\nスコア: {s}"; }
        public void HideGameOverPanel() { if (_gameOverPanel != null) _gameOverPanel.SetActive(false); }
    }
}
