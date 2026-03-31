using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game051_DrawBridge
{
    public class DrawBridgeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private GameObject _failPanel;
        [SerializeField] private DrawBridgeGameManager _gameManager;

        public void UpdateStage(int stage)
        {
            if (_stageText != null) _stageText.text = "ステージ " + stage;
        }

        public void ShowClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
        }

        public void ShowFailPanel()
        {
            if (_failPanel != null) _failPanel.SetActive(true);
        }

        public void HidePanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
            if (_failPanel != null) _failPanel.SetActive(false);
        }

        public void OnNextButton()
        {
            if (_gameManager != null) _gameManager.OnNextStage();
        }

        public void OnRetryButton()
        {
            if (_gameManager != null) _gameManager.OnRetry();
        }
    }
}
