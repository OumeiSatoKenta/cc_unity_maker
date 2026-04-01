using UnityEngine;

namespace Game031_BounceKing
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class BallController : MonoBehaviour
    {
        private const float Speed = 7f;

        private Rigidbody2D _rb;
        private CircleCollider2D _col;
        private bool _isActive;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.linearDamping = 0f;
            _rb.angularDamping = 0f;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            _col = GetComponent<CircleCollider2D>();
            _col.radius = 0.5f;
        }

        /// <summary>PhysicsMaterial2Dを設定する（Awake後に呼ぶこと）</summary>
        public void SetMaterial(PhysicsMaterial2D mat)
        {
            if (mat == null) return;
            _rb.sharedMaterial = mat;
            _col.sharedMaterial = mat;
        }

        public void Launch(Vector2 direction)
        {
            _isActive = true;
            _rb.linearVelocity = direction.normalized * Speed;
        }

        public void Stop()
        {
            _isActive = false;
            _rb.linearVelocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (!_isActive) return;

            // 物理演算による速度ドリフトを補正して一定速を維持
            float currentSpeed = _rb.linearVelocity.magnitude;
            if (currentSpeed > 0.1f && Mathf.Abs(currentSpeed - Speed) > 0.5f)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * Speed;
            }
        }

        public bool IsActive => _isActive;
    }
}
