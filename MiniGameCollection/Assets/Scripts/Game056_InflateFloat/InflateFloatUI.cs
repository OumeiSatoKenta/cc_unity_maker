using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game056_InflateFloat
{
    public class InflateFloatUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private GameObject _failPanel;
        [SerializeField] private TextMeshProUGUI _failReasonText;
        [SerializeField] private InflateFloatGameManager _gameManager;

        public void UpdateStage(int s) { if (_stageText != null) _stageText.text = "ステージ " + s; }
        public void ShowClearPanel() { if (_clearPanel != null) _clearPanel.SetActive(true); }
        public void ShowFailPanel(string reason) { if (_failPanel != null) _failPanel.SetActive(true); if (_failReasonText != null) _failReasonText.text = reason; }
        public void HidePanel() { if (_clearPanel != null) _clearPanel.SetActive(false); if (_failPanel != null) _failPanel.SetActive(false); }
        public void OnNextButton() { if (_gameManager != null) _gameManager.OnNextStage(); }
        public void OnRetryButton() { if (_gameManager != null) _gameManager.OnRetry(); }
    }
}
