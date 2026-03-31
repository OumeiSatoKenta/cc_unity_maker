using UnityEngine;

namespace Game023_ChainSlash
{
    public class ChainSlashGameManager : MonoBehaviour
    {
        [SerializeField] private ChainManager _chainManager;
        [SerializeField] private ChainSlashUI _ui;

        private int _score;
        private int _maxCombo;
        private bool _isGameOver;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _maxCombo = 0; _isGameOver = false;
            if (_chainManager != null) _chainManager.StartGame();
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateCombo(0); _ui.HideResultPanel(); }
        }

        public void OnComboSlash(int combo)
        {
            if (_isGameOver) return;
            int points = combo * combo * 10; // combo^2 scoring
            _score += points;
            if (combo > _maxCombo) _maxCombo = combo;
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateCombo(combo); }
        }

        public void OnTimeUpdate(float remaining)
        {
            if (_ui != null) _ui.UpdateTime(remaining);
        }

        public void OnTimeUp()
        {
            _isGameOver = true;
            if (_chainManager != null) _chainManager.StopGame();
            if (_ui != null) _ui.ShowResultPanel(_score, _maxCombo);
        }

        public void RestartGame() { StartGame(); }
    }
}
