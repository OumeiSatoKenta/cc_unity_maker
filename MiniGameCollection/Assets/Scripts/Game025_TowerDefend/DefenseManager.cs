using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game025_TowerDefend
{
    public class DefenseManager : MonoBehaviour
    {
        [SerializeField] private GameObject _towerPrefab;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private int _maxTowers = 5;
        [SerializeField] private float _waveInterval = 3f;
        [SerializeField] private int _totalWaves = 5;

        private readonly List<Tower> _towers = new List<Tower>();
        private readonly List<EnemyUnit> _enemies = new List<EnemyUnit>();
        private readonly List<GameObject> _bullets = new List<GameObject>();
        private float _waveTimer;
        private int _currentWave;
        private int _towersPlaced;
        private bool _isRunning;

        private TowerDefendGameManager _gameManager;
        private Camera _mainCamera;

        private void Awake()
        {
            _gameManager = GetComponentInParent<TowerDefendGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isRunning) return;
            HandleInput();
            UpdateTowers();
            UpdateBullets();
            CheckEnemyReached();

            _waveTimer -= Time.deltaTime;
            if (_waveTimer <= 0f && _currentWave < _totalWaves)
            {
                SpawnWave();
                _waveTimer = _waveInterval;
            }

            // Check win
            if (_currentWave >= _totalWaves && CountAliveEnemies() == 0)
            {
                if (_gameManager != null) _gameManager.OnAllWavesCleared();
            }
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            if (mouse.leftButton.wasPressedThisFrame && _towersPlaced < _maxTowers)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);

                // Only place on upper/lower area (not on path)
                if (Mathf.Abs(worldPos.y) > 1f && Mathf.Abs(worldPos.x) < 6f)
                {
                    PlaceTower(worldPos);
                }
            }
        }

        private void PlaceTower(Vector2 pos)
        {
            if (_towerPrefab == null) return;
            var obj = Instantiate(_towerPrefab, transform);
            obj.transform.position = new Vector3(pos.x, pos.y, 0f);
            var tower = obj.GetComponent<Tower>();
            if (tower != null) tower.Initialize(2.5f, 1f, 1);
            _towers.Add(tower);
            _towersPlaced++;
            if (_gameManager != null) _gameManager.OnTowerPlaced(_maxTowers - _towersPlaced);
        }

        private void UpdateTowers()
        {
            foreach (var tower in _towers)
            {
                if (tower == null || !tower.CanFire()) continue;

                EnemyUnit closest = null;
                float closestDist = float.MaxValue;
                foreach (var enemy in _enemies)
                {
                    if (enemy == null || !enemy.IsAlive) continue;
                    float dist = Vector2.Distance(tower.transform.position, enemy.transform.position);
                    if (dist < tower.Range && dist < closestDist)
                    {
                        closest = enemy;
                        closestDist = dist;
                    }
                }

                if (closest != null)
                {
                    closest.TakeDamage(tower.Damage);
                    tower.ResetFireTimer();

                    if (!closest.IsAlive && _gameManager != null)
                        _gameManager.OnEnemyKilled();

                    // Visual bullet
                    if (_bulletPrefab != null)
                    {
                        var bullet = Instantiate(_bulletPrefab, transform);
                        bullet.transform.position = tower.transform.position;
                        _bullets.Add(bullet);
                    }
                }
            }
        }

        private void UpdateBullets()
        {
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                if (_bullets[i] == null) { _bullets.RemoveAt(i); continue; }
                _bullets[i].transform.position += Vector3.up * 5f * Time.deltaTime;
                if (_bullets[i].transform.position.y > 6f)
                {
                    Destroy(_bullets[i]);
                    _bullets.RemoveAt(i);
                }
            }
        }

        private void CheckEnemyReached()
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i] == null || !_enemies[i].IsAlive) continue;
                if (_enemies[i].transform.position.x > 7f)
                {
                    Destroy(_enemies[i].gameObject);
                    _enemies.RemoveAt(i);
                    if (_gameManager != null) _gameManager.OnEnemyReached();
                }
            }
        }

        private void SpawnWave()
        {
            _currentWave++;
            int count = 2 + _currentWave;
            for (int i = 0; i < count; i++)
            {
                if (_enemyPrefab == null) continue;
                var obj = Instantiate(_enemyPrefab, transform);
                float y = Random.Range(-0.5f, 0.5f);
                obj.transform.position = new Vector3(-7f - i * 1.5f, y, 0f);
                var enemy = obj.GetComponent<EnemyUnit>();
                if (enemy != null) enemy.Initialize(0.8f + _currentWave * 0.1f, 1 + _currentWave / 2);
                _enemies.Add(enemy);
            }
            if (_gameManager != null) _gameManager.OnWaveStarted(_currentWave, _totalWaves);
        }

        private int CountAliveEnemies()
        {
            int count = 0;
            foreach (var e in _enemies) if (e != null && e.IsAlive) count++;
            return count;
        }

        public void StartGame()
        {
            ClearAll();
            _currentWave = 0;
            _towersPlaced = 0;
            _waveTimer = 2f;
            _isRunning = true;
        }

        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var t in _towers) if (t != null) Destroy(t.gameObject);
            _towers.Clear();
            foreach (var e in _enemies) if (e != null) Destroy(e.gameObject);
            _enemies.Clear();
            foreach (var b in _bullets) if (b != null) Destroy(b);
            _bullets.Clear();
        }
    }
}
