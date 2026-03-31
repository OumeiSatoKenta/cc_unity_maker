using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game034_DropZone
{
    public class DropManager : MonoBehaviour
    {
        [SerializeField] private GameObject _itemPrefab;
        [SerializeField] private float _spawnInterval = 1.2f;
        [SerializeField] private float _gameTime = 30f;

        private readonly List<DropItem> _items = new List<DropItem>();
        private DropItem _heldItem;
        private float _spawnTimer;
        private float _timeRemaining;
        private bool _isRunning;

        // Zone positions: left=-3, center=0, right=3 at y=-3.5
        private readonly float[] _zoneX = { -3f, 0f, 3f };
        private static readonly string[] ColorNames = { "red", "green", "blue" };

        private DropZoneGameManager _gameManager;
        private Camera _mainCamera;

        private void Awake()
        {
            _gameManager = GetComponentInParent<DropZoneGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isRunning) return;
            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f) { _isRunning = false; if (_gameManager != null) _gameManager.OnTimeUp(); return; }
            if (_gameManager != null) _gameManager.OnTimeUpdate(_timeRemaining);

            HandleInput();
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f) { SpawnItem(); _spawnTimer = Mathf.Max(_spawnInterval - (_gameTime - _timeRemaining) * 0.01f, 0.4f); }
            CleanupItems();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            Vector3 sp = mouse.position.ReadValue();
            sp.z = -_mainCamera.transform.position.z;
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);

            if (mouse.leftButton.wasPressedThisFrame)
            {
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var item = hit.GetComponent<DropItem>();
                    if (item != null && !item.IsCaught) { _heldItem = item; _heldItem.IsCaught = true; }
                }
            }

            if (_heldItem != null && mouse.leftButton.isPressed)
                _heldItem.transform.position = new Vector3(worldPos.x, worldPos.y, 0);

            if (mouse.leftButton.wasReleasedThisFrame && _heldItem != null)
            {
                // Check which zone it's dropped in
                bool sorted = false;
                for (int z = 0; z < 3; z++)
                {
                    if (Mathf.Abs(_heldItem.transform.position.x - _zoneX[z]) < 1.5f &&
                        _heldItem.transform.position.y < -2.5f)
                    {
                        if (_heldItem.ColorIndex == z)
                        {
                            if (_gameManager != null) _gameManager.OnCorrectSort();
                            sorted = true;
                        }
                        else
                        {
                            if (_gameManager != null) _gameManager.OnWrongSort();
                        }
                        Destroy(_heldItem.gameObject);
                        _items.Remove(_heldItem);
                        break;
                    }
                }
                if (!sorted && _heldItem != null) _heldItem.IsCaught = false;
                _heldItem = null;
            }
        }

        private void SpawnItem()
        {
            if (_itemPrefab == null) return;
            int colorIdx = Random.Range(0, 3);
            var obj = Instantiate(_itemPrefab, transform);
            obj.transform.position = new Vector3(Random.Range(-4f, 4f), 5.5f, 0);

            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var sprite = Resources.Load<Sprite>($"Sprites/Game034_DropZone/item_{ColorNames[colorIdx]}");
                if (sprite != null) sr.sprite = sprite;
            }

            var item = obj.GetComponent<DropItem>();
            if (item != null) item.Initialize(colorIdx);
            _items.Add(item);
        }

        private void CleanupItems()
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i] == null) { _items.RemoveAt(i); continue; }
                if (_items[i].transform.position.y < -6f)
                {
                    if (!_items[i].IsCaught && _gameManager != null) _gameManager.OnItemMissed();
                    Destroy(_items[i].gameObject); _items.RemoveAt(i);
                }
            }
        }

        public void StartGame()
        {
            ClearAll(); _spawnTimer = 0.5f; _timeRemaining = _gameTime; _isRunning = true;
        }

        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var i in _items) if (i != null) Destroy(i.gameObject);
            _items.Clear(); _heldItem = null;
        }
    }
}
