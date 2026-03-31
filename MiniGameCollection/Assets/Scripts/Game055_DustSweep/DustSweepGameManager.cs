using UnityEngine;

namespace Game055_DustSweep
{
    public class DustSweepGameManager : MonoBehaviour
    {
        [SerializeField] private SweepManager _sweepManager;
        [SerializeField] private DustSweepUI _ui;

        private float _timer;
        private int _starsFound;
        private bool _isCleared;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _timer = 0f; _starsFound = 0; _isCleared = false;
            if (_ui != null) { _ui.UpdateTimer(_timer); _ui.UpdateClean(0f); _ui.UpdateStars(_starsFound); _ui.HideClearPanel(); }
            if (_sweepManager != null) _sweepManager.Init();
        }

        private void Update()
        {
            if (_isCleared) return;
            _timer += Time.deltaTime;
            if (_ui != null) _ui.UpdateTimer(_timer);
        }

        public void OnCleanProgress(float percent)
        {
            if (_isCleared) return;
            if (_ui != null) _ui.UpdateClean(percent);
            if (percent >= 0.95f) { _isCleared = true; if (_ui != null) _ui.ShowClearPanel(_timer, _starsFound); }
        }

        public void OnStarFound()
        {
            _starsFound++;
            if (_ui != null) _ui.UpdateStars(_starsFound);
        }
    }
}
