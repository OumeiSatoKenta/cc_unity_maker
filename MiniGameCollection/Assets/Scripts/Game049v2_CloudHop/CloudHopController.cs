using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game049v2_CloudHop
{
    public class CloudHopController : MonoBehaviour
    {
        [SerializeField] CloudHopGameManager _gameManager;
        [SerializeField] SpriteRenderer _playerSprite;

        private Rigidbody2D _rb;
        private bool _isGrounded;
        private bool _isActive;
        private bool _isStunned;
        private HashSet<GameObject> _groundedClouds = new HashSet<GameObject>();
        private float _jumpForce = 10f;
        private float _horizontalSpeed = 4f;
        private float _quickDropForce = -15f;

        // Input tracking
        private Vector2 _touchStart;
        private bool _isDragging;
        private float _dragStartTime;
        private float _baseAltitude;

        // Camera follow
        private Camera _cam;
        private float _highestY;

        // Stage config
        private float _speedMultiplier = 1f;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            StopAllCoroutines();
            if (_playerSprite != null) _playerSprite.color = Color.white;
            transform.localScale = Vector3.one;

            _speedMultiplier = config.speedMultiplier;
            _jumpForce = 10f + stageNumber * 0.5f;
            _horizontalSpeed = 4f;
            _isActive = true;
            _isStunned = false;
            _isGrounded = false;
            _groundedClouds.Clear();
            _highestY = transform.position.y;
            _baseAltitude = transform.position.y;
            _cam = Camera.main;
        }

        public void SetActive(bool active) => _isActive = active;

        public float GetAltitude()
        {
            float raw = transform.position.y - _baseAltitude;
            return Mathf.Max(0f, raw);
        }

        public void ResetForNewStage()
        {
            StopAllCoroutines();
            if (_playerSprite != null) _playerSprite.color = Color.white;
            transform.localScale = Vector3.one;
            _isActive = false;
            _isStunned = false;
            _isGrounded = false;
            _groundedClouds.Clear();
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.gravityScale = 2f;
            }
        }

        void Update()
        {
            if (!_isActive || _isStunned) return;

            HandleInput();
            CameraFollow();
            CheckFallDeath();
        }

        void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 mousePos = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _touchStart = mousePos;
                _isDragging = true;
                _dragStartTime = Time.time;

                // Jump only when grounded
                if (_isGrounded)
                {
                    Jump(_jumpForce);
                }
            }

            if (mouse.leftButton.isPressed && _isDragging)
            {
                Vector2 delta = mousePos - _touchStart;

                // Horizontal movement via drag
                float hMove = delta.x * 0.015f;
                if (_rb != null)
                {
                    Vector2 vel = _rb.linearVelocity;
                    vel.x = Mathf.Lerp(vel.x, hMove * _horizontalSpeed, Time.deltaTime * 10f);
                    _rb.linearVelocity = vel;
                }

                // Quick drop: downward swipe
                if (delta.y < -80f && Time.time - _dragStartTime < 0.3f && !_isGrounded)
                {
                    QuickDrop();
                    _touchStart = mousePos; // reset to avoid repeated trigger
                }
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }
        }

        void Jump(float force)
        {
            if (_rb == null) return;
            _isGrounded = false;
            Vector2 vel = _rb.linearVelocity;
            vel.y = force;
            _rb.linearVelocity = vel;

            StartCoroutine(JumpSquash());
        }

        public void JumpBoost()
        {
            if (!_isActive || _isStunned) return;
            Jump(_jumpForce * 2f);
            StartCoroutine(SpringPulse());
        }

        void QuickDrop()
        {
            if (_rb == null) return;
            Vector2 vel = _rb.linearVelocity;
            vel.y = _quickDropForce;
            _rb.linearVelocity = vel;
            _gameManager?.OnQuickDrop();
        }

        void CameraFollow()
        {
            if (_cam == null) return;
            float targetY = Mathf.Max(transform.position.y, _cam.transform.position.y);
            Vector3 camPos = _cam.transform.position;
            camPos.y = Mathf.Lerp(camPos.y, targetY, Time.deltaTime * 3f);
            _cam.transform.position = camPos;
        }

        void CheckFallDeath()
        {
            if (_cam == null) return;
            float bottomLimit = _cam.transform.position.y - _cam.orthographicSize - 1f;
            if (transform.position.y < bottomLimit)
            {
                _isActive = false;
                _gameManager?.TriggerGameOver();
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Cloud")) return;

            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    _groundedClouds.Add(collision.gameObject);
                    _isGrounded = true;

                    var cloud = collision.gameObject.GetComponent<CloudObject>();
                    if (cloud != null && cloud.CloudType == CloudType.Spring && !_isStunned)
                    {
                        JumpBoost();
                    }
                    break;
                }
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Cloud")) return;
            _groundedClouds.Remove(collision.gameObject);
            _isGrounded = _groundedClouds.Count > 0;
        }

        public void TriggerStun()
        {
            if (_isStunned) return;
            StartCoroutine(StunCoroutine());
        }

        IEnumerator StunCoroutine()
        {
            _isStunned = true;

            // Red flash
            if (_playerSprite != null)
            {
                _playerSprite.color = Color.red;
            }

            // Camera shake
            StartCoroutine(CameraShake(0.2f, 0.15f));

            yield return new WaitForSeconds(1f);
            _isStunned = false;

            if (_playerSprite != null)
            {
                _playerSprite.color = Color.white;
            }
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            if (_cam == null) yield break;
            Vector3 origPos = _cam.transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = origPos.x + Random.Range(-1f, 1f) * magnitude;
                float y = origPos.y + Random.Range(-1f, 1f) * magnitude;
                _cam.transform.position = new Vector3(x, y, origPos.z);
                yield return null;
            }
            _cam.transform.position = origPos;
        }

        IEnumerator JumpSquash()
        {
            Vector3 orig = transform.localScale;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.2f;
                float sx = ratio < 0.5f ? Mathf.Lerp(1f, 1.3f, ratio * 2f) : Mathf.Lerp(1.3f, 1f, (ratio - 0.5f) * 2f);
                float sy = ratio < 0.5f ? Mathf.Lerp(1f, 0.7f, ratio * 2f) : Mathf.Lerp(0.7f, 1f, (ratio - 0.5f) * 2f);
                transform.localScale = new Vector3(orig.x * sx, orig.y * sy, orig.z);
                yield return null;
            }
            transform.localScale = orig;
        }

        IEnumerator SpringPulse()
        {
            Vector3 orig = transform.localScale;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.3f;
                float s = ratio < 0.5f ? Mathf.Lerp(1f, 1.5f, ratio * 2f) : Mathf.Lerp(1.5f, 1f, (ratio - 0.5f) * 2f);
                transform.localScale = orig * s;
                if (_playerSprite != null)
                    _playerSprite.color = Color.Lerp(Color.yellow, Color.white, ratio);
                yield return null;
            }
            transform.localScale = orig;
            if (_playerSprite != null) _playerSprite.color = Color.white;
        }
    }
}
