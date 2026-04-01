using UnityEngine;

namespace Game045_FingerPaint
{
    public class FingerPaintGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private PaintManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private FingerPaintUI _ui;

        [SerializeField, Tooltip("インク上限")]
        private float _maxInk = 100f;

        private float _ink;
        private float _matchRate;
        private bool _isPlaying;

        private void Start()
        {
            _ink = _maxInk;
            _matchRate = 0f;
            _isPlaying = true;
            _ui.UpdateInk(_ink, _maxInk);
            _ui.UpdateMatch(_matchRate);
            _manager.StartGame();
        }

        public void ConsumeInk(float amount)
        {
            if (!_isPlaying) return;
            _ink = Mathf.Max(0f, _ink - amount);
            _ui.UpdateInk(_ink, _maxInk);
            if (_ink <= 0f)
            {
                _isPlaying = false;
                _matchRate = _manager.CalculateMatch();
                _ui.UpdateMatch(_matchRate);
                int stars = _matchRate >= 90f ? 3 : (_matchRate >= 70f ? 2 : (_matchRate >= 50f ? 1 : 0));
                if (stars > 0) _ui.ShowClear(stars, _matchRate);
                else _ui.ShowGameOver(_matchRate);
            }
        }

        public void OnSubmit(float match)
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _matchRate = match;
            _ui.UpdateMatch(_matchRate);
            int stars = _matchRate >= 90f ? 3 : (_matchRate >= 70f ? 2 : (_matchRate >= 50f ? 1 : 0));
            if (stars > 0) _ui.ShowClear(stars, _matchRate);
            else _ui.ShowGameOver(_matchRate);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public float Ink => _ink;
    }
}
