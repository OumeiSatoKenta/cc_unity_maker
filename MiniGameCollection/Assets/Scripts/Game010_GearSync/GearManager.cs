using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game010_GearSync
{
    public class GearManager : MonoBehaviour
    {
        [SerializeField] private float _gearSpacing = 2.0f;
        [SerializeField] private GameObject _gearPrefab;

        private readonly List<GearController> _gears = new List<GearController>();
        private readonly List<GameObject> _stageObjects = new List<GameObject>();
        private readonly Dictionary<Vector2Int, GearController> _gearMap = new Dictionary<Vector2Int, GearController>();

        private GearSyncGameManager _gameManager;
        private Camera _mainCamera;

        public static int StageCount => 3;

        private static readonly Vector2Int[] Neighbors = {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        private void Awake()
        {
            _gameManager = GetComponentInParent<GearSyncGameManager>();
            _mainCamera = Camera.main;
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
                    var gear = hit.GetComponent<GearController>();
                    if (gear != null)
                    {
                        RotateGearChain(gear, true);
                        if (_gameManager != null)
                        {
                            _gameManager.OnGearRotated();
                            if (CheckAllAligned())
                                _gameManager.OnPuzzleSolved();
                        }
                    }
                }
            }
        }

        private void RotateGearChain(GearController gear, bool clockwise)
        {
            var visited = new HashSet<Vector2Int>();
            RotateRecursive(gear, clockwise, visited);
        }

        private void RotateRecursive(GearController gear, bool clockwise, HashSet<Vector2Int> visited)
        {
            if (visited.Contains(gear.GridPosition)) return;
            visited.Add(gear.GridPosition);

            if (clockwise)
                gear.RotateCW();
            else
                gear.RotateCCW();

            // Adjacent gears rotate in opposite direction
            foreach (var dir in Neighbors)
            {
                var neighborPos = gear.GridPosition + dir;
                if (_gearMap.TryGetValue(neighborPos, out var neighbor))
                {
                    RotateRecursive(neighbor, !clockwise, visited);
                }
            }
        }

        public bool CheckAllAligned()
        {
            foreach (var gear in _gears)
            {
                if (!gear.IsAligned()) return false;
            }
            return true;
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            var data = GetStageData(stageIndex);
            BuildStage(data);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
            _gears.Clear();
            _gearMap.Clear();
        }

        private void BuildStage(StageData data)
        {
            foreach (var gd in data.gears)
            {
                if (_gearPrefab == null) continue;
                var obj = Instantiate(_gearPrefab, transform);
                obj.transform.position = new Vector3(gd.pos.x * _gearSpacing, gd.pos.y * _gearSpacing, 0f);
                obj.name = $"Gear_{gd.pos.x}_{gd.pos.y}";

                var ctrl = obj.GetComponent<GearController>();
                if (ctrl != null)
                {
                    ctrl.Initialize(gd.pos, gd.startRotation, gd.targetRotation);
                    _gears.Add(ctrl);
                    _gearMap[gd.pos] = ctrl;
                }
                _stageObjects.Add(obj);
            }
        }

        #region Stage Data

        private struct GearData
        {
            public Vector2Int pos;
            public int startRotation;
            public int targetRotation;
            public GearData(int x, int y, int s, int t) { pos = new Vector2Int(x, y); startRotation = s; targetRotation = t; }
        }

        private struct StageData
        {
            public List<GearData> gears;
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

        // Stage1: 3 gears in a row
        private StageData GetStage1()
        {
            return new StageData { gears = new List<GearData> {
                new GearData(0, 0, 1, 0),
                new GearData(1, 0, 2, 0),
                new GearData(2, 0, 3, 0),
            }};
        }

        // Stage2: 2x2 grid
        private StageData GetStage2()
        {
            return new StageData { gears = new List<GearData> {
                new GearData(0, 0, 2, 0),
                new GearData(1, 0, 1, 0),
                new GearData(0, 1, 3, 0),
                new GearData(1, 1, 0, 0),
            }};
        }

        // Stage3: L-shape 5 gears
        private StageData GetStage3()
        {
            return new StageData { gears = new List<GearData> {
                new GearData(0, 0, 1, 0),
                new GearData(1, 0, 3, 0),
                new GearData(2, 0, 2, 0),
                new GearData(0, 1, 2, 0),
                new GearData(0, 2, 1, 0),
            }};
        }

        #endregion
    }
}
