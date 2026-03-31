using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game005_PipeConnect
{
    public class PipeManager : MonoBehaviour
    {
        [SerializeField] private int _gridWidth = 5;
        [SerializeField] private int _gridHeight = 5;
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private GameObject _pipeStraightPrefab;
        [SerializeField] private GameObject _pipeBendPrefab;
        [SerializeField] private GameObject _pipeCrossPrefab;
        [SerializeField] private GameObject _pipeTJunctionPrefab;
        [SerializeField] private GameObject _sourcePrefab;
        [SerializeField] private GameObject _goalPrefab;
        [SerializeField] private GameObject _cellBgPrefab;

        private PipeTile[,] _grid;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();
        private PipeConnectGameManager _gameManager;
        private Camera _mainCamera;

        public static int StageCount => 3;

        private static readonly Vector2Int[] Dirs = {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        private void Awake()
        {
            _gameManager = GetComponentInParent<PipeConnectGameManager>();
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
                Vector3 screenPos = mouse.position.ReadValue();
                screenPos.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var tile = hit.GetComponent<PipeTile>();
                    if (tile != null && tile.Type != PipeType.Source && tile.Type != PipeType.Goal)
                    {
                        tile.RotateCW();
                        if (_gameManager != null)
                        {
                            _gameManager.OnPipeRotated();
                            if (CheckConnection())
                                _gameManager.OnPuzzleSolved();
                        }
                    }
                }
            }
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            _grid = new PipeTile[_gridWidth, _gridHeight];
            BuildStage(GetStageData(stageIndex));
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
        }

        public bool CheckConnection()
        {
            // BFS from source to goal
            Vector2Int sourcePos = Vector2Int.zero;
            Vector2Int goalPos = Vector2Int.zero;

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_grid[x, y] == null) continue;
                    if (_grid[x, y].Type == PipeType.Source) sourcePos = new Vector2Int(x, y);
                    if (_grid[x, y].Type == PipeType.Goal) goalPos = new Vector2Int(x, y);
                }
            }

            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(sourcePos);
            visited.Add(sourcePos);

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                if (pos == goalPos) return true;

                var tile = _grid[pos.x, pos.y];
                if (tile == null) continue;
                var conn = tile.GetConnections();

                for (int d = 0; d < 4; d++)
                {
                    if (!conn[d]) continue;
                    var next = pos + Dirs[d];
                    if (!IsInBounds(next) || visited.Contains(next)) continue;
                    var nextTile = _grid[next.x, next.y];
                    if (nextTile == null) continue;
                    // Check if neighbor connects back
                    int opposite = (d + 2) % 4;
                    if (nextTile.GetConnections()[opposite])
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }
            return false;
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offsetX = (_gridWidth - 1) * _cellSize * 0.5f;
            float offsetY = (_gridHeight - 1) * _cellSize * 0.5f;
            return new Vector3(gridPos.x * _cellSize - offsetX, gridPos.y * _cellSize - offsetY, 0f);
        }

        private bool IsInBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _gridWidth && pos.y >= 0 && pos.y < _gridHeight;
        }

        private void BuildStage(StageData data)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_cellBgPrefab != null)
                    {
                        var bg = Instantiate(_cellBgPrefab, transform);
                        bg.transform.position = GridToWorld(new Vector2Int(x, y));
                        _stageObjects.Add(bg);
                    }
                }
            }

            foreach (var p in data.pipes)
            {
                var prefab = GetPrefab(p.type);
                if (prefab == null) continue;
                var obj = Instantiate(prefab, transform);
                var gp = new Vector2Int(p.x, p.y);
                obj.transform.position = GridToWorld(gp);
                obj.name = $"Pipe_{p.x}_{p.y}";

                var tile = obj.GetComponent<PipeTile>();
                if (tile != null)
                    tile.Initialize(gp, p.type, p.rotation);

                _grid[p.x, p.y] = tile;
                _stageObjects.Add(obj);
            }
        }

        private GameObject GetPrefab(PipeType type)
        {
            switch (type)
            {
                case PipeType.Straight: return _pipeStraightPrefab;
                case PipeType.Bend: return _pipeBendPrefab;
                case PipeType.Cross: return _pipeCrossPrefab;
                case PipeType.TJunction: return _pipeTJunctionPrefab;
                case PipeType.Source: return _sourcePrefab;
                case PipeType.Goal: return _goalPrefab;
                default: return null;
            }
        }

        #region Stage Data

        private struct PipePlacement
        {
            public int x, y;
            public PipeType type;
            public int rotation;
            public PipePlacement(int x, int y, PipeType t, int r) { this.x=x; this.y=y; type=t; rotation=r; }
        }

        private struct StageData
        {
            public List<PipePlacement> pipes;
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

        // Stage1: 5x5, simple path source(0,2)->goal(4,2)
        private StageData GetStage1()
        {
            return new StageData { pipes = new List<PipePlacement> {
                new PipePlacement(0, 2, PipeType.Source, 0),
                new PipePlacement(1, 2, PipeType.Straight, 1), // horizontal
                new PipePlacement(2, 2, PipeType.Bend, 3),     // scrambled
                new PipePlacement(2, 3, PipeType.Straight, 0),
                new PipePlacement(2, 4, PipeType.Bend, 2),
                new PipePlacement(3, 4, PipeType.Straight, 1),
                new PipePlacement(4, 4, PipeType.Bend, 1),
                new PipePlacement(4, 3, PipeType.Straight, 2),
                new PipePlacement(4, 2, PipeType.Goal, 0),
            }};
        }

        private StageData GetStage2()
        {
            return new StageData { pipes = new List<PipePlacement> {
                new PipePlacement(0, 0, PipeType.Source, 0),
                new PipePlacement(0, 1, PipeType.Bend, 2),
                new PipePlacement(1, 1, PipeType.Straight, 3),
                new PipePlacement(2, 1, PipeType.TJunction, 1),
                new PipePlacement(2, 2, PipeType.Straight, 0),
                new PipePlacement(2, 3, PipeType.Bend, 0),
                new PipePlacement(3, 3, PipeType.Straight, 3),
                new PipePlacement(3, 1, PipeType.Bend, 1),
                new PipePlacement(3, 0, PipeType.Straight, 0),
                new PipePlacement(4, 0, PipeType.Goal, 0),
                new PipePlacement(4, 3, PipeType.Goal, 0),
            }};
        }

        private StageData GetStage3()
        {
            return new StageData { pipes = new List<PipePlacement> {
                new PipePlacement(0, 4, PipeType.Source, 0),
                new PipePlacement(0, 3, PipeType.Bend, 3),
                new PipePlacement(1, 3, PipeType.TJunction, 2),
                new PipePlacement(1, 4, PipeType.Bend, 1),
                new PipePlacement(2, 4, PipeType.Straight, 1),
                new PipePlacement(2, 3, PipeType.Straight, 0),
                new PipePlacement(2, 2, PipeType.Bend, 3),
                new PipePlacement(3, 2, PipeType.Cross, 0),
                new PipePlacement(3, 3, PipeType.Straight, 0),
                new PipePlacement(3, 4, PipeType.Bend, 2),
                new PipePlacement(4, 2, PipeType.Bend, 0),
                new PipePlacement(4, 1, PipeType.Straight, 0),
                new PipePlacement(4, 0, PipeType.Goal, 0),
            }};
        }

        #endregion
    }
}
