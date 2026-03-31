using UnityEngine;

namespace Game051_DrawBridge
{
    public class DrawBridgeGameManager : MonoBehaviour
    {
        [SerializeField] private BridgeDrawManager _bridgeManager;
        [SerializeField] private DrawBridgeUI _ui;

        private int _currentStage;
        private bool _isCleared;
        private bool _isFailed;

        public bool IsActive => !_isCleared && !_isFailed;

        private void Start()
        {
            _currentStage = 1;
            StartGame();
        }

        public void StartGame()
        {
            _isCleared = false;
            _isFailed = false;
            if (_ui != null)
            {
                _ui.UpdateStage(_currentStage);
                _ui.HidePanel();
            }
            if (_bridgeManager != null) _bridgeManager.Init(_currentStage);
        }

        public void OnReachGoal()
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel();
        }

        public void OnBallFell()
        {
            if (_isFailed || _isCleared) return;
            _isFailed = true;
            if (_ui != null) _ui.ShowFailPanel();
        }

        public void OnNextStage()
        {
            _currentStage++;
            StartGame();
        }

        public void OnRetry()
        {
            StartGame();
        }
    }
}
