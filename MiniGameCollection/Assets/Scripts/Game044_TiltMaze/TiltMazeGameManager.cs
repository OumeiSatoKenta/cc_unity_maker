using UnityEngine;

namespace Game044_TiltMaze
{
    public class TiltMazeGameManager : MonoBehaviour
    {
        [SerializeField] private MazeManager _mazeManager;
        [SerializeField] private TiltMazeUI _ui;

        private int _currentStage;
        private float _timer;
        private bool _isCleared;
        private bool _isPlaying;

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
            if (_mazeManager != null) _mazeManager.GenerateStage(_currentStage);
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

        public void OnFallInHole()
        {
            if (_mazeManager != null) _mazeManager.ResetBall();
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

        public bool IsPlaying => _isPlaying && !_isCleared;
    }
}
