using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game008_IcePath
{
    public class IcePathManager : MonoBehaviour
    {
        [SerializeField] private int _gridWidth = 7;
        [SerializeField] private int _gridHeight = 7;
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private GameObject _icePrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _goalPrefab;
        [SerializeField] private GameObject _playerPrefab;

        private IceCell[,] _grid;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();
        private GameObject _playerObj;
        private Vector2Int _playerPos;
        private int _totalIceCells;
        private int _visitedCount;

        private IcePathGameManager _gameManager;
        private Camera _mainCamera;
        private Vector2 _dragStart;
        private bool _isDragging;

        private Sprite _iceSprite;
        private Sprite _visitedSprite;

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<IcePathGameManager>();
            _mainCamera = Camera.main;
            _iceSprite = Resources.Load<Sprite>("Sprites/Game008_IcePath/ice_cell");
            _visitedSprite = Resources.Load<Sprite>("Sprites/Game008_IcePath/visited_cell");
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _dragStart = mouse.position.ReadValue();
                _isDragging = true;
            }

            if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
            {
                _isDragging = false;
                Vector2 dragEnd = mouse.position.ReadValue();
                Vector2 delta = dragEnd - _dragStart;

                if (delta.magnitude < 30f) return;

                Vector2Int direction;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    direction = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
                else
                    direction = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

                SlidePlayer(direction);
            }
        }

        private void SlidePlayer(Vector2Int direction)
        {
            Vector2Int newPos = _playerPos;

            while (true)
            {
                Vector2Int candidate = newPos + direction;
                if (!IsInBounds(candidate)) break;
                if (_grid[candidate.x, candidate.y] != null && _grid[candidate.x, candidate.y].CellType == IceCellType.Wall) break;
                newPos = candidate;
            }

            if (newPos == _playerPos) return;

            // Mark all cells along the path as visited
            Vector2Int pos = _playerPos;
            while (pos != newPos)
            {
                pos += direction;
                var cell = _grid[pos.x, pos.y];
                if (cell != null && !cell.IsVisited && cell.CellType != IceCellType.Wall)
                {
                    cell.MarkVisited(_visitedSprite);
                    _visitedCount++;
                }
            }

            _playerPos = newPos;
            if (_playerObj != null)
                _playerObj.transform.position = GridToWorld(_playerPos);

            if (_gameManager != null)
            {
                _gameManager.OnPlayerMoved(_visitedCount, _totalIceCells);

                if (_visitedCount >= _totalIceCells)
                    _gameManager.OnPuzzleSolved();
            }
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            var data = GetStageData(stageIndex);
            _gridWidth = data.width;
            _gridHeight = data.height;
            _grid = new IceCell[_gridWidth, _gridHeight];
            _totalIceCells = 0;
            _visitedCount = 0;
            BuildStage(data);
        }

        public void ResetStage(int stageIndex)
        {
            // Reset visited state without rebuilding
            for (int x = 0; x < _gridWidth; x++)
                for (int y = 0; y < _gridHeight; y++)
                    if (_grid[x, y] != null && _grid[x, y].CellType == IceCellType.Ice)
                        _grid[x, y].ResetVisited(_iceSprite);

            var data = GetStageData(stageIndex);
            _playerPos = data.startPos;
            _visitedCount = 1; // Start cell counts
            if (_playerObj != null)
                _playerObj.transform.position = GridToWorld(_playerPos);

            // Mark start cell as visited
            if (_grid[_playerPos.x, _playerPos.y] != null)
                _grid[_playerPos.x, _playerPos.y].MarkVisited(_visitedSprite);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
            _playerObj = null;
        }

        private void BuildStage(StageData data)
        {
            // Build all cells
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    var gp = new Vector2Int(x, y);
                    bool isWall = data.walls.Contains(gp);
                    bool isGoal = gp == data.goalPos;

                    IceCellType cellType = isWall ? IceCellType.Wall : (isGoal ? IceCellType.Goal : IceCellType.Ice);
                    GameObject prefab = isWall ? _wallPrefab : (isGoal ? _goalPrefab : _icePrefab);

                    if (prefab == null) continue;
                    var obj = Instantiate(prefab, transform);
                    obj.transform.position = GridToWorld(gp);
                    obj.name = $"Cell_{x}_{y}";

                    var cell = obj.GetComponent<IceCell>();
                    if (cell != null) cell.Initialize(gp, cellType);
                    _grid[x, y] = cell;
                    _stageObjects.Add(obj);

                    if (!isWall) _totalIceCells++;
                }
            }

            // Player
            _playerPos = data.startPos;
            if (_playerPrefab != null)
            {
                _playerObj = Instantiate(_playerPrefab, transform);
                _playerObj.transform.position = GridToWorld(_playerPos);
                _playerObj.name = "Player";
                _stageObjects.Add(_playerObj);
            }

            // Mark start cell visited
            if (_grid[_playerPos.x, _playerPos.y] != null)
            {
                _grid[_playerPos.x, _playerPos.y].MarkVisited(_visitedSprite);
                _visitedCount = 1;
            }
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

        #region Stage Data

        private struct StageData
        {
            public int width, height;
            public Vector2Int startPos;
            public Vector2Int goalPos;
            public HashSet<Vector2Int> walls;
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

        private StageData GetStage1()
        {
            var walls = new HashSet<Vector2Int>();
            // Border walls
            for (int x = 0; x < 5; x++) { walls.Add(new Vector2Int(x, 0)); walls.Add(new Vector2Int(x, 4)); }
            for (int y = 1; y < 4; y++) { walls.Add(new Vector2Int(0, y)); walls.Add(new Vector2Int(4, y)); }
            // Inner wall
            walls.Add(new Vector2Int(2, 2));
            return new StageData { width = 5, height = 5, startPos = new Vector2Int(1, 1), goalPos = new Vector2Int(3, 3), walls = walls };
        }

        private StageData GetStage2()
        {
            var walls = new HashSet<Vector2Int>();
            for (int x = 0; x < 6; x++) { walls.Add(new Vector2Int(x, 0)); walls.Add(new Vector2Int(x, 5)); }
            for (int y = 1; y < 5; y++) { walls.Add(new Vector2Int(0, y)); walls.Add(new Vector2Int(5, y)); }
            walls.Add(new Vector2Int(2, 3));
            walls.Add(new Vector2Int(3, 2));
            return new StageData { width = 6, height = 6, startPos = new Vector2Int(1, 1), goalPos = new Vector2Int(4, 4), walls = walls };
        }

        private StageData GetStage3()
        {
            var walls = new HashSet<Vector2Int>();
            for (int x = 0; x < 7; x++) { walls.Add(new Vector2Int(x, 0)); walls.Add(new Vector2Int(x, 6)); }
            for (int y = 1; y < 6; y++) { walls.Add(new Vector2Int(0, y)); walls.Add(new Vector2Int(6, y)); }
            walls.Add(new Vector2Int(2, 2));
            walls.Add(new Vector2Int(4, 4));
            walls.Add(new Vector2Int(3, 1));
            walls.Add(new Vector2Int(3, 5));
            return new StageData { width = 7, height = 7, startPos = new Vector2Int(1, 1), goalPos = new Vector2Int(5, 5), walls = walls };
        }

        #endregion
    }
}
