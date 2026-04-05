using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game031v2_BounceKing
{
    public class BallController : MonoBehaviour
    {
        BounceKingGameManager _gameManager;
        PaddleController _paddle;

        Vector2 _velocity;
        float _speed = 5.0f;
        bool _launched;
        bool _isActive;
        CircleCollider2D _col;
        SpriteRenderer _sr;
        float _radius;

        // Bounds (set from camera)
        float _leftWall, _rightWall, _topWall, _bottomBound;

        public bool IsLaunched => _launched;

        void Awake()
        {
            _col = GetComponent<CircleCollider2D>();
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Initialize(float speed, PaddleController paddle, BounceKingGameManager gameManager = null)
        {
            _speed = speed;
            _paddle = paddle;
            if (gameManager != null) _gameManager = gameManager;
            _launched = false;
            _isActive = true;
            _velocity = Vector2.zero;
            _radius = _col != null ? _col.radius * transform.localScale.x : 0.18f;

            float camH = Camera.main.orthographicSize;
            float camW = camH * Camera.main.aspect;
            _leftWall = -camW + _radius;
            _rightWall = camW - _radius;
            _topWall = camH - _radius;
            _bottomBound = -camH - 1f;

            SnapToPaddle();
        }

        public void SetActive(bool active) => _isActive = active;

        void SnapToPaddle()
        {
            if (_paddle != null)
                transform.position = new Vector3(_paddle.transform.position.x,
                    _paddle.transform.position.y + 0.35f, 0f);
        }

        public void Launch(Vector2 direction)
        {
            _launched = true;
            _velocity = direction.normalized * _speed;
        }

        public void SetSpeed(float speed)
        {
            _speed = speed;
            if (_launched && _velocity.sqrMagnitude > 0.01f)
                _velocity = _velocity.normalized * _speed;
        }

        void Update()
        {
            if (!_isActive) return;

            if (!_launched)
            {
                SnapToPaddle();
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    _gameManager.RequestLaunch(this);
                return;
            }

            Move();
        }

        void Move()
        {
            Vector2 pos = transform.position;
            Vector2 newPos = pos + _velocity * Time.deltaTime;

            // Wall bounces
            if (newPos.x <= _leftWall)
            {
                newPos.x = _leftWall;
                _velocity.x = Mathf.Abs(_velocity.x);
            }
            else if (newPos.x >= _rightWall)
            {
                newPos.x = _rightWall;
                _velocity.x = -Mathf.Abs(_velocity.x);
            }
            if (newPos.y >= _topWall)
            {
                newPos.y = _topWall;
                _velocity.y = -Mathf.Abs(_velocity.y);
            }

            // Bottom: ball lost
            if (newPos.y < _bottomBound)
            {
                _isActive = false;
                _gameManager.OnBallLost(this);
                return;
            }

            transform.position = newPos;
            CheckPaddleCollision();
            CheckBlockCollision();
        }

        void CheckPaddleCollision()
        {
            if (_paddle == null) return;
            Vector2 paddlePos = _paddle.transform.position;
            Vector2 paddleScale = _paddle.transform.localScale;
            float pw = _paddle.PaddleHalfWidth * 2f;
            float ph = 0.3f;

            float bx = transform.position.x;
            float by = transform.position.y;
            float px = paddlePos.x;
            float py = paddlePos.y;

            // Only react when approaching from above
            if (_velocity.y > 0f) return;

            bool xOverlap = bx > px - pw / 2f - _radius && bx < px + pw / 2f + _radius;
            bool yOverlap = by > py - ph / 2f - _radius && by < py + ph / 2f + _radius;

            if (xOverlap && yOverlap)
            {
                _velocity = _paddle.GetReflectDirection(transform.position) * _speed;
                transform.position = new Vector3(bx, py + ph / 2f + _radius + 0.01f, 0f);
                StartCoroutine(_paddle.HitPulse());
            }
        }

        void CheckBlockCollision()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _radius * 0.9f,
                LayerMask.GetMask("Block"));
            if (hit == null) return;

            var block = hit.GetComponent<Block>();
            if (block == null || !block.IsAlive) return;

            // Determine bounce direction
            Vector2 blockCenter = hit.bounds.center;
            Vector2 ballPos = transform.position;
            Vector2 diff = ballPos - blockCenter;

            // Determine dominant axis
            float extX = Mathf.Max(hit.bounds.extents.x, 0.001f);
            float extY = Mathf.Max(hit.bounds.extents.y, 0.001f);
            float absX = Mathf.Abs(diff.x) / extX;
            float absY = Mathf.Abs(diff.y) / extY;

            if (absX > absY)
                _velocity.x = diff.x > 0 ? Mathf.Abs(_velocity.x) : -Mathf.Abs(_velocity.x);
            else
                _velocity.y = diff.y > 0 ? Mathf.Abs(_velocity.y) : -Mathf.Abs(_velocity.y);

            // Ensure speed is maintained
            _velocity = _velocity.normalized * _speed;

            block.TakeHit();
            _gameManager?.OnBlockHit(block);
        }

        public void Deactivate()
        {
            _isActive = false;
            Destroy(gameObject);
        }
    }
}
