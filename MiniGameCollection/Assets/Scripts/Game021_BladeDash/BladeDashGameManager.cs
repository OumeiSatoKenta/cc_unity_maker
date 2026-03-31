using UnityEngine;

namespace Game021_BladeDash
{
    public class BladeDashGameManager : MonoBehaviour
    {
        [SerializeField] private RunManager _runManager;
        [SerializeField] private BladeDashUI _ui;

        private int _score;
        private bool _isGameOver;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _score = 0;
            _isGameOver = false;
            if (_runManager != null) _runManager.StartRun();
            if (_ui != null)
            {
                _ui.UpdateScore(_score);
                _ui.HideGameOverPanel();
            }
        }

        public void OnCoinCollected()
        {
            if (_isGameOver) return;
            _score += 10;
            if (_ui != null) _ui.UpdateScore(_score);
        }

        public void OnGameOver()
        {
            if (_isGameOver) return;
            _isGameOver = true;
            if (_runManager != null) _runManager.StopRun();
            if (_ui != null) _ui.ShowGameOverPanel(_score);
        }

        public void RestartGame() { StartGame(); }
    }
}
