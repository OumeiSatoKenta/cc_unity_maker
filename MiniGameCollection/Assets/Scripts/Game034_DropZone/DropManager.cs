using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game034_DropZone
{
    public class DropManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private DropZoneGameManager _gameManager;

        [SerializeField, Tooltip("アイテムスプライト配列 [0]=apple,[1]=banana,[2]=paper,[3]=can,[4]=bottle,[5]=glass")]
        private Sprite[] _itemSprites;

        [SerializeField, Tooltip("ゾーンスプライト配列 [0]=green,[1]=gray,[2]=blue")]
        private Sprite[] _zoneSprites;

        private Camera _mainCamera;
        private DropItem _currentItem;
        private bool _isDragging;
        private int _combo;
        private int _spawnedCount;
        private float _baseFallSpeed = 1.5f;

        // ゾーン定義: X座標とカテゴリ
        private static readonly float[] ZoneXPositions = { -2.5f, 0f, 2.5f };
        private static readonly int[] ZoneCategories = { 0, 1, 2 }; // フルーツ, ゴミ, リサイクル
        private const float ZoneY = -3.5f;
        private const float SpawnY = 5.5f;

        // アイテム定義: (spriteIndex, category)
        private static readonly (int sprite, int category)[] ItemTypes = {
            (0, 0), // apple → フルーツ
            (1, 0), // banana → フルーツ
            (2, 1), // paper → ゴミ
            (3, 1), // can → ゴミ
            (4, 2), // bottle → リサイクル
            (5, 2), // glass → リサイクル
        };

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void StartGame()
        {
            _combo = 0;
            _spawnedCount = 0;
            SpawnZones();
            SpawnNextItem();
        }

        private void SpawnZones()
        {
            string[] labels = { "フルーツ", "ゴミ", "リサイクル" };
            for (int i = 0; i < 3; i++)
            {
                var zoneObj = new GameObject($"Zone_{i}");
                zoneObj.transform.position = new Vector3(ZoneXPositions[i], ZoneY, 0f);
                zoneObj.transform.localScale = new Vector3(2f, 1f, 1f);
                zoneObj.transform.SetParent(transform);

                var sr = zoneObj.AddComponent<SpriteRenderer>();
                if (_zoneSprites != null && i < _zoneSprites.Length)
                    sr.sprite = _zoneSprites[i];
                sr.sortingOrder = 1;

                // ゾーンラベル（TextMeshではなくSpriteRendererのみ）
            }
        }

        private void SpawnNextItem()
        {
            if (_spawnedCount >= _gameManager.TotalItems || !_gameManager.IsPlaying) return;

            var itemDef = ItemTypes[Random.Range(0, ItemTypes.Length)];
            var obj = new GameObject($"Item_{_spawnedCount}");
            obj.transform.position = new Vector3(0f, SpawnY, 0f);
            obj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            var item = obj.AddComponent<DropItem>();
            Sprite sprite = (_itemSprites != null && itemDef.sprite < _itemSprites.Length)
                ? _itemSprites[itemDef.sprite] : null;
            float speed = _baseFallSpeed + _spawnedCount * 0.05f; // 徐々に加速
            item.Initialize(sprite, itemDef.category, speed);

            _currentItem = item;
            _spawnedCount++;
            _isDragging = false;
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying || _currentItem == null) return;

            HandleInput();
            CheckZoneReached();
        }

        private void HandleInput()
        {
            if (_mainCamera == null || Mouse.current == null || _currentItem == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
            }

            if (_isDragging && Mouse.current.leftButton.isPressed)
            {
                Vector3 mousePos = Mouse.current.position.ReadValue();
                mousePos.z = -_mainCamera.transform.position.z;
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
                _currentItem.SetXPosition(worldPos.x);
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame && _isDragging)
            {
                _isDragging = false;
                _currentItem.AccelerateFall();
            }
        }

        private void CheckZoneReached()
        {
            if (_currentItem == null || !_currentItem.IsActive) return;

            if (_currentItem.transform.position.y <= ZoneY)
            {
                _currentItem.Stop();
                JudgeZone();
            }
        }

        private void JudgeZone()
        {
            float itemX = _currentItem.transform.position.x;
            int closestZone = 0;
            float closestDist = float.MaxValue;

            for (int i = 0; i < ZoneXPositions.Length; i++)
            {
                float dist = Mathf.Abs(itemX - ZoneXPositions[i]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestZone = i;
                }
            }

            int expectedCategory = ZoneCategories[closestZone];
            bool correct = _currentItem.Category == expectedCategory;

            // エフェクト: 正解→緑フラッシュ, 不正解→赤フラッシュ
            var sr = _currentItem.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = correct ? Color.green : Color.red;

            Destroy(_currentItem.gameObject, 0.3f);
            _currentItem = null;

            if (correct)
            {
                _combo++;
                _gameManager.OnCorrectDrop(_combo);
            }
            else
            {
                _combo = 0;
                _gameManager.OnWrongDrop();
            }

            // 次のアイテム生成（少し遅延）
            if (_gameManager.IsPlaying)
                Invoke(nameof(SpawnNextItem), 0.4f);
        }
    }
}
