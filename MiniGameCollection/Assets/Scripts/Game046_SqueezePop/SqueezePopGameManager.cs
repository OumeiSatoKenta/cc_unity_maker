using UnityEngine;

namespace Game046_SqueezePop
{
    public class SqueezePopGameManager : MonoBehaviour
    {
        [SerializeField] private SqueezeManager _squeezeManager;
        [SerializeField] private SqueezePopUI _ui;

        private int _squeezeCount;
        private bool _isCleared;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _squeezeCount = 0;
            _isCleared = false;
            if (_ui != null)
            {
                _ui.UpdateSqueezes(_squeezeCount);
                _ui.UpdateFillPercent(0f);
                _ui.HideClearPanel();
            }
            if (_squeezeManager != null) _squeezeManager.Init();
        }

        public void OnSqueeze(float fillPercent)
        {
            if (_isCleared) return;
            _squeezeCount++;
            if (_ui != null)
            {
                _ui.UpdateSqueezes(_squeezeCount);
                _ui.UpdateFillPercent(fillPercent);
            }
            if (fillPercent >= 0.9f)
            {
                _isCleared = true;
                if (_ui != null) _ui.ShowClearPanel(_squeezeCount);
            }
        }
    }
}
