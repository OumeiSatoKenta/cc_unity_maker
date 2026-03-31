using UnityEngine;

namespace Game045_FingerPaint
{
    public class FingerPaintGameManager : MonoBehaviour
    {
        [SerializeField] private PaintManager _paintManager;
        [SerializeField] private FingerPaintUI _ui;

        private float _timer;
        private float _timeLimit = 30f;
        private int _strokeCount;
        private bool _isFinished;

        public bool IsFinished => _isFinished;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _timer = _timeLimit;
            _strokeCount = 0;
            _isFinished = false;
            if (_ui != null)
            {
                _ui.UpdateTimer(_timer);
                _ui.UpdateStrokes(_strokeCount);
                _ui.HideFinishPanel();
            }
            if (_paintManager != null) _paintManager.Init();
        }

        private void Update()
        {
            if (_isFinished) return;
            _timer -= Time.deltaTime;
            if (_ui != null) _ui.UpdateTimer(Mathf.Max(0f, _timer));
            if (_timer <= 0f)
            {
                _isFinished = true;
                if (_ui != null) _ui.ShowFinishPanel(_strokeCount);
            }
        }

        public void OnStroke()
        {
            if (_isFinished) return;
            _strokeCount++;
            if (_ui != null) _ui.UpdateStrokes(_strokeCount);
        }
    }
}
