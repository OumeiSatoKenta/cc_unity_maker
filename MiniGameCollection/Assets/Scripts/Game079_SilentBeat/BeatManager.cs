using UnityEngine;
using UnityEngine.InputSystem;

namespace Game079_SilentBeat
{
    public class BeatManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private SilentBeatGameManager _gameManager;
        [SerializeField, Tooltip("サークルスプライト")] private Sprite _circleSprite;
        [SerializeField, Tooltip("目標BPM")] private float _targetBPM = 60f;

        private Camera _mainCamera;
        private bool _isActive;
        private float _targetInterval;
        private float _lastTapTime;
        private float _tolerance;
        private GameObject _circleObj;
        private SpriteRenderer _circleSr;
        private bool _firstTap;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame(float tolerance)
        {
            _isActive = true;
            _tolerance = tolerance;
            _targetInterval = 60f / _targetBPM;
            _lastTapTime = -1f;
            _firstTap = true;

            _circleObj = new GameObject("Circle");
            _circleObj.transform.position = Vector3.zero;
            _circleSr = _circleObj.AddComponent<SpriteRenderer>();
            _circleSr.sprite = _circleSprite; _circleSr.sortingOrder = 3;
            _circleObj.transform.localScale = Vector3.one * 1.5f;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Pulse animation
            if (_circleObj != null)
            {
                float pulse = 1.5f + Mathf.Sin(Time.time * 2f) * 0.05f;
                _circleObj.transform.localScale = Vector3.one * pulse;
            }

            if (Mouse.current == null) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleTap();
            }
        }

        private void HandleTap()
        {
            float now = Time.time;

            if (_firstTap)
            {
                _firstTap = false;
                _lastTapTime = now;
                // Visual feedback
                if (_circleSr != null) _circleSr.color = new Color(0.5f, 0.8f, 1f);
                Invoke(nameof(ResetColor), 0.15f);
                return;
            }

            float interval = now - _lastTapTime;
            float deviation = interval - _targetInterval;
            _lastTapTime = now;

            // Visual feedback based on accuracy
            if (_circleSr != null)
            {
                if (Mathf.Abs(deviation) <= _tolerance * 0.5f)
                    _circleSr.color = new Color(0.3f, 1f, 0.3f); // perfect
                else if (Mathf.Abs(deviation) <= _tolerance)
                    _circleSr.color = new Color(1f, 0.9f, 0.3f); // good
                else
                    _circleSr.color = new Color(1f, 0.3f, 0.3f); // off
            }
            Invoke(nameof(ResetColor), 0.15f);

            _gameManager.OnTap(deviation);
        }

        private void ResetColor()
        {
            if (_circleSr != null) _circleSr.color = new Color(0.4f, 0.47f, 0.63f);
        }
    }
}
