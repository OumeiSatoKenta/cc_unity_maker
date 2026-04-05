using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game015v2_TileTurn
{
    public class TileManager : MonoBehaviour
    {
        [SerializeField] TileTurnGameManager _gameManager;
        [SerializeField] TileTurnUI _ui;

        [SerializeField] Sprite _tileNormalSprite;
        [SerializeField] Sprite _tileLinkSprite;
        [SerializeField] Sprite _tileLockedSprite;
        [SerializeField] Sprite _tileFlippedSprite;
        [SerializeField] Sprite _tileCorrectSprite;

        // Stage config
        int _gridSize;
        int _maxRotations;
        float _linkedRatio;
        float _lockedRatio;
        float _flippedRatio;
        int _stageNumber;

        // Runtime
        TileCell[,] _grid;
        int _rotationCount;
        bool _previewUsed;
        bool _isActive;
        List<TileCell> _allTiles = new List<TileCell>();
        Camera _mainCamera;

        // Stage config table
        static readonly (int gridSize, int maxRotations, float linked, float locked, float flipped)[] StageTable =
        {
            (2, 16,  0.0f, 0.0f, 0.0f),
            (3, 30,  0.25f, 0.0f, 0.0f),
            (4, 52,  0.2f, 0.2f, 0.0f),
            (4, 48,  0.0f, 0.0f, 0.25f),
            (5, 80,  0.2f, 0.15f, 0.15f),
        };

        public int RotationCount => _rotationCount;
        public int MaxRotations => _maxRotations;

        void Start()
        {
            _mainCamera = Camera.main;
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            _stageNumber = stageNumber;
            int idx = Mathf.Clamp(stageNumber - 1, 0, StageTable.Length - 1);
            var st = StageTable[idx];
            _gridSize = st.gridSize;
            _maxRotations = st.maxRotations;
            _linkedRatio = st.linked;
            _lockedRatio = st.locked;
            _flippedRatio = st.flipped;
            _rotationCount = 0;
            _previewUsed = false;
            _isActive = true;

            if (_mainCamera == null) _mainCamera = Camera.main;
            ClearGrid();
            BuildGrid();
        }

        public void ResetStage()
        {
            if (_gridSize == 0) return;
            _rotationCount = 0;
            _previewUsed = false;
            _isActive = true;
            ClearGrid();
            BuildGrid();
        }

        void ClearGrid()
        {
            foreach (var t in _allTiles)
                if (t != null) Destroy(t.gameObject);
            _allTiles.Clear();
            _grid = null;
        }

        void BuildGrid()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            _grid = new TileCell[_gridSize, _gridSize];
            int totalTiles = _gridSize * _gridSize;

            // Determine tile types
            int lockedCount = Mathf.RoundToInt(totalTiles * _lockedRatio);
            int flippedCount = Mathf.RoundToInt(totalTiles * _flippedRatio);
            int linkedCount = Mathf.RoundToInt(totalTiles * _linkedRatio);

            // Shuffle indices
            var indices = new List<int>();
            for (int i = 0; i < totalTiles; i++) indices.Add(i);
            Shuffle(indices);

            var typeMap = new TileType[totalTiles];
            for (int i = 0; i < totalTiles; i++) typeMap[i] = TileType.Normal;
            int assigned = 0;
            for (int i = 0; i < lockedCount && assigned < totalTiles; i++, assigned++)
                typeMap[indices[assigned]] = TileType.Locked;
            for (int i = 0; i < flippedCount && assigned < totalTiles; i++, assigned++)
                typeMap[indices[assigned]] = TileType.Flipped;
            for (int i = 0; i < linkedCount && assigned < totalTiles; i++, assigned++)
                typeMap[indices[assigned]] = TileType.Linked;

            // Compute layout
            float camSize = _mainCamera.orthographicSize;
            float camWidth = camSize * _mainCamera.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 2.8f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            float cellSize = Mathf.Min(availableHeight / _gridSize, camWidth * 2f / _gridSize, 1.8f);
            float startX = -(_gridSize - 1) * cellSize * 0.5f;
            float startY = camSize - topMargin - cellSize * 0.5f;

            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    int tileIdx = row * _gridSize + col;
                    TileType ttype = typeMap[tileIdx];

                    var go = new GameObject($"Tile_{row}_{col}");
                    go.transform.SetParent(transform);
                    float wx = startX + col * cellSize;
                    float wy = startY - row * cellSize;
                    go.transform.localPosition = new Vector3(wx, wy, 0f);

                    var tc = go.AddComponent<TileCell>();
                    var sr = go.GetComponent<SpriteRenderer>();
                    if (sr == null) sr = go.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 1;

                    Sprite normalSprite = GetSpriteForType(ttype);
                    // Locked tiles start correct; Flipped tiles start in flipped state; others get random non-zero rotation
                    int initRot = (ttype == TileType.Locked) ? 0 : Random.Range(1, 4);
                    bool initFlipped = (ttype == TileType.Flipped);
                    tc.Initialize(ttype, initRot, initFlipped, normalSprite, _tileCorrectSprite);

                    // Scale tile to fit cell
                    if (normalSprite != null)
                    {
                        float spriteSize = normalSprite.bounds.size.x;
                        if (spriteSize > 0f)
                        {
                            float targetSize = cellSize * 0.92f;
                            float s = targetSize / spriteSize;
                            go.transform.localScale = new Vector3(s, s, 1f);
                            // Collider size in local space = 1/s so world size = targetSize
                            var col2d = go.AddComponent<BoxCollider2D>();
                            col2d.size = new Vector2(1f / s, 1f / s) * targetSize / s;
                            // Simpler: size=Vector2.one, scale handles world size
                            col2d.size = Vector2.one;
                        }
                        else
                        {
                            go.AddComponent<BoxCollider2D>();
                        }
                    }
                    else
                    {
                        go.AddComponent<BoxCollider2D>();
                    }

                    _grid[row, col] = tc;
                    _allTiles.Add(tc);
                }
            }

            // Wire linked neighbors (adjacent tiles)
            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    var tc = _grid[row, col];
                    if (tc.TileType != TileType.Linked) continue;
                    int[][] dirs = { new[]{-1,0}, new[]{1,0}, new[]{0,-1}, new[]{0,1} };
                    foreach (var d in dirs)
                    {
                        int nr = row + d[0], nc = col + d[1];
                        if (nr >= 0 && nr < _gridSize && nc >= 0 && nc < _gridSize)
                            tc.LinkedNeighbors.Add(_grid[nr, nc]);
                    }
                }
            }

            // Update UI with initial rotation count
            _ui?.UpdateRotations(_maxRotations - _rotationCount, _maxRotations);
        }

        Sprite GetSpriteForType(TileType t)
        {
            return t switch
            {
                TileType.Linked => _tileLinkSprite != null ? _tileLinkSprite : _tileNormalSprite,
                TileType.Locked => _tileLockedSprite != null ? _tileLockedSprite : _tileNormalSprite,
                TileType.Flipped => _tileFlippedSprite != null ? _tileFlippedSprite : _tileNormalSprite,
                _ => _tileNormalSprite,
            };
        }

        void Update()
        {
            if (!_isActive) return;
            if (_gameManager == null || _gameManager.State != TileTurnGameManager.GameState.Playing) return;
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
            if (_mainCamera == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
            worldPos.z = 0f;

            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            var tc = hit.GetComponent<TileCell>();
            if (tc == null) return;
            if (tc.TileType == TileType.Locked) return;

            if (tc.TileType == TileType.Flipped)
            {
                tc.Flip();
            }
            else
            {
                tc.Rotate();
                // Propagate to linked neighbors
                if (tc.TileType == TileType.Linked)
                {
                    foreach (var neighbor in tc.LinkedNeighbors)
                        neighbor.RotateLinked();
                }
            }

            _rotationCount++;
            _ui?.UpdateRotations(_maxRotations - _rotationCount, _maxRotations);

            // Check clear
            if (CheckAllCorrect())
            {
                _isActive = false;
                _gameManager.OnStageClear(_maxRotations - _rotationCount, _maxRotations, _previewUsed);
                return;
            }

            // Check game over
            if (_rotationCount >= _maxRotations)
            {
                _isActive = false;
                foreach (var t in _allTiles) t.PlayGameOverFlash();
                _gameManager.OnGameOver();
            }
        }

        bool CheckAllCorrect()
        {
            foreach (var t in _allTiles)
            {
                if (t.TileType == TileType.Flipped)
                {
                    // Correct when not flipped and rotation is 0
                    if (t.IsFlipped || t.CurrentRotation != 0) return false;
                }
                else if (!t.IsCorrect) return false;
            }
            return true;
        }

        public void SetPreviewUsed()
        {
            _previewUsed = true;
        }

        void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        void OnDestroy()
        {
            ClearGrid();
        }
    }
}
