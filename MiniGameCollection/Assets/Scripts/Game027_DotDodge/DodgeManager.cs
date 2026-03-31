using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game027_DotDodge
{
    public class DodgeManager : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _dotPrefab;
        [SerializeField] private float _spawnInterval = 0.3f;
        [SerializeField] private float _dotSpeed = 3f;

        private GameObject _playerObj;
        private readonly List<GameObject> _dots = new List<GameObject>();
        private float _spawnTimer;
        private float _elapsed;
        private bool _isRunning;

        private DotDodgeGameManager _gameManager;
        private Camera _mainCamera;

        private void Awake()
        {
            _gameManager = GetComponentInParent<DotDodgeGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isRunning) return;
            HandleInput();
            _elapsed += Time.deltaTime;
            SpawnDots();
            MoveDots();
            CheckCollision();
            if (_gameManager != null) _gameManager.OnSurvived(_elapsed);
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
                Vector2 newPos = Vector2.MoveTowards(current, target, 12f * Time.deltaTime);
                newPos.x = Mathf.Clamp(newPos.x, -5.5f, 5.5f);
                newPos.y = Mathf.Clamp(newPos.y, -4.5f, 4.5f);
                _playerObj.transform.position = newPos;
            }
        }

        private void SpawnDots()
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f) return;

            float speedup = Mathf.Min(_elapsed * 0.005f, 0.2f);
            _spawnTimer = Mathf.Max(_spawnInterval - speedup, 0.1f);

            if (_dotPrefab == null) return;
            var dot = Instantiate(_dotPrefab, transform);

            int side = Random.Range(0, 4);
            Vector2 pos, vel;
            float speed = _dotSpeed + _elapsed * 0.05f;

            switch (side)
            {
                case 0: pos = new Vector2(Random.Range(-6f, 6f), 6f); vel = new Vector2(Random.Range(-1f, 1f), -speed); break;
                case 1: pos = new Vector2(Random.Range(-6f, 6f), -6f); vel = new Vector2(Random.Range(-1f, 1f), speed); break;
                case 2: pos = new Vector2(-7f, Random.Range(-5f, 5f)); vel = new Vector2(speed, Random.Range(-1f, 1f)); break;
                default: pos = new Vector2(7f, Random.Range(-5f, 5f)); vel = new Vector2(-speed, Random.Range(-1f, 1f)); break;
            }

            dot.transform.position = pos;
            var rb = dot.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.linearVelocity = vel;
            _dots.Add(dot);
        }

        private void MoveDots()
        {
            for (int i = _dots.Count - 1; i >= 0; i--)
            {
                if (_dots[i] == null) { _dots.RemoveAt(i); continue; }
                if (Mathf.Abs(_dots[i].transform.position.x) > 8f || Mathf.Abs(_dots[i].transform.position.y) > 7f)
                {
                    Destroy(_dots[i]); _dots.RemoveAt(i);
                }
            }
        }

        private void CheckCollision()
        {
            if (_playerObj == null) return;
            Vector2 ppos = _playerObj.transform.position;
            foreach (var dot in _dots)
            {
                if (dot == null) continue;
                if (Vector2.Distance(ppos, dot.transform.position) < 0.35f)
                {
                    _isRunning = false;
                    if (_gameManager != null) _gameManager.OnGameOver();
                    return;
                }
            }
        }

        public void StartGame()
        {
            ClearAll();
            _elapsed = 0f; _spawnTimer = 1f; _isRunning = true;
            if (_playerPrefab != null)
            {
                _playerObj = Instantiate(_playerPrefab, transform);
                _playerObj.transform.position = Vector3.zero;
            }
        }

        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var d in _dots) if (d != null) Destroy(d);
            _dots.Clear();
            if (_playerObj != null) { Destroy(_playerObj); _playerObj = null; }
        }
    }
}
