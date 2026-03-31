using UnityEngine;

namespace Game029_MeteorShield
{
    public class MeteorShieldGameManager : MonoBehaviour
    {
        [SerializeField] private ShieldManager _shieldManager;
        [SerializeField] private MeteorShieldUI _ui;

        private int _score;
        private int _starHP;
        private bool _isGameOver;
        private const int MaxStarHP = 5;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _starHP = MaxStarHP; _isGameOver = false;
            if (_shieldManager != null) _shieldManager.StartGame();
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateHP(_starHP); _ui.HideGameOverPanel(); }
        }

        public void OnMeteorDeflected()
        {
            if (_isGameOver) return;
            _score += 10;
            if (_ui != null) _ui.UpdateScore(_score);
        }

        public void OnStarHit()
        {
            if (_isGameOver) return;
            _starHP--;
            if (_ui != null) _ui.UpdateHP(_starHP);
            if (_starHP <= 0)
            {
                _isGameOver = true;
                if (_shieldManager != null) _shieldManager.StopGame();
                if (_ui != null) _ui.ShowGameOverPanel(_score);
            }
        }

        public void RestartGame() { StartGame(); }
    }
}
