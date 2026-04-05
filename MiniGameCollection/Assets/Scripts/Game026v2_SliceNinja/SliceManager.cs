using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game026v2_SliceNinja
{
    public class SliceManager : MonoBehaviour
    {
        [SerializeField] Sprite _fruitSprite;
        [SerializeField] Sprite _bombSprite;
        [SerializeField] Sprite _frozenFruitSprite;
        [SerializeField] Sprite _miniFruitSprite;
        [SerializeField] LineRenderer _sliceTrail;

        SliceNinjaGameManager _gameManager;
        Camera _mainCamera;
        float _camSize;
        float _camWidth;

        // Stage parameters
        float _flySpeed = 3f;
        float _spawnInterval = 2f;
        float _bombRate = 0f;
        float _frozenRate = 0f;
        float _miniRate = 0f;
        float _stealthBombRate = 0f;

        // Swipe state
        bool _isSwiping;
        bool _inputEnabled = false;
        List<Vector2> _swipePoints = new();
        List<Vector3> _trailPoints = new();

        // Combo
        int _comboCount = 0;
        float _lastSliceTime = -999f;
        const float ComboTimeWindow = 5f;

        // Active objects
        List<FlyingObject> _activeObjects = new();
        Coroutine _spawnCoroutine;

        bool _isActive = false;

        public void Initialize(SliceNinjaGameManager gm, Camera cam)
        {
            _gameManager = gm;
            _mainCamera = cam;
            _camSize = cam.orthographicSize;
            _camWidth = _camSize * cam.aspect;
            _inputEnabled = true;

            if (_sliceTrail != null)
            {
                _sliceTrail.positionCount = 0;
                _sliceTrail.startWidth = 0.15f;
                _sliceTrail.endWidth = 0.02f;
            }
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            float speed = config.speedMultiplier;
            // countMultiplier is int (1, 1, 2, 2, 2 for stages 1-5)
            // Use it to adjust spawn interval
            float count = config.countMultiplier;

            _flySpeed = 3f * speed;
            _spawnInterval = Mathf.Max(0.6f, 2f / Mathf.Max(1f, count) * (1f / speed));

            _bombRate = 0f;
            _frozenRate = 0f;
            _miniRate = 0f;
            _stealthBombRate = 0f;

            switch (stageNumber)
            {
                case 2: _bombRate = 0.10f; break;
                case 3: _bombRate = 0.10f; _frozenRate = 0.15f; break;
                case 4: _bombRate = 0.20f; _frozenRate = 0.10f; _miniRate = 0.20f; break;
                case 5: _bombRate = 0.20f; _frozenRate = 0.05f; _miniRate = 0.10f; _stealthBombRate = 0.10f; break;
            }

            if (_spawnCoroutine != null)
                StopCoroutine(_spawnCoroutine);

            ClearAllObjects();
            _isActive = true;
            _inputEnabled = true;
            _spawnCoroutine = StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
        {
            _isActive = false;
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        public void DisableInput()
        {
            _inputEnabled = false;
            _isSwiping = false;
            if (_sliceTrail != null) _sliceTrail.positionCount = 0;
        }

        void ClearAllObjects()
        {
            foreach (var obj in _activeObjects)
            {
                if (obj != null) Destroy(obj.gameObject);
            }
            _activeObjects.Clear();
        }

        IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(0.5f);
            while (_isActive)
            {
                SpawnObject();
                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        void SpawnObject()
        {
            float rand = Random.value;
            ObjectType type;

            if (rand < _stealthBombRate)
                type = ObjectType.StealthBomb;
            else if (rand < _stealthBombRate + _bombRate)
                type = ObjectType.Bomb;
            else if (rand < _stealthBombRate + _bombRate + _frozenRate)
                type = ObjectType.FrozenFruit;
            else if (rand < _stealthBombRate + _bombRate + _frozenRate + _miniRate)
                type = ObjectType.MiniFruit;
            else
                type = ObjectType.Fruit;

            float spawnX = Random.Range(-_camWidth * 0.75f, _camWidth * 0.75f);
            float spawnY = -_camSize - 0.5f;
            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            var go = new GameObject($"FlyingObject_{type}");
            go.transform.position = spawnPos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _fruitSprite;
            sr.sortingOrder = 5;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0.4f;

            var fo = go.AddComponent<FlyingObject>();
            fo.SetSprites(_fruitSprite, _bombSprite, _frozenFruitSprite, _miniFruitSprite, sr);
            fo.Initialize(type, _gameManager, _mainCamera);

            // Launch upward with arc
            float angleOffset = Random.Range(-35f, 35f);
            float rad = (90f + angleOffset) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            rb.linearVelocity = dir * _flySpeed;

            _activeObjects.Add(fo);
            _activeObjects.RemoveAll(o => o == null);
        }

        void Update()
        {
            if (!_inputEnabled) return;
            HandleSwipeInput();
        }

        void HandleSwipeInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isSwiping = true;
                _swipePoints.Clear();
                _trailPoints.Clear();
                Vector2 wp = ScreenToWorld(mouse.position.ReadValue());
                _swipePoints.Add(wp);
                _trailPoints.Add(new Vector3(wp.x, wp.y, 0f));
                UpdateTrail();
            }
            else if (mouse.leftButton.isPressed && _isSwiping)
            {
                Vector2 wp = ScreenToWorld(mouse.position.ReadValue());
                if (_swipePoints.Count == 0 || Vector2.Distance(_swipePoints[_swipePoints.Count - 1], wp) > 0.1f)
                {
                    _swipePoints.Add(wp);
                    _trailPoints.Add(new Vector3(wp.x, wp.y, 0f));
                    if (_trailPoints.Count > 20) _trailPoints.RemoveAt(0);
                    UpdateTrail();
                }
            }
            else if (mouse.leftButton.wasReleasedThisFrame && _isSwiping)
            {
                _isSwiping = false;
                PerformSliceCheck();
                StartCoroutine(FadeTrail());
            }
        }

        Vector2 ScreenToWorld(Vector2 screenPos)
        {
            Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            return new Vector2(wp.x, wp.y);
        }

        void UpdateTrail()
        {
            if (_sliceTrail == null) return;
            _sliceTrail.positionCount = _trailPoints.Count;
            _sliceTrail.SetPositions(_trailPoints.ToArray());
        }

        IEnumerator FadeTrail()
        {
            if (_sliceTrail == null) yield break;
            float t = 0f;
            Color startColor = _sliceTrail.startColor;
            Color endColor = _sliceTrail.endColor;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float alpha = 1f - (t / 0.3f);
                _sliceTrail.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
                _sliceTrail.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha * 0.3f);
                yield return null;
            }
            _sliceTrail.positionCount = 0;
            _sliceTrail.startColor = startColor;
            _sliceTrail.endColor = endColor;
        }

        void PerformSliceCheck()
        {
            if (_gameManager == null || _gameManager.State == SliceNinjaGameState.GameOver) return;
            if (_swipePoints.Count < 2) return;

            var slicedObjects = new HashSet<FlyingObject>();

            for (int i = 0; i < _swipePoints.Count - 1; i++)
            {
                Vector2 segStart = _swipePoints[i];
                Vector2 segEnd = _swipePoints[i + 1];
                float dist = Vector2.Distance(segStart, segEnd);
                int steps = Mathf.Max(2, Mathf.CeilToInt(dist / 0.15f));

                for (int s = 0; s <= steps; s++)
                {
                    Vector2 point = Vector2.Lerp(segStart, segEnd, (float)s / steps);
                    var hits = Physics2D.OverlapCircleAll(point, 0.35f);
                    foreach (var hit in hits)
                    {
                        var fo = hit.GetComponent<FlyingObject>();
                        if (fo != null && !fo.IsSliced && !slicedObjects.Contains(fo))
                            slicedObjects.Add(fo);
                    }
                }
            }

            if (slicedObjects.Count == 0) return;

            // First pass: check for bombs
            bool bombCut = false;
            foreach (var fo in slicedObjects)
            {
                if (fo.IsBomb && fo.TrySlice())
                {
                    bombCut = true;
                    break;
                }
            }

            if (bombCut) return;

            // Second pass: slice fruits
            int slicedCount = 0;
            foreach (var fo in slicedObjects)
            {
                if (!fo.IsBomb && fo.TrySlice())
                    slicedCount++;
            }

            if (slicedCount > 0)
                ApplyComboAndScore(slicedCount);
        }

        void ApplyComboAndScore(int slicedThisSwipe)
        {
            float now = Time.time;
            bool inComboWindow = (now - _lastSliceTime) < ComboTimeWindow;

            if (!inComboWindow)
                _comboCount = 0;

            _comboCount++;
            _lastSliceTime = now;

            float swipeMultiplier = slicedThisSwipe switch
            {
                1 => 1f,
                2 => 2f,
                3 => 3f,
                _ => 5f
            };

            float comboMultiplier = _comboCount switch
            {
                1 => 1.0f,
                2 => 1.5f,
                3 => 2.0f,
                _ => 3.0f
            };

            int baseScore = slicedThisSwipe * 10;
            int finalScore = Mathf.RoundToInt(baseScore * swipeMultiplier * comboMultiplier);
            _gameManager.AddScore(finalScore, comboMultiplier);
        }

        public void ResetCombo()
        {
            _comboCount = 0;
        }
    }
}
