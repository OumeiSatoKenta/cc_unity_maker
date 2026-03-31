using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game036_CoinStack
{
    public class CoinStackUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _heightText;
        [SerializeField] private TextMeshProUGUI _perfectText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private CoinStackGameManager _gameManager;

        private void Awake() { if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); }); }
        public void UpdateHeight(int h) { if (_heightText != null) _heightText.text = $"高さ: {h}"; }
        public void UpdatePerfect(int p) { if (_perfectText != null) _perfectText.text = $"Perfect: {p}"; }
        public void ShowGameOverPanel(int h, int p)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverText != null) _gameOverText.text = $"崩壊!\n高さ: {h}\nPerfect: {p}";
        }
        public void HideGameOverPanel() { if (_gameOverPanel != null) _gameOverPanel.SetActive(false); }
    }
}
