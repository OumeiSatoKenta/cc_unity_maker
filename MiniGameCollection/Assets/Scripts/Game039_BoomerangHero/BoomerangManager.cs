using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game039_BoomerangHero
{
    public class BoomerangManager : MonoBehaviour
    {
        [SerializeField] private GameObject _boomerangPrefab;
        [SerializeField] private GameObject _heroPrefab;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private float _boomerangSpeed = 8f;
        [SerializeField] private float _spawnInterval = 2f;
        [SerializeField] private float _gameTime = 30f;
        [SerializeField] private int _maxEnemies = 6;

        private GameObject _heroObj;
        private GameObject _boomerangObj;
        private readonly List<Vector2> _path = new List<Vector2>();
        private readonly List<GameObject> _enemies = new List<GameObject>();
        private int _pathIndex;
        private bool _isFlying;
        private bool _isDrawing;
        private float _spawnTimer;
        private float _timeRemaining;
        private bool _isRunning;

        private BoomerangHeroGameManager _gameManager;
        private Camera _mainCamera;

        private void Awake() { _gameManager = GetComponentInParent<BoomerangHeroGameManager>(); _mainCamera = Camera.main; }

        private void Update()
        {
            if (!_isRunning) return;
            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f) { _isRunning = false; if (_gameManager != null) _gameManager.OnTimeUp(); return; }
            if (_gameManager != null) _gameManager.OnTimeUpdate(_timeRemaining);

            if (!_isFlying) HandleDrawing();
            else UpdateBoomerang();

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f && _enemies.Count < _maxEnemies) { SpawnEnemy(); _spawnTimer = _spawnInterval; }
        }

        private void HandleDrawing()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            if (mouse.leftButton.wasPressedThisFrame) { _isDrawing = true; _path.Clear(); }

            if (_isDrawing && mouse.leftButton.isPressed)
            {
                Vector3 sp = mouse.position.ReadValue(); sp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(sp);
                if (_path.Count == 0 || Vector2.Distance(wp, _path[_path.Count - 1]) > 0.3f) _path.Add(wp);
            }

            if (mouse.leftButton.wasReleasedThisFrame && _isDrawing)
            {
                _isDrawing = false;
                if (_path.Count > 3) LaunchBoomerang();
            }
        }

        private void LaunchBoomerang()
        {
            if (_boomerangPrefab == null || _heroObj == null) return;
            // Add return path
            _path.Add(_heroObj.transform.position);
            _boomerangObj = Instantiate(_boomerangPrefab, transform);
            _boomerangObj.transform.position = _heroObj.transform.position;
            _pathIndex = 0;
            _isFlying = true;
        }

        private void UpdateBoomerang()
        {
            if (_boomerangObj == null || _pathIndex >= _path.Count) { EndFlight(); return; }

            Vector2 target = _path[_pathIndex];
            Vector2 current = _boomerangObj.transform.position;
            Vector2 newPos = Vector2.MoveTowards(current, target, _boomerangSpeed * Time.deltaTime);
            _boomerangObj.transform.position = newPos;
            _boomerangObj.transform.Rotate(0, 0, 600f * Time.deltaTime);

            if (Vector2.Distance(newPos, target) < 0.2f) _pathIndex++;

            // Check enemy hits
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i] == null) { _enemies.RemoveAt(i); continue; }
                if (Vector2.Distance(newPos, _enemies[i].transform.position) < 0.5f)
                {
                    Destroy(_enemies[i]); _enemies.RemoveAt(i);
                    if (_gameManager != null) _gameManager.OnEnemyHit();
                }
            }
        }

        private void EndFlight()
        {
            if (_boomerangObj != null) { Destroy(_boomerangObj); _boomerangObj = null; }
            _isFlying = false;
            _path.Clear();
        }

        private void SpawnEnemy()
        {
            if (_enemyPrefab == null) return;
            var enemy = Instantiate(_enemyPrefab, transform);
            enemy.transform.position = new Vector3(Random.Range(-5f, 5f), Random.Range(-3f, 4f), 0);
            _enemies.Add(enemy);
        }

        public void StartGame()
        {
            ClearAll(); _spawnTimer = 0.5f; _timeRemaining = _gameTime; _isFlying = false; _isRunning = true;
            if (_heroPrefab != null) { _heroObj = Instantiate(_heroPrefab, transform); _heroObj.transform.position = new Vector3(0, -3.5f, 0); }
        }

        public void StopGame() { _isRunning = false; }
        private void ClearAll() { foreach (var e in _enemies) if (e != null) Destroy(e); _enemies.Clear(); if (_heroObj != null) { Destroy(_heroObj); _heroObj = null; } if (_boomerangObj != null) { Destroy(_boomerangObj); _boomerangObj = null; } }
    }
}
