using UnityEngine;

namespace Game012_BridgeBuilder
{
    public class BridgeBuilderGameManager : MonoBehaviour
    {
        [SerializeField] private BridgeManager _bridgeManager;
        [SerializeField] private BridgeBuilderUI _ui;

        private bool _isCleared;
        private int _currentStage;

        private void Start()
        {
            _currentStage = 0;
            StartGame();
        }

        public void StartGame()
        {
            _isCleared = false;
            if (_bridgeManager != null) _bridgeManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateStageText(_currentStage + 1);
                _ui.UpdatePlanksText(0);
                _ui.HideClearPanel();
            }
        }

        public void OnPlankPlaced(int remaining)
        {
            if (_isCleared) return;
            if (_ui != null) _ui.UpdatePlanksText(remaining);
        }

        public void OnBridgeComplete()
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel(_currentStage + 1);
        }

        public void RestartGame() { StartGame(); }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= BridgeManager.StageCount) _currentStage = 0;
            StartGame();
        }
    }
}
