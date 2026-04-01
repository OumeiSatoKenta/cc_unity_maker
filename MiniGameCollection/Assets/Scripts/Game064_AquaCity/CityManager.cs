using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game064_AquaCity
{
    public class CityManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private AquaCityGameManager _gameManager;
        [SerializeField, Tooltip("建物スプライト")] private Sprite _buildingSprite;
        [SerializeField, Tooltip("魚スプライト")] private Sprite _fishSprite;
        [SerializeField, Tooltip("サンゴスプライト")] private Sprite _coralSprite;
        [SerializeField, Tooltip("建設コスト")] private int _buildCost = 10;

        private Camera _mainCamera;
        private bool _isActive;
        private List<GameObject> _buildings = new List<GameObject>();
        private List<GameObject> _fishes = new List<GameObject>();
        private float _incomeTimer;
        private float _fishSpawnTimer;
        private int _fishCount;

        private static readonly Color[] FishColors = {
            new Color(1f, 0.6f, 0.2f), new Color(0.3f, 0.7f, 1f),
            new Color(1f, 0.3f, 0.5f), new Color(0.4f, 1f, 0.6f),
            new Color(1f, 1f, 0.3f),
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _fishCount = 0;
            _incomeTimer = 0f;
            _fishSpawnTimer = 3f;

            // Place initial coral decorations
            for (int i = 0; i < 3; i++)
            {
                var obj = new GameObject($"Coral_{i}");
                obj.transform.position = new Vector3(Random.Range(-4f, 4f), Random.Range(-4f, -2f), 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _coralSprite; sr.sortingOrder = 1;
                obj.transform.localScale = Vector3.one * Random.Range(0.5f, 1f);
                sr.color = Color.HSVToRGB(Random.Range(0f, 1f), 0.4f, 1f);
            }
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            HandleInput();
            AnimateFish();

            // Auto income
            if (_buildings.Count > 0)
                _incomeTimer += Time.deltaTime;

            // Spawn fish based on building count
            _fishSpawnTimer -= Time.deltaTime;
            if (_fishSpawnTimer <= 0f && _fishes.Count < _buildings.Count * 2 + 1)
            {
                SpawnFish();
                _fishSpawnTimer = Mathf.Max(1f, 5f - _buildings.Count * 0.5f);
            }
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector3 mp = Mouse.current.position.ReadValue();
            mp.z = -_mainCamera.transform.position.z;
            Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

            // Check if tapped on existing building
            var hit = Physics2D.OverlapPoint(wp);
            if (hit != null && hit.gameObject.name.StartsWith("Building"))
            {
                _gameManager.OnBuildingTapped();
                // Visual feedback
                hit.transform.localScale = Vector3.one * 0.55f;
                return;
            }

            // Try to build
            if (_gameManager.TrySpend(_buildCost))
            {
                PlaceBuilding(wp);
            }
        }

        private void PlaceBuilding(Vector2 pos)
        {
            var obj = new GameObject($"Building_{_buildings.Count}");
            obj.transform.position = pos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _buildingSprite; sr.sortingOrder = 3;
            obj.transform.localScale = Vector3.one * 0.5f;
            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.45f, 0.45f);
            _buildings.Add(obj);
        }

        private void SpawnFish()
        {
            var obj = new GameObject($"Fish_{_fishCount}");
            float side = Random.value > 0.5f ? -6f : 6f;
            obj.transform.position = new Vector3(side, Random.Range(-3f, 3f), 0f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _fishSprite; sr.sortingOrder = 4;
            sr.color = FishColors[_fishCount % FishColors.Length];
            obj.transform.localScale = new Vector3(side > 0 ? -0.6f : 0.6f, 0.6f, 1f);

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(side > 0 ? -0.5f : 0.5f, Random.Range(-0.2f, 0.2f));

            _fishes.Add(obj);
            _fishCount++;
        }

        private void AnimateFish()
        {
            for (int i = _fishes.Count - 1; i >= 0; i--)
            {
                if (_fishes[i] == null) { _fishes.RemoveAt(i); continue; }
                // Remove fish that swam off screen
                if (Mathf.Abs(_fishes[i].transform.position.x) > 8f)
                {
                    Destroy(_fishes[i]);
                    _fishes.RemoveAt(i);
                }
                else
                {
                    // Gentle wave motion
                    var pos = _fishes[i].transform.position;
                    pos.y += Mathf.Sin(Time.time * 2f + i) * 0.002f;
                    _fishes[i].transform.position = pos;
                }
            }
        }

        public int Population => _buildings.Count * 5;
        public int FishCount => _fishCount;
        public int BuildingCount => _buildings.Count;
        public int NextBuildCost => _buildCost + _buildings.Count * 5;

        public int AutoIncome
        {
            get
            {
                if (_buildings.Count <= 0) return 0;
                if (_incomeTimer >= 2f)
                {
                    _incomeTimer -= 2f;
                    return _buildings.Count;
                }
                return 0;
            }
        }
    }
}
