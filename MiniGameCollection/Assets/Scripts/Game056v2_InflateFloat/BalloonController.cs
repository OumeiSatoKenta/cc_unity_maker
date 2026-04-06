using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Game056v2_InflateFloat
{
    public class BalloonController : MonoBehaviour
    {
        [SerializeField] InflateFloatGameManager _gm;
        [SerializeField] SpriteRenderer _spriteRenderer;
        [SerializeField] CircleCollider2D _collider;

        const float MinSize = 0.4f;
        const float MaxSize = 1.2f;
        const float ExplodeSize = 1.25f;

        float _currentSize = 0.6f;
        float _inflateSpeed = 0.8f;
        float _deflateSpeed = 0.5f;
        float _liftMultiplier = 5f;
        float _horizontalSpeed = 3f;
        float _windForce = 0f;
        float _windTimer = 0f;
        bool _hasWind = false;
        bool _hasSpike = false;

        Rigidbody2D _rb;
        bool _isActive = false;
        Vector2 _dragStartPos;
        float _camHalfWidth;
        float _camSize;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public void SetupStage(StageManager.StageConfig config, int stageNum)
        {
            _inflateSpeed = 0.8f + config.speedMultiplier * 0.15f;
            _deflateSpeed = 0.5f + config.speedMultiplier * 0.1f;
            _hasWind = stageNum >= 4;
            _hasSpike = stageNum >= 5;
            _currentSize = 0.6f;
            ApplySize();

            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[BalloonController] Camera.main not found"); return; }
            _camSize = cam.orthographicSize;
            _camHalfWidth = _camSize * cam.aspect;

            float startY = -_camSize * 0.3f;
            transform.position = new Vector3(0f, startY, 0f);
            _rb.linearVelocity = Vector2.zero;
            _isActive = true;
        }

        void Update()
        {
            if (!_isActive) return;
            if (!_gm.IsPlaying()) return;

            HandleInput();
            HandleWind();
            ClampPosition();
            _gm.OnInflateGaugeChanged((_currentSize - MinSize) / (MaxSize - MinSize));
        }

        void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            bool pressing = mouse.leftButton.isPressed;

            if (pressing)
            {
                _currentSize += _inflateSpeed * Time.deltaTime;
                if (_currentSize >= ExplodeSize)
                {
                    _currentSize = ExplodeSize;
                    Explode();
                    return;
                }

                // Drag for horizontal movement
                Vector2 mouseDelta = mouse.delta.ReadValue();
                float moveX = mouseDelta.x * 0.01f * _horizontalSpeed;
                Vector3 pos = transform.position;
                pos.x += moveX;
                transform.position = pos;
            }
            else
            {
                _currentSize -= _deflateSpeed * Time.deltaTime;
                _currentSize = Mathf.Max(_currentSize, MinSize);
            }

            ApplySize();

            // Apply buoyancy
            float liftForce = (_currentSize - 0.6f) * _liftMultiplier;
            float gravity = -3f;
            float netForce = liftForce + gravity;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, netForce);
        }

        void HandleWind()
        {
            if (!_hasWind) return;
            _windTimer += Time.deltaTime;
            if (_windTimer > 3f)
            {
                _windTimer = 0f;
                _windForce = Random.Range(-2f, 2f);
            }
            // Apply wind via velocity X to avoid mixing transform.position with Rigidbody physics
            Vector2 vel = _rb.linearVelocity;
            vel.x = _windForce;
            _rb.linearVelocity = vel;
        }

        void ClampPosition()
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, -_camHalfWidth + 0.5f, _camHalfWidth - 0.5f);

            float topBound = _camSize - 0.8f;
            float botBound = -_camSize + 1.0f;

            if (pos.y < botBound)
            {
                pos.y = botBound;
                // Fell to ground = game over
                Explode();
                return;
            }
            if (pos.y > topBound) pos.y = topBound;
            transform.position = pos;
        }

        void ApplySize()
        {
            transform.localScale = new Vector3(_currentSize, _currentSize, 1f);
            if (_collider != null) _collider.radius = 0.45f;
        }

        void Explode()
        {
            if (!_isActive) return;
            _isActive = false;
            StopAllCoroutines(); // Stop CoinPulse etc. to avoid mid-animation scale corruption
            StartCoroutine(ExplodeAnim());
        }

        IEnumerator ExplodeAnim()
        {
            if (_spriteRenderer != null)
                _spriteRenderer.color = Color.red;
            float t = 0f;
            Vector3 origScale = transform.localScale;
            while (t < 0.3f)
            {
                transform.localScale = origScale * (1f + t * 3f);
                t += Time.deltaTime;
                yield return null;
            }
            _gm.OnBalloonPopped();
            gameObject.SetActive(false);
        }

        // Pop animation when hitting obstacle
        public void PopFromCollision()
        {
            Explode();
        }

        public void PlayCoinEffect()
        {
            StartCoroutine(CoinPulse());
        }

        IEnumerator CoinPulse()
        {
            Vector3 orig = transform.localScale;
            Vector3 big = orig * 1.3f;
            float t = 0f;
            while (t < 0.15f)
            {
                transform.localScale = Vector3.Lerp(orig, big, t / 0.075f > 1 ? 2f - t/0.075f : t/0.075f);
                t += Time.deltaTime;
                yield return null;
            }
            transform.localScale = orig;
        }
    }
}
