using UnityEngine;

namespace Game052_HammerNail
{
    public class HammerNailGameManager : MonoBehaviour
    {
        [SerializeField] private NailManager _nailManager;
        [SerializeField] private HammerNailUI _ui;

        private int _score;
        private int _misses;
        private bool _isGameOver;
        private const int MaxMisses = 3;

        public bool IsGameOver => _isGameOver;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _score = 0;
            _misses = 0;
            _isGameOver = false;
            if (_ui != null)
            {
                _ui.UpdateScore(_score);
                _ui.UpdateMisses(_misses, MaxMisses);
                _ui.HideGameOverPanel();
            }
            if (_nailManager != null) _nailManager.Init();
        }

        public void OnNailComplete(bool perfect)
        {
            if (_isGameOver) return;
            _score += perfect ? 2 : 1;
            if (_ui != null) _ui.UpdateScore(_score);
        }

        public void OnMiss()
        {
            if (_isGameOver) return;
            _misses++;
            if (_ui != null) _ui.UpdateMisses(_misses, MaxMisses);
            if (_misses >= MaxMisses)
            {
                _isGameOver = true;
                if (_ui != null) _ui.ShowGameOverPanel(_score);
            }
        }
    }
}
