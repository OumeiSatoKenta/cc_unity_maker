using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game035_WaveRider
{
    public class WaveRiderUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _distanceText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private WaveRiderGameManager _gameManager;

        private void Awake() { if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); }); }
        public void UpdateScore(int s) { if (_scoreText != null) _scoreText.text = $"トリック: {s}"; }
        public void UpdateDistance(float d) { if (_distanceText != null) _distanceText.text = $"距離: {d:F0}m"; }
        public void ShowGameOverPanel(int score, float dist)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverText != null) _gameOverText.text = $"クラッシュ!\n距離: {dist:F0}m\nトリック: {score}";
        }
        public void HideGameOverPanel() { if (_gameOverPanel != null) _gameOverPanel.SetActive(false); }
    }
}
