using UnityEngine;

namespace Game033_AimSniper
{
    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
    public class Target : MonoBehaviour
    {
        private float _speed;
        private float _minX;
        private float _maxX;
        private bool _isDead;
        private bool _movingRight = true;
        private System.Action<Target> _onKilled;

        public void Initialize(Sprite sprite, float speed, float minX, float maxX, System.Action<Target> onKilled)
        {
            _speed = speed;
            _minX = minX;
            _maxX = maxX;
            _isDead = false;
            _onKilled = onKilled;

            var sr = GetComponent<SpriteRenderer>();
            if (sprite != null) sr.sprite = sprite;
            sr.sortingOrder = 2;

            var col = GetComponent<CircleCollider2D>();
            col.radius = 0.5f;
        }

        private void Update()
        {
            if (_isDead) return;

            float dir = _movingRight ? 1f : -1f;
            transform.position += new Vector3(dir * _speed * Time.deltaTime, 0f, 0f);

            if (transform.position.x >= _maxX) _movingRight = false;
            if (transform.position.x <= _minX) _movingRight = true;
        }

        public void Hit()
        {
            if (_isDead) return;
            _isDead = true;
            _onKilled?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
