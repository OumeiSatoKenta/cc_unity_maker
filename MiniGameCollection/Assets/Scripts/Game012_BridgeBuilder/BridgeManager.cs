using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game012_BridgeBuilder
{
    public class BridgeManager : MonoBehaviour
    {
        [SerializeField] private int _gridWidth = 8;
        [SerializeField] private int _gridHeight = 5;
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private GameObject _slotPrefab;

        private BridgeSlot[,] _grid;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();

        private BridgeBuilderGameManager _gameManager;
        private Camera _mainCamera;
        private int _planksRemaining;

        private Sprite _emptySprite;
        private Sprite _plankSprite;
        private Sprite _supportSprite;
        private Sprite _cliffSprite;
        private Sprite _waterSprite;

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<BridgeBuilderGameManager>();
            _mainCamera = Camera.main;
            _emptySprite = Resources.Load<Sprite>("Sprites/Game012_BridgeBuilder/slot_empty");
            _plankSprite = Resources.Load<Sprite>("Sprites/Game012_BridgeBuilder/plank");
            _supportSprite = Resources.Load<Sprite>("Sprites/Game012_BridgeBuilder/support");
            _cliffSprite = Resources.Load<Sprite>("Sprites/Game012_BridgeBuilder/cliff");
            _waterSprite = Resources.Load<Sprite>("Sprites/Game012_BridgeBuilder/water");
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var slot = hit.GetComponent<BridgeSlot>();
                    if (slot != null && !slot.IsFixed && slot.Type == SlotType.Empty && _planksRemaining > 0)
                    {
                        slot.SetType(SlotType.Plank, _plankSprite);
                        _planksRemaining--;

                        if (_gameManager != null)
                        {
                            _gameManager.OnPlankPlaced(_planksRemaining);
                            if (CheckBridgeComplete())
                                _gameManager.OnBridgeComplete();
                        }
                    }
                }
            }
        }

        public bool CheckBridgeComplete()
        {
            // Check if there's a continuous path of planks/cliff/support from left to right on the bridge row
            int bridgeRow = _gridHeight - 2; // row above water
            for (int x = 0; x < _gridWidth; x++)
            {
                var slot = _grid[x, bridgeRow];
                if (slot == null) return false;
                if (slot.Type != SlotType.Plank && slot.Type != SlotType.Cliff && slot.Type != SlotType.Support)
                    return false;
            }
            return true;
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            var data = GetStageData(stageIndex);
            _gridWidth = data.width;
            _gridHeight = data.height;
            _planksRemaining = data.planksAvailable;
            _grid = new BridgeSlot[_gridWidth, _gridHeight];
            BuildStage(data);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
        }

        private void BuildStage(StageData data)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_slotPrefab == null) continue;
                    var obj = Instantiate(_slotPrefab, transform);
                    var gp = new Vector2Int(x, y);
                    obj.transform.position = GridToWorld(gp);
                    obj.name = $"Slot_{x}_{y}";

                    var slot = obj.GetComponent<BridgeSlot>();
                    SlotType type = SlotType.Empty;
                    bool isFixed = false;
                    Sprite sprite = _emptySprite;

                    if (data.cliffs.Contains(gp))
                    {
                        type = SlotType.Cliff; isFixed = true; sprite = _cliffSprite;
                    }
                    else if (data.water.Contains(gp))
                    {
                        type = SlotType.Water; isFixed = true; sprite = _waterSprite;
                    }
                    else if (data.supports.Contains(gp))
                    {
                        type = SlotType.Support; isFixed = true; sprite = _supportSprite;
                    }

                    if (slot != null)
                    {
                        slot.Initialize(gp, type, isFixed);
                        if (sprite != null)
                        {
                            var sr = obj.GetComponent<SpriteRenderer>();
                            if (sr != null) sr.sprite = sprite;
                        }
                    }

                    _grid[x, y] = slot;
                    _stageObjects.Add(obj);
                }
            }
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offsetX = (_gridWidth - 1) * _cellSize * 0.5f;
            float offsetY = (_gridHeight - 1) * _cellSize * 0.5f;
            return new Vector3(gridPos.x * _cellSize - offsetX, gridPos.y * _cellSize - offsetY, 0f);
        }

        #region Stage Data

        private struct StageData
        {
            public int width, height, planksAvailable;
            public HashSet<Vector2Int> cliffs;
            public HashSet<Vector2Int> water;
            public HashSet<Vector2Int> supports;
        }

        private StageData GetStageData(int index)
        {
            switch (index % StageCount)
            {
                case 0: return GetStage1();
                case 1: return GetStage2();
                case 2: return GetStage3();
                default: return GetStage1();
            }
        }

        // Stage1: 6x4, simple gap, 3 planks needed
        private StageData GetStage1()
        {
            var cliffs = new HashSet<Vector2Int>();
            var water = new HashSet<Vector2Int>();
            var supports = new HashSet<Vector2Int>();

            // Top row (sky) - all empty
            // Bridge row (y=2): cliff on edges, empty in middle
            cliffs.Add(new Vector2Int(0, 2)); cliffs.Add(new Vector2Int(1, 2));
            cliffs.Add(new Vector2Int(4, 2)); cliffs.Add(new Vector2Int(5, 2));
            // Support in middle
            supports.Add(new Vector2Int(3, 2));

            // Water row (y=1)
            for (int x = 2; x < 4; x++) water.Add(new Vector2Int(x, 1));

            // Ground (y=0)
            for (int x = 0; x < 6; x++) cliffs.Add(new Vector2Int(x, 0));
            cliffs.Add(new Vector2Int(0, 1)); cliffs.Add(new Vector2Int(1, 1));
            cliffs.Add(new Vector2Int(4, 1)); cliffs.Add(new Vector2Int(5, 1));

            return new StageData { width = 6, height = 4, planksAvailable = 2, cliffs = cliffs, water = water, supports = supports };
        }

        // Stage2: 8x4, wider gap, 4 planks
        private StageData GetStage2()
        {
            var cliffs = new HashSet<Vector2Int>();
            var water = new HashSet<Vector2Int>();
            var supports = new HashSet<Vector2Int>();

            cliffs.Add(new Vector2Int(0, 2)); cliffs.Add(new Vector2Int(1, 2));
            cliffs.Add(new Vector2Int(6, 2)); cliffs.Add(new Vector2Int(7, 2));
            supports.Add(new Vector2Int(4, 2));

            for (int x = 2; x < 6; x++) water.Add(new Vector2Int(x, 1));
            for (int x = 0; x < 8; x++) cliffs.Add(new Vector2Int(x, 0));
            cliffs.Add(new Vector2Int(0, 1)); cliffs.Add(new Vector2Int(1, 1));
            cliffs.Add(new Vector2Int(6, 1)); cliffs.Add(new Vector2Int(7, 1));

            return new StageData { width = 8, height = 4, planksAvailable = 4, cliffs = cliffs, water = water, supports = supports };
        }

        // Stage3: 8x5, two gaps with support
        private StageData GetStage3()
        {
            var cliffs = new HashSet<Vector2Int>();
            var water = new HashSet<Vector2Int>();
            var supports = new HashSet<Vector2Int>();

            cliffs.Add(new Vector2Int(0, 3)); cliffs.Add(new Vector2Int(1, 3));
            supports.Add(new Vector2Int(3, 3));
            cliffs.Add(new Vector2Int(5, 3));
            cliffs.Add(new Vector2Int(6, 3)); cliffs.Add(new Vector2Int(7, 3));

            for (int x = 2; x < 5; x++) water.Add(new Vector2Int(x, 2));
            water.Add(new Vector2Int(4, 2));
            for (int x = 0; x < 8; x++) { cliffs.Add(new Vector2Int(x, 0)); cliffs.Add(new Vector2Int(x, 1)); }
            cliffs.Add(new Vector2Int(0, 2)); cliffs.Add(new Vector2Int(1, 2));
            cliffs.Add(new Vector2Int(5, 2)); cliffs.Add(new Vector2Int(6, 2)); cliffs.Add(new Vector2Int(7, 2));

            return new StageData { width = 8, height = 5, planksAvailable = 4, cliffs = cliffs, water = water, supports = supports };
        }

        #endregion
    }
}
