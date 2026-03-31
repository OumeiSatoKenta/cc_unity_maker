using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game026_SliceNinja
{
    public class SliceManager : MonoBehaviour
    {
        [SerializeField] private GameObject _fruitPrefab;
        [SerializeField] private GameObject _bombPrefab;
        [SerializeField] private float _spawnInterval = 1.0f;
        [SerializeField] private float _bombChance = 0.2f;

        private readonly List<FlyingObject> _objects = new List<FlyingObject>();
        private float _spawnTimer;
        private bool _isRunning;
        private bool _isDragging;
        private Vector2 _lastDragPos;

        private SliceNinjaGameManager _gameManager;
        private Camera _mainCamera;

        private Sprite _fruitSprite;
        private Sprite _bombSprite;

        private void Awake()
        {
            _gameManager = GetComponentInParent<SliceNinjaGameManager>();
            _mainCamera = Camera.main;
            _fruitSprite = Resources.Load<Sprite>("Sprites/Game026_SliceNinja/fruit");
            _bombSprite = Resources.Load<Sprite>("Sprites/Game026_SliceNinja/bomb");
        }

        private void Update()
        {
            if (!_isRunning) return;
            HandleInput();
            CleanupObjects();
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f) { SpawnObject(); _spawnTimer = _spawnInterval; }
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                Vector3 sp = mouse.position.ReadValue(); sp.z = -_mainCamera.transform.position.z;
                _lastDragPos = _mainCamera.ScreenToWorldPoint(sp);
            }

            if (_isDragging && mouse.leftButton.isPressed)
            {
                Vector3 sp = mouse.position.ReadValue(); sp.z = -_mainCamera.transform.position.z;
                Vector2 currentPos = _mainCamera.ScreenToWorldPoint(sp);

                if (Vector2.Distance(currentPos, _lastDragPos) > 0.2f)
                {
                    CheckSlice(_lastDragPos, currentPos);
                    _lastDragPos = currentPos;
                }
            }

            if (mouse.leftButton.wasReleasedThisFrame) _isDragging = false;
        }

        private void CheckSlice(Vector2 from, Vector2 to)
        {
            foreach (var obj in _objects)
            {
                if (obj == null || obj.IsSliced) continue;
                float dist = DistanceToSegment(obj.transform.position, from, to);
                if (dist < 0.5f)
                {
                    obj.Slice();
                    if (obj.IsBomb) { if (_gameManager != null) _gameManager.OnBombSliced(); }
                    else { if (_gameManager != null) _gameManager.OnFruitSliced(); }
                }
            }
        }

        private float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / ab.sqrMagnitude);
            Vector2 closest = a + t * ab;
            return Vector2.Distance(point, closest);
        }

        private void SpawnObject()
        {
            bool isBomb = Random.value < _bombChance;
            var prefab = isBomb ? _bombPrefab : _fruitPrefab;
            if (prefab == null) return;

            var obj = Instantiate(prefab, transform);
            float x = Random.Range(-4f, 4f);
            obj.transform.position = new Vector3(x, -6f, 0f);

            float vx = Random.Range(-1f, 1f);
            float vy = Random.Range(8f, 12f);

            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = isBomb ? _bombSprite : _fruitSprite;

            var fo = obj.GetComponent<FlyingObject>();
            if (fo != null) fo.Initialize(isBomb, new Vector2(vx, vy));
            _objects.Add(fo);
        }

        private void CleanupObjects()
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                if (_objects[i] == null) { _objects.RemoveAt(i); continue; }
                if (_objects[i].transform.position.y < -7f)
                {
                    if (!_objects[i].IsSliced && !_objects[i].IsBomb)
                        if (_gameManager != null) _gameManager.OnFruitMissed();
                    Destroy(_objects[i].gameObject);
                    _objects.RemoveAt(i);
                }
            }
        }

        public void StartGame() { ClearAll(); _spawnTimer = 0.5f; _isRunning = true; }
        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var o in _objects) if (o != null) Destroy(o.gameObject);
            _objects.Clear();
        }
    }
}
