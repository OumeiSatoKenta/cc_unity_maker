using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game033v2_AimSniper
{
    public class AimSniperMechanic : MonoBehaviour
    {
        [SerializeField] AimSniperGameManager _gameManager;
        [SerializeField] Transform _scopeTransform;
        [SerializeField] SpriteRenderer _scopeRenderer;
        [SerializeField] AimSniperUI _ui;

        [SerializeField] Sprite _targetSprite;
        [SerializeField] Sprite _movingTargetSprite;
        [SerializeField] Sprite _obstacleSprite;

        // Stage parameters
        int _bulletCount;
        int _remainingBullets;
        float _targetSpeed;
        bool _hasWind;
        bool _hasDistance;
        bool _hasObstacle;
        int _targetCount;

        float _swayAmplitude = 0.5f;
        float _swayFreq = 1.2f;
        Vector2 _windOffset = Vector2.zero;
        Vector2 _scopeCenter;
        Vector2 _scopeVisualOffset;

        readonly float _hitRadius = 0.6f;
        readonly float _headshotRadius = 0.25f;

        bool _isDragging;
        bool _isActive;
        List<TargetController> _targets = new();
        List<GameObject> _obstacles = new();

        Coroutine _obstacleRoutine;

        int _headshotStreak;
        float _comboMultiplier = 1.0f;

        public int RemainingBullets => _remainingBullets;
        public int RemainingTargets { get; private set; }

        void Update()
        {
            if (!_isActive) return;

            HandleInput();
            UpdateScopeVisuals();
        }

        void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.isPressed)
            {
                _isDragging = true;
                Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
                _scopeCenter = Vector2.Lerp(_scopeCenter, mouseWorld, Time.deltaTime * 10f);
            }

            if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
            {
                _isDragging = false;
                TryShoot();
            }
        }

        void UpdateScopeVisuals()
        {
            float noiseX = (Mathf.PerlinNoise(Time.time * _swayFreq, 0f) - 0.5f) * 2f * _swayAmplitude;
            float noiseY = (Mathf.PerlinNoise(0f, Time.time * _swayFreq) - 0.5f) * 2f * _swayAmplitude;
            _scopeVisualOffset = new Vector2(noiseX, noiseY) + _windOffset;

            if (_scopeTransform != null)
            {
                _scopeTransform.position = new Vector3(_scopeCenter.x + _scopeVisualOffset.x,
                    _scopeCenter.y + _scopeVisualOffset.y, 0f);
            }

            // Update wind indicator
            if (_ui != null)
            {
                _ui.UpdateWindIndicator(_windOffset);
            }
        }

        void TryShoot()
        {
            if (_remainingBullets <= 0 || !_isActive) return;

            _remainingBullets--;
            _ui?.UpdateBullets(_remainingBullets);

            Vector2 actualAimPos = _scopeCenter + _scopeVisualOffset;
            bool hit = false;

            foreach (var target in _targets)
            {
                if (!target.IsAlive) continue;
                float dist = Vector2.Distance(actualAimPos, target.WorldPosition);
                if (dist < _hitRadius)
                {
                    bool headshot = dist < _headshotRadius;
                    target.OnHit(headshot);
                    RemainingTargets--;

                    if (headshot)
                    {
                        _headshotStreak++;
                        _comboMultiplier = _headshotStreak >= 3 ? 2.0f : (_headshotStreak >= 2 ? 1.5f : 1.0f);
                    }
                    else
                    {
                        _headshotStreak = 0;
                        _comboMultiplier = 1.0f;
                    }

                    int pts = headshot ? Mathf.RoundToInt(30 * _comboMultiplier) : Mathf.RoundToInt(10 * _comboMultiplier);
                    _gameManager.OnTargetHit(pts, headshot, _comboMultiplier, RemainingTargets);
                    hit = true;
                    break;
                }
            }

            if (!hit)
            {
                // Miss flash on scope
                _headshotStreak = 0;
                _comboMultiplier = 1.0f;
                StartCoroutine(ScopeMissFlash());
                _gameManager.OnMiss(_remainingBullets, RemainingTargets);
            }
        }

        IEnumerator ScopeMissFlash()
        {
            if (_scopeRenderer != null)
            {
                _scopeRenderer.color = new Color(1f, 0.2f, 0.2f, 1f);
                yield return new WaitForSeconds(0.2f);
                _scopeRenderer.color = Color.white;
            }
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            // Clean up previous
            foreach (var t in _targets)
                if (t != null) Destroy(t.gameObject);
            _targets.Clear();
            foreach (var o in _obstacles)
                if (o != null) Destroy(o);
            _obstacles.Clear();
            if (_obstacleRoutine != null) StopCoroutine(_obstacleRoutine);

            _isActive = true;
            _headshotStreak = 0;
            _comboMultiplier = 1.0f;

            // Map config to stage params
            _targetCount = config.countMultiplier;
            _targetSpeed = config.speedMultiplier;
            _hasWind = config.complexityFactor >= 0.3f;
            _hasDistance = config.complexityFactor >= 0.5f;
            _hasObstacle = config.complexityFactor >= 0.7f;
            _bulletCount = new int[] { 5, 6, 7, 7, 8 }[Mathf.Clamp(stageIndex, 0, 4)];
            _remainingBullets = _bulletCount;

            // Sway amplitude based on stage
            _swayAmplitude = stageIndex < 2 ? 0.5f : (stageIndex < 4 ? 0.7f : 0.9f);

            // Wind
            if (_hasWind)
            {
                float windAngle = Random.Range(0f, 360f);
                float windStr = Random.Range(0.15f, 0.35f);
                _windOffset = new Vector2(Mathf.Cos(windAngle * Mathf.Deg2Rad), 0f) * windStr;
            }
            else
            {
                _windOffset = Vector2.zero;
            }

            // Scope initial center
            _scopeCenter = Vector2.zero;

            RemainingTargets = _targetCount;

            SpawnTargets(stageIndex);

            if (_hasObstacle)
                _obstacleRoutine = StartCoroutine(ObstacleCycle());

            _ui?.UpdateBullets(_remainingBullets);
            _ui?.UpdateTargets(RemainingTargets);
        }

        void SpawnTargets(int stageIndex)
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topY = camSize - 1.5f;
            float bottomY = -camSize + 3.0f;

            bool moving = stageIndex >= 1;

            for (int i = 0; i < _targetCount; i++)
            {
                float x = Random.Range(-camWidth + 1f, camWidth - 1f);
                float y = Random.Range(bottomY + 0.5f, topY - 0.5f);

                var obj = new GameObject($"Target_{i}");
                obj.transform.SetParent(transform);
                obj.transform.position = new Vector3(x, y, 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                var tc = obj.AddComponent<TargetController>();

                Sprite spr = (moving && i % 2 == 0) ? _movingTargetSprite : _targetSprite;
                if (spr == null) spr = _targetSprite;

                TargetController.TargetType type = (moving && i % 2 == 0) ? TargetController.TargetType.Moving : TargetController.TargetType.Static;
                float swayMult = _hasDistance ? Random.Range(0.5f, 2.0f) : 1.0f;
                float moveRange = Random.Range(1.5f, 3.0f);

                sr.sortingOrder = 5;
                tc.Initialize(type, spr, _targetSpeed, moveRange, swayMult);

                _targets.Add(tc);
            }
        }

        IEnumerator ObstacleCycle()
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;

            // Spawn 2 obstacles
            for (int i = 0; i < 2; i++)
            {
                float x = Random.Range(-camWidth + 1f, camWidth - 1f);
                float y = Random.Range(-camSize + 3.5f, camSize - 2f);
                var obj = new GameObject($"Obstacle_{i}");
                obj.transform.SetParent(transform);
                obj.transform.position = new Vector3(x, y, 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _obstacleSprite;
                sr.sortingOrder = 6;
                sr.transform.localScale = Vector3.one * 1.2f;
                _obstacles.Add(obj);
            }

            while (_isActive)
            {
                yield return new WaitForSeconds(2.0f);
                // Toggle obstacles to hide/show nearby targets
                foreach (var obs in _obstacles)
                {
                    if (obs == null) continue;
                    // Find targets near obstacle and toggle hidden
                    foreach (var t in _targets)
                    {
                        if (t == null || !t.IsAlive) continue;
                        if (Vector2.Distance(t.WorldPosition, obs.transform.position) < 1.5f)
                            t.SetHidden(true);
                    }
                }
                yield return new WaitForSeconds(1.5f);
                foreach (var t in _targets)
                {
                    if (t != null && t.IsAlive) t.SetHidden(false);
                }
            }
        }

        public void Deactivate()
        {
            _isActive = false;
            if (_obstacleRoutine != null) { StopCoroutine(_obstacleRoutine); _obstacleRoutine = null; }
        }
    }
}
