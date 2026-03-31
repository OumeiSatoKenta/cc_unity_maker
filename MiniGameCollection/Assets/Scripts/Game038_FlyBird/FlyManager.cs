using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game038_FlyBird
{
    public class FlyManager : MonoBehaviour
    {
        [SerializeField] private GameObject _birdPrefab;
        [SerializeField] private GameObject _pipePrefab;
        [SerializeField] private float _gravity = 10f;
        [SerializeField] private float _flapForce = 5f;
        [SerializeField] private float _scrollSpeed = 3f;
        [SerializeField] private float _pipeSpawnInterval = 2f;
        [SerializeField] private float _gapSize = 3f;

        private GameObject _birdObj;
        private float _birdY, _birdVel;
        private readonly List<GameObject> _pipes = new List<GameObject>();
        private float _spawnTimer;
        private bool _isRunning;

        private FlyBirdGameManager _gameManager;

        private void Awake() { _gameManager = GetComponentInParent<FlyBirdGameManager>(); }

        private void Update()
        {
            if (!_isRunning) return;
            HandleInput();
            UpdateBird();
            ScrollPipes();
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f) { SpawnPipe(); _spawnTimer = _pipeSpawnInterval; }
            CheckCollisions();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame)
                _birdVel = _flapForce;
        }

        private void UpdateBird()
        {
            _birdVel -= _gravity * Time.deltaTime;
            _birdY += _birdVel * Time.deltaTime;
            if (_birdObj != null)
            {
                _birdObj.transform.position = new Vector3(-2f, _birdY, 0);
                float angle = Mathf.Clamp(_birdVel * 5f, -60f, 30f);
                _birdObj.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            if (_birdY < -5.5f || _birdY > 5.5f)
            {
                _isRunning = false;
                if (_gameManager != null) _gameManager.OnGameOver();
            }
        }

        private void ScrollPipes()
        {
            for (int i = _pipes.Count - 1; i >= 0; i--)
            {
                if (_pipes[i] == null) { _pipes.RemoveAt(i); continue; }
                _pipes[i].transform.position += Vector3.left * _scrollSpeed * Time.deltaTime;
                // Score when pipe passes bird
                if (_pipes[i].transform.position.x < -2.1f && _pipes[i].transform.position.x > -2.1f - _scrollSpeed * Time.deltaTime)
                {
                    if (_gameManager != null) _gameManager.OnPipePassed();
                }
                if (_pipes[i].transform.position.x < -8f) { Destroy(_pipes[i]); _pipes.RemoveAt(i); }
            }
        }

        private void SpawnPipe()
        {
            if (_pipePrefab == null) return;
            float gapCenter = Random.Range(-2f, 2f);

            // Top pipe
            var top = Instantiate(_pipePrefab, transform);
            float topY = gapCenter + _gapSize * 0.5f + 3.2f;
            top.transform.position = new Vector3(7f, topY, 0);
            top.transform.rotation = Quaternion.Euler(0, 0, 180);
            _pipes.Add(top);

            // Bottom pipe
            var bot = Instantiate(_pipePrefab, transform);
            float botY = gapCenter - _gapSize * 0.5f - 3.2f;
            bot.transform.position = new Vector3(7f, botY, 0);
            _pipes.Add(bot);
        }

        private void CheckCollisions()
        {
            if (_birdObj == null) return;
            Vector2 birdPos = _birdObj.transform.position;

            foreach (var pipe in _pipes)
            {
                if (pipe == null) continue;
                Vector2 pipePos = pipe.transform.position;
                // Simple AABB
                if (Mathf.Abs(birdPos.x - pipePos.x) < 0.5f &&
                    Mathf.Abs(birdPos.y - pipePos.y) < 3.5f)
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
            _birdY = 0; _birdVel = 0; _spawnTimer = 1.5f; _isRunning = true;
            if (_birdPrefab != null) { _birdObj = Instantiate(_birdPrefab, transform); _birdObj.transform.position = new Vector3(-2f, 0, 0); }
        }

        public void StopGame() { _isRunning = false; }
        private void ClearAll() { foreach (var p in _pipes) if (p != null) Destroy(p); _pipes.Clear(); if (_birdObj != null) { Destroy(_birdObj); _birdObj = null; } }
    }
}
