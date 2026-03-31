using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game012_BridgeBuilder
{
    public class BridgeBuilderUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _planksText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private BridgeBuilderGameManager _gameManager;

        private void Awake()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.NextStage(); });
        }

        public void UpdatePlanksText(int remaining)
        {
            if (_planksText != null) _planksText.text = $"残り板: {remaining}";
        }

        public void UpdateStageText(int stageNum)
        {
            if (_stageText != null) _stageText.text = $"ステージ {stageNum}";
        }

        public void ShowClearPanel(int stageNum)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = $"クリア!\nステージ {stageNum}\n橋が完成!";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }
    }
}
