using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game029_MeteorShield
{
    public class ShieldManager : MonoBehaviour
    {
        [SerializeField, Tooltip("シールドTransform")]
        private Transform _shield;

        [SerializeField, Tooltip("星Transform")]
        private Transform _star;

        [SerializeField, Tooltip("隕石プレハブ")]
        private GameObject _meteorPrefab;

        [SerializeField, Tooltip("初期スポーン間隔(秒)")]
        private float _initialSpawnInterval = 1.2f;

        [SerializeField, Tooltip("最小スポーン間隔(秒)")]
        private float _minSpawnInterval = 0.4f;

        [SerializeField, Tooltip("初期落下速度")]
        private float _initialFallSpeed = 3f;

        [SerializeField, Tooltip("最大落下速度")]
        private float _maxFallSpeed = 6f;

        [SerializeField, Tooltip("シールド衝突幅")]
        private float _shieldHalfWidth = 1.0f;

        [SerializeField, Tooltip("シールドY座標")]
        private float _shieldY = -3.5f;

        [SerializeField, Tooltip("星の衝突半径")]
        private float _starRadius = 0.5f;

        private MeteorShieldGameManager _gameManager;
        private Camera _mainCamera;
        private List<GameObject> _meteors = new List<GameObject>();
        private float _spawnTimer;
        private float _elapsed;
        private bool _isRunning;
        private float _xMin = -4f;
        private float _xMax = 4f;

        private void Awake()
        {
            _gameManager = GetComponentInParent<MeteorShieldGameManager>();
            _mainCamera = Camera.main;
        }

        public void StartGame()
        {
            _elapsed = 0f;
            _spawnTimer = 0f;
            _isRunning = true;

            foreach (var m in _meteors)
            {
                if (m != null) Destroy(m);
            }
            _meteors.Clear();

            if (_shield != null)
                _shield.position = new Vector3(0f, _shieldY, 0f);
        }

        public void StopGame()
        {
            _isRunning = false;
        }

        private void Update()
        {
            if (!_isRunning) return;

            _elapsed += Time.deltaTime;
            HandleInput();
            SpawnMeteors();
            UpdateMeteors();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _shield == null) return;

            if (mouse.leftButton.isPressed)
            {
                Vector3 screenPos = mouse.position.ReadValue();
                screenPos.z = -_mainCamera.transform.position.z;
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
                float clampedX = Mathf.Clamp(worldPos.x, _xMin, _xMax);
                _shield.position = new Vector3(clampedX, _shieldY, 0f);
            }
        }

        private void SpawnMeteors()
        {
            float clearTime = _gameManager != null ? _gameManager.ClearTime : 60f;
            float t = Mathf.Clamp01(_elapsed / clearTime);
            float interval = Mathf.Lerp(_initialSpawnInterval, _minSpawnInterval, t);

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= interval)
            {
                _spawnTimer = 0f;
                SpawnOneMeteor();
            }
        }

        private void SpawnOneMeteor()
        {
            if (_meteorPrefab == null) return;

            float x = Random.Range(_xMin, _xMax);
            float y = 6f;
            var meteor = Instantiate(_meteorPrefab, new Vector3(x, y, 0f), Quaternion.identity);
            _meteors.Add(meteor);
        }

        private void UpdateMeteors()
        {
            float clearTime = _gameManager != null ? _gameManager.ClearTime : 60f;
            float t = Mathf.Clamp01(_elapsed / clearTime);
            float speed = Mathf.Lerp(_initialFallSpeed, _maxFallSpeed, t);

            for (int i = _meteors.Count - 1; i >= 0; i--)
            {
                var m = _meteors[i];
                if (m == null)
                {
                    _meteors.RemoveAt(i);
                    continue;
                }

                Vector3 pos = m.transform.position;
                pos.y -= speed * Time.deltaTime;
                m.transform.position = pos;

                // シールドとの衝突判定
                if (_shield != null &&
                    Mathf.Abs(pos.y - _shieldY) < 0.4f &&
                    Mathf.Abs(pos.x - _shield.position.x) < _shieldHalfWidth)
                {
                    Destroy(m);
                    _meteors.RemoveAt(i);
                    continue;
                }

                // 星との衝突判定
                if (_star != null)
                {
                    float dist = Vector2.Distance(
                        new Vector2(pos.x, pos.y),
                        new Vector2(_star.position.x, _star.position.y));
                    if (dist < _starRadius)
                    {
                        Destroy(m);
                        _meteors.RemoveAt(i);
                        if (_gameManager != null)
                            _gameManager.OnMeteorHitStar();
                        continue;
                    }
                }

                // 画面外で破棄
                if (pos.y < -6f)
                {
                    Destroy(m);
                    _meteors.RemoveAt(i);
                }
            }
        }
    }
}
