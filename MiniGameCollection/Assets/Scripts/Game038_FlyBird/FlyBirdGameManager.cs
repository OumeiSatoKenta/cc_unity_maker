using UnityEngine;
namespace Game038_FlyBird
{
    public class FlyBirdGameManager : MonoBehaviour
    {
        [SerializeField] private FlyManager _flyManager;
        [SerializeField] private FlyBirdUI _ui;
        private int _score; private bool _isGameOver;
        private void Start() { StartGame(); }
        public void StartGame() { _score = 0; _isGameOver = false; if (_flyManager != null) _flyManager.StartGame(); if (_ui != null) { _ui.UpdateScore(_score); _ui.HideGameOverPanel(); } }
        public void OnPipePassed() { if (_isGameOver) return; _score++; if (_ui != null) _ui.UpdateScore(_score); }
        public void OnGameOver() { _isGameOver = true; if (_flyManager != null) _flyManager.StopGame(); if (_ui != null) _ui.ShowGameOverPanel(_score); }
        public void RestartGame() { StartGame(); }
    }
}
