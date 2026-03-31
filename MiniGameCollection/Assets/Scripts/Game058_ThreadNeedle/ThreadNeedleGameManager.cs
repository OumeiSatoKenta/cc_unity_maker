using UnityEngine;

namespace Game058_ThreadNeedle
{
    public class ThreadNeedleGameManager : MonoBehaviour
    {
        [SerializeField] private NeedleManager _needleManager;
        [SerializeField] private ThreadNeedleUI _ui;

        private int _score;
        private int _misses;
        private bool _isGameOver;
        private const int MaxMisses = 3;

        public bool IsGameOver => _isGameOver;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _misses = 0; _isGameOver = false;
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateMisses(_misses, MaxMisses); _ui.HideGameOverPanel(); }
            if (_needleManager != null) _needleManager.Init();
        }

        public void OnThreaded(bool perfect)
        {
            if (_isGameOver) return;
            _score += perfect ? 3 : 1;
            if (_ui != null) _ui.UpdateScore(_score);
        }

        public void OnMissed()
        {
            if (_isGameOver) return;
            _misses++;
            if (_ui != null) _ui.UpdateMisses(_misses, MaxMisses);
            if (_misses >= MaxMisses) { _isGameOver = true; if (_ui != null) _ui.ShowGameOverPanel(_score); }
        }
    }
}
