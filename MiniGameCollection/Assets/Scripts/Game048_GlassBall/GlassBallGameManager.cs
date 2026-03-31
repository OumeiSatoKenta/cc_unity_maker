using UnityEngine;

namespace Game048_GlassBall
{
    public class GlassBallGameManager : MonoBehaviour
    {
        [SerializeField] private SlopeManager _slopeManager;
        [SerializeField] private GlassBallUI _ui;

        private int _currentStage;
        private float _timer;
        private bool _isCleared;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying && !_isCleared;

        private void Start()
        {
            _currentStage = 1;
            StartGame();
        }

        public void StartGame()
        {
            _timer = 0f;
            _isCleared = false;
            _isPlaying = true;
            if (_ui != null)
            {
                _ui.UpdateTimer(_timer);
                _ui.UpdateStage(_currentStage);
                _ui.HideClearPanel();
            }
            if (_slopeManager != null) _slopeManager.GenerateStage(_currentStage);
        }

        private void Update()
        {
            if (!_isPlaying || _isCleared) return;
            _timer += Time.deltaTime;
            if (_ui != null) _ui.UpdateTimer(_timer);
        }

        public void OnReachGoal()
        {
            if (_isCleared) return;
            _isCleared = true;
            _isPlaying = false;
            if (_ui != null) _ui.ShowClearPanel(_timer);
        }

        public void OnFallOff()
        {
            if (_slopeManager != null) _slopeManager.ResetBall();
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
