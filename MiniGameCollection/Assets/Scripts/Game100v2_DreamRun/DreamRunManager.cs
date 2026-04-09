using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game100v2_DreamRun
{
    public class DreamRunManager : MonoBehaviour
    {
        [SerializeField] DreamRunGameManager _gameManager;
        [SerializeField] Sprite _runnerSprite;
        [SerializeField] Sprite _runnerJumpSprite;
        [SerializeField] Sprite _obstacleGroundSprite;
        [SerializeField] Sprite _obstacleAirSprite;
        [SerializeField] Sprite _fragmentSprite;
        [SerializeField] Sprite _backgroundSprite;
        [SerializeField] Sprite _bgLayerSprite;
        [SerializeField] Sprite _gravityZoneSprite;

        bool _isActive;
        int _laneCount = 2;
        int _totalFragments = 5;
        float _obstacleInterval = 0.8f;
        bool _airObstacleEnabled = false;
        bool _gravityFlipEnabled = false;
        float _scrollSpeed = 3f;
        bool _gravityFlipped = false;

        // Lane Y positions (calculated from camera)
        float[] _lanePositions;
        int _currentLane = 0;
        float _characterX;
        float _spawnX;
        float _despawnX;

        // Jump state
        bool _isJumping = false;
        float _jumpVelocity = 0f;
        const float JumpForce = 8f;
        const float Gravity = -20f;
        float _groundY = 0f;

        // Objects
        GameObject _runner;
        SpriteRenderer _runnerSr;
        List<GameObject> _obstacles = new();
        List<GameObject> _fragments = new();
        List<GameObject> _bgObjects = new();

        // Background scroll layers
        GameObject _bg1, _bg2, _bgLayer1a, _bgLayer1b;
        float _bgWidth;

        // Spawn timer
        float _spawnTimer;
        int _collectedFragments;
        int _currentStageIndex;
        int _spawnedFragments;

        // Gravity zone
        float _gravityZoneX = -99f;
        float _gravityZoneWidth = 4f;
        bool _inGravityZone = false;
        GameObject _gravityZoneObj;

        // Camera shake
        Camera _mainCamera;
        Vector3 _cameraOriginalPos;
        Coroutine _shakeCoroutine;

        // NearMiss detection
        bool _nearMissThisFrame = false;

        public void SetActive(bool active)
        {
            _isActive = active;
            if (_runner != null) _runner.SetActive(active);
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            _currentStageIndex = stageIndex;
            _scrollSpeed = 3f * config.speedMultiplier;

            // Parse customData: "laneCount,fragmentCount,obstacleInterval,airObstacle,gravityFlip"
            var parts = (config.customData ?? "2,5,0.8,false,false").Split(',');
            _laneCount = parts.Length > 0 && int.TryParse(parts[0], out int lc) ? lc : 2;
            _totalFragments = parts.Length > 1 && int.TryParse(parts[1], out int tf) ? tf : 5;
            _obstacleInterval = parts.Length > 2 && float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float oi) ? oi : 0.8f;
            _airObstacleEnabled = parts.Length > 3 && parts[3] == "true";
            _gravityFlipEnabled = parts.Length > 4 && parts[4] == "true";

            ClearObjects();
            SetupCamera();
            SetupLanes();
            SetupBackground();
            SpawnRunner();

            _spawnTimer = _obstacleInterval;
            _collectedFragments = 0;
            _spawnedFragments = 0;
            _currentLane = _laneCount == 2 ? 0 : 1;
            _isJumping = false;
            _jumpVelocity = 0f;
            _gravityFlipped = false;
            _inGravityZone = false;
            _isActive = true;

            if (_gravityFlipEnabled)
                ScheduleGravityZone();
        }

        void SetupCamera()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) return;
            _cameraOriginalPos = _mainCamera.transform.position;

            float camSize = _mainCamera.orthographicSize;
            float camWidth = camSize * _mainCamera.aspect;
            _characterX = -camWidth * 0.5f;
            _spawnX = camWidth + 1.5f;
            _despawnX = -camWidth - 2f;
        }

        void SetupLanes()
        {
            float camSize = _mainCamera != null ? _mainCamera.orthographicSize : 6f;
            float laneSpacing = camSize * 0.38f;

            _lanePositions = new float[3];
            if (_laneCount == 2)
            {
                _lanePositions[0] = -laneSpacing * 0.5f;
                _lanePositions[1] = laneSpacing * 0.5f;
                _lanePositions[2] = laneSpacing * 1.2f;
            }
            else
            {
                _lanePositions[0] = -laneSpacing;
                _lanePositions[1] = 0f;
                _lanePositions[2] = laneSpacing;
            }
            _groundY = _lanePositions[_currentLane < _lanePositions.Length ? _currentLane : 0];
        }

        void SetupBackground()
        {
            float camSize = _mainCamera != null ? _mainCamera.orthographicSize : 6f;
            float camWidth = camSize * (_mainCamera != null ? _mainCamera.aspect : 16f / 9f);

            if (_backgroundSprite != null)
            {
                float bgScaleX = (camWidth * 2f + 0.5f) / _backgroundSprite.bounds.size.x;
                float bgScaleY = (camSize * 2f) / _backgroundSprite.bounds.size.y;
                _bgWidth = camWidth * 2f + 0.5f;

                _bg1 = CreateSpriteObj("BG1", _backgroundSprite, new Vector3(0, 0, 10), -10, new Vector3(bgScaleX, bgScaleY, 1));
                _bg2 = CreateSpriteObj("BG2", _backgroundSprite, new Vector3(_bgWidth, 0, 10), -10, new Vector3(bgScaleX, bgScaleY, 1));
            }

            if (_bgLayerSprite != null)
            {
                float layerScaleX = (camWidth * 2f + 0.5f) / _bgLayerSprite.bounds.size.x;
                float layerScaleY = camSize / _bgLayerSprite.bounds.size.y;
                float layerY = -camSize * 0.3f;
                _bgLayer1a = CreateSpriteObj("BGLayer1a", _bgLayerSprite, new Vector3(0, layerY, 9), -9, new Vector3(layerScaleX, layerScaleY, 1));
                _bgLayer1b = CreateSpriteObj("BGLayer1b", _bgLayerSprite, new Vector3(_bgWidth, layerY, 9), -9, new Vector3(layerScaleX, layerScaleY, 1));
            }
        }

        GameObject CreateSpriteObj(string name, Sprite sprite, Vector3 pos, int sortOrder, Vector3 scale)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortOrder;
            _bgObjects.Add(go);
            return go;
        }

        void SpawnRunner()
        {
            if (_runner != null) Destroy(_runner);
            _runner = new GameObject("Runner");
            _runner.transform.position = new Vector3(_characterX, _lanePositions[_currentLane < _lanePositions.Length ? _currentLane : 0], 0);
            _runnerSr = _runner.AddComponent<SpriteRenderer>();
            _runnerSr.sprite = _runnerSprite;
            _runnerSr.sortingOrder = 5;
            float runnerSize = _mainCamera != null ? _mainCamera.orthographicSize * 0.28f : 1.5f;
            _runner.transform.localScale = Vector3.one * runnerSize;
        }

        void ScheduleGravityZone()
        {
            StartCoroutine(SpawnGravityZoneLater());
        }

        IEnumerator SpawnGravityZoneLater()
        {
            yield return new WaitForSeconds(5f);
            if (!_isActive) yield break;
            float camWidth = _mainCamera != null ? _mainCamera.orthographicSize * _mainCamera.aspect : 6f;
            _gravityZoneX = camWidth * 0.5f;
            SpawnGravityZoneVisual();
        }

        void SpawnGravityZoneVisual()
        {
            if (_gravityZoneObj != null) Destroy(_gravityZoneObj);
            _gravityZoneObj = new GameObject("GravityZone");
            _gravityZoneObj.transform.position = new Vector3(_gravityZoneX, 0, 0);

            if (_gravityZoneSprite != null)
            {
                float camSize = _mainCamera != null ? _mainCamera.orthographicSize : 6f;
                _gravityZoneObj.transform.localScale = new Vector3(_gravityZoneWidth / _gravityZoneSprite.bounds.size.x,
                    camSize * 2f / _gravityZoneSprite.bounds.size.y, 1);
                var sr = _gravityZoneObj.AddComponent<SpriteRenderer>();
                sr.sprite = _gravityZoneSprite;
                sr.sortingOrder = 3;
                sr.color = new Color(1f, 0.5f, 1f, 0.5f);
            }
        }

        void ClearObjects()
        {
            foreach (var obj in _obstacles) if (obj != null) Destroy(obj);
            _obstacles.Clear();
            foreach (var obj in _fragments) if (obj != null) Destroy(obj);
            _fragments.Clear();
            foreach (var obj in _bgObjects) if (obj != null) Destroy(obj);
            _bgObjects.Clear();
            if (_runner != null) { Destroy(_runner); _runner = null; }
            if (_gravityZoneObj != null) { Destroy(_gravityZoneObj); _gravityZoneObj = null; }
        }

        void Update()
        {
            if (!_isActive || _gameManager == null || !_gameManager.IsPlaying) return;

            HandleInput();
            UpdateRunner();
            ScrollObjects();
            UpdateGravityZone();
            CheckCollisions();
            SpawnObjects();
            _nearMissThisFrame = false;
        }

        void HandleInput()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            float viewportX = mousePos.x / Screen.width;

            if (viewportX < 0.33f)
                MoveLane(-1);
            else if (viewportX < 0.67f)
                TryJump();
            else
                MoveLane(1);
        }

        void MoveLane(int dir)
        {
            int newLane = _currentLane + ((_gravityFlipped && _inGravityZone) ? -dir : dir);
            newLane = Mathf.Clamp(newLane, 0, _laneCount - 1);
            _currentLane = newLane;
        }

        void TryJump()
        {
            if (_isJumping) return;
            _isJumping = true;
            _jumpVelocity = _gravityFlipped ? -JumpForce : JumpForce;
            if (_runnerSr != null && _runnerJumpSprite != null)
                _runnerSr.sprite = _runnerJumpSprite;
        }

        void UpdateRunner()
        {
            if (_runner == null) return;

            float targetY = _lanePositions[_currentLane < _lanePositions.Length ? _currentLane : 0];

            if (_isJumping)
            {
                float effectiveGravity = _gravityFlipped ? -Gravity : Gravity;
                _jumpVelocity += effectiveGravity * Time.deltaTime;
                float newY = _runner.transform.position.y + _jumpVelocity * Time.deltaTime;

                // Land check
                if ((!_gravityFlipped && newY <= targetY && _jumpVelocity < 0) ||
                    (_gravityFlipped && newY >= targetY && _jumpVelocity > 0))
                {
                    newY = targetY;
                    _isJumping = false;
                    _jumpVelocity = 0f;
                    if (_runnerSr != null && _runnerSprite != null)
                        _runnerSr.sprite = _runnerSprite;
                }
                _runner.transform.position = new Vector3(_runner.transform.position.x, newY, _runner.transform.position.z);
            }
            else
            {
                float currentY = _runner.transform.position.y;
                float newY = Mathf.MoveTowards(currentY, targetY, Time.deltaTime * 8f);
                _runner.transform.position = new Vector3(_runner.transform.position.x, newY, _runner.transform.position.z);
            }
        }

        void ScrollObjects()
        {
            float bgScrollSpeed = _scrollSpeed * 0.3f;
            float layerScrollSpeed = _scrollSpeed * 0.5f;

            // Scroll backgrounds (seamless loop)
            ScrollBgPair(_bg1, _bg2, bgScrollSpeed);
            ScrollBgPair(_bgLayer1a, _bgLayer1b, layerScrollSpeed);

            // Scroll obstacles and fragments
            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                if (_obstacles[i] == null) { _obstacles.RemoveAt(i); continue; }
                _obstacles[i].transform.position += Vector3.left * _scrollSpeed * Time.deltaTime;
                if (_obstacles[i].transform.position.x < _despawnX)
                {
                    Destroy(_obstacles[i]);
                    _obstacles.RemoveAt(i);
                }
            }
            for (int i = _fragments.Count - 1; i >= 0; i--)
            {
                if (_fragments[i] == null) { _fragments.RemoveAt(i); continue; }
                _fragments[i].transform.position += Vector3.left * _scrollSpeed * Time.deltaTime;
                if (_fragments[i].transform.position.x < _despawnX)
                {
                    Destroy(_fragments[i]);
                    _fragments.RemoveAt(i);
                }
            }

            // Scroll gravity zone
            if (_gravityZoneObj != null)
                _gravityZoneObj.transform.position += Vector3.left * _scrollSpeed * Time.deltaTime;
        }

        void ScrollBgPair(GameObject a, GameObject b, float speed)
        {
            if (a == null || b == null) return;
            a.transform.position += Vector3.left * speed * Time.deltaTime;
            b.transform.position += Vector3.left * speed * Time.deltaTime;
            if (a.transform.position.x < -_bgWidth * 0.5f)
                a.transform.position = new Vector3(b.transform.position.x + _bgWidth, a.transform.position.y, a.transform.position.z);
            if (b.transform.position.x < -_bgWidth * 0.5f)
                b.transform.position = new Vector3(a.transform.position.x + _bgWidth, b.transform.position.y, b.transform.position.z);
        }

        void UpdateGravityZone()
        {
            if (!_gravityFlipEnabled || _runner == null) return;
            float rx = _runner.transform.position.x;
            bool nowInZone = Mathf.Abs(rx - (_gravityZoneObj != null ? _gravityZoneObj.transform.position.x : _gravityZoneX)) < _gravityZoneWidth * 0.5f;
            if (nowInZone != _inGravityZone)
            {
                _inGravityZone = nowInZone;
                _gravityFlipped = _inGravityZone;
            }
        }

        void CheckCollisions()
        {
            if (_runner == null) return;
            float runnerSize = _runner.transform.localScale.x * 0.4f;

            // Check obstacles
            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                if (_obstacles[i] == null) continue;
                float dist = Vector2.Distance(_runner.transform.position, _obstacles[i].transform.position);
                float objSize = _obstacles[i].transform.localScale.x * 0.4f;
                if (dist < runnerSize + objSize)
                {
                    // Hit!
                    StartCoroutine(HitFlash());
                    TriggerCameraShake();
                    Destroy(_obstacles[i]);
                    _obstacles.RemoveAt(i);
                    _gameManager.OnObstacleHit();
                    return;
                }
                // Near miss detection
                if (!_nearMissThisFrame && dist < (runnerSize + objSize) * 2.5f)
                    _nearMissThisFrame = true;
            }

            // Check fragments
            for (int i = _fragments.Count - 1; i >= 0; i--)
            {
                if (_fragments[i] == null) continue;
                float dist = Vector2.Distance(_runner.transform.position, _fragments[i].transform.position);
                float fragSize = _fragments[i].transform.localScale.x * 0.5f;
                if (dist < runnerSize + fragSize)
                {
                    StartCoroutine(CollectEffect(_fragments[i]));
                    _fragments[i] = null;
                    _collectedFragments++;
                    _gameManager.OnFragmentCollected(_nearMissThisFrame, _collectedFragments, _totalFragments);

                    if (_collectedFragments >= _totalFragments)
                    {
                        _isActive = false;
                        _gameManager.OnStageClear(_currentStageIndex + 1);
                    }
                    return;
                }
            }
        }

        IEnumerator HitFlash()
        {
            if (_runnerSr == null) yield break;
            _runnerSr.color = new Color(1f, 0.2f, 0.2f, 1f);
            yield return new WaitForSeconds(0.15f);
            if (_runnerSr == null) yield break;
            _runnerSr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            if (_runnerSr == null) yield break;
            _runnerSr.color = new Color(1f, 0.2f, 0.2f, 1f);
            yield return new WaitForSeconds(0.15f);
            if (_runnerSr == null) yield break;
            _runnerSr.color = Color.white;
        }

        IEnumerator CollectEffect(GameObject obj)
        {
            if (obj == null) yield break;
            float t = 0f;
            Vector3 startScale = obj.transform.localScale;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.2f;
                float scale = Mathf.Lerp(1f, 1.8f, ratio);
                if (obj != null) obj.transform.localScale = startScale * scale;
                var sr = obj != null ? obj.GetComponent<SpriteRenderer>() : null;
                if (sr != null) sr.color = new Color(1f, 1f, 0.3f, 1f - ratio);
                yield return null;
            }
            if (obj != null) Destroy(obj);
        }

        void TriggerCameraShake()
        {
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(CameraShake());
        }

        IEnumerator CameraShake()
        {
            if (_mainCamera == null) yield break;
            float elapsed = 0f;
            float duration = 0.2f;
            float magnitude = 0.15f;
            while (elapsed < duration)
            {
                float x = Random.Range(-magnitude, magnitude);
                float y = Random.Range(-magnitude, magnitude);
                _mainCamera.transform.position = _cameraOriginalPos + new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _mainCamera.transform.position = _cameraOriginalPos;
        }

        void SpawnObjects()
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0) return;
            _spawnTimer = _obstacleInterval;

            // Decide to spawn obstacle or fragment
            bool spawnFragment = _spawnedFragments < _totalFragments &&
                                 (Random.value < 0.35f || (_totalFragments - _spawnedFragments) > (_obstacles.Count + 3));

            if (spawnFragment)
            {
                SpawnFragment();
                _spawnedFragments++;
            }
            else
            {
                SpawnObstacle();
            }
        }

        void SpawnObstacle()
        {
            bool isAir = _airObstacleEnabled && Random.value < 0.4f;
            int lane = isAir ? Random.Range(1, _laneCount) : Random.Range(0, _laneCount);
            float y = _lanePositions[Mathf.Clamp(lane, 0, _lanePositions.Length - 1)];
            if (isAir) y += _mainCamera != null ? _mainCamera.orthographicSize * 0.25f : 1f;

            var go = new GameObject("Obstacle");
            go.transform.position = new Vector3(_spawnX, y, 0);
            float size = _mainCamera != null ? _mainCamera.orthographicSize * 0.28f : 1.5f;
            go.transform.localScale = Vector3.one * size;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = isAir ? _obstacleAirSprite : _obstacleGroundSprite;
            sr.sortingOrder = 4;
            _obstacles.Add(go);
        }

        void SpawnFragment()
        {
            int lane = Random.Range(0, _laneCount);
            float y = _lanePositions[Mathf.Clamp(lane, 0, _lanePositions.Length - 1)];
            // Sometimes place high (requires jump)
            bool highPlacement = Random.value < 0.3f && _currentStageIndex >= 1;
            if (highPlacement) y += _mainCamera != null ? _mainCamera.orthographicSize * 0.3f : 1.5f;

            var go = new GameObject("Fragment");
            go.transform.position = new Vector3(_spawnX + Random.Range(0f, 1f), y, 0);
            float size = _mainCamera != null ? _mainCamera.orthographicSize * 0.22f : 1.2f;
            go.transform.localScale = Vector3.one * size;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _fragmentSprite;
            sr.sortingOrder = 4;
            _fragments.Add(go);

            // Floating animation
            StartCoroutine(FloatAnimation(go));
        }

        IEnumerator FloatAnimation(GameObject obj)
        {
            if (obj == null) yield break;
            float startY = obj.transform.position.y;
            float t = Random.Range(0f, Mathf.PI * 2f);
            while (obj != null)
            {
                t += Time.deltaTime * 2f;
                obj.transform.position = new Vector3(
                    obj.transform.position.x,
                    startY + Mathf.Sin(t) * 0.2f,
                    obj.transform.position.z);
                yield return null;
            }
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            if (_mainCamera != null && _shakeCoroutine != null)
                _mainCamera.transform.position = _cameraOriginalPos;
            ClearObjects();
        }
    }
}
