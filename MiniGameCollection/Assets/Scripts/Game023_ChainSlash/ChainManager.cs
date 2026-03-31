using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game023_ChainSlash
{
    public class ChainManager : MonoBehaviour
    {
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private float _spawnInterval = 1.5f;
        [SerializeField] private int _maxEnemies = 8;
        [SerializeField] private float _gameTime = 30f;

        private readonly List<Enemy> _enemies = new List<Enemy>();
        private readonly List<Enemy> _chainedEnemies = new List<Enemy>();
        private readonly List<GameObject> _chainLinks = new List<GameObject>();
        private float _spawnTimer;
        private float _timeRemaining;
        private bool _isRunning;

        private ChainSlashGameManager _gameManager;
        private Camera _mainCamera;

        private Sprite _enemySprite;
        private Sprite _chainedSprite;

        private void Awake()
        {
            _gameManager = GetComponentInParent<ChainSlashGameManager>();
            _mainCamera = Camera.main;
            _enemySprite = Resources.Load<Sprite>("Sprites/Game023_ChainSlash/enemy");
            _chainedSprite = Resources.Load<Sprite>("Sprites/Game023_ChainSlash/enemy_chained");
        }

        private void Update()
        {
            if (!_isRunning) return;

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f)
            {
                _isRunning = false;
                if (_gameManager != null) _gameManager.OnTimeUp();
                return;
            }

            if (_gameManager != null) _gameManager.OnTimeUpdate(_timeRemaining);

            HandleInput();

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f && CountAliveEnemies() < _maxEnemies)
            {
                SpawnEnemy();
                _spawnTimer = _spawnInterval;
            }
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var enemy = hit.GetComponent<Enemy>();
                    if (enemy != null && enemy.IsAlive && !enemy.IsChained)
                    {
                        enemy.Chain(_chainedEnemies.Count, _chainedSprite);
                        _chainedEnemies.Add(enemy);

                        // Auto-slash at 3+ chain
                        if (_chainedEnemies.Count >= 3)
                            ExecuteSlash();
                    }
                }
            }
        }

        private void ExecuteSlash()
        {
            int combo = _chainedEnemies.Count;
            foreach (var enemy in _chainedEnemies)
                enemy.Slash();

            if (_gameManager != null) _gameManager.OnComboSlash(combo);
            _chainedEnemies.Clear();
        }

        private void SpawnEnemy()
        {
            if (_enemyPrefab == null) return;
            var obj = Instantiate(_enemyPrefab, transform);
            float x = Random.Range(-4f, 4f);
            float y = Random.Range(-3f, 3f);
            obj.transform.position = new Vector3(x, y, 0f);

            var enemy = obj.GetComponent<Enemy>();
            if (enemy != null) enemy.Initialize();
            _enemies.Add(enemy);
        }

        private int CountAliveEnemies()
        {
            int count = 0;
            foreach (var e in _enemies)
                if (e != null && e.IsAlive) count++;
            return count;
        }

        public void StartGame()
        {
            ClearAll();
            _timeRemaining = _gameTime;
            _spawnTimer = 0.5f;
            _isRunning = true;
        }

        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var e in _enemies)
                if (e != null) Destroy(e.gameObject);
            _enemies.Clear();
            _chainedEnemies.Clear();
        }
    }
}
