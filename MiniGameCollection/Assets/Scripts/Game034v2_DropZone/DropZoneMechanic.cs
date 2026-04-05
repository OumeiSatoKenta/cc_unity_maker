using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game034v2_DropZone
{
    public class DropZoneMechanic : MonoBehaviour
    {
        [SerializeField] DropZoneGameManager _gameManager;

        // Sprites assigned by SceneSetup
        [SerializeField] Sprite _spriteFruit;
        [SerializeField] Sprite _spriteTrash;
        [SerializeField] Sprite _spriteRecycle;
        [SerializeField] Sprite _spriteTrickyFruit;
        [SerializeField] Sprite _spriteTrickyTrash;
        [SerializeField] Sprite _spriteBonus;

        // Zone sprites / renderers assigned by SceneSetup
        [SerializeField] Transform[] _zones; // 4 zones
        [SerializeField] SpriteRenderer[] _zoneRenderers;

        bool _isActive;
        int _zoneCount;
        bool _dualDrop;
        float _fallSpeed;
        float _complexityFactor;
        int _totalItemsForStage;
        int _itemsProcessed;
        float _spawnInterval;
        float _spawnTimer;
        int _activeDropCount; // items currently on screen

        FallingItem _dragItem;
        List<FallingItem> _spawnedItems = new List<FallingItem>();

        // Zone mapping: which ItemType goes to which zone index
        // Zone 0 = Fruit, Zone 1 = Trash, Zone 2 = Recycle, Zone 3 = Bonus
        ItemType[] _zoneItemTypes = { ItemType.Fruit, ItemType.Trash, ItemType.Recycle, ItemType.Bonus };

        // Sprites lookup
        Sprite GetSpriteForType(ItemType t) => t switch
        {
            ItemType.Fruit       => _spriteFruit,
            ItemType.Trash       => _spriteTrash,
            ItemType.Recycle     => _spriteRecycle,
            ItemType.TrickyFruit => _spriteTrickyFruit,
            ItemType.TrickyTrash => _spriteTrickyTrash,
            ItemType.Bonus       => _spriteBonus,
            _ => _spriteFruit
        };

        // True zone for tricky items
        ItemType TrueType(ItemType t) => t switch
        {
            ItemType.TrickyFruit => ItemType.Fruit,
            ItemType.TrickyTrash => ItemType.Trash,
            _ => t
        };

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            ClearItems();

            _zoneCount = stageIndex switch { 0 => 2, 1 => 3, 2 => 3, 3 => 4, _ => 4 };
            _dualDrop = stageIndex >= 4;
            _fallSpeed = 1.5f * config.speedMultiplier;
            _complexityFactor = config.complexityFactor;
            _totalItemsForStage = 8 + stageIndex * 2;
            _itemsProcessed = 0;
            _activeDropCount = 0;
            _spawnInterval = Mathf.Max(0.4f, 2.5f / config.speedMultiplier);
            _spawnTimer = 0.5f;

            // Show/hide zones based on zoneCount
            for (int i = 0; i < _zones.Length; i++)
                if (_zones[i] != null) _zones[i].gameObject.SetActive(i < _zoneCount);

            // Position zones responsively
            var cam = Camera.main;
            if (cam != null)
            {
                float camSize = cam.orthographicSize;
                float camW = camSize * cam.aspect;
                float zoneY = -camSize + 1.8f;
                float zoneSpacing = (camW * 2f) / _zoneCount;
                for (int i = 0; i < _zoneCount; i++)
                {
                    if (_zones[i] == null) continue;
                    float x = -camW + zoneSpacing * (i + 0.5f);
                    _zones[i].position = new Vector3(x, zoneY, 0f);
                }
            }

            _isActive = true;
        }

        public void Deactivate()
        {
            _isActive = false;
            _dragItem = null;
            ClearItems();
        }

        void ClearItems()
        {
            foreach (var item in _spawnedItems)
                if (item != null) item.Deactivate();
            _spawnedItems.Clear();
            _dragItem = null;
            _activeDropCount = 0;
        }

        void Update()
        {
            if (!_isActive) return;

            HandleInput();
            HandleSpawn();
        }

        void HandleSpawn()
        {
            if (_itemsProcessed >= _totalItemsForStage) return;
            // Limit active drops: 1 normally, 2 in dualDrop
            int maxActive = _dualDrop ? 2 : 1;
            if (_activeDropCount >= maxActive) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f) return;

            _spawnTimer = _spawnInterval;

            int needed = _dualDrop ? Mathf.Min(2 - _activeDropCount, _totalItemsForStage - _itemsProcessed) : 1;
            for (int i = 0; i < needed; i++)
                SpawnItem(i);
        }

        void SpawnItem(int offsetIndex)
        {
            var cam = Camera.main;
            if (cam == null) return;

            float camSize = cam.orthographicSize;
            float camW = camSize * cam.aspect;
            float spawnY = camSize - 0.5f;
            float spawnX = Random.Range(-camW + 0.5f, camW - 0.5f);

            // Avoid overlap in dual drop
            if (offsetIndex > 0) spawnX = Mathf.Clamp(spawnX + (offsetIndex % 2 == 0 ? 1f : -1f) * camW * 0.4f, -camW + 0.5f, camW - 0.5f);

            ItemType type = PickRandomItemType();
            var go = new GameObject("FallingItem");
            go.transform.position = new Vector3(spawnX, spawnY, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;

            var item = go.AddComponent<FallingItem>();
            item.Initialize(type, GetSpriteForType(type), _fallSpeed, this);

            // Scale to ~0.9 units
            if (sr.sprite != null)
            {
                float sprW = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
                float targetSize = 0.9f;
                float s = targetSize / sprW;
                go.transform.localScale = Vector3.one * s;
            }

            _spawnedItems.Add(item);
            _activeDropCount++;
        }

        ItemType PickRandomItemType()
        {
            // Stage-appropriate items
            List<ItemType> pool = new List<ItemType> { ItemType.Fruit, ItemType.Trash };
            if (_zoneCount >= 3) pool.Add(ItemType.Recycle);
            if (_zoneCount >= 4) pool.Add(ItemType.Bonus);
            // Add tricky items based on complexityFactor
            if (_complexityFactor > 0f && Random.value < _complexityFactor)
            {
                return Random.value < 0.5f ? ItemType.TrickyFruit : ItemType.TrickyTrash;
            }
            return pool[Random.Range(0, pool.Count)];
        }

        void HandleInput()
        {
            bool pressed = false, held = false, released = false;
            Vector2 screenPos = Vector2.zero;

            if (Mouse.current != null)
            {
                screenPos = Mouse.current.position.ReadValue();
                pressed = Mouse.current.leftButton.wasPressedThisFrame;
                held = Mouse.current.leftButton.isPressed;
                released = Mouse.current.leftButton.wasReleasedThisFrame;
            }
            else if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                var touch = Touchscreen.current.touches[0];
                screenPos = touch.position.ReadValue();
                pressed = touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began;
                held = touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved ||
                       touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary;
                released = touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
                           touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled;
            }

            if (screenPos == Vector2.zero && !pressed && !held && !released) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z));
            worldPos.z = 0f;

            if (pressed)
            {
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var item = hit.GetComponent<FallingItem>();
                    if (item != null)
                    {
                        _dragItem = item;
                        _dragItem.StartDrag(worldPos);
                    }
                }
            }
            else if (held && _dragItem != null)
            {
                _dragItem.UpdateDrag(worldPos);
            }
            else if (released && _dragItem != null)
            {
                HandleDrop(_dragItem, worldPos);
                _dragItem = null;
            }
        }

        void HandleDrop(FallingItem item, Vector3 dropPos)
        {
            if (item.HasBeenProcessed) return;
            item.MarkProcessed();
            // Find nearest zone
            int nearestZone = -1;
            float nearestDist = float.MaxValue;
            for (int i = 0; i < _zoneCount; i++)
            {
                if (_zones[i] == null) continue;
                float dist = Vector2.Distance(dropPos, _zones[i].position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestZone = i;
                }
            }

            item.EndDrag();

            bool isBonus = item.ItemType == ItemType.Bonus;
            bool isCorrect = false;

            if (nearestZone >= 0 && nearestZone < _zoneCount)
            {
                ItemType trueType = TrueType(item.ItemType);
                int correctZone = System.Array.IndexOf(_zoneItemTypes, trueType);
                isCorrect = nearestZone == correctZone && correctZone < _zoneCount;

                if (isCorrect)
                {
                    _spawnedItems.Remove(item);
                    _activeDropCount = Mathf.Max(0, _activeDropCount - 1);
                    StartCoroutine(ZonePop(_zones[nearestZone]));
                    item.PlayCorrectAnimation(_zones[nearestZone].position, () =>
                    {
                        _gameManager.OnCorrectDrop(isBonus);
                        _itemsProcessed++;
                        CheckStageClear();
                    });
                    return;
                }
            }

            // Wrong drop
            _spawnedItems.Remove(item);
            _activeDropCount = Mathf.Max(0, _activeDropCount - 1);
            item.PlayWrongAnimation(() =>
            {
                _gameManager.OnWrongDrop();
                _itemsProcessed++;
                CheckStageClear();
            });
        }

        public void OnItemFellOff(FallingItem item)
        {
            if (!_isActive) return;
            if (item.HasBeenProcessed) return;
            item.MarkProcessed();
            _spawnedItems.Remove(item);
            _activeDropCount = Mathf.Max(0, _activeDropCount - 1);
            _gameManager.OnWrongDrop();
            _itemsProcessed++;
            CheckStageClear();
        }

        void CheckStageClear()
        {
            if (!_isActive) return;
            if (_itemsProcessed >= _totalItemsForStage && _activeDropCount <= 0)
            {
                _isActive = false;
                _gameManager.OnStageClear();
            }
        }

        IEnumerator ZonePop(Transform zone)
        {
            if (zone == null) yield break;
            Vector3 orig = zone.localScale;
            float t = 0f;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                zone.localScale = Vector3.Lerp(orig, orig * 1.2f, t / 0.1f);
                yield return null;
            }
            t = 0f;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                zone.localScale = Vector3.Lerp(orig * 1.2f, orig, t / 0.1f);
                yield return null;
            }
            zone.localScale = orig;
        }
    }
}
