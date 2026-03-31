using UnityEngine;

namespace Game035_WaveRider
{
    public class WaveRiderGameManager : MonoBehaviour
    {
        [SerializeField] private WaveManager _waveManager;
        [SerializeField] private WaveRiderUI _ui;
        private int _score; private float _distance; private bool _isGameOver; private float _trickCooldown;

        private void Start() { StartGame(); }
        public void StartGame()
        {
            _score = 0; _distance = 0; _isGameOver = false; _trickCooldown = 0;
            if (_waveManager != null) _waveManager.StartGame();
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateDistance(0); _ui.HideGameOverPanel(); }
        }
        public void OnDistanceUpdate(float d) { _distance = d; if (_ui != null) _ui.UpdateDistance(d); }
        public void OnTrick()
        {
            if (_isGameOver) return;
            _trickCooldown -= Time.deltaTime;
            if (_trickCooldown <= 0) { _score += 5; _trickCooldown = 0.5f; if (_ui != null) _ui.UpdateScore(_score); }
        }
        public void OnCrash()
        {
            _isGameOver = true;
            if (_waveManager != null) _waveManager.StopGame();
            if (_ui != null) _ui.ShowGameOverPanel(_score, _distance);
        }
        public void RestartGame() { StartGame(); }
    }
}
