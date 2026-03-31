using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game032_SpinCutter
{
    public class SpinManager : MonoBehaviour
    {
        [SerializeField] private GameObject _bladePrefab;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private float _bladeOrbitRadius = 2f;
        [SerializeField] private float _bladeSpeed = 300f;
        [SerializeField] private float _spawnInterval = 2f;
        [SerializeField] private float _gameTime = 30f;

        private GameObject _playerObj;
        private GameObject _bladeObj;
        private float _bladeAngle;
        private readonly List<GameObject> _enemies = new List<GameObject>();
        private float _spawnTimer;
        private float _timeRemaining;
        private bool _isRunning;

        private SpinCutterGameManager _gameManager;
        private Camera _mainCamera;

        private void Awake()
        {
            _gameManager = GetComponentInParent<SpinCutterGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isRunning) return;

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f) { _isRunning = false; if (_gameManager != null) _gameManager.OnTimeUp(); return; }
            if (_gameManager != null) _gameManager.OnTimeUpdate(_timeRemaining);

            HandleInput();
            UpdateBlade();
            SpawnEnemies();
            MoveEnemies();
            CheckCollisions();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null || _playerObj == null) return;

            if (mouse.leftButton.isPressed)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                Vector2 target = _mainCamera.ScreenToWorldPoint(sp);
                Vector2 current = _playerObj.transform.position;
                Vector2 newPos = Vector2.MoveTowards(current, target, 8f * Time.deltaTime);
                newPos.x = Mathf.Clamp(newPos.x, -5f, 5f);
                newPos.y = Mathf.Clamp(newPos.y, -4f, 4f);
                _playerObj.transform.position = newPos;
            }
        }

        private void UpdateBlade()
        {
            if (_bladeObj == null || _playerObj == null) return;
            _bladeAngle += _bladeSpeed * Time.deltaTime;
            float rad = _bladeAngle * Mathf.Deg2Rad;
            Vector2 playerPos = _playerObj.transform.position;
            _bladeObj.transform.position = playerPos + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * _bladeOrbitRadius;
            _bladeObj.transform.Rotate(0, 0, _bladeSpeed * 2f * Time.deltaTime);
        }

        private void SpawnEnemies()
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f) return;
            _spawnTimer = Mathf.Max(_spawnInterval - _timeRemaining * 0.01f, 0.5f);

            if (_enemyPrefab == null) return;
            var enemy = Instantiate(_enemyPrefab, transform);
            float side = Random.value;
            Vector2 pos;
            if (side < 0.25f) pos = new Vector2(Random.Range(-5f, 5f), 5.5f);
            else if (side < 0.5f) pos = new Vector2(Random.Range(-5f, 5f), -5.5f);
            else if (side < 0.75f) pos = new Vector2(-6.5f, Random.Range(-4f, 4f));
            else pos = new Vector2(6.5f, Random.Range(-4f, 4f));
            enemy.transform.position = pos;
            _enemies.Add(enemy);
        }

        private void MoveEnemies()
        {
            if (_playerObj == null) return;
            Vector2 playerPos = _playerObj.transform.position;
            float speed = 1.5f + (30f - _timeRemaining) * 0.03f;

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i] == null) { _enemies.RemoveAt(i); continue; }
                Vector2 dir = (playerPos - (Vector2)_enemies[i].transform.position).normalized;
                _enemies[i].transform.position += (Vector3)(dir * speed * Time.deltaTime);
            }
        }

        private void CheckCollisions()
        {
            if (_bladeObj == null || _playerObj == null) return;
            Vector2 bladePos = _bladeObj.transform.position;
            Vector2 playerPos = _playerObj.transform.position;

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i] == null) continue;
                Vector2 ePos = _enemies[i].transform.position;

                // Blade kills enemy
                if (Vector2.Distance(bladePos, ePos) < 0.6f)
                {
                    Destroy(_enemies[i]); _enemies.RemoveAt(i);
                    if (_gameManager != null) _gameManager.OnEnemyKilled();
                    continue;
                }

                // Enemy reaches player
                if (Vector2.Distance(playerPos, ePos) < 0.4f)
                {
                    _isRunning = false;
                    if (_gameManager != null) _gameManager.OnPlayerHit();
                    return;
                }
            }
        }

        public void StartGame()
        {
            ClearAll();
            _bladeAngle = 0f; _spawnTimer = 1f; _timeRemaining = _gameTime; _isRunning = true;

            if (_playerPrefab != null)
            {
                _playerObj = Instantiate(_playerPrefab, transform);
                _playerObj.transform.position = Vector3.zero;
            }
            if (_bladePrefab != null)
            {
                _bladeObj = Instantiate(_bladePrefab, transform);
                _bladeObj.transform.position = new Vector3(_bladeOrbitRadius, 0, 0);
            }
        }

        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var e in _enemies) if (e != null) Destroy(e);
            _enemies.Clear();
            if (_playerObj != null) { Destroy(_playerObj); _playerObj = null; }
            if (_bladeObj != null) { Destroy(_bladeObj); _bladeObj = null; }
        }
    }
}
