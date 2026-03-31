using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game028_RopeSwing
{
    public class SwingManager : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _platformPrefab;
        [SerializeField] private GameObject _anchorPrefab;
        [SerializeField] private float _gravity = 15f;
        [SerializeField] private float _ropeLength = 3f;

        private GameObject _playerObj;
        private Vector2 _playerPos;
        private Vector2 _playerVel;
        private bool _isSwinging;
        private Vector2 _anchorPos;
        private float _swingAngle;
        private float _swingAngularVel;
        private float _scrollX;
        private float _bestX;

        private readonly List<Vector2> _platforms = new List<Vector2>();
        private readonly List<Vector2> _anchors = new List<Vector2>();
        private readonly List<GameObject> _stageObjects = new List<GameObject>();
        private bool _isRunning;

        private RopeSwingGameManager _gameManager;
        private Camera _mainCamera;

        private void Awake()
        {
            _gameManager = GetComponentInParent<RopeSwingGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isRunning) return;
            HandleInput();

            if (_isSwinging)
                UpdateSwing();
            else
                UpdateFreefall();

            // Scroll camera
            if (_playerObj != null && _mainCamera != null)
            {
                float targetX = _playerPos.x;
                var camPos = _mainCamera.transform.position;
                camPos.x = Mathf.Lerp(camPos.x, targetX, 5f * Time.deltaTime);
                _mainCamera.transform.position = camPos;
            }

            if (_playerPos.x > _bestX) _bestX = _playerPos.x;

            // Fall off screen
            if (_playerPos.y < -8f)
            {
                _isRunning = false;
                if (_gameManager != null) _gameManager.OnGameOver();
            }

            // Generate platforms ahead
            GenerateAhead();

            if (_gameManager != null) _gameManager.OnDistanceUpdate(_bestX);
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (!_isSwinging)
                {
                    // Try to grab nearest anchor
                    Vector2 nearest = Vector2.zero;
                    float nearestDist = float.MaxValue;
                    foreach (var a in _anchors)
                    {
                        float dist = Vector2.Distance(_playerPos, a);
                        if (dist < _ropeLength * 1.5f && dist < nearestDist)
                        {
                            nearest = a;
                            nearestDist = dist;
                        }
                    }
                    if (nearestDist < _ropeLength * 1.5f)
                    {
                        _anchorPos = nearest;
                        _isSwinging = true;
                        Vector2 diff = _playerPos - _anchorPos;
                        _swingAngle = Mathf.Atan2(diff.x, -diff.y);
                        _swingAngularVel = _playerVel.x * 0.3f;
                    }
                }
                else
                {
                    // Release
                    _isSwinging = false;
                    _playerVel = new Vector2(
                        _swingAngularVel * _ropeLength * Mathf.Cos(_swingAngle),
                        _swingAngularVel * _ropeLength * Mathf.Sin(_swingAngle) * 0.5f + 3f
                    );
                }
            }
        }

        private void UpdateSwing()
        {
            float gAccel = -_gravity / _ropeLength * Mathf.Sin(_swingAngle);
            _swingAngularVel += gAccel * Time.deltaTime;
            _swingAngularVel *= 0.998f; // damping
            _swingAngle += _swingAngularVel * Time.deltaTime;

            _playerPos = _anchorPos + new Vector2(
                Mathf.Sin(_swingAngle) * _ropeLength,
                -Mathf.Cos(_swingAngle) * _ropeLength
            );

            if (_playerObj != null)
                _playerObj.transform.position = _playerPos;
        }

        private void UpdateFreefall()
        {
            _playerVel.y -= _gravity * Time.deltaTime;
            _playerPos += _playerVel * Time.deltaTime;

            // Platform collision
            foreach (var plat in _platforms)
            {
                if (_playerVel.y < 0 &&
                    _playerPos.x > plat.x - 1.5f && _playerPos.x < plat.x + 1.5f &&
                    _playerPos.y < plat.y + 0.3f && _playerPos.y > plat.y - 0.2f)
                {
                    _playerPos.y = plat.y + 0.3f;
                    _playerVel.y = 0;
                    _playerVel.x *= 0.9f;
                }
            }

            if (_playerObj != null)
                _playerObj.transform.position = _playerPos;
        }

        private void GenerateAhead()
        {
            while (_scrollX < _playerPos.x + 20f)
            {
                _scrollX += Random.Range(3f, 5f);
                float py = Random.Range(-2f, 1f);
                _platforms.Add(new Vector2(_scrollX, py));

                if (_platformPrefab != null)
                {
                    var obj = Instantiate(_platformPrefab, transform);
                    obj.transform.position = new Vector3(_scrollX, py, 0);
                    _stageObjects.Add(obj);
                }

                // Anchor above platform
                float ay = py + Random.Range(3f, 5f);
                _anchors.Add(new Vector2(_scrollX + Random.Range(-1f, 1f), ay));

                if (_anchorPrefab != null)
                {
                    var obj = Instantiate(_anchorPrefab, transform);
                    obj.transform.position = new Vector3(_scrollX + Random.Range(-1f, 1f), ay, 0);
                    _stageObjects.Add(obj);
                }
            }
        }

        public void StartGame()
        {
            ClearAll();
            _playerPos = new Vector2(0, 0);
            _playerVel = Vector2.zero;
            _isSwinging = false;
            _scrollX = -5f;
            _bestX = 0;
            _isRunning = true;

            // Initial platform
            _platforms.Add(new Vector2(0, -1));
            if (_platformPrefab != null)
            {
                var obj = Instantiate(_platformPrefab, transform);
                obj.transform.position = new Vector3(0, -1, 0);
                _stageObjects.Add(obj);
            }

            if (_playerPrefab != null)
            {
                _playerObj = Instantiate(_playerPrefab, transform);
                _playerObj.transform.position = _playerPos;
            }

            if (_mainCamera != null)
                _mainCamera.transform.position = new Vector3(0, 0, _mainCamera.transform.position.z);

            GenerateAhead();
        }

        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var o in _stageObjects) if (o != null) Destroy(o);
            _stageObjects.Clear();
            _platforms.Clear();
            _anchors.Clear();
            if (_playerObj != null) { Destroy(_playerObj); _playerObj = null; }
        }
    }
}
