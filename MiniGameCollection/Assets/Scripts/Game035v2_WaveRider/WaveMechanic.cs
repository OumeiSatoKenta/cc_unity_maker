using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game035v2_WaveRider
{
    public class WaveMechanic : MonoBehaviour
    {
        [SerializeField] WaveRiderGameManager _gameManager;
        [SerializeField] Transform _surferTransform;
        [SerializeField] SpriteRenderer _surferRenderer;
        [SerializeField] GameObject _rockPrefab;
        [SerializeField] GameObject _whirlpoolPrefab;
        [SerializeField] Transform _shieldVisual;
        [SerializeField] Camera _mainCamera;

        // Lane settings (responsive)
        float[] _laneX = new float[3];
        int _currentLane = 1; // 0=left, 1=center, 2=right
        float _surferY;

        // Wave simulation
        float _waveTime;
        float _waveFrequency = 1.5f;
        float _waveAmplitude = 0.4f;

        // Jump state
        bool _isJumping;
        float _jumpTime;
        float _jumpDuration = 0.5f;
        float _jumpHeight = 1.5f;
        float _baseY;

        // Scroll & distance
        float _scrollSpeed = 3f;
        float _distanceTraveled;
        float _goalDistance = 200f;
        float _distanceTimer;

        // Obstacle management
        List<GameObject> _activeObstacles = new List<GameObject>();
        bool _whirlpoolEnabled;
        bool _stormEnabled;
        float _spawnInterval = 2.5f;
        float _spawnTimer;
        int _maxObstacles = 5;
        int _obstacleCount;

        // Storm
        float _stormTimer;
        float _stormInterval = 4f;
        bool _isStormDark;
        float _stormDarkTimer;

        // Input
        Vector2 _touchStartPos;
        bool _isTouching;
        float _swipeThreshold = 80f;
        bool _swipeHandled;

        bool _isActive;

        // Camera bounds
        float _camSize;
        float _camW;
        float _spawnTopY;
        float _despawnBottomY;

        Coroutine _slideLaneCo;
        Coroutine _trickAnimCo;
        Coroutine _hitFlashCo;
        Coroutine _cameraShakeCo;
        Vector3 _cameraBasePos;

        void Awake()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            _cameraBasePos = _mainCamera.transform.position;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            // Clear existing obstacles
            foreach (var obs in _activeObstacles)
                if (obs != null) Destroy(obs);
            _activeObstacles.Clear();

            _isActive = true;
            _distanceTraveled = 0f;
            _distanceTimer = 0f;
            _obstacleCount = 0;
            _spawnTimer = 0f;
            _stormTimer = 0f;
            _isStormDark = false;

            // Apply config
            _scrollSpeed = 3f * config.speedMultiplier;
            _maxObstacles = config.countMultiplier;
            _whirlpoolEnabled = stageIndex >= 2;
            _stormEnabled = stageIndex >= 4;

            float[] goalDistances = { 200f, 300f, 400f, 500f, 600f };
            _goalDistance = goalDistances[Mathf.Clamp(stageIndex, 0, 4)];

            // Adjust spawn interval based on speed
            _spawnInterval = Mathf.Max(1.0f, 3.5f / config.speedMultiplier);

            // Wave params
            _waveFrequency = 1.5f + stageIndex * 0.3f;
            _waveAmplitude = 0.35f + stageIndex * 0.05f;

            // Responsive layout
            _camSize = _mainCamera.orthographicSize;
            _camW = _camSize * _mainCamera.aspect;
            float laneSpacing = _camW * 0.42f;
            _laneX[0] = -laneSpacing;
            _laneX[1] = 0f;
            _laneX[2] = laneSpacing;
            _surferY = -_camSize + 2.5f;
            _baseY = _surferY;
            _spawnTopY = _camSize + 1.5f;
            _despawnBottomY = -_camSize - 1.5f;

            // Reset surfer to center lane
            _currentLane = 1;
            if (_surferTransform != null)
                _surferTransform.position = new Vector3(_laneX[1], _surferY, 0f);

            // Reset jump state
            _isJumping = false;
            _jumpTime = 0f;
            _waveTime = 0f;

            // Hide storm if not active
            if (_mainCamera != null)
                _mainCamera.backgroundColor = new Color(0.05f, 0.10f, 0.20f);
        }

        public void Deactivate()
        {
            _isActive = false;
            if (_isStormDark && _mainCamera != null)
                _mainCamera.backgroundColor = new Color(0.05f, 0.10f, 0.20f);
        }

        public void ActivateShield()
        {
            if (_shieldVisual != null) _shieldVisual.gameObject.SetActive(true);
        }

        public void DeactivateShield()
        {
            if (_shieldVisual != null) _shieldVisual.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!_isActive) return;

            HandleInput();
            UpdateWave();
            UpdateJump();
            UpdateObstacles();
            UpdateDistance();

            if (_stormEnabled) UpdateStorm();
        }

        void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _touchStartPos = mouse.position.ReadValue();
                _isTouching = true;
                _swipeHandled = false;
            }

            if (_isTouching && mouse.leftButton.isPressed && !_swipeHandled)
            {
                Vector2 currentPos = mouse.position.ReadValue();
                float deltaX = currentPos.x - _touchStartPos.x;
                if (Mathf.Abs(deltaX) > _swipeThreshold)
                {
                    _swipeHandled = true;
                    if (deltaX > 0)
                        MoveLane(1);
                    else
                        MoveLane(-1);
                }
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                if (_isTouching && !_swipeHandled && !_isJumping)
                {
                    // Tap = jump
                    TryJump();
                }
                _isTouching = false;
                _swipeHandled = false;
            }
        }

        void MoveLane(int dir)
        {
            int newLane = Mathf.Clamp(_currentLane + dir, 0, 2);
            if (newLane == _currentLane) return;
            _currentLane = newLane;
            if (_surferTransform != null)
            {
                float targetX = _laneX[_currentLane];
                if (_slideLaneCo != null) StopCoroutine(_slideLaneCo);
                _slideLaneCo = StartCoroutine(SlideLane(targetX));
            }
        }

        IEnumerator SlideLane(float targetX)
        {
            float startX = _surferTransform.position.x;
            float elapsed = 0f;
            float dur = 0.12f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);
                var pos = _surferTransform.position;
                pos.x = Mathf.Lerp(startX, targetX, t);
                _surferTransform.position = pos;
                yield return null;
            }
            var finalPos = _surferTransform.position;
            finalPos.x = targetX;
            _surferTransform.position = finalPos;
        }

        void UpdateWave()
        {
            _waveTime += Time.deltaTime * _waveFrequency;
        }

        float GetWaveY() => Mathf.Sin(_waveTime) * _waveAmplitude;

        bool IsNearWavePeak() => Mathf.Sin(_waveTime) > 0.7f;

        void TryJump()
        {
            if (_isJumping) return;
            _isJumping = true;
            _jumpTime = 0f;

            bool isPerfect = IsNearWavePeak();
            _gameManager.OnTrickSuccess(isPerfect);

            if (_trickAnimCo != null) StopCoroutine(_trickAnimCo);
            _trickAnimCo = StartCoroutine(TrickAnimation(isPerfect));
        }

        void UpdateJump()
        {
            if (!_isJumping || _surferTransform == null) return;
            _jumpTime += Time.deltaTime;
            float t = _jumpTime / _jumpDuration;

            float waveOffset = GetWaveY();
            float jumpArc = Mathf.Sin(t * Mathf.PI) * _jumpHeight;
            float newY = _baseY + waveOffset + jumpArc;

            var pos = _surferTransform.position;
            pos.y = newY;
            _surferTransform.position = pos;

            if (_jumpTime >= _jumpDuration)
            {
                _isJumping = false;
                pos.y = _baseY + GetWaveY();
                _surferTransform.position = pos;
            }
        }

        void UpdateObstacles()
        {
            if (_maxObstacles <= 0) return;

            // Scroll obstacles downward
            for (int i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                var obs = _activeObstacles[i];
                if (obs == null) { _activeObstacles.RemoveAt(i); continue; }

                obs.transform.position += Vector3.down * _scrollSpeed * Time.deltaTime;

                // Whirlpool pull effect
                if (_whirlpoolEnabled && obs.name.Contains("Whirlpool"))
                {
                    float pullRadius = 2.5f;
                    float dist = Vector3.Distance(obs.transform.position, _surferTransform.position);
                    if (dist < pullRadius && dist > 0.5f)
                    {
                        float pullStr = 0.8f * (1f - dist / pullRadius);
                        float dx = obs.transform.position.x - _surferTransform.position.x;
                        // Pull surfer horizontally toward whirlpool
                        var sPos = _surferTransform.position;
                        sPos.x += Mathf.Sign(dx) * pullStr * Time.deltaTime;
                        sPos.x = Mathf.Clamp(sPos.x, _laneX[0] - 0.3f, _laneX[2] + 0.3f);
                        _surferTransform.position = sPos;
                    }
                }

                // Collision check
                if (!_isJumping)
                {
                    float colRadius = obs.name.Contains("Whirlpool") ? 0.6f : 0.5f;
                    float dist2 = Vector2.Distance(
                        new Vector2(obs.transform.position.x, obs.transform.position.y),
                        new Vector2(_surferTransform.position.x, _surferTransform.position.y));
                    if (dist2 < colRadius)
                    {
                        _activeObstacles.RemoveAt(i);
                        Destroy(obs);
                        _obstacleCount--;
                        OnHitObstacle();
                        return;
                    }
                }

                // Despawn
                if (obs.transform.position.y < _despawnBottomY)
                {
                    _activeObstacles.RemoveAt(i);
                    Destroy(obs);
                    _obstacleCount--;
                }
            }

            // Spawn new obstacles
            if (_obstacleCount < _maxObstacles)
            {
                _spawnTimer += Time.deltaTime;
                if (_spawnTimer >= _spawnInterval)
                {
                    _spawnTimer = 0f;
                    SpawnObstacle();
                }
            }
        }

        void SpawnObstacle()
        {
            int lane = Random.Range(0, 3);
            float spawnX = _laneX[lane];
            float spawnY = _spawnTopY;

            bool isWhirlpool = _whirlpoolEnabled && _whirlpoolPrefab != null && Random.value < 0.3f;
            GameObject prefab = isWhirlpool ? _whirlpoolPrefab : _rockPrefab;
            if (prefab == null) return;

            var obs = Instantiate(prefab, new Vector3(spawnX, spawnY, 0f), Quaternion.identity);
            obs.name = isWhirlpool ? "Whirlpool_" + _obstacleCount : "Rock_" + _obstacleCount;
            _activeObstacles.Add(obs);
            _obstacleCount++;
        }

        void UpdateDistance()
        {
            _distanceTimer += Time.deltaTime;
            if (_distanceTimer >= 0.5f)
            {
                _distanceTimer = 0f;
                _distanceTraveled += _scrollSpeed * 0.5f;
                _gameManager.OnDistanceUpdate(_distanceTraveled, _goalDistance);

                if (_distanceTraveled >= _goalDistance)
                {
                    _isActive = false;
                    _gameManager.OnStageGoalReached();
                }
            }
        }

        void UpdateStorm()
        {
            _stormTimer += Time.deltaTime;
            if (_stormTimer >= _stormInterval && !_isStormDark)
            {
                _stormTimer = 0f;
                _isStormDark = true;
                _stormDarkTimer = 0f;
                if (_mainCamera != null)
                    _mainCamera.backgroundColor = new Color(0.02f, 0.02f, 0.05f);
            }

            if (_isStormDark)
            {
                _stormDarkTimer += Time.deltaTime;
                if (_stormDarkTimer >= 0.6f)
                {
                    _isStormDark = false;
                    if (_mainCamera != null)
                        _mainCamera.backgroundColor = new Color(0.05f, 0.10f, 0.20f);
                }
            }
        }

        void OnHitObstacle()
        {
            if (!_isActive) return;
            _isActive = false;

            if (_hitFlashCo != null) StopCoroutine(_hitFlashCo);
            _hitFlashCo = StartCoroutine(HitFlash());
            if (_cameraShakeCo != null) StopCoroutine(_cameraShakeCo);
            _cameraShakeCo = StartCoroutine(CameraShake(0.3f, 0.2f));

            _gameManager.OnHitObstacle();
        }

        IEnumerator TrickAnimation(bool isPerfect)
        {
            if (_surferTransform == null) yield break;
            Color flashColor = isPerfect ? new Color(1f, 0.95f, 0.3f) : new Color(0.5f, 1f, 0.6f);
            if (_surferRenderer != null) _surferRenderer.color = flashColor;

            float dur = 0.25f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float s = t < 0.5f ? Mathf.Lerp(1f, 1.4f, t * 2f) : Mathf.Lerp(1.4f, 1f, (t - 0.5f) * 2f);
                _surferTransform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            _surferTransform.localScale = Vector3.one;
            if (_surferRenderer != null) _surferRenderer.color = Color.white;
        }

        IEnumerator HitFlash()
        {
            if (_surferRenderer == null) yield break;
            float dur = 0.3f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin(elapsed / dur * Mathf.PI * 4f);
                _surferRenderer.color = Color.Lerp(Color.white, Color.red, (t + 1f) * 0.5f);
                yield return null;
            }
            _surferRenderer.color = Color.white;
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-magnitude, magnitude);
                float y = Random.Range(-magnitude, magnitude);
                _mainCamera.transform.position = _cameraBasePos + new Vector3(x, y, 0f);
                yield return null;
            }
            _mainCamera.transform.position = _cameraBasePos;
        }

        void OnDestroy()
        {
            foreach (var obs in _activeObstacles)
                if (obs != null) Destroy(obs);
            _activeObstacles.Clear();
        }
    }
}
