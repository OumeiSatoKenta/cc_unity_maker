using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game015_TileTurn
{
    public class TileManager : MonoBehaviour
    {
        [SerializeField] private int _gridSize = 4;
        [SerializeField] private float _cellSize = 1.2f;
        [SerializeField] private GameObject _tilePrefab;

        private TileController[,] _grid;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();

        private TileTurnGameManager _gameManager;
        private Camera _mainCamera;

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<TileTurnGameManager>();
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
                    var tile = hit.GetComponent<TileController>();
                    if (tile != null)
                    {
                        tile.RotateCW();
                        if (_gameManager != null)
                        {
                            _gameManager.OnTileRotated();
                            if (CheckAllCorrect())
                                _gameManager.OnPuzzleSolved();
                        }
                    }
                }
            }
        }

        public bool CheckAllCorrect()
        {
            for (int x = 0; x < _gridSize; x++)
                for (int y = 0; y < _gridSize; y++)
                    if (_grid[x, y] != null && !_grid[x, y].IsCorrect())
                        return false;
            return true;
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            var data = GetStageData(stageIndex);
            _gridSize = data.size;
            _grid = new TileController[_gridSize, _gridSize];
            BuildGrid(data);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
        }

        private void BuildGrid(StageData data)
        {
            var rand = new System.Random(data.seed);
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    if (_tilePrefab == null) continue;
                    var obj = Instantiate(_tilePrefab, transform);
                    var gp = new Vector2Int(x, y);
                    obj.transform.position = GridToWorld(gp);
                    obj.name = $"Tile_{x}_{y}";

                    int tileIdx = (x + y * _gridSize) % 4;
                    // Random start rotation (1-3, never 0 to ensure puzzle isn't solved)
                    int startRot = rand.Next(1, 4);

                    var tile = obj.GetComponent<TileController>();
                    if (tile != null)
                        tile.Initialize(gp, tileIdx, startRot);

                    // Set sprite based on tile index
                    var sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        var sprite = Resources.Load<Sprite>($"Sprites/Game015_TileTurn/tile_{tileIdx}");
                        if (sprite != null) sr.sprite = sprite;
                    }

                    _grid[x, y] = tile;
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
            public int seed;
        }

        private StageData GetStageData(int index)
        {
            switch (index % StageCount)
            {
                case 0: return new StageData { size = 3, seed = 42 };
                case 1: return new StageData { size = 4, seed = 123 };
                case 2: return new StageData { size = 5, seed = 456 };
                default: return new StageData { size = 3, seed = 42 };
            }
        }

        #endregion
    }
}
