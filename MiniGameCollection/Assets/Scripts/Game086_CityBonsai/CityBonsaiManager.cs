using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game086_CityBonsai
{
    public class CityBonsaiManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private CityBonsaiGameManager _gameManager;
        [SerializeField, Tooltip("建物スプライト")] private Sprite _buildingSprite;
        [SerializeField, Tooltip("花スプライト")] private Sprite _flowerSprite;
        [SerializeField, Tooltip("盆栽ベーススプライト")] private Sprite _bonsaiBaseSprite;
        [SerializeField, Tooltip("建設コスト")] private int _buildCost = 8;

        private Camera _mainCamera;
        private bool _isActive;
        private List<GameObject> _buildings = new List<GameObject>();
        private List<GameObject> _flowers = new List<GameObject>();
        private float _incomeTimer;
        private float _flowerTimer;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _incomeTimer = 0f; _flowerTimer = 0f;

            var baseObj = new GameObject("BonsaiBase");
            baseObj.transform.position = new Vector3(0f, -2f, 0f);
            var sr = baseObj.AddComponent<SpriteRenderer>();
            sr.sprite = _bonsaiBaseSprite; sr.sortingOrder = 1;
            baseObj.transform.localScale = Vector3.one * 1.5f;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            if (_buildings.Count > 0) _incomeTimer += Time.deltaTime;

            // Flowers bloom based on building count
            _flowerTimer += Time.deltaTime;
            if (_flowerTimer >= 5f && _buildings.Count > _flowers.Count)
            {
                _flowerTimer = 0f;
                SpawnFlower();
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                if (wp.y < 1f && wp.y > -3f)
                {
                    Build(wp);
                }
            }
        }

        private void Build(Vector2 pos)
        {
            if (_gameManager.TrySpend(_buildCost + _buildings.Count * 3))
            {
                var obj = new GameObject($"Building_{_buildings.Count}");
                obj.transform.position = pos;
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _buildingSprite; sr.sortingOrder = 3;
                float scale = Random.Range(0.3f, 0.5f);
                obj.transform.localScale = Vector3.one * scale;
                sr.color = Color.HSVToRGB(Random.Range(0f, 1f), 0.2f, 0.9f);
                _buildings.Add(obj);
            }
        }

        private void SpawnFlower()
        {
            var obj = new GameObject($"Flower_{_flowers.Count}");
            float x = Random.Range(-2.5f, 2.5f);
            float y = Random.Range(-1.5f, 0.5f);
            obj.transform.position = new Vector3(x, y, 0f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _flowerSprite; sr.sortingOrder = 4;
            obj.transform.localScale = Vector3.one * Random.Range(0.2f, 0.4f);
            sr.color = Color.HSVToRGB(Random.Range(0f, 1f), 0.4f, 1f);
            _flowers.Add(obj);
        }

        public int AutoIncome
        {
            get
            {
                if (_buildings.Count <= 0) return 0;
                if (_incomeTimer >= 2f) { _incomeTimer -= 2f; return _buildings.Count; }
                return 0;
            }
        }

        public int Population => _buildings.Count * 3 + _flowers.Count;
        public int BuildingCount => _buildings.Count;
    }
}
