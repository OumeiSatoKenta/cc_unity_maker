using UnityEngine;

namespace Game025_TowerDefend
{
    public class TowerDefendGameManager : MonoBehaviour
    {
        [SerializeField] private DefenseManager _defenseManager;
        [SerializeField] private TowerDefendUI _ui;

        private int _score;
        private int _lives;
        private bool _isGameOver;
        private const int MaxLives = 10;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _lives = MaxLives; _isGameOver = false;
            if (_defenseManager != null) _defenseManager.StartGame();
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateLives(_lives); _ui.UpdateTowers(5); _ui.HideResultPanel(); }
        }

        public void OnEnemyKilled()
        {
            if (_isGameOver) return;
            _score += 10;
            if (_ui != null) _ui.UpdateScore(_score);
        }

        public void OnEnemyReached()
        {
            if (_isGameOver) return;
            _lives--;
            if (_ui != null) _ui.UpdateLives(_lives);
            if (_lives <= 0) { _isGameOver = true; if (_defenseManager != null) _defenseManager.StopGame(); if (_ui != null) _ui.ShowResultPanel(_score, false); }
        }

        public void OnTowerPlaced(int remaining)
        {
            if (_ui != null) _ui.UpdateTowers(remaining);
        }

        public void OnWaveStarted(int wave, int total)
        {
            if (_ui != null) _ui.UpdateWave(wave, total);
        }

        public void OnAllWavesCleared()
        {
            if (_isGameOver) return;
            _isGameOver = true;
            if (_defenseManager != null) _defenseManager.StopGame();
            if (_ui != null) _ui.ShowResultPanel(_score, true);
        }

        public void RestartGame() { StartGame(); }
    }
}
