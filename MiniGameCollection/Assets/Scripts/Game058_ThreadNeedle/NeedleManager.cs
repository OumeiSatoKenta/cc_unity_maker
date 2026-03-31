using UnityEngine;
using UnityEngine.InputSystem;

namespace Game058_ThreadNeedle
{
    public class NeedleManager : MonoBehaviour
    {
        [SerializeField] private ThreadNeedleGameManager _gameManager;

        private const float NeedleSwingSpeed = 2f;
        private const float NeedleSwingRange = 2.5f;
        private const float ThreadSpeed = 12f;
        private const float HoleSize = 0.8f;
        private const float PerfectSize = 0.3f;

        private GameObject _needle;
        private GameObject _thread;
        private GameObject _target;
        private Sprite _needleSprite, _threadSprite, _targetSprite;
        private Camera _mainCamera;
        private float _swingPhase;
        private float _currentHoleSize;
        private bool _threadFlying;
        private Vector3 _threadVel;
        private int _roundCount;

        public void Init()
        {
            _mainCamera = Camera.main;
            _needleSprite = Resources.Load<Sprite>("Sprites/Game058_ThreadNeedle/needle");
            _threadSprite = Resources.Load<Sprite>("Sprites/Game058_ThreadNeedle/thread");
            _targetSprite = Resources.Load<Sprite>("Sprites/Game058_ThreadNeedle/target");

            CleanUp();
            _roundCount = 0;
            _swingPhase = 0f;
            _threadFlying = false;
            _currentHoleSize = HoleSize;

            _needle = new GameObject("Needle");
            _needle.transform.position = new Vector3(0f, 2f, 0f);
            _needle.transform.localScale = Vector3.one * 2f;
            var nsr = _needle.AddComponent<SpriteRenderer>(); nsr.sprite = _needleSprite; nsr.sortingOrder = 5;

            _target = new GameObject("Target");
            _target.transform.position = new Vector3(0f, 2f, 0f);
            _target.transform.localScale = Vector3.one * _currentHoleSize;
            var tsr = _target.AddComponent<SpriteRenderer>(); tsr.sprite = _targetSprite; tsr.sortingOrder = 3;
            tsr.color = new Color(1f, 1f, 0.5f, 0.6f);

            ResetThread();
        }

        private void ResetThread()
        {
            if (_thread != null) Destroy(_thread);
            _thread = new GameObject("Thread");
            _thread.transform.position = new Vector3(0f, -3f, 0f);
            _thread.transform.localScale = new Vector3(0.5f, 2f, 1f);
            var sr = _thread.AddComponent<SpriteRenderer>(); sr.sprite = _threadSprite; sr.sortingOrder = 8;
            sr.color = new Color(0.9f, 0.2f, 0.2f);
            _threadFlying = false;
        }

        private void CleanUp()
        {
            if (_needle != null) Destroy(_needle);
            if (_thread != null) Destroy(_thread);
            if (_target != null) Destroy(_target);
        }

        private void Update()
        {
            if (_gameManager != null && _gameManager.IsGameOver) return;

            float speed = NeedleSwingSpeed + _roundCount * 0.3f;
            _swingPhase += Time.deltaTime * speed;
            float nx = Mathf.Sin(_swingPhase) * NeedleSwingRange;
            float ny = 2f + Mathf.Cos(_swingPhase * 1.3f) * 0.5f;
            if (_needle != null) _needle.transform.position = new Vector3(nx, ny, 0f);
            if (_target != null) _target.transform.position = new Vector3(nx, ny, 0f);

            if (!_threadFlying)
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _threadFlying = true;
                    _threadVel = Vector3.up * ThreadSpeed;
                }
            }
            else
            {
                _thread.transform.position += _threadVel * Time.deltaTime;

                float dist = Vector2.Distance(_thread.transform.position, _needle.transform.position);
                if (dist < _currentHoleSize)
                {
                    bool perfect = dist < PerfectSize;
                    if (_gameManager != null) _gameManager.OnThreaded(perfect);
                    _roundCount++;
                    _currentHoleSize = Mathf.Max(0.3f, HoleSize - _roundCount * 0.05f);
                    if (_target != null) _target.transform.localScale = Vector3.one * _currentHoleSize;
                    ResetThread();
                }
                else if (_thread.transform.position.y > 6f)
                {
                    if (_gameManager != null) _gameManager.OnMissed();
                    ResetThread();
                }
            }
        }
    }
}
