using UnityEngine;
using System.Collections;

namespace Game057v2_CandyDrop
{
    public class CandyController : MonoBehaviour
    {
        float _meltTime;
        bool _isMelting;
        bool _hasLanded;
        bool _isActive;
        CandySpawner.CandyColor _color;
        CandySpawner.CandyShape _shape;
        CandyDropGameManager _gameManager;
        CandySpawner _spawner;
        SpriteRenderer _sr;
        Rigidbody2D _rb;
        float _elapsed;

        public CandySpawner.CandyColor Color => _color;
        public CandySpawner.CandyShape Shape => _shape;
        public bool HasLanded => _hasLanded;

        public void Init(float meltTime, CandySpawner.CandyColor color, CandySpawner.CandyShape shape,
            CandyDropGameManager gm, CandySpawner spawner)
        {
            _meltTime = meltTime;
            _isMelting = meltTime > 0f;
            _color = color;
            _shape = shape;
            _gameManager = gm;
            _spawner = spawner;
            _isActive = true;
        }

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (!_isActive) return;
            if (!_hasLanded) return;
            if (!_isMelting) return;

            _elapsed += Time.deltaTime;
            float ratio = _elapsed / _meltTime;
            if (_sr != null)
            {
                Color c = _sr.color;
                _sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0.85f, 0f, ratio));
            }

            if (_elapsed >= _meltTime)
            {
                _isActive = false;
                Destroy(gameObject);
            }
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (_hasLanded) return;
            if (!_isActive) return;
            // Landed on ground or other candy
            if (col.gameObject.CompareTag("Ground") || col.gameObject.GetComponent<CandyController>() != null)
            {
                _hasLanded = true;
                _gameManager?.OnCandyLanded();
                StartCoroutine(LandingPulse());
            }
        }

        IEnumerator LandingPulse()
        {
            Vector3 originalScale = transform.localScale;
            float elapsed = 0f;
            float duration = 0.2f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = t < 0.5f ? Mathf.Lerp(1f, 1.3f, t * 2f) : Mathf.Lerp(1.3f, 1f, (t - 0.5f) * 2f);
                transform.localScale = originalScale * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = originalScale;
        }
    }
}
