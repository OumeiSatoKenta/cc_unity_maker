using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game054_FruitSlash
{
    public class FruitManager : MonoBehaviour
    {
        [SerializeField] private FruitSlashGameManager _gameManager;

        private const float SpawnInterval = 0.8f;
        private const float BombChance = 0.15f;

        private Sprite[] _fruitSprites;
        private Sprite _bombSprite;
        private Camera _mainCamera;
        private List<GameObject> _items = new List<GameObject>();
        private List<Vector2> _velocities = new List<Vector2>();
        private List<bool> _isBomb = new List<bool>();
        private List<bool> _slashed = new List<bool>();
        private float _spawnTimer;
        private bool _spawning;
        private Vector3 _lastMousePos;
        private bool _wasDragging;

        public void Init()
        {
            _mainCamera = Camera.main;
            _fruitSprites = new Sprite[] {
                Resources.Load<Sprite>("Sprites/Game054_FruitSlash/fruit_apple"),
                Resources.Load<Sprite>("Sprites/Game054_FruitSlash/fruit_banana"),
                Resources.Load<Sprite>("Sprites/Game054_FruitSlash/fruit_orange"),
            };
            _bombSprite = Resources.Load<Sprite>("Sprites/Game054_FruitSlash/bomb");

            foreach (var i in _items) if (i != null) Destroy(i);
            _items.Clear(); _velocities.Clear(); _isBomb.Clear(); _slashed.Clear();
            _spawnTimer = 0f; _spawning = true; _wasDragging = false;
        }

        public void StopSpawning() { _spawning = false; }

        private void Update()
        {
            if (_gameManager != null && _gameManager.IsGameOver) return;

            if (_spawning)
            {
                _spawnTimer += Time.deltaTime;
                if (_spawnTimer >= SpawnInterval) { _spawnTimer = 0f; SpawnItem(); }
            }

            HandleSlash();
            UpdateItems();
        }

        private void SpawnItem()
        {
            float x = Random.Range(-3f, 3f);
            float vx = Random.Range(-1.5f, 1.5f);
            float vy = Random.Range(8f, 12f);
            bool bomb = Random.value < BombChance;

            var go = new GameObject(bomb ? "Bomb" : "Fruit");
            go.transform.position = new Vector3(x, -6f, 0f);
            go.transform.localScale = Vector3.one * 1.2f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = bomb ? _bombSprite : _fruitSprites[Random.Range(0, _fruitSprites.Length)];
            sr.sortingOrder = 5;

            _items.Add(go); _velocities.Add(new Vector2(vx, vy));
            _isBomb.Add(bomb); _slashed.Add(false);
        }

        private void HandleSlash()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.isPressed)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
                wp.z = 0f;

                if (_wasDragging)
                {
                    float slashDist = Vector3.Distance(wp, _lastMousePos);
                    if (slashDist > 0.3f)
                    {
                        for (int i = 0; i < _items.Count; i++)
                        {
                            if (_items[i] == null || _slashed[i]) continue;
                            float d = DistPointToSegment(_items[i].transform.position, _lastMousePos, wp);
                            if (d < 0.8f)
                            {
                                _slashed[i] = true;
                                if (_isBomb[i])
                                {
                                    if (_gameManager != null) _gameManager.OnBombHit();
                                }
                                else
                                {
                                    if (_gameManager != null) _gameManager.OnFruitSlashed();
                                    var sr = _items[i].GetComponent<SpriteRenderer>();
                                    sr.color = new Color(1f, 1f, 1f, 0.3f);
                                }
                            }
                        }
                    }
                }
                _lastMousePos = wp;
                _wasDragging = true;
            }
            else
            {
                _wasDragging = false;
            }
        }

        private float DistPointToSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
            Vector3 closest = a + ab * t;
            return Vector3.Distance(p, closest);
        }

        private void UpdateItems()
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i] == null) { RemoveAt(i); continue; }

                var v = _velocities[i];
                v.y -= 15f * Time.deltaTime;
                _velocities[i] = v;
                _items[i].transform.position += (Vector3)v * Time.deltaTime;
                _items[i].transform.Rotate(0, 0, 200f * Time.deltaTime);

                if (_items[i].transform.position.y < -7f)
                {
                    if (!_slashed[i] && !_isBomb[i])
                    {
                        if (_gameManager != null) _gameManager.OnFruitMissed();
                    }
                    Destroy(_items[i]); RemoveAt(i);
                }
            }
        }

        private void RemoveAt(int i)
        {
            _items.RemoveAt(i); _velocities.RemoveAt(i);
            _isBomb.RemoveAt(i); _slashed.RemoveAt(i);
        }
    }
}
