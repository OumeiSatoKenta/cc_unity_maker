using UnityEngine;

namespace Game022_GravityBall
{
    public class GravityBallGameManager : MonoBehaviour
    {
        [SerializeField] private GravityBallManager _ballManager;
        [SerializeField] private GravityBallUI _ui;

        private int _score;
        private bool _isGameOver;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _isGameOver = false;
            if (_ballManager != null) _ballManager.StartRun();
            if (_ui != null) { _ui.UpdateScore(_score); _ui.HideGameOverPanel(); }
        }

        public void OnObstaclePassed()
        {
            if (_isGameOver) return;
            _score++;
            if (_ui != null) _ui.UpdateScore(_score);
        }

        public void OnGameOver()
        {
            if (_isGameOver) return;
            _isGameOver = true;
            if (_ballManager != null) _ballManager.StopRun();
            if (_ui != null) _ui.ShowGameOverPanel(_score);
        }

        public void RestartGame() { StartGame(); }
    }
}
