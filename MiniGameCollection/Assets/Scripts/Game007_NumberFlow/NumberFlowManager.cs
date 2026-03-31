using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game007_NumberFlow
{
    public class NumberFlowManager : MonoBehaviour
    {
        [SerializeField] private int _gridSize = 5;
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private GameObject _cellPrefab;

        private NumberCell[,] _grid;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();
        private readonly List<NumberCell> _path = new List<NumberCell>();

        private NumberFlowGameManager _gameManager;
        private Camera _mainCamera;
        private int _currentNumber;
        private int _totalCells;

        private Sprite _normalSprite;
        private Sprite _startSprite;
        private Sprite _visitedSprite;
        private Sprite _currentSprite;

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<NumberFlowGameManager>();
            _mainCamera = Camera.main;
            _normalSprite = Resources.Load<Sprite>("Sprites/Game007_NumberFlow/cell_normal");
            _startSprite = Resources.Load<Sprite>("Sprites/Game007_NumberFlow/cell_start");
            _visitedSprite = Resources.Load<Sprite>("Sprites/Game007_NumberFlow/cell_visited");
            _currentSprite = Resources.Load<Sprite>("Sprites/Game007_NumberFlow/cell_current");
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
                    var cell = hit.GetComponent<NumberCell>();
                    if (cell != null) OnCellClicked(cell);
                }
            }
        }

        private void OnCellClicked(NumberCell cell)
        {
            if (cell.IsVisited) return;

            // First click must be on cell with number 1
            if (_currentNumber == 0)
            {
                if (cell.Number != 1) return;
                _currentNumber = 1;
                cell.MarkVisited(_currentNumber, _visitedSprite, _currentSprite);
                _path.Add(cell);
                if (_gameManager != null) _gameManager.OnCellPlaced(_currentNumber, _totalCells);
                return;
            }

            // Must be adjacent to last cell
            var lastCell = _path[_path.Count - 1];
            if (!IsAdjacent(lastCell.GridPosition, cell.GridPosition)) return;

            // If cell has a fixed number, it must match the next expected number
            if (cell.Number > 0 && cell.Number != _currentNumber + 1) return;

            _currentNumber++;
            lastCell.MarkPrevious(_visitedSprite);
            cell.MarkVisited(_currentNumber, _visitedSprite, _currentSprite);
            _path.Add(cell);

            if (_gameManager != null) _gameManager.OnCellPlaced(_currentNumber, _totalCells);

            if (_currentNumber >= _totalCells)
            {
                if (_gameManager != null) _gameManager.OnPuzzleSolved();
            }
        }

        private bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return (dx + dy) == 1;
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            var data = GetStageData(stageIndex);
            _gridSize = data.size;
            _grid = new NumberCell[_gridSize, _gridSize];
            _totalCells = _gridSize * _gridSize;
            _currentNumber = 0;
            _path.Clear();
            BuildGrid(data);
        }

        public void ResetPath()
        {
            _currentNumber = 0;
            _path.Clear();
            for (int x = 0; x < _gridSize; x++)
                for (int y = 0; y < _gridSize; y++)
                    if (_grid[x, y] != null)
                        _grid[x, y].Reset(_normalSprite, _startSprite);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
            _path.Clear();
        }

        private void BuildGrid(StageData data)
        {
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    if (_cellPrefab == null) continue;
                    var obj = Instantiate(_cellPrefab, transform);
                    var gp = new Vector2Int(x, y);
                    obj.transform.position = GridToWorld(gp);
                    obj.name = $"Cell_{x}_{y}";

                    var cell = obj.GetComponent<NumberCell>();
                    int hint = 0;
                    if (data.hints != null && data.hints.TryGetValue(gp, out int h))
                        hint = h;
                    if (cell != null) cell.Initialize(gp, hint);

                    // Set start sprite for cell 1
                    if (hint == 1)
                    {
                        var sr = obj.GetComponent<SpriteRenderer>();
                        if (sr != null && _startSprite != null) sr.sprite = _startSprite;
                    }

                    _grid[x, y] = cell;
                    _stageObjects.Add(obj);
                }
            }
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offset = (_gridSize - 1) * _cellSize * 0.5f;
            return new Vector3(gridPos.x * _cellSize - offset, gridPos.y * _cellSize - offset, 0f);
        }

        #region Stage Data

        private struct StageData
        {
            public int size;
            public Dictionary<Vector2Int, int> hints;
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

        // Stage1: 4x4, hints at 1 and 16
        private StageData GetStage1()
        {
            return new StageData
            {
                size = 4,
                hints = new Dictionary<Vector2Int, int>
                {
                    { new Vector2Int(0, 0), 1 },
                    { new Vector2Int(3, 3), 16 },
                }
            };
        }

        // Stage2: 5x5, hints at 1, 13, 25
        private StageData GetStage2()
        {
            return new StageData
            {
                size = 5,
                hints = new Dictionary<Vector2Int, int>
                {
                    { new Vector2Int(0, 0), 1 },
                    { new Vector2Int(2, 2), 13 },
                    { new Vector2Int(4, 4), 25 },
                }
            };
        }

        // Stage3: 5x5, more hints
        private StageData GetStage3()
        {
            return new StageData
            {
                size = 5,
                hints = new Dictionary<Vector2Int, int>
                {
                    { new Vector2Int(0, 4), 1 },
                    { new Vector2Int(2, 4), 5 },
                    { new Vector2Int(4, 2), 15 },
                    { new Vector2Int(0, 0), 25 },
                }
            };
        }

        #endregion
    }
}
