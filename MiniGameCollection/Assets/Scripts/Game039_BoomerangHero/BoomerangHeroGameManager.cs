using UnityEngine;
namespace Game039_BoomerangHero
{
    public class BoomerangHeroGameManager : MonoBehaviour
    {
        [SerializeField] private BoomerangManager _boomerangManager;
        [SerializeField] private BoomerangHeroUI _ui;
        private int _score; private bool _isGameOver;
        private void Start() { StartGame(); }
        public void StartGame() { _score = 0; _isGameOver = false; if (_boomerangManager != null) _boomerangManager.StartGame(); if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateTime(30f); _ui.HideResultPanel(); } }
        public void OnEnemyHit() { if (_isGameOver) return; _score += 10; if (_ui != null) _ui.UpdateScore(_score); }
        public void OnTimeUpdate(float t) { if (_ui != null) _ui.UpdateTime(t); }
        public void OnTimeUp() { _isGameOver = true; if (_boomerangManager != null) _boomerangManager.StopGame(); if (_ui != null) _ui.ShowResultPanel(_score); }
        public void RestartGame() { StartGame(); }
    }
}
