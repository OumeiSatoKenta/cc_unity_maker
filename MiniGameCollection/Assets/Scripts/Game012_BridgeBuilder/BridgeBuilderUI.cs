using UnityEngine;
using UnityEngine.UI;

namespace Game012_BridgeBuilder
{
    public class BridgeBuilderUI : MonoBehaviour
    {
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _budgetText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private GameObject _buildButtons;
        [SerializeField] private Button _testButton;
        [SerializeField] private BridgeBuilderGameManager _gameManager;
        [SerializeField] private BridgeManager _bridgeManager;

        private void Start()
        {
            _gameManager.OnLevelCleared.AddListener(_ => ShowClearPanel());
            if (_clearPanel) _clearPanel.SetActive(false);
        }

        public void SetLevelText(string text)
        {
            if (_levelText) _levelText.text = text;
        }

        public void SetBudgetText(int remaining)
        {
            if (_budgetText) _budgetText.text = $"Parts: {remaining}";
        }

        public void SetTestMode(bool testing)
        {
            if (_buildButtons) _buildButtons.SetActive(!testing);
            if (_testButton) _testButton.interactable = !testing;
        }

        public void ShowClearPanel()
        {
            if (_clearPanel) _clearPanel.SetActive(true);
        }

        public void HideClearPanel()
        {
            if (_clearPanel) _clearPanel.SetActive(false);
        }

        public void OnPlankSelected() => _bridgeManager?.SelectPartType(0);
        public void OnSupportSelected() => _bridgeManager?.SelectPartType(1);
        public void OnTestClicked() => _gameManager?.StartTest();
        public void OnUndoClicked() => _bridgeManager?.UndoLastPart();
        public void OnResetClicked() => _gameManager?.ResetLevel();
        public void OnNextLevelClicked() => _gameManager?.LoadNextLevel();
        public void OnMenuClicked() => _gameManager?.LoadMenu();
    }
}
