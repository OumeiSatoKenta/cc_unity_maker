using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game037_ZapChain
{
    public class ZapManager : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private float _spawnInterval = 1.5f;
        [SerializeField] private float _chainRadius = 2f;
        [SerializeField] private float _gameTime = 30f;
        [SerializeField] private int _maxEnemies = 12;

        private GameObject _playerObj;
        private readonly List<GameObject> _enemies = new List<GameObject>();
        private float _spawnTimer;
        private float _timeRemaining;
        private bool _isRunning;

        private ZapChainGameManager _gameManager;
        private Camera _mainCamera;

        private void Awake() { _gameManager = GetComponentInParent<ZapChainGameManager>(); _mainCamera = Camera.main; }

        private void Update()
        {
            if (!_isRunning) return;
            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f) { _isRunning = false; if (_gameManager != null) _gameManager.OnTimeUp(); return; }
            if (_gameManager != null) _gameManager.OnTimeUpdate(_timeRemaining);
            HandleInput();
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f && _enemies.Count < _maxEnemies) { SpawnEnemy(); _spawnTimer = _spawnInterval; }
            CheckZap();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null || _playerObj == null) return;
            if (mouse.leftButton.isPressed)
            {
                Vector3 sp = mouse.position.ReadValue(); sp.z = -_mainCamera.transform.position.z;
                Vector2 target = _mainCamera.ScreenToWorldPoint(sp);
                Vector2 current = _playerObj.transform.position;
                _playerObj.transform.position = Vector2.MoveTowards(current, target, 10f * Time.deltaTime);
            }
        }

        private void CheckZap()
        {
            if (_playerObj == null) return;
            Vector2 pPos = _playerObj.transform.position;

            // Find first enemy in range
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i] == null) { _enemies.RemoveAt(i); continue; }
                if (Vector2.Distance(pPos, _enemies[i].transform.position) < 0.5f)
                {
                    // Chain reaction!
                    var zapped = new List<int> { i };
                    ChainZap(_enemies[i].transform.position, zapped);
                    int chainCount = zapped.Count;

                    // Remove zapped enemies (reverse order)
                    zapped.Sort(); zapped.Reverse();
                    foreach (int idx in zapped)
                    {
                        if (idx < _enemies.Count && _enemies[idx] != null)
                        {
                            Destroy(_enemies[idx]);
                            _enemies.RemoveAt(idx);
                        }
                    }

                    if (_gameManager != null) _gameManager.OnChainZap(chainCount);
                    break;
                }
            }
        }

        private void ChainZap(Vector2 origin, List<int> zapped)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (zapped.Contains(i) || _enemies[i] == null) continue;
                if (Vector2.Distance(origin, _enemies[i].transform.position) < _chainRadius)
                {
                    zapped.Add(i);
                    ChainZap(_enemies[i].transform.position, zapped);
                }
            }
        }

        private void SpawnEnemy()
        {
            if (_enemyPrefab == null) return;
            var enemy = Instantiate(_enemyPrefab, transform);
            enemy.transform.position = new Vector3(Random.Range(-5f, 5f), Random.Range(-4f, 4f), 0);
            _enemies.Add(enemy);
        }

        public void StartGame()
        {
            ClearAll(); _spawnTimer = 0.3f; _timeRemaining = _gameTime; _isRunning = true;
            if (_playerPrefab != null) { _playerObj = Instantiate(_playerPrefab, transform); _playerObj.transform.position = Vector3.zero; }
        }

        public void StopGame() { _isRunning = false; }
        private void ClearAll() { foreach (var e in _enemies) if (e != null) Destroy(e); _enemies.Clear(); if (_playerObj != null) { Destroy(_playerObj); _playerObj = null; } }
    }
}
