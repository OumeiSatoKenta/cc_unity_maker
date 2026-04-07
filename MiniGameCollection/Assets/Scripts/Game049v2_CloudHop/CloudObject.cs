using System.Collections;
using UnityEngine;

namespace Game049v2_CloudHop
{
    public enum CloudType { Normal, Spring, Thunder, Moving }

    public class CloudObject : MonoBehaviour
    {
        public CloudType CloudType { get; private set; }
        public bool IsAlive { get; private set; } = true;

        private float _lifetime;
        private float _elapsed;
        private SpriteRenderer _sr;
        private bool _isRandomFade;
        private bool _isMoving;
        private float _moveSpeed;
        private float _moveRange;
        private float _startX;

        private CloudHopGameManager _gameManager;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Initialize(CloudType type, float lifetime, bool randomFade, bool moving, float moveSpeed, float moveRange, CloudHopGameManager gameManager)
        {
            CloudType = type;
            _lifetime = lifetime;
            _isRandomFade = randomFade;
            _isMoving = moving;
            _moveSpeed = moveSpeed;
            _moveRange = moveRange;
            _gameManager = gameManager;
            _startX = transform.position.x;
            IsAlive = true;
            _elapsed = 0f;

            if (_isRandomFade && Random.value < 0.33f)
            {
                float delay = Random.Range(_lifetime * 0.3f, _lifetime * 0.8f);
                StartCoroutine(RandomFadeCoroutine(delay));
            }
        }

        IEnumerator RandomFadeCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (IsAlive) Dissolve();
        }

        void Update()
        {
            if (!IsAlive) return;

            _elapsed += Time.deltaTime;

            // Moving cloud oscillation
            if (_isMoving)
            {
                float x = _startX + Mathf.Sin(_elapsed * _moveSpeed) * _moveRange;
                transform.position = new Vector3(x, transform.position.y, transform.position.z);
            }

            // Blink warning when close to disappearing
            float remaining = _lifetime - _elapsed;
            if (remaining < 1.5f && remaining > 0f)
            {
                float blink = Mathf.Sin(_elapsed * Mathf.PI * 4f) * 0.5f + 0.5f;
                Color c = _sr.color;
                c.a = Mathf.Lerp(0.3f, 1.0f, blink);
                _sr.color = c;
            }

            if (_elapsed >= _lifetime)
            {
                Dissolve();
            }
        }

        public void Dissolve()
        {
            if (!IsAlive) return;
            IsAlive = false;
            StartCoroutine(FadeOut());
        }

        IEnumerator FadeOut()
        {
            float t = 0f;
            Color c = _sr.color;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / 0.3f);
                _sr.color = c;
                yield return null;
            }
            Destroy(gameObject);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsAlive || !collision.gameObject.CompareTag("Player")) return;

            // Only react when player lands from above
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    _gameManager?.OnCloudLanded(CloudType);

                    if (CloudType == CloudType.Spring)
                        StartCoroutine(ScalePulse());
                    else if (CloudType == CloudType.Thunder)
                        StartCoroutine(ThunderEffect());
                    break;
                }
            }
        }

        IEnumerator ScalePulse()
        {
            Vector3 orig = transform.localScale;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.3f;
                float s = ratio < 0.5f ? Mathf.Lerp(1f, 0.7f, ratio * 2f) : Mathf.Lerp(0.7f, 1f, (ratio - 0.5f) * 2f);
                transform.localScale = orig * s;
                yield return null;
            }
            transform.localScale = orig;
        }

        IEnumerator ThunderEffect()
        {
            Color orig = _sr.color;
            Color flash = new Color(1f, 1f, 0f, 1f);
            float t = 0f;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.4f;
                _sr.color = Color.Lerp(flash, orig, ratio);
                yield return null;
            }
            _sr.color = orig;
        }
    }
}
