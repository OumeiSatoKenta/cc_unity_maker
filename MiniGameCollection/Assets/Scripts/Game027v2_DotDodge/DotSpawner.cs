using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game027v2_DotDodge
{
    public enum DotType { Normal, Chaser, Expander }

    public class DotSpawner : MonoBehaviour
    {
        [SerializeField] DotDodgeGameManager _gameManager;
        [SerializeField] PlayerController _player;
        [SerializeField] Sprite _spriteNormal;
        [SerializeField] Sprite _spriteChaser;
        [SerializeField] Sprite _spriteExpander;
        [SerializeField] Sprite _spriteSafeZone;

        bool _isActive = false;
        int _currentStage = 1;
        float _baseSpeed = 2.5f;
        int _maxDots = 8;
        float _complexityFactor = 0f;
        Coroutine _spawnCoroutine;
        Coroutine _safeZoneCoroutine;
        Coroutine _screenShakeCoroutine;
        List<GameObject> _activeDots = new List<GameObject>();
        List<GameObject> _activeSafeZones = new List<GameObject>();

        Camera _mainCamera;

        void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void SetupStage(StageManager.StageConfig config, int stageNum)
        {
            StopSpawning();
            CleanupDots();

            _currentStage = stageNum;
            _baseSpeed = 2.5f * config.speedMultiplier;
            _maxDots = 8 * config.countMultiplier;
            _complexityFactor = config.complexityFactor;
            _isActive = true;

            _spawnCoroutine = StartCoroutine(SpawnLoop());

            // Stage 4+: SafeZone
            if (_currentStage >= 4)
                _safeZoneCoroutine = StartCoroutine(SafeZoneLoop());

            // Stage 5: Screen shake
            if (_currentStage >= 5)
                _screenShakeCoroutine = StartCoroutine(ScreenShakeLoop());
        }

        public void StopSpawning()
        {
            _isActive = false;
            if (_spawnCoroutine != null) { StopCoroutine(_spawnCoroutine); _spawnCoroutine = null; }
            if (_safeZoneCoroutine != null) { StopCoroutine(_safeZoneCoroutine); _safeZoneCoroutine = null; }
            if (_screenShakeCoroutine != null)
            {
                StopCoroutine(_screenShakeCoroutine);
                _screenShakeCoroutine = null;
                if (_mainCamera != null)
                {
                    float camZ = _mainCamera.transform.position.z;
                    _mainCamera.transform.position = new Vector3(0f, 0f, camZ);
                }
            }
        }

        void CleanupDots()
        {
            foreach (var d in _activeDots)
                if (d != null) Destroy(d);
            _activeDots.Clear();
            foreach (var s in _activeSafeZones)
                if (s != null) Destroy(s);
            _activeSafeZones.Clear();
        }

        IEnumerator SpawnLoop()
        {
            while (_isActive)
            {
                // Remove null entries
                _activeDots.RemoveAll(d => d == null);

                if (_activeDots.Count < _maxDots)
                    SpawnDot();

                float interval = Mathf.Max(0.3f, 1.5f / _baseSpeed);
                yield return new WaitForSeconds(interval);
            }
        }

        void SpawnDot()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            float camSize = _mainCamera.orthographicSize;
            float camWidth = camSize * _mainCamera.aspect;

            // Random edge spawn
            int edge = Random.Range(0, 4);
            Vector2 spawnPos;
            Vector2 dir;
            float margin = 0.5f;
            switch (edge)
            {
                case 0: // top
                    spawnPos = new Vector2(Random.Range(-camWidth, camWidth), camSize + margin);
                    dir = new Vector2(Random.Range(-0.5f, 0.5f), -1f).normalized;
                    break;
                case 1: // bottom
                    spawnPos = new Vector2(Random.Range(-camWidth, camWidth), -camSize - margin);
                    dir = new Vector2(Random.Range(-0.5f, 0.5f), 1f).normalized;
                    break;
                case 2: // left
                    spawnPos = new Vector2(-camWidth - margin, Random.Range(-camSize, camSize));
                    dir = new Vector2(1f, Random.Range(-0.5f, 0.5f)).normalized;
                    break;
                default: // right
                    spawnPos = new Vector2(camWidth + margin, Random.Range(-camSize, camSize));
                    dir = new Vector2(-1f, Random.Range(-0.5f, 0.5f)).normalized;
                    break;
            }

            DotType dotType = PickDotType();
            Sprite sprite = GetSprite(dotType);

            var dotObj = new GameObject("Dot_" + dotType);
            dotObj.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);

            var sr = dotObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;
            float radius = 0.25f;
            float spriteSize = sprite != null ? sprite.rect.width / sprite.pixelsPerUnit : 0.5f;
            float scale = (radius * 2f) / spriteSize;
            dotObj.transform.localScale = new Vector3(scale, scale, 1f);

            var dot = dotObj.AddComponent<DotBehavior>();
            dot.Initialize(dir, _baseSpeed, dotType, _player, _gameManager, radius, _activeSafeZones);

            _activeDots.Add(dotObj);
        }

        DotType PickDotType()
        {
            if (_complexityFactor >= 1.0f)
            {
                float r = Random.value;
                if (r < 0.33f) return DotType.Chaser;
                if (r < 0.6f) return DotType.Expander;
                return DotType.Normal;
            }
            else if (_complexityFactor >= 0.4f)
            {
                float r = Random.value;
                if (r < 0.2f) return DotType.Chaser;
                if (r < 0.35f) return DotType.Expander;
                return DotType.Normal;
            }
            else if (_complexityFactor >= 0.2f)
            {
                return Random.value < 0.2f ? DotType.Chaser : DotType.Normal;
            }
            return DotType.Normal;
        }

        Sprite GetSprite(DotType t)
        {
            return t switch
            {
                DotType.Chaser => _spriteChaser != null ? _spriteChaser : _spriteNormal,
                DotType.Expander => _spriteExpander != null ? _spriteExpander : _spriteNormal,
                _ => _spriteNormal
            };
        }

        IEnumerator SafeZoneLoop()
        {
            while (_isActive)
            {
                yield return new WaitForSeconds(5f);
                if (!_isActive) yield break;
                SpawnSafeZone();
                yield return new WaitForSeconds(3f);
                if (_activeSafeZones.Count > 0)
                {
                    var sz = _activeSafeZones[0];
                    _activeSafeZones.RemoveAt(0);
                    if (sz != null) Destroy(sz);
                }
            }
        }

        void SpawnSafeZone()
        {
            if (_mainCamera == null) return;
            float camSize = _mainCamera.orthographicSize;
            float camWidth = camSize * _mainCamera.aspect;
            Vector2 pos = new Vector2(Random.Range(-camWidth * 0.6f, camWidth * 0.6f), Random.Range(-camSize * 0.4f, camSize * 0.4f));

            var sz = new GameObject("SafeZone");
            sz.transform.position = new Vector3(pos.x, pos.y, 0f);
            var sr = sz.AddComponent<SpriteRenderer>();
            sr.sprite = _spriteSafeZone;
            sr.color = new Color(1f, 1f, 0.9f, 0.5f);
            sr.sortingOrder = 3;
            float szRadius = 1.2f;
            float spriteSize = _spriteSafeZone != null ? _spriteSafeZone.rect.width / _spriteSafeZone.pixelsPerUnit : 1f;
            float sc = (szRadius * 2f) / spriteSize;
            sz.transform.localScale = new Vector3(sc, sc, 1f);
            _activeSafeZones.Add(sz);
        }

        IEnumerator ScreenShakeLoop()
        {
            if (_mainCamera == null) yield break;
            float camZ = _mainCamera.transform.position.z;
            while (_isActive)
            {
                float t = Time.time;
                float shakeX = Mathf.PerlinNoise(t * 3f, 0f) * 0.15f - 0.075f;
                float shakeY = Mathf.PerlinNoise(0f, t * 3f) * 0.15f - 0.075f;
                _mainCamera.transform.position = new Vector3(shakeX, shakeY, camZ);
                yield return null;
            }
        }

        void OnDestroy()
        {
            CleanupDots();
        }
    }

    public class DotBehavior : MonoBehaviour
    {
        Vector2 _dir;
        float _speed;
        DotType _type;
        PlayerController _player;
        DotDodgeGameManager _gm;
        float _radius;
        List<GameObject> _safeZones;
        SpriteRenderer _sr;

        float _expandTimer = 0f;
        float _currentScale = 1f;
        float _initialScale = 1f;
        bool _nearMissTriggered = false;

        Camera _cam;

        public void Initialize(Vector2 dir, float speed, DotType type, PlayerController player, DotDodgeGameManager gm, float radius, List<GameObject> safeZones)
        {
            _dir = dir;
            _speed = speed;
            _type = type;
            _player = player;
            _gm = gm;
            _radius = radius;
            _safeZones = safeZones;
            _sr = GetComponent<SpriteRenderer>();
            _cam = Camera.main;
            _initialScale = transform.localScale.x;
        }

        void Update()
        {
            if (_gm == null || _gm.State != DotDodgeState.Playing) return;

            // Chaser: steer toward player
            if (_type == DotType.Chaser && _player != null)
            {
                Vector2 toPlayer = ((Vector2)_player.transform.position - (Vector2)transform.position).normalized;
                _dir = Vector2.Lerp(_dir, toPlayer, 0.05f).normalized;
            }

            // Expander: grow over time (max 2x initial scale)
            if (_type == DotType.Expander)
            {
                _expandTimer += Time.deltaTime;
                if (_expandTimer >= 3f)
                {
                    _expandTimer = 0f;
                    _currentScale = Mathf.Min(_currentScale * 1.2f, 2f);
                    transform.localScale = Vector3.one * (_initialScale * _currentScale);
                }
            }

            // SafeZone avoidance
            bool inSafeZone = false;
            if (_safeZones != null)
            {
                foreach (var sz in _safeZones)
                {
                    if (sz == null) continue;
                    float dist = Vector2.Distance(transform.position, sz.transform.position);
                    float szRadius = sz.transform.localScale.x * 0.5f;
                    if (dist < szRadius)
                    {
                        Vector2 away = ((Vector2)transform.position - (Vector2)sz.transform.position).normalized;
                        _dir = away;
                        inSafeZone = true;
                        break;
                    }
                }
            }

            transform.position += (Vector3)(_dir * _speed * Time.deltaTime);

            // Player collision
            if (_player != null)
            {
                float distToPlayer = Vector2.Distance(transform.position, _player.transform.position);
                float hitThreshold = _radius + _player.CurrentRadius;
                float nearMissThreshold = hitThreshold * 1.8f;

                if (distToPlayer < hitThreshold)
                {
                    _player.TriggerHitFlash();
                    _gm.OnPlayerHit();
                    Destroy(gameObject);
                    return;
                }
                else if (!inSafeZone && !_nearMissTriggered && distToPlayer < nearMissThreshold)
                {
                    _nearMissTriggered = true;
                    _gm.OnNearMiss();
                    _player.TriggerNearMissFlash();
                }
                else if (distToPlayer >= nearMissThreshold)
                {
                    _nearMissTriggered = false;
                }
            }

            // Out of bounds check
            if (_cam != null)
            {
                float camSize = _cam.orthographicSize;
                float camWidth = camSize * _cam.aspect;
                float margin = 1.5f;
                Vector3 pos = transform.position;
                if (pos.x < -camWidth - margin || pos.x > camWidth + margin || pos.y < -camSize - margin || pos.y > camSize + margin)
                    Destroy(gameObject);
            }
        }
    }
}
