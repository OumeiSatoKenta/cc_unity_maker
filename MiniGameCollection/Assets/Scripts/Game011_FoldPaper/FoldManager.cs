using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game011_FoldPaper
{
    public class FoldManager : MonoBehaviour
    {
        [SerializeField] private int _gridSize = 5;
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _targetMarkPrefab;

        private PaperCell[,] _grid;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();
        private readonly List<GameObject> _targetMarks = new List<GameObject>();

        private FoldPaperGameManager _gameManager;
        private Camera _mainCamera;
        private bool[,] _targetPattern;

        private Sprite _whiteSprite;
        private Sprite _foldedSprite;

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<FoldPaperGameManager>();
            _mainCamera = Camera.main;
            _whiteSprite = Resources.Load<Sprite>("Sprites/Game011_FoldPaper/paper_white");
            _foldedSprite = Resources.Load<Sprite>("Sprites/Game011_FoldPaper/paper_folded");
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
                    var cell = hit.GetComponent<PaperCell>();
                    if (cell != null)
                    {
                        // Toggle clicked cell and adjacent cells (cross pattern)
                        ToggleCross(cell.GridPosition);
                        if (_gameManager != null)
                        {
                            _gameManager.OnFoldMade();
                            if (CheckPattern())
                                _gameManager.OnPuzzleSolved();
                        }
                    }
                }
            }
        }

        private void ToggleCross(Vector2Int center)
        {
            Vector2Int[] positions = {
                center,
                center + Vector2Int.up,
                center + Vector2Int.down,
                center + Vector2Int.left,
                center + Vector2Int.right
            };

            foreach (var pos in positions)
            {
                if (pos.x >= 0 && pos.x < _gridSize && pos.y >= 0 && pos.y < _gridSize)
                {
                    _grid[pos.x, pos.y].Toggle(_whiteSprite, _foldedSprite);
                }
            }
        }

        public bool CheckPattern()
        {
            for (int x = 0; x < _gridSize; x++)
                for (int y = 0; y < _gridSize; y++)
                    if (_grid[x, y].IsFolded != _targetPattern[x, y])
                        return false;
            return true;
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            var data = GetStageData(stageIndex);
            _gridSize = data.size;
            _grid = new PaperCell[_gridSize, _gridSize];
            _targetPattern = data.target;
            BuildGrid(data);
            ShowTarget();
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
            foreach (var obj in _targetMarks)
                if (obj != null) Destroy(obj);
            _targetMarks.Clear();
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
                    obj.name = $"Paper_{x}_{y}";

                    var cell = obj.GetComponent<PaperCell>();
                    if (cell != null)
                        cell.Initialize(gp, data.initial[x, y], _whiteSprite, _foldedSprite);

                    _grid[x, y] = cell;
                    _stageObjects.Add(obj);
                }
            }
        }

        private void ShowTarget()
        {
            float targetOffsetX = (_gridSize + 1) * _cellSize;
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    if (!_targetPattern[x, y]) continue;
                    if (_targetMarkPrefab == null) continue;
                    var obj = Instantiate(_targetMarkPrefab, transform);
                    var worldPos = GridToWorld(new Vector2Int(x, y));
                    worldPos.x += targetOffsetX;
                    obj.transform.position = worldPos;
                    obj.name = $"Target_{x}_{y}";
                    _targetMarks.Add(obj);
                }
            }

            // Target background cells (unfilled)
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    if (_cellPrefab == null) continue;
                    var obj = Instantiate(_cellPrefab, transform);
                    var worldPos = GridToWorld(new Vector2Int(x, y));
                    worldPos.x += (_gridSize + 1) * _cellSize;
                    obj.transform.position = worldPos;
                    obj.name = $"TargetBg_{x}_{y}";
                    var sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sortingOrder = -1;
                    // Remove collider so it doesn't interfere
                    var col = obj.GetComponent<BoxCollider2D>();
                    if (col != null) Destroy(col);
                    _targetMarks.Add(obj);
                }
            }
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offset = (_gridSize - 1) * _cellSize * 0.5f;
            return new Vector3(gridPos.x * _cellSize - offset - 2f, gridPos.y * _cellSize - offset, 0f);
        }

        #region Stage Data

        private struct StageData
        {
            public int size;
            public bool[,] initial;
            public bool[,] target;
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

        // Stage1: 4x4, simple cross pattern
        private StageData GetStage1()
        {
            var initial = new bool[4, 4];
            var target = new bool[4, 4];
            // Target: cross in center
            target[1, 1] = true; target[1, 2] = true;
            target[2, 1] = true; target[2, 2] = true;
            return new StageData { size = 4, initial = initial, target = target };
        }

        // Stage2: 4x4, diagonal pattern
        private StageData GetStage2()
        {
            var initial = new bool[4, 4];
            var target = new bool[4, 4];
            target[0, 0] = true; target[1, 1] = true;
            target[2, 2] = true; target[3, 3] = true;
            return new StageData { size = 4, initial = initial, target = target };
        }

        // Stage3: 5x5, border pattern
        private StageData GetStage3()
        {
            var initial = new bool[5, 5];
            var target = new bool[5, 5];
            for (int i = 0; i < 5; i++)
            {
                target[i, 0] = true; target[i, 4] = true;
                target[0, i] = true; target[4, i] = true;
            }
            return new StageData { size = 5, initial = initial, target = target };
        }

        #endregion
    }
}
