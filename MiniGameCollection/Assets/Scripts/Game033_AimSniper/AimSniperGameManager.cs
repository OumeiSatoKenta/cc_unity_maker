using UnityEngine;

namespace Game033_AimSniper
{
    public class AimSniperGameManager : MonoBehaviour
    {
        [SerializeField] private SniperManager _sniperManager;
        [SerializeField] private AimSniperUI _ui;

        private int _hits;
        private int _misses;
        private bool _isGameOver;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _hits = 0; _misses = 0; _isGameOver = false;
            if (_sniperManager != null) _sniperManager.StartGame();
            if (_ui != null) { _ui.UpdateHits(_hits); _ui.UpdateMisses(_misses); _ui.UpdateTime(20f); _ui.HideResultPanel(); }
        }

        public void OnTargetHit() { if (_isGameOver) return; _hits++; if (_ui != null) _ui.UpdateHits(_hits); }
        public void OnMiss() { if (_isGameOver) return; _misses++; if (_ui != null) _ui.UpdateMisses(_misses); }
        public void OnTimeUpdate(float t) { if (_ui != null) _ui.UpdateTime(t); }

        public void OnTimeUp()
        {
            _isGameOver = true;
            if (_sniperManager != null) _sniperManager.StopGame();
            if (_ui != null) _ui.ShowResultPanel(_hits, _misses);
        }

        public void RestartGame() { StartGame(); }
    }
}
