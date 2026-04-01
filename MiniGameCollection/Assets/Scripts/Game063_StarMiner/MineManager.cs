using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game063_StarMiner
{
    public class MineManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private StarMinerGameManager _gameManager;
        [SerializeField, Tooltip("小惑星スプライト")] private Sprite _asteroidSprite;
        [SerializeField, Tooltip("鉱石スプライト群")] private Sprite[] _oreSprites;
        [SerializeField, Tooltip("ドリルスプライト")] private Sprite _drillSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int _drillLevel;
        private float _autoTimer;
        private List<GameObject> _asteroids = new List<GameObject>();

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _drillLevel = 0;
            _autoTimer = 0f;
            SpawnAsteroids();
        }

        public void StopGame() { _isActive = false; }

        private void SpawnAsteroids()
        {
            for (int i = 0; i < 5; i++)
            {
                float x = Random.Range(-3.5f, 3.5f);
                float y = Random.Range(-3f, 2f);
                var obj = new GameObject($"Asteroid_{i}");
                obj.transform.position = new Vector3(x, y, 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _asteroidSprite; sr.sortingOrder = 2;
                float scale = Random.Range(0.6f, 1.2f);
                obj.transform.localScale = Vector3.one * scale;
                var col = obj.AddComponent<CircleCollider2D>();
                col.radius = 0.3f;
                _asteroids.Add(obj);
            }
        }

        private void Update()
        {
            if (!_isActive) return;

            // Tap to mine
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                var hit = Physics2D.OverlapPoint(wp);
                if (hit != null)
                {
                    int yield = 1 + _drillLevel;
                    _gameManager.OnOreMined(yield);
                    SpawnOreEffect(hit.transform.position);

                    // Shake asteroid
                    hit.transform.position += (Vector3)Random.insideUnitCircle * 0.05f;
                }
            }

            // Auto mine
            if (_drillLevel > 0)
            {
                _autoTimer += Time.deltaTime;
            }
        }

        private void SpawnOreEffect(Vector2 pos)
        {
            if (_oreSprites == null || _oreSprites.Length == 0) return;
            var obj = new GameObject("OreFX");
            obj.transform.position = pos + Random.insideUnitCircle * 0.3f;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _oreSprites[Random.Range(0, _oreSprites.Length)];
            sr.sortingOrder = 10;
            obj.transform.localScale = Vector3.one * 0.3f;
            Destroy(obj, 0.4f);
        }

        public void UpgradeDrill()
        {
            if (_gameManager.TrySpend(NextDrillCost))
            {
                _drillLevel++;
            }
        }

        public int AutoMine
        {
            get
            {
                if (_drillLevel <= 0) return 0;
                if (_autoTimer >= 1f)
                {
                    _autoTimer -= 1f;
                    return _drillLevel;
                }
                return 0;
            }
        }

        public int DrillLevel => _drillLevel;
        public int NextDrillCost => 15 + _drillLevel * 10;
    }
}
