using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game048_GlassBall
{
    public class GlassBallUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearTimeText;
        [SerializeField] private GlassBallGameManager _gameManager;

        public void UpdateTimer(float time)
        {
            if (_timerText != null) _timerText.text = time.ToString("F1") + "s";
        }

        public void UpdateStage(int stage)
        {
            if (_stageText != null) _stageText.text = "ステージ " + stage;
        }

        public void ShowClearPanel(float time)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearTimeText != null) _clearTimeText.text = time.ToString("F1") + "秒でクリア!";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
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
