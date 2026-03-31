using UnityEngine;

namespace Game026_SliceNinja
{
    public class SliceNinjaGameManager : MonoBehaviour
    {
        [SerializeField] private SliceManager _sliceManager;
        [SerializeField] private SliceNinjaUI _ui;

        private int _score;
        private int _lives;
        private bool _isGameOver;
        private const int MaxLives = 3;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _lives = MaxLives; _isGameOver = false;
            if (_sliceManager != null) _sliceManager.StartGame();
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateLives(_lives); _ui.HideGameOverPanel(); }
        }

        public void OnFruitSliced() { if (_isGameOver) return; _score += 10; if (_ui != null) _ui.UpdateScore(_score); }

        public void OnBombSliced()
        {
            if (_isGameOver) return;
            _lives = 0; _isGameOver = true;
            if (_sliceManager != null) _sliceManager.StopGame();
            if (_ui != null) _ui.ShowGameOverPanel(_score);
        }

        public void OnFruitMissed()
        {
            if (_isGameOver) return;
            _lives--; if (_ui != null) _ui.UpdateLives(_lives);
            if (_lives <= 0) { _isGameOver = true; if (_sliceManager != null) _sliceManager.StopGame(); if (_ui != null) _ui.ShowGameOverPanel(_score); }
        }

        public void RestartGame() { StartGame(); }
    }
}
