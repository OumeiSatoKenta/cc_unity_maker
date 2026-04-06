using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game054v2_FruitSlash
{
    public class FruitManager : MonoBehaviour
    {
        [SerializeField, Tooltip("フルーツスプライト配列 (Apple, Watermelon, Gold, Ice, Bomb, BigBomb)")] private Sprite[] _fruitSprites;

        private FruitSlashGameManager _gameManager;
        private bool _isSpawning;
        private float _spawnInterval = 1.2f;
        private float _speedMultiplier = 1.0f;
        private int _maxSimultaneous = 1;
        private float _bombRatio = 0f;
        private bool _hasGold = false;
        private bool _hasIce = false;
        private bool _hasBigBomb = false;

        private readonly List<FruitObject> _activeFruits = new List<FruitObject>();
        private Coroutine _spawnCoroutine;

        private float _camSize;
        private float _camWidth;
        private float _spawnY;

        public void Initialize(FruitSlashGameManager gameManager)
        {
            _gameManager = gameManager;
            var cam = Camera.main;
            _camSize = cam.orthographicSize;
            _camWidth = _camSize * cam.aspect;
            _spawnY = -_camSize - 0.5f;
        }

        public void SetupStage(StageManager.StageConfig config)
        {
            StopSpawning();
            ClearAllFruits();

            _speedMultiplier = config.speedMultiplier;
            _maxSimultaneous = config.countMultiplier;
            float complexity = config.complexityFactor;

            _bombRatio = complexity >= 0.1f ? Mathf.Lerp(0.05f, 0.25f, (complexity - 0.1f) / 0.9f) : 0f;
            _hasGold = complexity >= 0.4f;
            _hasIce = complexity >= 0.7f;
            _hasBigBomb = complexity >= 1.0f;
            _spawnInterval = Mathf.Lerp(1.2f, 0.4f, (complexity));
        }

        public void StartSpawning()
        {
            _isSpawning = true;
            _spawnCoroutine = StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
        {
            _isSpawning = false;
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        public void ClearAllFruits()
        {
            foreach (var f in _activeFruits)
                if (f != null) Destroy(f.gameObject);
            _activeFruits.Clear();
        }

        private IEnumerator SpawnLoop()
        {
            while (_isSpawning)
            {
                int count = Random.Range(1, _maxSimultaneous + 1);
                for (int i = 0; i < count; i++)
                    SpawnOne();

                yield return new WaitForSeconds(_spawnInterval / _speedMultiplier);
            }
        }

        private void SpawnOne()
        {
            FruitType type = DetermineFruitType();
            GameObject go = new GameObject("Fruit_" + type);
            go.transform.position = new Vector3(
                Random.Range(-_camWidth * 0.8f, _camWidth * 0.8f),
                _spawnY, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            int spriteIdx = (int)type;
            if (_fruitSprites != null && spriteIdx < _fruitSprites.Length)
                sr.sprite = _fruitSprites[spriteIdx];
            sr.sortingOrder = 5;

            float scale = type switch
            {
                FruitType.Gold => 0.5f,
                FruitType.BigBomb => 1.4f,
                _ => 1.0f
            };
            go.transform.localScale = Vector3.one * scale;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = type == FruitType.BigBomb ? 0.9f : 0.5f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1.5f;

            float launchSpeed = (5f + Random.Range(0f, 1.5f)) * _speedMultiplier;
            float angle = Random.Range(60f, 120f);
            float vx = Mathf.Cos(angle * Mathf.Deg2Rad) * launchSpeed;
            float vy = Mathf.Sin(angle * Mathf.Deg2Rad) * launchSpeed;

            var fruit = go.AddComponent<FruitObject>();
            fruit.Initialize(type, new Vector2(vx, vy));
            _activeFruits.Add(fruit);
        }

        private FruitType DetermineFruitType()
        {
            float roll = Random.value;
            if (roll < _bombRatio)
                return _hasBigBomb && Random.value < 0.3f ? FruitType.BigBomb : FruitType.Bomb;

            float fruitRoll = Random.value;
            if (_hasIce && fruitRoll < 0.15f) return FruitType.Ice;
            if (_hasGold && fruitRoll < 0.25f) return FruitType.Gold;
            return Random.value < 0.4f ? FruitType.Apple : FruitType.Watermelon;
        }

        public void CheckSlash(Vector2 worldStart, Vector2 worldEnd)
        {
            if (_gameManager == null) return;

            int slicedCount = 0;
            var toRemove = new List<FruitObject>();

            foreach (var fruit in _activeFruits)
            {
                if (fruit == null || fruit.IsSliced) continue;
                Vector2 pos = fruit.transform.position;
                float radius = fruit.GetComponent<CircleCollider2D>()?.radius * fruit.transform.localScale.x ?? 0.5f;

                if (SegmentCircleIntersects(worldStart, worldEnd, pos, radius))
                {
                    fruit.Slice();
                    toRemove.Add(fruit);
                    slicedCount++;
                    PlaySliceEffect(fruit);

                    if (fruit.IsBomb)
                        _gameManager.OnBombCut();
                    else if (fruit.Type == FruitType.Ice)
                        _gameManager.OnIceFruitCut(fruit.Score);
                    else
                        _gameManager.OnFruitCut(fruit.Score, false);
                }
            }

            foreach (var f in toRemove)
            {
                _activeFruits.Remove(f);
                if (f != null) Destroy(f.gameObject, 0.3f);
            }

            if (slicedCount >= 3)
                _gameManager.OnMultiSlash(slicedCount);
        }

        private bool SegmentCircleIntersects(Vector2 a, Vector2 b, Vector2 center, float radius)
        {
            Vector2 ab = b - a;
            float sqLen = ab.sqrMagnitude;
            if (sqLen < 1e-6f)
                return (center - a).sqrMagnitude <= radius * radius;
            Vector2 ac = center - a;
            float t = Mathf.Clamp01(Vector2.Dot(ac, ab) / sqLen);
            Vector2 closest = a + t * ab;
            return (center - closest).sqrMagnitude <= radius * radius;
        }

        private void PlaySliceEffect(FruitObject fruit)
        {
            StartCoroutine(SliceAnim(fruit.transform));
        }

        private IEnumerator SliceAnim(Transform t)
        {
            if (t == null) yield break;
            float elapsed = 0f;
            Vector3 origScale = t.localScale;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / 0.3f;
                if (t == null) yield break;
                t.localScale = origScale * (1f - ratio);
                yield return null;
            }
        }

        public void CheckMissedFruits()
        {
            float bottomY = -Camera.main.orthographicSize - 1f;
            var toRemove = new List<FruitObject>();
            foreach (var f in _activeFruits)
            {
                if (f == null) { toRemove.Add(f); continue; }
                if (!f.IsBomb && f.transform.position.y < bottomY)
                {
                    toRemove.Add(f);
                    _gameManager.OnFruitMissed();
                    Destroy(f.gameObject);
                }
            }
            foreach (var f in toRemove) _activeFruits.Remove(f);
        }

        private void Update()
        {
            if (!_isSpawning) return;
            CheckMissedFruits();
        }
    }
}
