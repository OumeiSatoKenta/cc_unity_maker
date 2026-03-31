using UnityEngine;

namespace Game034_DropZone
{
    public class DropZoneGameManager : MonoBehaviour
    {
        [SerializeField] private DropManager _dropManager;
        [SerializeField] private DropZoneUI _ui;

        private int _score;
        private int _misses;
        private bool _isGameOver;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _misses = 0; _isGameOver = false;
            if (_dropManager != null) _dropManager.StartGame();
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateMisses(_misses); _ui.UpdateTime(30f); _ui.HideResultPanel(); }
        }

        public void OnCorrectSort() { if (_isGameOver) return; _score += 10; if (_ui != null) _ui.UpdateScore(_score); }
        public void OnWrongSort() { if (_isGameOver) return; _misses++; if (_ui != null) _ui.UpdateMisses(_misses); }
        public void OnItemMissed() { if (_isGameOver) return; _misses++; if (_ui != null) _ui.UpdateMisses(_misses); }
        public void OnTimeUpdate(float t) { if (_ui != null) _ui.UpdateTime(t); }

        public void OnTimeUp()
        {
            _isGameOver = true;
            if (_dropManager != null) _dropManager.StopGame();
            if (_ui != null) _ui.ShowResultPanel(_score, _misses);
        }

        public void RestartGame() { StartGame(); }
    }
}
