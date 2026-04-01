using UnityEngine;

namespace Game046_SqueezePop
{
    public class Bubble : MonoBehaviour
    {
        private bool _isPopped;
        private float _squeezeAmount;
        private Sprite _popSprite;
        private System.Action<Bubble> _onPopped;
        private Vector3 _originalScale;

        private const float PopThreshold = 1.0f;

        public void Initialize(Sprite popSprite, System.Action<Bubble> onPopped)
        {
            _isPopped = false;
            _squeezeAmount = 0f;
            _popSprite = popSprite;
            _onPopped = onPopped;
            _originalScale = transform.localScale;
        }

        public void Squeeze(float dt)
        {
            if (_isPopped) return;
            _squeezeAmount += dt * 2f;
            // 膨張アニメーション
            float grow = 1f + _squeezeAmount * 0.3f;
            transform.localScale = _originalScale * grow;

            if (_squeezeAmount >= PopThreshold)
            {
                Pop();
            }
        }

        private void Pop()
        {
            _isPopped = true;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && _popSprite != null)
            {
                sr.sprite = _popSprite;
                sr.color = Color.yellow;
            }
            _onPopped?.Invoke(this);
            Destroy(gameObject, 0.3f);
        }

        public bool IsPopped => _isPopped;
    }
}
