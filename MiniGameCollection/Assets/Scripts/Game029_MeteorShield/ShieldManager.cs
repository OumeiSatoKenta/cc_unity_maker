using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game029_MeteorShield
{
    public class ShieldManager : MonoBehaviour
    {
        [SerializeField] private GameObject _meteorPrefab;
        [SerializeField] private GameObject _shieldPrefab;
        [SerializeField] private float _spawnInterval = 0.8f;
        [SerializeField] private float _meteorSpeed = 4f;

        private GameObject _shieldObj;
        private readonly List<GameObject> _meteors = new List<GameObject>();
        private float _spawnTimer;
        private float _elapsed;
        private bool _isRunning;

        private MeteorShieldGameManager _gameManager;
        private Camera _mainCamera;

        private void Awake()
        {
            _gameManager = GetComponentInParent<MeteorShieldGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isRunning) return;
            HandleInput();
            _elapsed += Time.deltaTime;
            SpawnMeteors();
            MoveMeteors();
            CheckCollisions();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null || _shieldObj == null) return;

            if (mouse.leftButton.isPressed)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);
                var pos = _shieldObj.transform.position;
                pos.x = Mathf.Clamp(worldPos.x, -5f, 5f);
                _shieldObj.transform.position = pos;
            }
        }

        private void SpawnMeteors()
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f) return;
            float speedup = Mathf.Min(_elapsed * 0.003f, 0.4f);
            _spawnTimer = Mathf.Max(_spawnInterval - speedup, 0.2f);

            if (_meteorPrefab == null) return;
            var meteor = Instantiate(_meteorPrefab, transform);
            float x = Random.Range(-5f, 5f);
            float scale = Random.Range(0.8f, 1.5f);
            meteor.transform.position = new Vector3(x, 6f, 0f);
            meteor.transform.localScale = Vector3.one * scale;
            _meteors.Add(meteor);
        }

        private void MoveMeteors()
        {
            float speed = _meteorSpeed + _elapsed * 0.05f;
            for (int i = _meteors.Count - 1; i >= 0; i--)
            {
                if (_meteors[i] == null) { _meteors.RemoveAt(i); continue; }
                _meteors[i].transform.position += Vector3.down * speed * Time.deltaTime;
            }
        }

        private void CheckCollisions()
        {
            if (_shieldObj == null) return;
            float shieldX = _shieldObj.transform.position.x;
            float shieldY = _shieldObj.transform.position.y;

            for (int i = _meteors.Count - 1; i >= 0; i--)
            {
                if (_meteors[i] == null) continue;
                var mPos = _meteors[i].transform.position;

                // Shield bounce
                if (mPos.y < shieldY + 0.3f && mPos.y > shieldY - 0.3f &&
                    Mathf.Abs(mPos.x - shieldX) < 1.2f)
                {
                    Destroy(_meteors[i]);
                    _meteors.RemoveAt(i);
                    if (_gameManager != null) _gameManager.OnMeteorDeflected();
                    continue;
                }

                // Hit star (center bottom)
                if (mPos.y < -3.8f && Mathf.Abs(mPos.x) < 1f)
                {
                    Destroy(_meteors[i]);
                    _meteors.RemoveAt(i);
                    if (_gameManager != null) _gameManager.OnStarHit();
                    continue;
                }

                // Off screen
                if (mPos.y < -6f)
                {
                    Destroy(_meteors[i]);
                    _meteors.RemoveAt(i);
                }
            }
        }

        public void StartGame()
        {
            ClearAll();
            _spawnTimer = 1f; _elapsed = 0f; _isRunning = true;
            if (_shieldPrefab != null)
            {
                _shieldObj = Instantiate(_shieldPrefab, transform);
                _shieldObj.transform.position = new Vector3(0, -2f, 0);
            }
        }

        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var m in _meteors) if (m != null) Destroy(m);
            _meteors.Clear();
            if (_shieldObj != null) { Destroy(_shieldObj); _shieldObj = null; }
        }
    }
}
