using UnityEngine;

namespace Game024_BubblePop
{
    public class BubblePopGameManager : MonoBehaviour
    {
        [SerializeField] private BubbleManager _bubbleManager;
        [SerializeField] private BubblePopUI _ui;

        private int _score;
        private int _lives;
        private bool _isGameOver;
        private const int MaxLives = 5;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _lives = MaxLives; _isGameOver = false;
            if (_bubbleManager != null) _bubbleManager.StartGame();
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateLives(_lives); _ui.HideGameOverPanel(); }
        }

        public void OnBubblePopped()
        {
            if (_isGameOver) return;
            _score += 10;
            if (_ui != null) _ui.UpdateScore(_score);
        }

        public void OnBubbleEscaped()
        {
            if (_isGameOver) return;
            _lives--;
            if (_ui != null) _ui.UpdateLives(_lives);
            if (_lives <= 0)
            {
                _isGameOver = true;
                if (_bubbleManager != null) _bubbleManager.StopGame();
                if (_ui != null) _ui.ShowGameOverPanel(_score);
            }
        }

        public void RestartGame() { StartGame(); }
    }
}
