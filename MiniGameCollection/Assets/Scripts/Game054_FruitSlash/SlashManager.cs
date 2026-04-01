using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game054_FruitSlash
{
    public class SlashManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private FruitSlashGameManager _gameManager;
        [SerializeField, Tooltip("フルーツスプライト群")] private Sprite[] _fruitSprites;
        [SerializeField, Tooltip("爆弾スプライト")] private Sprite _bombSprite;
        [SerializeField, Tooltip("スポーン間隔")] private float _spawnInterval = 1.2f;
        [SerializeField, Tooltip("爆弾確率")] private float _bombChance = 0.15f;

        private Camera _mainCamera;
        private bool _isActive;
        private float _spawnTimer;
        private List<FruitItem> _items = new List<FruitItem>();
        private bool _isDragging;
        private Vector2 _lastDragPos;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _spawnTimer = 0.5f;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnItem();
                _spawnTimer = _spawnInterval;
            }

            HandleInput();
            CleanupItems();
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _lastDragPos = GetWorldMousePos();
            }

            if (Mouse.current.leftButton.isPressed && _isDragging)
            {
                Vector2 currentPos = GetWorldMousePos();
                if (Vector2.Distance(currentPos, _lastDragPos) > 0.2f)
                {
                    CheckSlash(_lastDragPos, currentPos);
                    _lastDragPos = currentPos;
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }
        }

        private void CheckSlash(Vector2 from, Vector2 to)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                if (item == null || item.IsSlashed) continue;

                float dist = DistanceToSegment(item.transform.position, from, to);
                if (dist < 0.6f)
                {
                    item.Slash();
                    if (item.IsBomb)
                        _gameManager.OnBombHit();
                    else
                        _gameManager.OnFruitSlashed();
                    _items.RemoveAt(i);
                }
            }
        }

        private float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            Vector2 ap = p - a;
            float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / Vector2.Dot(ab, ab));
            Vector2 closest = a + t * ab;
            return Vector2.Distance(p, closest);
        }

        private void SpawnItem()
        {
            bool isBomb = Random.value < _bombChance;
            var obj = new GameObject(isBomb ? "Bomb" : "Fruit");

            float x = Random.Range(-3f, 3f);
            obj.transform.position = new Vector3(x, -6f, 0f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            if (isBomb)
            {
                sr.sprite = _bombSprite;
            }
            else
            {
                sr.sprite = _fruitSprites[Random.Range(0, _fruitSprites.Length)];
            }

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0.5f;
            float vx = Random.Range(-1f, 1f);
            float vy = Random.Range(8f, 12f);
            rb.linearVelocity = new Vector2(vx, vy);
            rb.angularVelocity = Random.Range(-180f, 180f);

            var item = obj.AddComponent<FruitItem>();
            item.Initialize(isBomb, OnItemFellOff);
            _items.Add(item);
        }

        private void OnItemFellOff(FruitItem item)
        {
            if (!item.IsBomb && !item.IsSlashed)
                _gameManager.OnFruitMissed();
            _items.Remove(item);
        }

        private void CleanupItems()
        {
            _items.RemoveAll(i => i == null);
        }

        private Vector2 GetWorldMousePos()
        {
            Vector3 mp = Mouse.current.position.ReadValue();
            mp.z = -_mainCamera.transform.position.z;
            return _mainCamera.ScreenToWorldPoint(mp);
        }
    }
}
