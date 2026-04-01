using UnityEngine;

namespace Game039_BoomerangHero
{
    public class Enemy : MonoBehaviour
    {
        private bool _isDead;
        private System.Action<Enemy> _onKilled;

        public void Initialize(Sprite sprite, System.Action<Enemy> onKilled)
        {
            _isDead = false;
            _onKilled = onKilled;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && sprite != null) sr.sprite = sprite;
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
