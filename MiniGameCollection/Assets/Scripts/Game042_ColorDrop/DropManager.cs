using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game042_ColorDrop
{
    public class DropManager : MonoBehaviour
    {
        [SerializeField] private ColorDropGameManager _gameManager;

        private static readonly Color[] DropColors = {
            new Color(1f, 0.3f, 0.3f), // Red
            new Color(0.3f, 0.6f, 1f), // Blue
            new Color(0.3f, 1f, 0.4f), // Green
        };

        private const float SpawnInterval = 1.2f;
        private const float MinInterval = 0.4f;
        private const float SpeedIncrease = 0.02f;
        private const float BaseDropSpeed = 3f;
        private const float BucketY = -3.5f;

        private List<GameObject> _buckets = new List<GameObject>();
        private List<GameObject> _drops = new List<GameObject>();
        private Sprite _dropSprite;
        private Sprite _bucketSprite;
        private float _spawnTimer;
        private float _currentInterval;
        private float _currentSpeed;
        private bool _spawning;
        private Camera _mainCamera;
        private bool _isDragging;
        private GameObject _draggedDrop;
        private Vector3 _dragOffset;

        public void Init()
        {
            _mainCamera = Camera.main;
            _dropSprite = Resources.Load<Sprite>("Sprites/Game042_ColorDrop/raindrop");
            _bucketSprite = Resources.Load<Sprite>("Sprites/Game042_ColorDrop/bucket");

            foreach (var d in _drops) if (d != null) Destroy(d);
            _drops.Clear();
            foreach (var b in _buckets) if (b != null) Destroy(b);
            _buckets.Clear();

            float[] bucketXPositions = { -2.5f, 0f, 2.5f };
            for (int i = 0; i < 3; i++)
            {
                var bucket = new GameObject("Bucket_" + i);
                bucket.transform.position = new Vector3(bucketXPositions[i], BucketY, 0f);
                bucket.transform.localScale = new Vector3(1.5f, 1f, 1f);
                var sr = bucket.AddComponent<SpriteRenderer>();
                sr.sprite = _bucketSprite;
                sr.color = DropColors[i];
                sr.sortingOrder = 3;
                var col = bucket.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 0.8f);
                col.isTrigger = true;
                var bi = bucket.AddComponent<BucketInfo>();
                bi.ColorIndex = i;
                _buckets.Add(bucket);
            }

            _spawnTimer = 0f;
            _currentInterval = SpawnInterval;
            _currentSpeed = BaseDropSpeed;
            _spawning = true;
            _isDragging = false;
            _draggedDrop = null;
        }

        public void StopSpawning()
        {
            _spawning = false;
        }

        private void Update()
        {
            if (_gameManager != null && _gameManager.IsGameOver) return;

            if (_spawning)
            {
                _spawnTimer += Time.deltaTime;
                if (_spawnTimer >= _currentInterval)
                {
                    _spawnTimer = 0f;
                    SpawnDrop();
                    _currentInterval = Mathf.Max(MinInterval, _currentInterval - SpeedIncrease);
                    _currentSpeed += SpeedIncrease * 2f;
                }
            }

            HandleInput();
            UpdateDrops();
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));

                foreach (var drop in _drops)
                {
                    if (drop == null) continue;
                    if (Vector2.Distance(wp, drop.transform.position) < 0.8f)
                    {
                        _isDragging = true;
                        _draggedDrop = drop;
                        _dragOffset = drop.transform.position - wp;
                        break;
                    }
                }
            }

            if (_isDragging && _draggedDrop != null)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
                _draggedDrop.transform.position = new Vector3(wp.x + _dragOffset.x, _draggedDrop.transform.position.y, 0f);
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
                _draggedDrop = null;
            }
        }

        private void SpawnDrop()
        {
            int colorIdx = Random.Range(0, DropColors.Length);
            float x = Random.Range(-3f, 3f);
            var drop = new GameObject("Drop");
            drop.transform.position = new Vector3(x, 5.5f, 0f);
            drop.transform.localScale = Vector3.one * 0.8f;
            var sr = drop.AddComponent<SpriteRenderer>();
            sr.sprite = _dropSprite;
            sr.color = DropColors[colorIdx];
            sr.sortingOrder = 5;
            var col = drop.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;
            var di = drop.AddComponent<DropInfo>();
            di.ColorIndex = colorIdx;
            di.FallSpeed = _currentSpeed;
            _drops.Add(drop);
        }

        private void UpdateDrops()
        {
            for (int i = _drops.Count - 1; i >= 0; i--)
            {
                var drop = _drops[i];
                if (drop == null) { _drops.RemoveAt(i); continue; }

                var di = drop.GetComponent<DropInfo>();
                drop.transform.position += Vector3.down * di.FallSpeed * Time.deltaTime;

                if (drop.transform.position.y <= BucketY + 0.3f)
                {
                    bool caught = false;
                    foreach (var bucket in _buckets)
                    {
                        if (bucket == null) continue;
                        if (Mathf.Abs(drop.transform.position.x - bucket.transform.position.x) < 1f)
                        {
                            var bi = bucket.GetComponent<BucketInfo>();
                            if (bi.ColorIndex == di.ColorIndex)
                            {
                                if (_gameManager != null) _gameManager.OnCorrectCatch();
                            }
                            else
                            {
                                if (_gameManager != null) _gameManager.OnMiss();
                            }
                            caught = true;
                            break;
                        }
                    }
                    if (!caught && _gameManager != null) _gameManager.OnMiss();
                    Destroy(drop);
                    _drops.RemoveAt(i);
                }
                else if (drop.transform.position.y < -6f)
                {
                    Destroy(drop);
                    _drops.RemoveAt(i);
                }
            }
        }
    }

    public class DropInfo : MonoBehaviour
    {
        public int ColorIndex;
        public float FallSpeed;
    }

    public class BucketInfo : MonoBehaviour
    {
        public int ColorIndex;
    }
}
