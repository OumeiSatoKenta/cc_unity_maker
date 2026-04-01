using UnityEngine;
using UnityEngine.InputSystem;

namespace Game058_ThreadNeedle
{
    public class NeedleManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private ThreadNeedleGameManager _gameManager;
        [SerializeField, Tooltip("針スプライト")] private Sprite _needleSprite;
        [SerializeField, Tooltip("糸スプライト")] private Sprite _threadSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private GameObject _needle;
        private GameObject _thread;
        private float _needleSwayAngle;
        private float _needleSwaySpeed = 2f;
        private float _holeSize = 0.4f;
        private bool _threadFlying;
        private Vector2 _threadVelocity;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            SetupStage(0);
        }

        public void StopGame() { _isActive = false; }

        public void NextStage(int stage)
        {
            SetupStage(stage);
        }

        private void SetupStage(int stage)
        {
            // Increase difficulty
            _needleSwaySpeed = 2f + stage * 0.5f;
            _holeSize = Mathf.Max(0.2f, 0.4f - stage * 0.04f);

            if (_needle != null) Destroy(_needle);
            _needle = new GameObject("Needle");
            _needle.transform.position = new Vector3(0f, 2f, 0f);
            var sr = _needle.AddComponent<SpriteRenderer>();
            sr.sprite = _needleSprite; sr.sortingOrder = 3;
            _needle.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            if (_thread != null) Destroy(_thread);
            _thread = new GameObject("Thread");
            _thread.transform.position = new Vector3(0f, -3f, 0f);
            var tsr = _thread.AddComponent<SpriteRenderer>();
            tsr.sprite = _threadSprite; tsr.sortingOrder = 5;
            tsr.color = Color.red;

            _threadFlying = false;
            _needleSwayAngle = 0f;
        }

        private void Update()
        {
            if (!_isActive) return;

            // Sway needle
            _needleSwayAngle += _needleSwaySpeed * Time.deltaTime;
            if (_needle != null)
            {
                float x = Mathf.Sin(_needleSwayAngle) * 2.5f;
                _needle.transform.position = new Vector3(x, 2f, 0f);
            }

            if (_threadFlying && _thread != null)
            {
                _thread.transform.position += (Vector3)_threadVelocity * Time.deltaTime;

                // Check if thread reached needle height
                if (_thread.transform.position.y >= 2f)
                {
                    float dist = Mathf.Abs(_thread.transform.position.x - _needle.transform.position.x);
                    _threadFlying = false;

                    if (dist < _holeSize)
                    {
                        _gameManager.OnThreadPassed();
                    }
                    else
                    {
                        _thread.GetComponent<SpriteRenderer>().color = Color.gray;
                        _gameManager.OnMiss();
                    }
                }

                if (_thread.transform.position.y > 5f)
                {
                    _threadFlying = false;
                    _gameManager.OnMiss();
                }
            }
            else if (!_threadFlying && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                ShootThread();
            }
        }

        private void ShootThread()
        {
            if (_thread == null) return;
            _thread.transform.position = new Vector3(0f, -3f, 0f);
            _thread.GetComponent<SpriteRenderer>().color = Color.red;
            _threadVelocity = new Vector2(0f, 8f);
            _threadFlying = true;
        }
    }
}
