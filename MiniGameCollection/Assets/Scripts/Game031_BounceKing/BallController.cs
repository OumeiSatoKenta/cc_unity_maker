using UnityEngine;

namespace Game031_BounceKing
{
    public class BallController : MonoBehaviour
    {
        private const float Speed = 7f;

        private Rigidbody2D _rb;
        private CircleCollider2D _col;
        private bool _isActive;

        /// <summary>SpawnBall側から呼ぶ初期化メソッド</summary>
        public void Initialize(PhysicsMaterial2D mat)
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CircleCollider2D>();

            if (_rb == null) { Debug.LogError("[BallController] Rigidbody2D not found"); return; }

            _rb.gravityScale = 0f;
            _rb.linearDamping = 0f;
            _rb.angularDamping = 0f;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (_col != null) _col.radius = 0.5f;

            if (mat != null)
            {
                _rb.sharedMaterial = mat;
                if (_col != null) _col.sharedMaterial = mat;
            }
        }

        public void Launch(Vector2 direction)
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            _isActive = true;
            _rb.linearVelocity = direction.normalized * Speed;
        }

        public void Stop()
        {
            _isActive = false;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (!_isActive || _rb == null) return;

            // 速度を一定に保つ
            float currentSpeed = _rb.linearVelocity.magnitude;
            if (currentSpeed > 0.1f && Mathf.Abs(currentSpeed - Speed) > 0.5f)
                _rb.linearVelocity = _rb.linearVelocity.normalized * Speed;

            // OverlapCircleでブロック衝突を手動検出（OnCollisionEnter2Dの代替）
            if (_col != null)
            {
                float worldRadius = _col.radius * transform.lossyScale.x;
                var hits = Physics2D.OverlapCircleAll(transform.position, worldRadius);
                foreach (var hit in hits)
                {
                    if (hit.gameObject == gameObject) continue; // 自分自身を除外
                    var block = hit.GetComponent<Block>();
                    if (block != null) block.Hit();
                }
            }
        }

        public bool IsActive => _isActive;
    }
}
