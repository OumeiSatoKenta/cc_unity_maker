using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game022v2_GravityBall
{
    public class GravityBallController : MonoBehaviour
    {
        [SerializeField] GravityBallGameManager _gameManager;
        [SerializeField] SpriteRenderer _ballRenderer;
        [SerializeField] Sprite _obstacleSprite;
        [SerializeField] Sprite _gravityZoneSprite;
        [SerializeField] Sprite _triggerObstacleSprite;

        // Camera layout (computed in Start and SetupStage)
        void ComputeLayout()
        {
            float camSize = Camera.main.orthographicSize;
            _camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 2.8f;
            _gameAreaTop = camSize - topMargin;
            _gameAreaBottom = -camSize + bottomMargin;
        }

        // Stage config
        float _baseScrollSpeed = 3.0f;
        float _scrollSpeed;
        float _baseGravityAccel = 12f;
        float _gravityAccel;
        float _gapWidth = 3.0f;
        float _complexityFactor;
        float _targetDistance;
        float _obstacleInterval = 2.5f;

        // Ball state
        float _gravityDir = -1f; // -1 = down, +1 = up
        float _gravityVelocity;
        float _ballY;
        float _distanceTraveled;
        bool _isActive;

        // Bounds
        float _gameAreaTop;
        float _gameAreaBottom;
        float _camWidth;

        // Obstacle pool
        readonly List<ObstacleData> _activeObstacles = new List<ObstacleData>();
        readonly List<GameObject> _obstaclePool = new List<GameObject>();
        float _nextObstacleX;
        float _spawnX;
        int _obstaclePassedCount;
        bool _gravityZoneActive;
        float _gravityZoneEndX;
        float _gravityZoneEndWorld;

        // Score tracking
        float _lastScoreDistance;

        // Visual feedback
        Coroutine _ballPulseCoroutine;

        class ObstacleData
        {
            public GameObject topObj;
            public GameObject botObj;
            public float centerX;
            public float gapCenterY;
            public float gapHalf;
            public bool isNarrow;
            public bool isTrigger;
            public bool isMoving;
            public float moveSpeed;
            public float moveDir = 1f;
            public float moveBoundsTop;
            public float moveBoundsBot;
            public bool passed;
        }

        void Start()
        {
            ComputeLayout();
            _ballY = (_gameAreaTop + _gameAreaBottom) * 0.5f;
            if (_ballRenderer != null)
                _ballRenderer.transform.position = new Vector3(-_camWidth * 0.4f, _ballY, 0f);
        }

        public void SetupStage(StageManager.StageConfig config, float targetDistance)
        {
            ComputeLayout();
            _targetDistance = targetDistance;
            _scrollSpeed = _baseScrollSpeed * config.speedMultiplier;
            _gravityAccel = _baseGravityAccel * config.speedMultiplier;
            _complexityFactor = config.complexityFactor;
            _obstacleInterval = Mathf.Lerp(2.5f, 1.5f, config.countMultiplier - 1f);

            // Gap width based on stage
            _gapWidth = Mathf.Lerp(3.0f, 1.8f, config.complexityFactor);

            ClearObstacles();
            _distanceTraveled = 0f;
            _lastScoreDistance = 0f;
            _obstaclePassedCount = 0;
            _gravityVelocity = 0f;
            _gravityDir = -1f;
            _gravityZoneActive = false;

            float camSize = Camera.main.orthographicSize;
            _ballY = (_gameAreaTop + _gameAreaBottom) * 0.5f;
            if (_ballRenderer != null)
                _ballRenderer.transform.position = new Vector3(-_camWidth * 0.4f, _ballY, 0f);

            _spawnX = _camWidth + 1f;
            _nextObstacleX = _distanceTraveled + 3f; // first obstacle after 3m
            _isActive = true;
        }

        public void StopGame()
        {
            _isActive = false;
        }

        void Update()
        {
            if (!_isActive) return;

            // Input: gravity flip
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                FlipGravity();
            // Touch support
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                FlipGravity();

            float dt = Time.deltaTime;

            // Gravity
            float effectiveGravity = _gravityAccel;
            if (_gravityZoneActive)
                effectiveGravity *= 1.5f;

            _gravityVelocity += _gravityDir * effectiveGravity * dt;
            _gravityVelocity = Mathf.Clamp(_gravityVelocity, -15f, 15f);
            _ballY += _gravityVelocity * dt;

            // Gravity zone: end based on distance
            if (_gravityZoneActive && _distanceTraveled > _gravityZoneEndWorld)
                _gravityZoneActive = false;

            // Update ball position
            if (_ballRenderer != null)
            {
                var pos = _ballRenderer.transform.position;
                _ballRenderer.transform.position = new Vector3(pos.x, _ballY, pos.z);
            }

            // Scroll distance
            _distanceTraveled += _scrollSpeed * dt;

            // Distance score (every 10m)
            if (_distanceTraveled - _lastScoreDistance >= 10f)
            {
                _lastScoreDistance = Mathf.Floor(_distanceTraveled / 10f) * 10f;
            }
            _gameManager.OnDistanceUpdated(_distanceTraveled, _targetDistance);

            // Spawn obstacles
            if (_distanceTraveled >= _nextObstacleX)
            {
                SpawnObstaclePair();
                bool spawnPair = _complexityFactor >= 0.8f && Random.value < 0.4f;
                if (spawnPair)
                {
                    _nextObstacleX = _distanceTraveled + _obstacleInterval * 0.5f;
                    // second of pair will be spawned next frame
                }
                else
                    _nextObstacleX = _distanceTraveled + _obstacleInterval;
            }

            // Move obstacles and check
            UpdateObstacles(dt);

            // Gravity zone spawn
            if (_complexityFactor >= 0.6f && Random.value < 0.002f && !_gravityZoneActive)
            {
                _gravityZoneActive = true;
                _gravityZoneEndWorld = _distanceTraveled + 20f;
                SpawnGravityZoneVisual();
            }

            // Boundary check
            if (_ballY > _gameAreaTop || _ballY < _gameAreaBottom)
            {
                _isActive = false;
                StartCoroutine(CollisionFlash());
                _gameManager.OnGameOver();
                return;
            }
        }

        void FlipGravity()
        {
            _gravityDir *= -1f;
            _gravityVelocity = 0f;
            if (_ballPulseCoroutine != null) StopCoroutine(_ballPulseCoroutine);
            _ballPulseCoroutine = StartCoroutine(BallPulse());
        }

        IEnumerator BallPulse()
        {
            if (_ballRenderer == null) yield break;
            var t = _ballRenderer.transform;
            float dur = 0.15f;
            float half = dur * 0.5f;
            float elapsed = 0f;
            Vector3 baseScale = Vector3.one;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(1f, 1.3f, elapsed / half);
                t.localScale = baseScale * s;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(1.3f, 1f, elapsed / half);
                t.localScale = baseScale * s;
                yield return null;
            }
            t.localScale = baseScale;
        }

        IEnumerator CollisionFlash()
        {
            if (_ballRenderer == null) yield break;
            float dur = 0.3f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / dur;
                _ballRenderer.color = Color.Lerp(new Color(1f, 0.2f, 0.2f), Color.white, ratio);
                yield return null;
            }
            _ballRenderer.color = Color.white;
        }

        void SpawnObstaclePair()
        {
            bool isMoving = _complexityFactor >= 0.3f && Random.value < _complexityFactor * 0.4f;
            bool isTrigger = _complexityFactor >= 1.0f && Random.value < 0.2f;
            bool isNarrow = Random.value < 0.35f;
            float actualGap = isNarrow ? _gapWidth * 0.7f : _gapWidth;

            float halfGame = (_gameAreaTop - _gameAreaBottom) * 0.5f;
            float gapCenter = Random.Range(_gameAreaBottom + actualGap * 0.5f + 0.3f,
                                           _gameAreaTop - actualGap * 0.5f - 0.3f);

            float topY = gapCenter + actualGap * 0.5f;
            float botY = gapCenter - actualGap * 0.5f;

            Sprite sprite = isTrigger ? _triggerObstacleSprite : _obstacleSprite;
            Color col = isTrigger ? new Color(1f, 0.3f, 0.3f) : Color.white;

            // Top obstacle (from top to gapCenter+gap/2)
            var topObj = GetOrCreateObstacle();
            var topSr = topObj.GetComponent<SpriteRenderer>();
            topSr.sprite = sprite;
            topSr.color = col;
            topSr.sortingOrder = 5;
            float topObjH = _gameAreaTop - topY;
            topObj.transform.position = new Vector3(_spawnX, topY + topObjH * 0.5f, 0f);
            topObj.transform.localScale = new Vector3(0.8f, topObjH, 1f);
            topObj.SetActive(true);

            // Bottom obstacle
            var botObj = GetOrCreateObstacle();
            var botSr = botObj.GetComponent<SpriteRenderer>();
            botSr.sprite = sprite;
            botSr.color = col;
            botSr.sortingOrder = 5;
            float botObjH = botY - _gameAreaBottom;
            botObj.transform.position = new Vector3(_spawnX, botY - botObjH * 0.5f, 0f);
            botObj.transform.localScale = new Vector3(0.8f, botObjH, 1f);
            botObj.SetActive(true);

            var data = new ObstacleData
            {
                topObj = topObj,
                botObj = botObj,
                centerX = _spawnX,
                gapCenterY = gapCenter,
                gapHalf = actualGap * 0.5f,
                isNarrow = isNarrow,
                isTrigger = isTrigger,
                isMoving = isMoving,
                passed = false
            };
            if (isMoving)
            {
                data.moveSpeed = 1.5f * _complexityFactor;
                data.moveBoundsTop = _gameAreaTop - actualGap * 0.5f - 0.3f;
                data.moveBoundsBot = _gameAreaBottom + actualGap * 0.5f + 0.3f;
            }
            _activeObstacles.Add(data);
        }

        void SpawnGravityZoneVisual()
        {
            if (_gravityZoneSprite == null) return;
            var go = new GameObject("GravityZone");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _gravityZoneSprite;
            sr.sortingOrder = -3;
            sr.color = new Color(1f, 1f, 1f, 0.4f);
            go.transform.position = new Vector3(_spawnX + 8f, (_gameAreaTop + _gameAreaBottom) * 0.5f, 0f);
            float zoneW = 20f;
            float zoneH = _gameAreaTop - _gameAreaBottom;
            if (_gravityZoneSprite.rect.width > 0)
                go.transform.localScale = new Vector3(zoneW, zoneH / (_gravityZoneSprite.rect.height / _gravityZoneSprite.pixelsPerUnit), 1f);
            StartCoroutine(ScrollAndDestroy(go));
        }

        IEnumerator ScrollAndDestroy(GameObject go)
        {
            while (go != null && go.transform.position.x > -_camWidth - 2f)
            {
                go.transform.position += Vector3.left * _scrollSpeed * Time.deltaTime;
                yield return null;
            }
            if (go != null) Destroy(go);
        }

        void UpdateObstacles(float dt)
        {
            float ballX = _ballRenderer != null ? _ballRenderer.transform.position.x : -_camWidth * 0.4f;
            float ballRadius = 0.3f;

            for (int i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                var obs = _activeObstacles[i];
                // Scroll
                obs.centerX -= _scrollSpeed * dt;

                // Move obstacle Y if moving
                if (obs.isMoving)
                {
                    obs.gapCenterY += obs.moveSpeed * obs.moveDir * dt;
                    if (obs.gapCenterY > obs.moveBoundsTop) { obs.gapCenterY = obs.moveBoundsTop; obs.moveDir = -1f; }
                    if (obs.gapCenterY < obs.moveBoundsBot) { obs.gapCenterY = obs.moveBoundsBot; obs.moveDir = 1f; }
                }

                // Update positions
                float topY = obs.gapCenterY + obs.gapHalf;
                float botY = obs.gapCenterY - obs.gapHalf;
                float topObjH = _gameAreaTop - topY;
                float botObjH = botY - _gameAreaBottom;

                if (obs.topObj != null)
                    obs.topObj.transform.position = new Vector3(obs.centerX, topY + topObjH * 0.5f, 0f);
                if (obs.botObj != null)
                    obs.botObj.transform.position = new Vector3(obs.centerX, botY - botObjH * 0.5f, 0f);

                // Passed check
                if (!obs.passed && obs.centerX < ballX)
                {
                    obs.passed = true;
                    bool isPerfect = Mathf.Abs(_ballY - obs.gapCenterY) < obs.gapHalf * 0.2f;
                    _obstaclePassedCount++;
                    _gameManager.OnObstaclePassed(obs.isNarrow, isPerfect);
                }

                // Collision check (trigger obstacle: flip gravity instead of game over)
                float obsHalfW = 0.4f;
                bool inX = Mathf.Abs(obs.centerX - ballX) < obsHalfW + ballRadius;
                if (inX)
                {
                    bool hitTop = _ballY + ballRadius > obs.gapCenterY + obs.gapHalf;
                    bool hitBot = _ballY - ballRadius < obs.gapCenterY - obs.gapHalf;
                    if (hitTop || hitBot)
                    {
                        if (obs.isTrigger)
                        {
                            FlipGravity();
                            obs.isTrigger = false; // only trigger once
                        }
                        else
                        {
                            _isActive = false;
                            StartCoroutine(CollisionFlash());
                            _gameManager.OnGameOver();
                            return;
                        }
                    }
                }

                // Remove off-screen
                if (obs.centerX < -_camWidth - 2f)
                {
                    ReturnObstacle(obs.topObj);
                    ReturnObstacle(obs.botObj);
                    _activeObstacles.RemoveAt(i);
                }
            }
        }

        GameObject GetOrCreateObstacle()
        {
            foreach (var go in _obstaclePool)
                if (go != null && !go.activeSelf) return go;
            var newGo = new GameObject("Obstacle");
            newGo.AddComponent<SpriteRenderer>();
            _obstaclePool.Add(newGo);
            return newGo;
        }

        void ReturnObstacle(GameObject go)
        {
            if (go != null) go.SetActive(false);
        }

        void ClearObstacles()
        {
            foreach (var obs in _activeObstacles)
            {
                ReturnObstacle(obs.topObj);
                ReturnObstacle(obs.botObj);
            }
            _activeObstacles.Clear();
        }

        void OnDestroy()
        {
            foreach (var go in _obstaclePool)
                if (go != null) Destroy(go);
        }
    }
}
