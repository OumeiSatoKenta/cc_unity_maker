using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game047_SpinBalance
{
    public class BalanceManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private SpinBalanceGameManager _gameManager;

        [SerializeField, Tooltip("盤面Transform")]
        private Transform _board;

        [SerializeField, Tooltip("コマスプライト")]
        private Sprite _pieceSprite;

        [SerializeField, Tooltip("初期コマ数")]
        private int _initialPieceCount = 3;

        [SerializeField, Tooltip("最大コマ数")]
        private int _maxPieceCount = 8;

        [SerializeField, Tooltip("コマ追加間隔(秒)")]
        private float _spawnInterval = 3f;

        [SerializeField, Tooltip("回転感度")]
        private float _rotationSensitivity = 0.3f;

        private Camera _mainCamera;
        private List<Piece> _pieces = new List<Piece>();
        private float _spawnTimer;
        private bool _isDragging;
        private float _lastMouseX;
        private float _currentAngle;
        private bool _isActive;
        private PhysicsMaterial2D _pieceMat;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _pieceMat = new PhysicsMaterial2D("PieceMat") { friction = 0.6f, bounciness = 0.1f };
        }

        private void OnDestroy()
        {
            if (_pieceMat != null) Destroy(_pieceMat);
        }

        public void StartGame()
        {
            _currentAngle = 0f;
            _spawnTimer = _spawnInterval;
            _isActive = true;
            for (int i = 0; i < _initialPieceCount; i++)
            {
                SpawnPiece();
            }
        }

        public void StopGame()
        {
            _isActive = false;
        }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;

            HandleInput();
            HandleSpawnTimer();
        }

        private void HandleInput()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _lastMouseX = Mouse.current.position.ReadValue().x;
            }

            if (Mouse.current.leftButton.isPressed && _isDragging)
            {
                float currentX = Mouse.current.position.ReadValue().x;
                float deltaX = currentX - _lastMouseX;
                _currentAngle -= deltaX * _rotationSensitivity;
                _currentAngle = Mathf.Clamp(_currentAngle, -45f, 45f);
                _board.rotation = Quaternion.Euler(0f, 0f, _currentAngle);
                _lastMouseX = currentX;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }
        }

        private void HandleSpawnTimer()
        {
            if (_pieces.Count >= _maxPieceCount) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnPiece();
                _spawnTimer = _spawnInterval;
            }
        }

        private void SpawnPiece()
        {
            var obj = new GameObject($"Piece_{_pieces.Count}");
            float x = Random.Range(-1.5f, 1.5f);
            obj.transform.position = _board.position + new Vector3(x, 1.5f, 0f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _pieceSprite;
            sr.sortingOrder = 3;
            sr.color = Color.HSVToRGB(Random.Range(0f, 1f), 0.6f, 0.95f);

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.mass = 0.5f;
            rb.gravityScale = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;

            col.sharedMaterial = _pieceMat;

            var piece = obj.AddComponent<Piece>();
            piece.Initialize(OnPieceFallen);
            _pieces.Add(piece);
        }

        private void OnPieceFallen(Piece p)
        {
            _pieces.Remove(p);
            Destroy(p.gameObject);
        }

        public int CurrentPieceCount => _pieces.Count;
    }
}
