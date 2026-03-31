using UnityEngine;

namespace Game032_SpinCutter
{
    public class SpinCutterGameManager : MonoBehaviour
    {
        [SerializeField] private SpinManager _spinManager;
        [SerializeField] private SpinCutterUI _ui;

        private int _score;
        private bool _isGameOver;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _isGameOver = false;
            if (_spinManager != null) _spinManager.StartGame();
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateTime(30f); _ui.HideResultPanel(); }
        }

        public void OnEnemyKilled() { if (_isGameOver) return; _score += 10; if (_ui != null) _ui.UpdateScore(_score); }
        public void OnTimeUpdate(float t) { if (_ui != null) _ui.UpdateTime(t); }

        public void OnTimeUp()
        {
            _isGameOver = true;
            if (_spinManager != null) _spinManager.StopGame();
            if (_ui != null) _ui.ShowResultPanel(_score, true);
        }

        public void OnPlayerHit()
        {
            _isGameOver = true;
            if (_spinManager != null) _spinManager.StopGame();
            if (_ui != null) _ui.ShowResultPanel(_score, false);
        }

        public void RestartGame() { StartGame(); }
    }
}
