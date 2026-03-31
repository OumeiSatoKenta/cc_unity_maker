using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game022_GravityBall
{
    public class GravityBallManager : MonoBehaviour
    {
        [SerializeField] private GameObject _ballPrefab;
        [SerializeField] private GameObject _obstaclePrefab;
        [SerializeField] private float _gravityStrength = 12f;
        [SerializeField] private float _scrollSpeed = 3f;
        [SerializeField] private float _spawnInterval = 2f;
        [SerializeField] private float _ceilingY = 4f;
        [SerializeField] private float _floorY = -4f;

        private GameObject _ballObj;
        private float _ballY;
        private float _ballVelocity;
        private bool _gravityDown = true;
        private bool _isRunning;
        private float _spawnTimer;
        private float _distanceTraveled;

        private readonly List<GameObject> _obstacles = new List<GameObject>();
        private GravityBallGameManager _gameManager;

        private void Awake()
        {
            _gameManager = GetComponentInParent<GravityBallGameManager>();
        }

        private void Update()
        {
            if (!_isRunning) return;
            HandleInput();
            UpdateBall();
            ScrollObstacles();
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnObstacle();
                _spawnTimer = _spawnInterval;
            }
            _distanceTraveled += _scrollSpeed * Time.deltaTime;
            CheckCollisions();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame)
            {
                _gravityDown = !_gravityDown;
                _ballVelocity = 0f;
            }
        }

        private void UpdateBall()
        {
            float gravity = _gravityDown ? -_gravityStrength : _gravityStrength;
            _ballVelocity += gravity * Time.deltaTime;
            _ballY += _ballVelocity * Time.deltaTime;

            // Bounce off ceiling/floor
            if (_ballY > _ceilingY)
            {
                _ballY = _ceilingY;
                _ballVelocity = -Mathf.Abs(_ballVelocity) * 0.5f;
            }
            else if (_ballY < _floorY)
            {
                _ballY = _floorY;
                _ballVelocity = Mathf.Abs(_ballVelocity) * 0.5f;
            }

            if (_ballObj != null)
                _ballObj.transform.position = new Vector3(-3f, _ballY, 0f);
        }

        private void ScrollObstacles()
        {
            float dx = -_scrollSpeed * Time.deltaTime;
            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                if (_obstacles[i] == null) { _obstacles.RemoveAt(i); continue; }
                _obstacles[i].transform.position += new Vector3(dx, 0, 0);
                if (_obstacles[i].transform.position.x < -8f)
                {
                    Destroy(_obstacles[i]);
                    _obstacles.RemoveAt(i);
                    if (_gameManager != null) _gameManager.OnObstaclePassed();
                }
            }
        }

        private void SpawnObstacle()
        {
            if (_obstaclePrefab == null) return;

            // Random gap position
            float gapCenter = Random.Range(_floorY + 1.5f, _ceilingY - 1.5f);
            float gapSize = 2.5f;

            // Top obstacle
            var top = Instantiate(_obstaclePrefab, transform);
            float topHeight = _ceilingY - (gapCenter + gapSize * 0.5f);
            top.transform.position = new Vector3(8f, _ceilingY - topHeight * 0.5f, 0f);
            top.transform.localScale = new Vector3(0.5f, topHeight * 0.25f, 1f);
            _obstacles.Add(top);

            // Bottom obstacle
            var bot = Instantiate(_obstaclePrefab, transform);
            float botHeight = (gapCenter - gapSize * 0.5f) - _floorY;
            bot.transform.position = new Vector3(8f, _floorY + botHeight * 0.5f, 0f);
            bot.transform.localScale = new Vector3(0.5f, botHeight * 0.25f, 1f);
            _obstacles.Add(bot);
        }

        private void CheckCollisions()
        {
            if (_ballObj == null) return;
            Vector2 ballPos = _ballObj.transform.position;

            foreach (var obs in _obstacles)
            {
                if (obs == null) continue;
                // Simple AABB check
                var obsPos = obs.transform.position;
                var obsScale = obs.transform.localScale;
                float halfW = obsScale.x * 0.5f;
                float halfH = obsScale.y * 2f; // obstacle sprite is 128px tall

                if (Mathf.Abs(ballPos.x - obsPos.x) < halfW + 0.3f &&
                    Mathf.Abs(ballPos.y - obsPos.y) < halfH + 0.3f)
                {
                    _isRunning = false;
                    if (_gameManager != null) _gameManager.OnGameOver();
                    return;
                }
            }
        }

        public void StartRun()
        {
            ClearAll();
            _ballY = 0f;
            _ballVelocity = 0f;
            _gravityDown = true;
            _spawnTimer = _spawnInterval;
            _distanceTraveled = 0f;
            _isRunning = true;

            if (_ballPrefab != null)
            {
                _ballObj = Instantiate(_ballPrefab, transform);
                _ballObj.transform.position = new Vector3(-3f, 0f, 0f);
            }
        }

        public void StopRun() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var obs in _obstacles) if (obs != null) Destroy(obs);
            _obstacles.Clear();
            if (_ballObj != null) { Destroy(_ballObj); _ballObj = null; }
        }
    }
}
