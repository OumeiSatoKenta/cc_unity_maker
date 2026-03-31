using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game013_SymmetryDraw
{
    public class CanvasDrawManager : MonoBehaviour
    {
        [SerializeField] private int _gridWidth = 8;
        [SerializeField] private int _gridHeight = 6;
        [SerializeField] private float _cellSize = 0.9f;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _targetPrefab;

        private CellView[,] _grid;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();

        private SymmetryDrawGameManager _gameManager;
        private Camera _mainCamera;
        private bool _isDrawing;

        private Sprite _emptySprite;
        private Sprite _paintedSprite;

        private bool[,] _targetPattern;
        private int _targetCount;
        private int _paintedTargetCount;

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<SymmetryDrawGameManager>();
            _mainCamera = Camera.main;
            _emptySprite = Resources.Load<Sprite>("Sprites/Game013_SymmetryDraw/cell_empty");
            _paintedSprite = Resources.Load<Sprite>("Sprites/Game013_SymmetryDraw/cell_painted");
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
                _isDrawing = true;

            if (mouse.leftButton.wasReleasedThisFrame)
                _isDrawing = false;

            if (_isDrawing && mouse.leftButton.isPressed)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var cell = hit.GetComponent<CellView>();
                    if (cell != null && !cell.IsPainted)
                    {
                        PaintWithSymmetry(cell);
                    }
                }
            }
        }

        private void PaintWithSymmetry(CellView cell)
        {
            int x = cell.GridPosition.x;
            int y = cell.GridPosition.y;

            // Paint the clicked cell
            PaintCell(x, y);

            // Paint the mirrored cell (horizontal symmetry around center)
            int mirrorX = _gridWidth - 1 - x;
            if (mirrorX != x)
                PaintCell(mirrorX, y);

            if (_gameManager != null)
            {
                _gameManager.OnCellPainted(_paintedTargetCount, _targetCount);
                if (_paintedTargetCount >= _targetCount)
                    _gameManager.OnPuzzleSolved();
            }
        }

        private void PaintCell(int x, int y)
        {
            if (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight) return;
            var cell = _grid[x, y];
            if (cell == null || cell.IsPainted) return;

            cell.Paint(_paintedSprite);

            if (_targetPattern[x, y])
                _paintedTargetCount++;
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            var data = GetStageData(stageIndex);
            _gridWidth = data.width;
            _gridHeight = data.height;
            _grid = new CellView[_gridWidth, _gridHeight];
            _targetPattern = data.target;
            _targetCount = 0;
            _paintedTargetCount = 0;

            // Count targets
            for (int x = 0; x < _gridWidth; x++)
                for (int y = 0; y < _gridHeight; y++)
                    if (_targetPattern[x, y]) _targetCount++;

            BuildGrid(data);
        }

        public void ResetStage(int stageIndex)
        {
            _paintedTargetCount = 0;
            for (int x = 0; x < _gridWidth; x++)
                for (int y = 0; y < _gridHeight; y++)
                    if (_grid[x, y] != null)
                        _grid[x, y].ResetCell(_emptySprite);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
        }

        private void BuildGrid(StageData data)
        {
            // Target indicators (behind cells)
            var targetSprite = Resources.Load<Sprite>("Sprites/Game013_SymmetryDraw/cell_target");
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (data.target[x, y] && _targetPrefab != null)
                    {
                        var tObj = Instantiate(_targetPrefab, transform);
                        tObj.transform.position = GridToWorld(new Vector2Int(x, y));
                        tObj.name = $"Target_{x}_{y}";
                        _stageObjects.Add(tObj);
                    }
                }
            }

            // Cells
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_cellPrefab == null) continue;
                    var obj = Instantiate(_cellPrefab, transform);
                    var gp = new Vector2Int(x, y);
                    obj.transform.position = GridToWorld(gp);
                    obj.name = $"Cell_{x}_{y}";

                    var cell = obj.GetComponent<CellView>();
                    if (cell != null)
                        cell.Initialize(gp, data.target[x, y]);

                    _grid[x, y] = cell;
                    _stageObjects.Add(obj);
                }
            }

            // Symmetry line (center vertical)
            var lineSprite = Resources.Load<Sprite>("Sprites/Game013_SymmetryDraw/symmetry_line");
            if (lineSprite != null)
            {
                var lineObj = new GameObject("SymmetryLine");
                var sr = lineObj.AddComponent<SpriteRenderer>();
                sr.sprite = lineSprite;
                sr.sortingOrder = 15;
                float centerX = (_gridWidth - 1) * _cellSize * 0.5f;
                lineObj.transform.position = new Vector3(0f, 0f, 0f);
                lineObj.transform.localScale = new Vector3(0.5f, _gridHeight * _cellSize * 0.5f, 1f);
                lineObj.transform.SetParent(transform);
                _stageObjects.Add(lineObj);
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
            public int width, height;
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

        // Stage1: 6x6, simple horizontal bar
        private StageData GetStage1()
        {
            var t = new bool[6, 6];
            for (int x = 1; x < 5; x++) t[x, 3] = true;
            for (int x = 1; x < 5; x++) t[x, 2] = true;
            return new StageData { width = 6, height = 6, target = t };
        }

        // Stage2: 8x6, diamond shape
        private StageData GetStage2()
        {
            var t = new bool[8, 6];
            // Diamond
            t[3, 0] = true; t[4, 0] = true;
            t[2, 1] = true; t[3, 1] = true; t[4, 1] = true; t[5, 1] = true;
            t[1, 2] = true; t[2, 2] = true; t[5, 2] = true; t[6, 2] = true;
            t[1, 3] = true; t[2, 3] = true; t[5, 3] = true; t[6, 3] = true;
            t[2, 4] = true; t[3, 4] = true; t[4, 4] = true; t[5, 4] = true;
            t[3, 5] = true; t[4, 5] = true;
            return new StageData { width = 8, height = 6, target = t };
        }

        // Stage3: 8x8, heart shape
        private StageData GetStage3()
        {
            var t = new bool[8, 8];
            // Heart top
            t[1, 6] = true; t[2, 6] = true; t[5, 6] = true; t[6, 6] = true;
            t[0, 5] = true; t[1, 5] = true; t[2, 5] = true; t[3, 5] = true;
            t[4, 5] = true; t[5, 5] = true; t[6, 5] = true; t[7, 5] = true;
            for (int x = 0; x < 8; x++) t[x, 4] = true;
            t[1, 3] = true; t[2, 3] = true; t[3, 3] = true; t[4, 3] = true; t[5, 3] = true; t[6, 3] = true;
            t[2, 2] = true; t[3, 2] = true; t[4, 2] = true; t[5, 2] = true;
            t[3, 1] = true; t[4, 1] = true;
            return new StageData { width = 8, height = 8, target = t };
        }

        #endregion
    }
}
