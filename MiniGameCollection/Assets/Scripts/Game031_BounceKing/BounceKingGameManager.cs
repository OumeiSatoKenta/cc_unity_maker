using UnityEngine;

namespace Game031_BounceKing
{
    public class BounceKingGameManager : MonoBehaviour
    {
        [SerializeField] private BounceManager _bounceManager;
        [SerializeField] private BounceKingUI _ui;

        private int _score;
        private int _lives;
        private bool _isGameOver;
        private const int MaxLives = 3;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _lives = MaxLives; _isGameOver = false;
            if (_bounceManager != null) _bounceManager.StartGame();
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateLives(_lives); _ui.HidePanel(); }
        }

        public void OnBlockDestroyed(int destroyed, int total)
        {
            if (_isGameOver) return;
            _score += 10;
            if (_ui != null) _ui.UpdateScore(_score);
        }

        public void OnAllBlocksDestroyed()
        {
            _isGameOver = true;
            if (_bounceManager != null) _bounceManager.StopGame();
            if (_ui != null) _ui.ShowWinPanel(_score);
        }

        public void OnBallLost()
        {
            if (_isGameOver) return;
            _lives--;
            if (_ui != null) _ui.UpdateLives(_lives);
            if (_lives <= 0)
            {
                _isGameOver = true;
                if (_ui != null) _ui.ShowLosePanel(_score);
            }
            else
            {
                if (_bounceManager != null) _bounceManager.StartGame();
            }
        }

        public void RestartGame() { StartGame(); }
    }
}
