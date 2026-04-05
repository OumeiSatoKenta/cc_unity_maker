using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game042v2_ColorDrop
{
    public enum DropType { Red, Blue, Green, Rainbow, Bomb }

    public class ColorDrop
    {
        public GameObject obj;
        public SpriteRenderer sr;
        public DropType type;
        public float speed;
        public bool isActive;
    }

    public class ColorDropMechanic : MonoBehaviour
    {
        [SerializeField] ColorDropGameManager _gameManager;
        [SerializeField] Sprite _spriteDropRed;
        [SerializeField] Sprite _spriteDropBlue;
        [SerializeField] Sprite _spriteDropGreen;
        [SerializeField] Sprite _spriteDropRainbow;
        [SerializeField] Sprite _spriteDropBomb;
        [SerializeField] Sprite _spriteBucketRed;
        [SerializeField] Sprite _spriteBucketBlue;
        [SerializeField] Sprite _spriteBucketGreen;
        [SerializeField] Sprite _spriteBucketGlow;

        // Stage parameters
        int _colorCount = 2;
        float _dropSpeed = 3.0f;
        int _targetCount = 20;
        float _rainbowChance = 0f;
        float _bombChance = 0f;
        float _shuffleInterval = 0f;

        // Game state
        bool _isActive = false;
        int _processedCount = 0;
        int _comboCount = 0;
        float _spawnTimer = 0f;
        float _spawnInterval = 1.2f;
        float _shuffleTimer = 0f;

        List<ColorDrop> _drops = new List<ColorDrop>();
        List<GameObject> _buckets = new List<GameObject>();
        List<DropType> _bucketColors = new List<DropType>();

        Camera _mainCamera;
        float _camSize;
        float _camWidth;
        float _dropY;
        float _bucketY;
        float _bucketW;
        float _bucketH;

        // Input state
        Vector2 _touchStart;
        bool _isSwiping = false;
        const float SwipeThreshold = 50f;

        void Awake()
        {
            _mainCamera = Camera.main;
        }

        void Update()
        {
            if (!_isActive) return;

            HandleInput();
            MoveDrop();

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer = 0f;
                SpawnDrop();
            }

            if (_shuffleInterval > 0f)
            {
                _shuffleTimer += Time.deltaTime;
                if (_shuffleTimer >= _shuffleInterval)
                {
                    _shuffleTimer = 0f;
                    ShuffleBuckets();
                }
            }
        }

        void HandleInput()
        {
            // Touch input
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    _touchStart = touch.position.ReadValue();
                    _isSwiping = true;
                }
                else if (touch.press.wasReleasedThisFrame && _isSwiping)
                {
                    _isSwiping = false;
                    ProcessSwipe(touch.position.ReadValue());
                }
                return;
            }

            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _touchStart = Mouse.current.position.ReadValue();
                _isSwiping = true;
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && _isSwiping)
            {
                _isSwiping = false;
                ProcessSwipe(Mouse.current.position.ReadValue());
            }
        }

        void ProcessSwipe(Vector2 endPos)
        {
            Vector2 delta = endPos - _touchStart;

            ColorDrop activeDrop = GetActiveFallingDrop();
            if (activeDrop == null || !activeDrop.isActive) return;

            if (activeDrop.type == DropType.Rainbow)
            {
                int bucketIdx = GetBucketIndexFromSwipe(delta);
                HandleDropInBucket(activeDrop, bucketIdx);
                return;
            }

            if (activeDrop.type == DropType.Bomb)
            {
                _comboCount = 0;
                _gameManager.OnComboChanged(0);
                StartCoroutine(FlashDrop(activeDrop, Color.red));
                activeDrop.isActive = false;
                StartCoroutine(FlyDropOff(activeDrop));
                _gameManager.OnLifeLost();
                return;
            }

            if (Mathf.Abs(delta.x) < SwipeThreshold && Mathf.Abs(delta.y) < SwipeThreshold)
                return;

            int idx = GetBucketIndexFromSwipe(delta);
            HandleDropInBucket(activeDrop, idx);
        }

        ColorDrop GetActiveFallingDrop()
        {
            ColorDrop best = null;
            float lowestY = float.MaxValue;
            foreach (var d in _drops)
            {
                if (d.obj == null || !d.isActive) continue;
                float y = d.obj.transform.position.y;
                if (y < lowestY)
                {
                    lowestY = y;
                    best = d;
                }
            }
            return best;
        }

        int GetBucketIndexFromSwipe(Vector2 delta)
        {
            if (_colorCount == 2)
            {
                return delta.x < 0 ? 0 : 1;
            }
            else
            {
                // 3 buckets: left, center, right based on magnitude
                if (delta.x < -SwipeThreshold * 0.5f) return 0;
                if (delta.x > SwipeThreshold * 0.5f) return 2;
                return 1;
            }
        }

        void HandleDropInBucket(ColorDrop drop, int bucketIdx)
        {
            if (bucketIdx < 0 || bucketIdx >= _buckets.Count) return;
            drop.isActive = false;

            DropType bucketColor = _bucketColors[bucketIdx];
            bool isCorrect = drop.type == bucketColor || drop.type == DropType.Rainbow;

            if (drop.type == DropType.Bomb)
            {
                // Should not reach here but safety
                _gameManager.OnLifeLost();
                StartCoroutine(FlyDropOff(drop));
                return;
            }

            if (isCorrect)
            {
                _comboCount++;
                int baseScore = drop.type == DropType.Rainbow ? 500 : 100;
                int comboBonus = _comboCount * 50;
                float multiplier = _comboCount >= 20 ? 5f : _comboCount >= 10 ? 3f : _comboCount >= 5 ? 2f : 1f;
                int score = Mathf.RoundToInt((baseScore + comboBonus) * multiplier);
                _gameManager.OnScoreAdded(score);
                _gameManager.OnComboChanged(_comboCount);

                StartCoroutine(FlyDropToBucket(drop, _buckets[bucketIdx].transform.position));
                StartCoroutine(BucketPulse(_buckets[bucketIdx]));

                _processedCount++;
                _gameManager.OnProgressChanged(_processedCount, _targetCount);

                if (_processedCount >= _targetCount)
                {
                    _isActive = false;
                    _gameManager.OnStageClear(0);
                }
            }
            else
            {
                _comboCount = 0;
                _gameManager.OnComboChanged(0);
                _gameManager.OnLifeLost();
                StartCoroutine(FlashDrop(drop, Color.red));
                StartCoroutine(CameraShake(0.1f, 0.3f));
                StartCoroutine(FlyDropOff(drop));
            }
        }

        void MoveDrop()
        {
            float camBottom = -_camSize - 1f;
            List<ColorDrop> toRemove = new List<ColorDrop>();

            foreach (var d in _drops)
            {
                if (d.obj == null) { toRemove.Add(d); continue; }
                if (!d.isActive) continue;

                d.obj.transform.Translate(Vector3.down * d.speed * Time.deltaTime);

                if (d.obj.transform.position.y < camBottom)
                {
                    // Drop fell through
                    if (d.type == DropType.Bomb)
                    {
                        // Bomb passed - no penalty
                    }
                    else
                    {
                        _comboCount = 0;
                        _gameManager.OnComboChanged(0);
                        _gameManager.OnLifeLost();
                    }
                    d.isActive = false;
                    Destroy(d.obj);
                    toRemove.Add(d);
                }
            }

            foreach (var d in toRemove) _drops.Remove(d);
        }

        void SpawnDrop()
        {
            if (!_isActive) return;

            DropType type = ChooseDropType();
            Sprite sp = GetSpriteForType(type);

            var obj = new GameObject($"Drop_{type}");
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = 5;

            float sprSize = 0.8f;
            if (sp != null)
            {
                float sw = sp.bounds.size.x;
                float sh = sp.bounds.size.y;
                obj.transform.localScale = new Vector3(
                    sw > 0 ? sprSize / sw : sprSize,
                    sh > 0 ? sprSize / sh : sprSize,
                    1f
                );
            }
            else
            {
                obj.transform.localScale = Vector3.one * sprSize;
            }

            // Random X within game area
            float xRange = _camWidth * 0.7f;
            float x = Random.Range(-xRange, xRange);
            obj.transform.position = new Vector3(x, _dropY, 0f);

            var drop = new ColorDrop { obj = obj, sr = sr, type = type, speed = _dropSpeed, isActive = true };
            _drops.Add(drop);
        }

        DropType ChooseDropType()
        {
            float r = Random.value;
            if (r < _bombChance) return DropType.Bomb;
            r -= _bombChance;
            if (r < _rainbowChance) return DropType.Rainbow;

            // Normal color
            int idx = Random.Range(0, _colorCount);
            return idx == 0 ? DropType.Red : idx == 1 ? DropType.Blue : DropType.Green;
        }

        Sprite GetSpriteForType(DropType type)
        {
            return type switch
            {
                DropType.Red => _spriteDropRed,
                DropType.Blue => _spriteDropBlue,
                DropType.Green => _spriteDropGreen,
                DropType.Rainbow => _spriteDropRainbow,
                DropType.Bomb => _spriteDropBomb,
                _ => _spriteDropRed
            };
        }

        void ShuffleBuckets()
        {
            if (_buckets.Count < 2) return;

            // Shuffle bucket colors
            for (int i = _bucketColors.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_bucketColors[i], _bucketColors[j]) = (_bucketColors[j], _bucketColors[i]);
            }

            // Update bucket sprites
            for (int i = 0; i < _buckets.Count; i++)
            {
                var sr = _buckets[i].GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = GetBucketSpriteForColor(_bucketColors[i]);
                StartCoroutine(BucketSlide(_buckets[i]));
            }
        }

        Sprite GetBucketSpriteForColor(DropType color)
        {
            return color switch
            {
                DropType.Red => _spriteBucketRed,
                DropType.Blue => _spriteBucketBlue,
                DropType.Green => _spriteBucketGreen,
                _ => _spriteBucketRed
            };
        }

        public void SetupStage(int stageIndex)
        {
            ClearAll();
            _processedCount = 0;
            _comboCount = 0;
            _spawnTimer = 0f;
            _shuffleTimer = 0f;

            switch (stageIndex)
            {
                case 0:
                    _colorCount = 2; _dropSpeed = 3.0f; _targetCount = 20;
                    _rainbowChance = 0f; _bombChance = 0f; _shuffleInterval = 0f;
                    _spawnInterval = 1.2f;
                    break;
                case 1:
                    _colorCount = 3; _dropSpeed = 4.0f; _targetCount = 30;
                    _rainbowChance = 0f; _bombChance = 0f; _shuffleInterval = 0f;
                    _spawnInterval = 1.1f;
                    break;
                case 2:
                    _colorCount = 3; _dropSpeed = 4.5f; _targetCount = 40;
                    _rainbowChance = 0.15f; _bombChance = 0f; _shuffleInterval = 0f;
                    _spawnInterval = 1.0f;
                    break;
                case 3:
                    _colorCount = 3; _dropSpeed = 5.5f; _targetCount = 50;
                    _rainbowChance = 0.15f; _bombChance = 0.10f; _shuffleInterval = 0f;
                    _spawnInterval = 0.9f;
                    break;
                case 4:
                    _colorCount = 3; _dropSpeed = 6.5f; _targetCount = 60;
                    _rainbowChance = 0.20f; _bombChance = 0.15f; _shuffleInterval = 8f;
                    _spawnInterval = 0.8f;
                    break;
                default:
                    _colorCount = 2; _dropSpeed = 3.0f; _targetCount = 20;
                    _rainbowChance = 0f; _bombChance = 0f; _shuffleInterval = 0f;
                    _spawnInterval = 1.2f;
                    break;
            }

            // Calculate layout
            _camSize = _mainCamera != null ? _mainCamera.orthographicSize : 5f;
            _camWidth = _mainCamera != null ? _camSize * _mainCamera.aspect : _camSize * 0.56f;
            _dropY = _camSize - 0.5f;
            _bucketY = -_camSize + 1.5f;
            _bucketW = Mathf.Min(1.4f, _camWidth * 2f / (_colorCount + 0.5f) * 0.85f);
            _bucketH = _bucketW * 0.75f;

            CreateBuckets();
            _isActive = true;
        }

        void CreateBuckets()
        {
            foreach (var b in _buckets) if (b != null) Destroy(b);
            _buckets.Clear();
            _bucketColors.Clear();

            DropType[] colors = _colorCount == 2
                ? new[] { DropType.Red, DropType.Blue }
                : new[] { DropType.Red, DropType.Blue, DropType.Green };

            float totalW = _camWidth * 2f;
            float spacing = totalW / _colorCount;
            float startX = -_camWidth + spacing * 0.5f;

            for (int i = 0; i < _colorCount; i++)
            {
                DropType color = colors[i];
                _bucketColors.Add(color);

                var obj = new GameObject($"Bucket_{color}");
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = GetBucketSpriteForColor(color);
                sr.sortingOrder = 3;

                if (sr.sprite != null)
                {
                    float sw = sr.sprite.bounds.size.x;
                    float sh = sr.sprite.bounds.size.y;
                    obj.transform.localScale = new Vector3(
                        sw > 0 ? _bucketW / sw : _bucketW,
                        sh > 0 ? _bucketH / sh : _bucketH,
                        1f
                    );
                }
                else
                {
                    obj.transform.localScale = new Vector3(_bucketW, _bucketH, 1f);
                }

                obj.transform.position = new Vector3(startX + spacing * i, _bucketY, 0f);
                _buckets.Add(obj);
            }
        }

        void ClearAll()
        {
            _isActive = false;
            StopAllCoroutines();
            foreach (var d in _drops) if (d.obj != null) Destroy(d.obj);
            _drops.Clear();
            foreach (var b in _buckets) if (b != null) Destroy(b);
            _buckets.Clear();
            _bucketColors.Clear();
        }

        IEnumerator FlyDropToBucket(ColorDrop drop, Vector3 targetPos)
        {
            if (drop.obj == null) yield break;
            Vector3 startPos = drop.obj.transform.position;
            Vector3 origScale = drop.obj.transform.localScale;
            float elapsed = 0f;
            float duration = 0.25f;
            while (elapsed < duration && drop.obj != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                drop.obj.transform.position = Vector3.Lerp(startPos, targetPos, t * t);
                drop.obj.transform.localScale = origScale * Mathf.Max(0.01f, 1f - t * 0.5f);
                yield return null;
            }
            if (drop.obj != null) Destroy(drop.obj);
        }

        IEnumerator FlyDropOff(ColorDrop drop)
        {
            if (drop.obj == null) yield break;
            float elapsed = 0f;
            Vector3 startPos = drop.obj.transform.position;
            while (elapsed < 0.4f && drop.obj != null)
            {
                elapsed += Time.deltaTime;
                drop.obj.transform.position = startPos + Vector3.down * elapsed * elapsed * 8f;
                var sr = drop.obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1, 1, 1, 1f - elapsed / 0.4f);
                yield return null;
            }
            if (drop.obj != null) Destroy(drop.obj);
        }

        IEnumerator FlashDrop(ColorDrop drop, Color flashColor)
        {
            if (drop.obj == null || drop.sr == null) yield break;
            Color orig = drop.sr.color;
            drop.sr.color = flashColor;
            yield return new WaitForSeconds(0.15f);
            if (drop.sr != null) drop.sr.color = orig;
        }

        IEnumerator BucketPulse(GameObject bucket)
        {
            if (bucket == null) yield break;
            float elapsed = 0f;
            float dur = 0.2f;
            Vector3 origScale = bucket.transform.localScale;
            while (elapsed < dur && bucket != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float s = 1f + 0.3f * Mathf.Sin(t * Mathf.PI);
                bucket.transform.localScale = origScale * s;
                yield return null;
            }
            if (bucket != null) bucket.transform.localScale = origScale;
        }

        IEnumerator BucketSlide(GameObject bucket)
        {
            if (bucket == null) yield break;
            Vector3 origPos = bucket.transform.position;
            float elapsed = 0f;
            while (elapsed < 0.3f && bucket != null)
            {
                elapsed += Time.deltaTime;
                float shake = Mathf.Sin(elapsed * 30f) * 0.1f * (1f - elapsed / 0.3f);
                bucket.transform.position = origPos + new Vector3(shake, 0f, 0f);
                yield return null;
            }
            if (bucket != null) bucket.transform.position = origPos;
        }

        IEnumerator CameraShake(float magnitude, float duration)
        {
            if (_mainCamera == null) yield break;
            Vector3 origPos = _mainCamera.transform.position;
            float elapsed = 0f;
            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float x = Random.Range(-magnitude, magnitude);
                    float y = Random.Range(-magnitude, magnitude);
                    _mainCamera.transform.position = origPos + new Vector3(x, y, 0f);
                    yield return null;
                }
            }
            finally
            {
                if (_mainCamera != null) _mainCamera.transform.position = origPos;
            }
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        void OnDestroy()
        {
            ClearAll();
        }
    }
}
