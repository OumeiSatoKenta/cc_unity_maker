using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game049_CloudHop
{
    public class HopManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private CloudHopGameManager _gameManager;
        [SerializeField, Tooltip("プレイヤーRB")] private Rigidbody2D _playerRb;
        [SerializeField, Tooltip("雲スプライト")] private Sprite _cloudSprite;
        [SerializeField, Tooltip("ジャンプ力")] private float _jumpForce = 10f;
        [SerializeField, Tooltip("左右移動速度")] private float _moveSpeed = 8f;

        private Camera _mainCamera;
        private bool _isActive;
        private float _lastMouseX;
        private bool _isDragging;
        private float _highestCloudY;
        private List<Cloud> _clouds = new List<Cloud>();

        private const float CloudSpacing = 2.5f;
        private const float CloudLifetime = 4f;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _highestCloudY = _playerRb.transform.position.y;
            // Spawn initial clouds
            for (int i = 0; i < 8; i++)
            {
                SpawnCloud(_playerRb.transform.position.y + (i + 1) * CloudSpacing);
            }
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;

            HandleInput();
            ManageClouds();
        }

        private void HandleInput()
        {
            // Jump on tap (when grounded)
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _lastMouseX = Mouse.current.position.ReadValue().x;

                if (IsGrounded())
                {
                    _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, _jumpForce);
                }
            }

            // Horizontal movement via drag
            if (Mouse.current.leftButton.isPressed && _isDragging)
            {
                float currentX = Mouse.current.position.ReadValue().x;
                float deltaX = (currentX - _lastMouseX) / Screen.width * _moveSpeed;
                var vel = _playerRb.linearVelocity;
                vel.x = deltaX * 20f;
                _playerRb.linearVelocity = vel;
                _lastMouseX = currentX;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
                var vel = _playerRb.linearVelocity;
                vel.x = 0f;
                _playerRb.linearVelocity = vel;
            }
        }

        private void ManageClouds()
        {
            // Spawn new clouds above
            float playerY = _playerRb.transform.position.y;
            while (_highestCloudY < playerY + 20f)
            {
                _highestCloudY += CloudSpacing;
                SpawnCloud(_highestCloudY);
            }

            // Clean up destroyed clouds
            _clouds.RemoveAll(c => c == null);
        }

        private void SpawnCloud(float y)
        {
            float x = Random.Range(-3f, 3f);
            var obj = new GameObject("Cloud");
            obj.transform.position = new Vector3(x, y, 0f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _cloudSprite;
            sr.sortingOrder = 1;
            sr.color = new Color(1f, 1f, 1f, 0.9f);
            obj.transform.localScale = new Vector3(1f, 0.5f, 1f);

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 0.3f);
            col.offset = new Vector2(0f, 0.1f);

            var effector = obj.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 170f;
            col.usedByEffector = true;

            var cloud = obj.AddComponent<Cloud>();
            cloud.Initialize(CloudLifetime);
            _clouds.Add(cloud);
        }

        private bool IsGrounded()
        {
            Vector2 origin = _playerRb.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.6f);
            return hit.collider != null && hit.collider.GetComponent<Cloud>() != null;
        }
    }
}
