using UnityEngine;

namespace Game032_SpinCutter
{
    public class BladeController : MonoBehaviour
    {
        private Transform _pivot;
        private float _radius;
        private float _speed; // rad/s
        private float _angle;
        private bool _isActive;
        private CircleCollider2D _col;
        private System.Action _onFinished;

        // 1周半で終了
        private const float TotalRotation = Mathf.PI * 3f;

        public void Initialize(Transform pivot, System.Action onFinished)
        {
            _pivot = pivot;
            _onFinished = onFinished;
            _col = GetComponent<CircleCollider2D>();
            gameObject.SetActive(false);
        }

        public void Launch(float radius, float speed)
        {
            _radius = radius;
            _speed = speed;
            _angle = 0f;
            _isActive = true;
            gameObject.SetActive(true);
            UpdatePosition();
        }

        public void ResetBlade()
        {
            _isActive = false;
            gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (!_isActive || _pivot == null) return;

            _angle += _speed * Time.fixedDeltaTime;
            UpdatePosition();

            // 衝突検出
            if (_col != null)
            {
                float worldRadius = _col.radius * transform.lossyScale.x;
                var hits = Physics2D.OverlapCircleAll(transform.position, worldRadius);
                foreach (var hit in hits)
                {
                    if (hit.gameObject == gameObject) continue;
                    var enemy = hit.GetComponent<Enemy>();
                    if (enemy != null) enemy.Hit();
                }
            }

            // 1周半で終了
            if (_angle >= TotalRotation)
            {
                _isActive = false;
                _onFinished?.Invoke(); // コールバック後に非アクティブ化
                gameObject.SetActive(false);
            }
        }

        private void UpdatePosition()
        {
            Vector2 pivotPos = _pivot.position;
            float x = pivotPos.x + Mathf.Cos(_angle) * _radius;
            float y = pivotPos.y + Mathf.Sin(_angle) * _radius;
            transform.position = new Vector3(x, y, 0f);
            // 刃を回転させて視覚的に動いて見えるようにする
            transform.Rotate(0f, 0f, _speed * Mathf.Rad2Deg * Time.fixedDeltaTime);
        }

        public bool IsActive => _isActive;
    }
}
