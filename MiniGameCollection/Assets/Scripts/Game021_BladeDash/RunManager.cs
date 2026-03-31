using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game021_BladeDash
{
    public class RunManager : MonoBehaviour
    {
        [SerializeField] private GameObject _bladePrefab;
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private float _laneWidth = 2.0f;
        [SerializeField] private float _scrollSpeed = 4.0f;
        [SerializeField] private float _spawnInterval = 1.2f;

        private readonly List<GameObject> _obstacles = new List<GameObject>();
        private readonly List<GameObject> _coins = new List<GameObject>();
        private GameObject _playerObj;
        private int _currentLane; // 0=left, 1=center, 2=right
        private float _spawnTimer;
        private bool _isRunning;

        private BladeDashGameManager _gameManager;
        private Vector2 _dragStart;
        private bool _isDragging;

        private void Awake()
        {
            _gameManager = GetComponentInParent<BladeDashGameManager>();
        }

        private void Update()
        {
            if (!_isRunning) return;
            HandleInput();
            ScrollObjects();
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnRow();
                _spawnTimer = _spawnInterval;
            }
            CheckCollisions();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _dragStart = mouse.position.ReadValue();
                _isDragging = true;
            }

            if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
            {
                _isDragging = false;
                float dx = mouse.position.ReadValue().x - _dragStart.x;
                if (Mathf.Abs(dx) > 40f)
                {
                    if (dx > 0 && _currentLane < 2)
                        _currentLane++;
                    else if (dx < 0 && _currentLane > 0)
                        _currentLane--;

                    if (_playerObj != null)
                        _playerObj.transform.position = new Vector3(GetLaneX(_currentLane), -3.5f, 0f);
                }
            }
        }

        private void ScrollObjects()
        {
            float dy = -_scrollSpeed * Time.deltaTime;

            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                if (_obstacles[i] == null) { _obstacles.RemoveAt(i); continue; }
                _obstacles[i].transform.position += new Vector3(0, dy, 0);
                if (_obstacles[i].transform.position.y < -6f)
                {
                    Destroy(_obstacles[i]);
                    _obstacles.RemoveAt(i);
                }
            }

            for (int i = _coins.Count - 1; i >= 0; i--)
            {
                if (_coins[i] == null) { _coins.RemoveAt(i); continue; }
                _coins[i].transform.position += new Vector3(0, dy, 0);
                if (_coins[i].transform.position.y < -6f)
                {
                    Destroy(_coins[i]);
                    _coins.RemoveAt(i);
                }
            }
        }

        private void SpawnRow()
        {
            int bladeLane = Random.Range(0, 3);
            int coinLane = Random.Range(0, 3);
            while (coinLane == bladeLane) coinLane = Random.Range(0, 3);

            if (_bladePrefab != null)
            {
                var blade = Instantiate(_bladePrefab, transform);
                blade.transform.position = new Vector3(GetLaneX(bladeLane), 6f, 0f);
                _obstacles.Add(blade);
            }

            if (_coinPrefab != null && Random.value > 0.3f)
            {
                var coin = Instantiate(_coinPrefab, transform);
                coin.transform.position = new Vector3(GetLaneX(coinLane), 6f, 0f);
                _coins.Add(coin);
            }
        }

        private void CheckCollisions()
        {
            if (_playerObj == null) return;
            Vector2 playerPos = _playerObj.transform.position;

            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                if (_obstacles[i] == null) continue;
                if (Vector2.Distance(playerPos, _obstacles[i].transform.position) < 0.6f)
                {
                    _isRunning = false;
                    if (_gameManager != null) _gameManager.OnGameOver();
                    return;
                }
            }

            for (int i = _coins.Count - 1; i >= 0; i--)
            {
                if (_coins[i] == null) continue;
                if (Vector2.Distance(playerPos, _coins[i].transform.position) < 0.6f)
                {
                    Destroy(_coins[i]);
                    _coins.RemoveAt(i);
                    if (_gameManager != null) _gameManager.OnCoinCollected();
                }
            }
        }

        private float GetLaneX(int lane)
        {
            return (lane - 1) * _laneWidth;
        }

        public void StartRun()
        {
            ClearAll();
            _currentLane = 1;
            _spawnTimer = _spawnInterval;
            _isRunning = true;

            if (_playerPrefab != null)
            {
                _playerObj = Instantiate(_playerPrefab, transform);
                _playerObj.transform.position = new Vector3(GetLaneX(1), -3.5f, 0f);
            }
        }

        public void StopRun()
        {
            _isRunning = false;
        }

        private void ClearAll()
        {
            foreach (var obj in _obstacles) if (obj != null) Destroy(obj);
            _obstacles.Clear();
            foreach (var obj in _coins) if (obj != null) Destroy(obj);
            _coins.Clear();
            if (_playerObj != null) { Destroy(_playerObj); _playerObj = null; }
        }
    }
}
