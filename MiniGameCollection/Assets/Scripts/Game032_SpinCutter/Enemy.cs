using UnityEngine;

namespace Game032_SpinCutter
{
    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
    public class Enemy : MonoBehaviour
    {
        private bool _isDead;
        private System.Action<Enemy> _onKilled;

        public void Initialize(Sprite sprite, System.Action<Enemy> onKilled)
        {
            _isDead = false;
            _onKilled = onKilled;
            var sr = GetComponent<SpriteRenderer>();
            if (sprite != null) sr.sprite = sprite;
            var col = GetComponent<CircleCollider2D>();
            col.radius = 0.5f;
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
