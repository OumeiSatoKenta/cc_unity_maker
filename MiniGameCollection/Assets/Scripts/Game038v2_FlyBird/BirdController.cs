using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Game038v2_FlyBird
{
    public class BirdController : MonoBehaviour
    {
        [SerializeField] FlyBirdGameManager _gameManager;
        [SerializeField] Sprite _spriteNormal;
        [SerializeField] Sprite _spriteFlap;

        Rigidbody2D _rb;
        SpriteRenderer _sr;
        bool _isActive;
        float _flapForce = 5.0f;
        Vector3 _startPosition;
        Camera _camera;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _camera = Camera.main;
            _startPosition = transform.position;
        }

        public void SetupStage(int stage)
        {
            // Bird behavior consistent across stages; spawner handles difficulty
        }

        public void ResetBird()
        {
            _isActive = true;
            transform.position = _startPosition;
            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 2.0f;
            if (_sr != null && _spriteNormal != null) _sr.sprite = _spriteNormal;
            if (_sr != null) _sr.color = Color.white;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active)
            {
                _rb.linearVelocity = Vector2.zero;
                StopAllCoroutines();
            }
        }

        void Update()
        {
            if (!_isActive) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                _rb.linearVelocity = new Vector2(0f, _flapForce);
                StartCoroutine(FlapAnimation());
            }

            // Clamp position within camera bounds
            if (_camera != null)
            {
                float topY = _camera.orthographicSize - 0.5f;
                float botY = -_camera.orthographicSize + 0.5f;
                Vector3 pos = transform.position;
                if (pos.y > topY)
                {
                    pos.y = topY;
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -1f);
                    transform.position = pos;
                    _isActive = false;
                    _gameManager.OnBirdDied();
                    return;
                }
                else if (pos.y < botY)
                {
                    pos.y = botY;
                    transform.position = pos;
                    _isActive = false;
                    _gameManager.OnBirdDied();
                    return;
                }
            }

            // Rotate bird based on velocity
            float vel = _rb.linearVelocity.y;
            float angle = Mathf.Clamp(vel * 8f, -80f, 40f);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        IEnumerator FlapAnimation()
        {
            if (_sr != null && _spriteFlap != null) _sr.sprite = _spriteFlap;
            yield return new WaitForSeconds(0.12f);
            if (_sr != null && _spriteNormal != null) _sr.sprite = _spriteNormal;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isActive) return;

            if (other.CompareTag("Obstacle") || other.CompareTag("Ground"))
            {
                _isActive = false;
                StartCoroutine(DeathEffect());
                _gameManager.OnBirdDied();
            }
            else if (other.CompareTag("Coin"))
            {
                _gameManager.OnCoinCollected();
                StartCoroutine(CollectCoinEffect(other.gameObject));
            }
            else if (other.CompareTag("ScoreTrigger"))
            {
                PipePair pipe = other.GetComponentInParent<PipePair>();
                if (pipe != null && !pipe.Scored)
                {
                    pipe.Scored = true;
                    StartCoroutine(PassEffect());
                    _gameManager.OnPipePassed(pipe.CurrentStage);
                }
            }
        }

        IEnumerator DeathEffect()
        {
            // Red flash
            if (_sr != null)
            {
                _sr.color = new Color(1f, 0.3f, 0.3f);
                yield return new WaitForSeconds(0.1f);
                _sr.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                _sr.color = new Color(1f, 0.3f, 0.3f);
                yield return new WaitForSeconds(0.1f);
                _sr.color = Color.white;
            }
            // Camera shake
            if (_camera != null) StartCoroutine(CameraShake());
        }

        IEnumerator PassEffect()
        {
            // Scale pulse
            float elapsed = 0f;
            float duration = 0.15f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = 1f + 0.2f * Mathf.Sin(t * Mathf.PI);
                transform.localScale = new Vector3(scale, scale, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = Vector3.one;
        }

        IEnumerator CollectCoinEffect(GameObject coin)
        {
            SpriteRenderer coinSr = coin.GetComponent<SpriteRenderer>();
            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 origScale = coin.transform.localScale;
            while (elapsed < duration)
            {
                if (coin == null) yield break;
                float t = elapsed / duration;
                coin.transform.localScale = origScale * (1f - t);
                if (coinSr != null) coinSr.color = new Color(1f, 1f, 0f, 1f - t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            coin.SetActive(false);
        }

        IEnumerator CameraShake()
        {
            Vector3 origPos = _camera.transform.localPosition;
            float elapsed = 0f;
            float duration = 0.3f;
            float magnitude = 0.15f;
            while (elapsed < duration)
            {
                float t = 1f - elapsed / duration;
                _camera.transform.localPosition = origPos + (Vector3)Random.insideUnitCircle * magnitude * t;
                elapsed += Time.deltaTime;
                yield return null;
            }
            _camera.transform.localPosition = origPos;
        }
    }
}
