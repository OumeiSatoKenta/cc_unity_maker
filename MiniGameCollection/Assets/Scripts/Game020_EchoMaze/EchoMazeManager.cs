using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game020_EchoMaze
{
    public class EchoMazeManager : MonoBehaviour
    {
        [SerializeField] private int _gridWidth = 7;
        [SerializeField] private int _gridHeight = 7;
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private GameObject _floorPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _fogPrefab;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _goalPrefab;

        private MazeCell[,] _grid;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();
        private GameObject _playerObj;
        private Vector2Int _playerPos;
        private Vector2Int _goalPos;

        private bool _isDragging;
        private Vector2 _dragStart;
        private int _revealRadius = 1;

        private EchoMazeGameManager _gameManager;
        private Camera _mainCamera;

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<EchoMazeGameManager>();
            _mainCamera = Camera.main;
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
                Vector2 delta = mouse.position.ReadValue() - _dragStart;
                if (delta.magnitude < 30f) return;

                Vector2Int direction;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    direction = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
                else
                    direction = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

                TryMove(direction);
            }
        }

        private void TryMove(Vector2Int direction)
        {
            Vector2Int newPos = _playerPos + direction;
            if (!IsInBounds(newPos)) return;

            if (_grid[newPos.x, newPos.y].IsWall)
            {
                // Hit wall - "echo" feedback: reveal nearby cells
                RevealAround(_playerPos, _revealRadius + 1);
                if (_gameManager != null) _gameManager.OnWallHit();
                return;
            }

            _playerPos = newPos;
            if (_playerObj != null)
                _playerObj.transform.position = GridToWorld(_playerPos);

            // Reveal cells around player
            RevealAround(_playerPos, _revealRadius);

            // Update echo hints (proximity to walls)
            UpdateEchoHints();

            if (_gameManager != null)
            {
                _gameManager.OnPlayerMoved();
                if (_playerPos == _goalPos)
                    _gameManager.OnPuzzleSolved();
            }
        }

        private void RevealAround(Vector2Int center, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    var pos = center + new Vector2Int(dx, dy);
                    if (IsInBounds(pos))
                        _grid[pos.x, pos.y].Reveal();
                }
            }
        }

        private void UpdateEchoHints()
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_grid[x, y].IsRevealed || _grid[x, y].IsWall) continue;
                    float dist = Vector2Int.Distance(_playerPos, new Vector2Int(x, y));
                    float proximity = Mathf.Clamp01(1f - dist / 4f);
                    _grid[x, y].SetEchoHint(proximity);
                }
            }
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            var data = GetStageData(stageIndex);
            _gridWidth = data.width;
            _gridHeight = data.height;
            _grid = new MazeCell[_gridWidth, _gridHeight];
            _goalPos = data.goalPos;
            BuildStage(data);
            RevealAround(_playerPos, _revealRadius);
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
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    bool isWall = data.walls.Contains(new Vector2Int(x, y));
                    var prefab = isWall ? _wallPrefab : _floorPrefab;
                    if (prefab == null) continue;

                    var obj = Instantiate(prefab, transform);
                    obj.transform.position = GridToWorld(new Vector2Int(x, y));
                    obj.name = $"Cell_{x}_{y}";

                    var cell = obj.AddComponent<MazeCell>();
                    cell.Initialize(new Vector2Int(x, y), isWall, _fogPrefab);

                    _grid[x, y] = cell;
                    _stageObjects.Add(obj);
                }
            }

            // Goal
            if (_goalPrefab != null)
            {
                var obj = Instantiate(_goalPrefab, transform);
                obj.transform.position = GridToWorld(data.goalPos);
                _stageObjects.Add(obj);
            }

            // Player
            _playerPos = data.startPos;
            if (_playerPrefab != null)
            {
                _playerObj = Instantiate(_playerPrefab, transform);
                _playerObj.transform.position = GridToWorld(_playerPos);
                _stageObjects.Add(_playerObj);
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
            public Vector2Int startPos, goalPos;
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
            var w = new HashSet<Vector2Int>();
            for (int x = 0; x < 7; x++) { w.Add(new Vector2Int(x, 0)); w.Add(new Vector2Int(x, 6)); }
            for (int y = 1; y < 6; y++) { w.Add(new Vector2Int(0, y)); w.Add(new Vector2Int(6, y)); }
            w.Add(new Vector2Int(2, 1)); w.Add(new Vector2Int(2, 2)); w.Add(new Vector2Int(2, 3));
            w.Add(new Vector2Int(4, 3)); w.Add(new Vector2Int(4, 4)); w.Add(new Vector2Int(4, 5));
            return new StageData { width = 7, height = 7, startPos = new Vector2Int(1, 1), goalPos = new Vector2Int(5, 5), walls = w };
        }

        private StageData GetStage2()
        {
            var w = new HashSet<Vector2Int>();
            for (int x = 0; x < 9; x++) { w.Add(new Vector2Int(x, 0)); w.Add(new Vector2Int(x, 8)); }
            for (int y = 1; y < 8; y++) { w.Add(new Vector2Int(0, y)); w.Add(new Vector2Int(8, y)); }
            w.Add(new Vector2Int(2, 2)); w.Add(new Vector2Int(2, 3)); w.Add(new Vector2Int(3, 5));
            w.Add(new Vector2Int(4, 2)); w.Add(new Vector2Int(4, 3)); w.Add(new Vector2Int(4, 4));
            w.Add(new Vector2Int(6, 4)); w.Add(new Vector2Int(6, 5)); w.Add(new Vector2Int(6, 6));
            w.Add(new Vector2Int(5, 6)); w.Add(new Vector2Int(3, 7)); w.Add(new Vector2Int(2, 6));
            return new StageData { width = 9, height = 9, startPos = new Vector2Int(1, 1), goalPos = new Vector2Int(7, 7), walls = w };
        }

        private StageData GetStage3()
        {
            var w = new HashSet<Vector2Int>();
            for (int x = 0; x < 9; x++) { w.Add(new Vector2Int(x, 0)); w.Add(new Vector2Int(x, 8)); }
            for (int y = 1; y < 8; y++) { w.Add(new Vector2Int(0, y)); w.Add(new Vector2Int(8, y)); }
            // Spiral maze
            for (int x = 2; x <= 6; x++) w.Add(new Vector2Int(x, 2));
            for (int y = 2; y <= 6; y++) w.Add(new Vector2Int(6, y));
            for (int x = 2; x <= 6; x++) w.Add(new Vector2Int(x, 6));
            for (int y = 4; y <= 6; y++) w.Add(new Vector2Int(2, y));
            w.Add(new Vector2Int(4, 4)); w.Remove(new Vector2Int(2, 2));
            w.Remove(new Vector2Int(6, 6)); w.Remove(new Vector2Int(4, 6));
            return new StageData { width = 9, height = 9, startPos = new Vector2Int(1, 1), goalPos = new Vector2Int(4, 5), walls = w };
        }

        #endregion
    }
}
