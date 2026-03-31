using UnityEngine;

namespace Game056_InflateFloat
{
    public class InflateFloatGameManager : MonoBehaviour
    {
        [SerializeField] private BalloonManager _balloonManager;
        [SerializeField] private InflateFloatUI _ui;

        private int _currentStage;
        private bool _isCleared;
        private bool _isFailed;

        public bool IsActive => !_isCleared && !_isFailed;

        private void Start() { _currentStage = 1; StartGame(); }

        public void StartGame()
        {
            _isCleared = false; _isFailed = false;
            if (_ui != null) { _ui.UpdateStage(_currentStage); _ui.HidePanel(); }
            if (_balloonManager != null) _balloonManager.Init(_currentStage);
        }

        public void OnReachGoal() { if (_isCleared) return; _isCleared = true; if (_ui != null) _ui.ShowClearPanel(); }
        public void OnBalloonPopped() { if (_isFailed || _isCleared) return; _isFailed = true; if (_ui != null) _ui.ShowFailPanel("割れた!"); }
        public void OnBalloonFell() { if (_isFailed || _isCleared) return; _isFailed = true; if (_ui != null) _ui.ShowFailPanel("落下!"); }
        public void OnNextStage() { _currentStage++; StartGame(); }
        public void OnRetry() { StartGame(); }
    }
}
