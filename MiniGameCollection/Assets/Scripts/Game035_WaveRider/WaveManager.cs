using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game035_WaveRider
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private GameObject _surferPrefab;
        [SerializeField] private GameObject _rockPrefab;
        [SerializeField] private float _scrollSpeed = 4f;
        [SerializeField] private float _spawnInterval = 1.5f;

        private GameObject _surferObj;
        private readonly List<GameObject> _rocks = new List<GameObject>();
        private float _surferY;
        private float _spawnTimer;
        private float _elapsed;
        private bool _isRunning;

        private WaveRiderGameManager _gameManager;

        private void Awake() { _gameManager = GetComponentInParent<WaveRiderGameManager>(); }

        private void Update()
        {
            if (!_isRunning) return;
            _elapsed += Time.deltaTime;
            HandleInput();
            ScrollRocks();
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f) { SpawnRock(); _spawnTimer = Mathf.Max(_spawnInterval - _elapsed * 0.01f, 0.5f); }
            CheckCollisions();
            if (_gameManager != null) _gameManager.OnDistanceUpdate(_elapsed * _scrollSpeed);
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _surferObj == null) return;
            if (mouse.leftButton.isPressed)
            {
                _surferY += 5f * Time.deltaTime;
                // Trick bonus when at top
                if (_surferY > 3.5f && _gameManager != null) _gameManager.OnTrick();
            }
            else _surferY -= 3f * Time.deltaTime;
            _surferY = Mathf.Clamp(_surferY, -3.5f, 4f);
            // Wave motion
            float waveOffset = Mathf.Sin(_elapsed * 2f) * 0.3f;
            _surferObj.transform.position = new Vector3(-3f, _surferY + waveOffset, 0);
        }

        private void ScrollRocks()
        {
            float speed = _scrollSpeed + _elapsed * 0.05f;
            for (int i = _rocks.Count - 1; i >= 0; i--)
            {
                if (_rocks[i] == null) { _rocks.RemoveAt(i); continue; }
                _rocks[i].transform.position += Vector3.left * speed * Time.deltaTime;
                if (_rocks[i].transform.position.x < -8f) { Destroy(_rocks[i]); _rocks.RemoveAt(i); }
            }
        }

        private void SpawnRock()
        {
            if (_rockPrefab == null) return;
            var rock = Instantiate(_rockPrefab, transform);
            rock.transform.position = new Vector3(8f, Random.Range(-3f, 3f), 0);
            float scale = Random.Range(0.8f, 1.5f);
            rock.transform.localScale = Vector3.one * scale;
            _rocks.Add(rock);
        }

        private void CheckCollisions()
        {
            if (_surferObj == null) return;
            Vector2 sp = _surferObj.transform.position;
            foreach (var rock in _rocks)
            {
                if (rock == null) continue;
                if (Vector2.Distance(sp, rock.transform.position) < 0.6f * rock.transform.localScale.x)
                {
                    _isRunning = false;
                    if (_gameManager != null) _gameManager.OnCrash();
                    return;
                }
            }
        }

        public void StartGame()
        {
            ClearAll(); _surferY = 0; _spawnTimer = 1f; _elapsed = 0; _isRunning = true;
            if (_surferPrefab != null) { _surferObj = Instantiate(_surferPrefab, transform); _surferObj.transform.position = new Vector3(-3f, 0, 0); }
        }

        public void StopGame() { _isRunning = false; }
        private void ClearAll() { foreach (var r in _rocks) if (r != null) Destroy(r); _rocks.Clear(); if (_surferObj != null) { Destroy(_surferObj); _surferObj = null; } }
    }
}
