using UnityEngine;
using System.Collections;

namespace Game033v2_AimSniper
{
    public class TargetController : MonoBehaviour
    {
        public enum TargetType { Static, Moving }

        SpriteRenderer _sr;
        float _moveSpeed;
        float _moveRange;
        float _dirX = 1f;
        bool _isActive;
        bool _isHidden;
        TargetType _type;
        float _initialWorldX;

        // Distance-based sway modifier (1.0 = normal, >1.0 = more sway)
        public float ScopeSwayMultiplier { get; private set; } = 1.0f;

        public bool IsAlive { get; private set; } = true;
        public Vector3 WorldPosition => transform.position;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Initialize(TargetType type, Sprite sprite, float moveSpeed, float moveRange, float swayMult)
        {
            _type = type;
            _moveSpeed = moveSpeed;
            _moveRange = moveRange;
            ScopeSwayMultiplier = swayMult;
            IsAlive = true;
            _isActive = true;
            _isHidden = false;
            _sr.sprite = sprite;
            _sr.color = Color.white;
            transform.localScale = Vector3.one;
            _initialWorldX = transform.position.x;
        }

        void Update()
        {
            if (!_isActive || !IsAlive || _isHidden) return;

            if (_type == TargetType.Moving)
            {
                transform.Translate(_dirX * _moveSpeed * Time.deltaTime, 0f, 0f);
                float ox = transform.position.x - _initialWorldX;
                if (Mathf.Abs(ox) >= _moveRange)
                {
                    _dirX *= -1f;
                }
            }
        }

        public void SetHidden(bool hidden)
        {
            _isHidden = hidden;
            _sr.enabled = !hidden;
        }

        public void OnHit(bool headshot)
        {
            if (!IsAlive) return;
            IsAlive = false;
            _isActive = false;
            StartCoroutine(HitAnimation(headshot));
        }

        IEnumerator HitAnimation(bool headshot)
        {
            if (this == null || !gameObject) yield break;
            // Color flash
            _sr.color = headshot ? new Color(1f, 0.9f, 0f) : Color.white;
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.15f;
                // Scale up then down
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
                transform.localScale = Vector3.one * scale;
                yield return null;
            }
            gameObject.SetActive(false);
        }

        public void OnDestroy()
        {
            // No dynamic textures to clean up
        }
    }
}
