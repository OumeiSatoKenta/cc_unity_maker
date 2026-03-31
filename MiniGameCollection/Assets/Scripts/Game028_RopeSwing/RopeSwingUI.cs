using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game028_RopeSwing
{
    public class RopeSwingUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _distanceText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private RopeSwingGameManager _gameManager;

        private void Awake()
        {
            if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
        }

        public void UpdateDistance(float dist) { if (_distanceText != null) _distanceText.text = $"距離: {dist:F0}m"; }

        public void ShowGameOverPanel(float dist)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverText != null) _gameOverText.text = $"ゲームオーバー\n距離: {dist:F0}m";
        }

        public void HideGameOverPanel() { if (_gameOverPanel != null) _gameOverPanel.SetActive(false); }
    }
}
