using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game050v2_BubbleSort
{
    public class BubbleGridManager : MonoBehaviour
    {
        [SerializeField] BubbleSortGameManager _gameManager;

        [Header("Sprites")]
        [SerializeField] Sprite[] _colorSprites;  // index 0-4: Green,Yellow,Blue,Red,Purple
        [SerializeField] Sprite _fixedSprite;
        [SerializeField] Sprite _timerSprite;
        [SerializeField] Sprite _bombSprite;
        [SerializeField] Sprite _selectedSprite;

        private int _cols;
        private int _rows;
        private int _colorCount;
        private int _maxMoves;
        private int _movesRemaining;
        private int _minimumMoves;
        private bool _isActive;
        private float _timerDuration = 3f;
        private float _speedMultiplier = 1f;
        private float _complexityFactor;
        private bool _enableMatch;
        private bool _enableFixed;
        private bool _enableTimer;
        private bool _enableBomb;

        private BubbleCell[,] _grid;
        private BubbleCell _selectedCell;
        private List<BubbleCell> _allCells = new List<BubbleCell>();

        private bool _isProcessing;
        private Transform _gridRoot;

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            _speedMultiplier = config.speedMultiplier;
            _complexityFactor = config.complexityFactor;

            switch (stageNumber)
            {
                case 1: _cols = 3; _rows = 3; _colorCount = 2; _maxMoves = 10; _enableMatch = false; _enableFixed = false; _enableTimer = false; _enableBomb = false; break;
                case 2: _cols = 4; _rows = 4; _colorCount = 3; _maxMoves = 15; _enableMatch = true;  _enableFixed = false; _enableTimer = false; _enableBomb = false; break;
                case 3: _cols = 4; _rows = 5; _colorCount = 4; _maxMoves = 18; _enableMatch = true;  _enableFixed = true;  _enableTimer = false; _enableBomb = false; break;
                case 4: _cols = 5; _rows = 5; _colorCount = 4; _maxMoves = 20; _enableMatch = true;  _enableFixed = true;  _enableTimer = true;  _enableBomb = false; break;
                case 5: _cols = 5; _rows = 6; _colorCount = 5; _maxMoves = 22; _enableMatch = true;  _enableFixed = true;  _enableTimer = true;  _enableBomb = true;  break;
            }

            _movesRemaining = _maxMoves;
            _minimumMoves = _maxMoves / 2;
            _isActive = true;
            _isProcessing = false;
            _selectedCell = null;

            BuildGrid(stageNumber);
            _gameManager.OnSwapPerformed(_movesRemaining, _maxMoves);
        }

        void BuildGrid(int stageNumber)
        {
            ClearGrid();

            if (_gridRoot == null)
            {
                var go = new GameObject("GridRoot");
                go.transform.SetParent(transform);
                _gridRoot = go.transform;
            }

            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 2.8f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            float cellSize = Mathf.Min(availableHeight / _rows, camWidth * 2f / _cols, 1.2f);

            float totalW = cellSize * _cols;
            float totalH = cellSize * _rows;
            float startX = -totalW * 0.5f + cellSize * 0.5f;
            float startY = camSize - topMargin - cellSize * 0.5f;

            _grid = new BubbleCell[_cols, _rows];

            // Create initial color assignment (one color per row for easy undo check)
            int[] colorAssignment = CreateShuffledColorAssignment();

            // Determine fixed/timer/bomb positions
            HashSet<int> fixedPositions = new HashSet<int>();
            HashSet<int> timerPositions = new HashSet<int>();
            HashSet<int> bombPositions = new HashSet<int>();

            int totalCells = _cols * _rows;
            if (_enableFixed)
            {
                int fixedCount = Mathf.Max(1, Mathf.RoundToInt(totalCells * _complexityFactor));
                while (fixedPositions.Count < fixedCount)
                    fixedPositions.Add(Random.Range(0, totalCells));
            }
            if (_enableTimer && stageNumber >= 4)
            {
                int timerCount = 2;
                int attempts = 0;
                while (timerPositions.Count < timerCount && attempts < 50)
                {
                    int idx = Random.Range(0, totalCells);
                    if (!fixedPositions.Contains(idx)) timerPositions.Add(idx);
                    attempts++;
                }
            }
            if (_enableBomb && stageNumber >= 5)
            {
                int attempts = 0;
                while (bombPositions.Count < 2 && attempts < 50)
                {
                    int idx = Random.Range(0, totalCells);
                    if (!fixedPositions.Contains(idx) && !timerPositions.Contains(idx)) bombPositions.Add(idx);
                    attempts++;
                }
            }

            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _cols; col++)
                {
                    int flatIdx = row * _cols + col;
                    float x = startX + col * cellSize;
                    float y = startY - row * cellSize;

                    var cellGo = new GameObject($"Bubble_{col}_{row}");
                    cellGo.transform.SetParent(_gridRoot);
                    cellGo.transform.position = new Vector3(x, y, 0f);

                    var col2d = cellGo.AddComponent<CircleCollider2D>();
                    col2d.radius = cellSize * 0.45f;

                    var cell = cellGo.AddComponent<BubbleCell>();
                    cell.GridCol = col;
                    cell.GridRow = row;

                    BubbleType btype = BubbleType.Normal;
                    Sprite sprite;
                    int colorIdx = colorAssignment[flatIdx];

                    if (fixedPositions.Contains(flatIdx))
                    {
                        btype = BubbleType.Fixed;
                        colorIdx = -1; // fixed has no sort color
                        sprite = _fixedSprite;
                    }
                    else if (bombPositions.Contains(flatIdx))
                    {
                        btype = BubbleType.Bomb;
                        sprite = _bombSprite ?? _colorSprites[colorIdx % _colorSprites.Length];
                    }
                    else if (timerPositions.Contains(flatIdx))
                    {
                        btype = BubbleType.Timer;
                        sprite = _timerSprite ?? _colorSprites[colorIdx % _colorSprites.Length];
                    }
                    else
                    {
                        sprite = GetColorSprite(colorIdx);
                    }

                    float scale = cellSize * 0.9f;
                    float spriteSize = sprite != null ? sprite.rect.width / sprite.pixelsPerUnit : 1f;
                    float scaleF = spriteSize > 0 ? scale / spriteSize : 1f;
                    cellGo.transform.localScale = new Vector3(scaleF, scaleF, 1f);

                    cell.Setup(colorIdx, btype, sprite, col, row);
                    _grid[col, row] = cell;
                    _allCells.Add(cell);

                    if (btype == BubbleType.Timer)
                    {
                        cell.StartTimer(_timerDuration, _speedMultiplier, OnTimerBubbleExpired);
                    }
                }
            }
        }

        int[] CreateShuffledColorAssignment()
        {
            int total = _cols * _rows;
            int[] arr = new int[total];
            // Distribute colors evenly
            for (int i = 0; i < total; i++)
                arr[i] = i % _colorCount;
            // Fisher-Yates shuffle
            for (int i = total - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
            }
            return arr;
        }

        Sprite GetColorSprite(int colorIdx)
        {
            if (_colorSprites == null || colorIdx < 0 || colorIdx >= _colorSprites.Length) return null;
            return _colorSprites[colorIdx];
        }

        void Update()
        {
            if (!_isActive || _isProcessing) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 screenPos = Mouse.current.position.ReadValue();
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
                worldPos.z = 0f;

                Collider2D hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var cell = hit.GetComponent<BubbleCell>();
                    if (cell != null && cell.IsActive && cell.BubbleType != BubbleType.Fixed)
                    {
                        HandleCellTap(cell);
                    }
                }
                else
                {
                    // Deselect
                    if (_selectedCell != null)
                    {
                        _selectedCell.SetHighlight(false);
                        _selectedCell = null;
                    }
                }
            }
        }

        void HandleCellTap(BubbleCell cell)
        {
            if (_selectedCell == null)
            {
                _selectedCell = cell;
                cell.SetHighlight(true);
            }
            else if (_selectedCell == cell)
            {
                _selectedCell.SetHighlight(false);
                _selectedCell = null;
            }
            else
            {
                // Check adjacency
                int dc = Mathf.Abs(cell.GridCol - _selectedCell.GridCol);
                int dr = Mathf.Abs(cell.GridRow - _selectedCell.GridRow);
                if ((dc == 1 && dr == 0) || (dc == 0 && dr == 1))
                {
                    _selectedCell.SetHighlight(false);
                    StartCoroutine(PerformSwap(_selectedCell, cell));
                    _selectedCell = null;
                }
                else
                {
                    // Switch selection
                    _selectedCell.SetHighlight(false);
                    _selectedCell = cell;
                    cell.SetHighlight(true);
                }
            }
        }

        IEnumerator PerformSwap(BubbleCell a, BubbleCell b)
        {
            _isProcessing = true;

            // Swap in grid
            int ac = a.GridCol; int ar = a.GridRow;
            int bc = b.GridCol; int br = b.GridRow;
            _grid[ac, ar] = b;
            _grid[bc, br] = a;
            a.GridCol = bc; a.GridRow = br;
            b.GridCol = ac; b.GridRow = ar;

            // Swap positions
            Vector3 posA = a.transform.position;
            Vector3 posB = b.transform.position;
            a.transform.position = posB;
            b.transform.position = posA;

            a.PlaySwapAnimation();
            b.PlaySwapAnimation();
            yield return new WaitForSeconds(0.2f);

            _movesRemaining--;
            _gameManager.OnSwapPerformed(_movesRemaining, _maxMoves);

            bool chainMatch = false;
            if (_enableMatch)
            {
                yield return StartCoroutine(CheckAndClearMatches(() => chainMatch = true));
            }

            if (!chainMatch) _gameManager.ResetCombo();

            // Check sort complete
            if (IsSortComplete())
            {
                _isProcessing = false;
                _gameManager.OnSortComplete(_movesRemaining, _minimumMoves);
                yield break;
            }

            if (_movesRemaining <= 0)
            {
                _isProcessing = false;
                _gameManager.OnMovesExhausted();
                yield break;
            }

            _isProcessing = false;
        }

        IEnumerator CheckAndClearMatches(System.Action onMatch)
        {
            bool foundAny = false;
            bool found = true;
            while (found)
            {
                found = false;
                HashSet<BubbleCell> toRemove = new HashSet<BubbleCell>();

                // Check horizontal
                for (int row = 0; row < _rows; row++)
                {
                    for (int col = 0; col < _cols - 2; col++)
                    {
                        var list = GetHorizontalRun(col, row);
                        if (list.Count >= 3) foreach (var c in list) toRemove.Add(c);
                    }
                }
                // Check vertical
                for (int col = 0; col < _cols; col++)
                {
                    for (int row = 0; row < _rows - 2; row++)
                    {
                        var list = GetVerticalRun(col, row);
                        if (list.Count >= 3) foreach (var c in list) toRemove.Add(c);
                    }
                }

                if (toRemove.Count > 0)
                {
                    bool isChain = foundAny; // true only if a previous match already occurred
                    found = true;
                    foundAny = true;

                    // Handle bomb effect
                    if (_enableBomb)
                    {
                        HashSet<BubbleCell> bombExplosion = new HashSet<BubbleCell>();
                        foreach (var cell in toRemove)
                        {
                            if (cell.BubbleType == BubbleType.Bomb)
                            {
                                AddNeighbors(cell, bombExplosion);
                            }
                        }
                        foreach (var cell in bombExplosion)
                        {
                            if (cell != null && cell.BubbleType != BubbleType.Fixed) toRemove.Add(cell);
                        }
                    }

                    _gameManager.OnMatchCleared(toRemove.Count, isChain);

                    // Dissolve animation
                    int done = 0;
                    int total = toRemove.Count;
                    foreach (var cell in toRemove)
                    {
                        cell.StopTimer();
                        cell.PlayDissolveAnimation(() => done++);
                    }
                    yield return new WaitUntil(() => done >= total);

                    // Remove and drop down
                    foreach (var cell in toRemove)
                    {
                        if (_selectedCell == cell) _selectedCell = null;
                        _grid[cell.GridCol, cell.GridRow] = null;
                        _allCells.Remove(cell);
                        Destroy(cell.gameObject);
                    }

                    DropAndRefill();
                    yield return new WaitForSeconds(0.3f);
                }
            }

            if (foundAny) onMatch?.Invoke();
        }

        List<BubbleCell> GetHorizontalRun(int startCol, int row)
        {
            var result = new List<BubbleCell>();
            var first = _grid[startCol, row];
            if (first == null || first.BubbleType == BubbleType.Fixed) return result;
            int colorIdx = first.ColorIndex;
            for (int col = startCol; col < _cols; col++)
            {
                var c = _grid[col, row];
                if (c == null || c.ColorIndex != colorIdx || c.BubbleType == BubbleType.Fixed) break;
                result.Add(c);
            }
            return result.Count >= 3 ? result : new List<BubbleCell>();
        }

        List<BubbleCell> GetVerticalRun(int col, int startRow)
        {
            var result = new List<BubbleCell>();
            var first = _grid[col, startRow];
            if (first == null || first.BubbleType == BubbleType.Fixed) return result;
            int colorIdx = first.ColorIndex;
            for (int row = startRow; row < _rows; row++)
            {
                var c = _grid[col, row];
                if (c == null || c.ColorIndex != colorIdx || c.BubbleType == BubbleType.Fixed) break;
                result.Add(c);
            }
            return result.Count >= 3 ? result : new List<BubbleCell>();
        }

        void AddNeighbors(BubbleCell cell, HashSet<BubbleCell> set)
        {
            int[][] dirs = { new[]{1,0}, new[]{-1,0}, new[]{0,1}, new[]{0,-1} };
            foreach (var d in dirs)
            {
                int nc = cell.GridCol + d[0];
                int nr = cell.GridRow + d[1];
                if (nc >= 0 && nc < _cols && nr >= 0 && nr < _rows && _grid[nc, nr] != null)
                    set.Add(_grid[nc, nr]);
            }
        }

        void DropAndRefill()
        {
            // Drop existing cells down
            for (int col = 0; col < _cols; col++)
            {
                int writeRow = _rows - 1;
                for (int row = _rows - 1; row >= 0; row--)
                {
                    if (_grid[col, row] != null && _grid[col, row].BubbleType != BubbleType.Fixed)
                    {
                        if (row != writeRow)
                        {
                            _grid[col, writeRow] = _grid[col, row];
                            _grid[col, row] = null;
                            _grid[col, writeRow].GridRow = writeRow;
                            // Update position
                            UpdateCellPosition(_grid[col, writeRow]);
                        }
                        writeRow--;
                    }
                    else if (_grid[col, row] != null && _grid[col, row].BubbleType == BubbleType.Fixed)
                    {
                        writeRow = row - 1;
                    }
                }

                // Fill empty top rows with new bubbles
                for (int row = writeRow; row >= 0; row--)
                {
                    if (_grid[col, row] == null)
                    {
                        SpawnRefillBubble(col, row);
                    }
                }
            }
        }

        void UpdateCellPosition(BubbleCell cell)
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 2.8f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            float cellSize = Mathf.Min(availableHeight / _rows, camWidth * 2f / _cols, 1.2f);
            float totalW = cellSize * _cols;
            float startX = -totalW * 0.5f + cellSize * 0.5f;
            float startY = camSize - topMargin - cellSize * 0.5f;
            cell.transform.position = new Vector3(startX + cell.GridCol * cellSize, startY - cell.GridRow * cellSize, 0f);
        }

        void SpawnRefillBubble(int col, int row)
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 2.8f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            float cellSize = Mathf.Min(availableHeight / _rows, camWidth * 2f / _cols, 1.2f);
            float totalW = cellSize * _cols;
            float startX = -totalW * 0.5f + cellSize * 0.5f;
            float startY = camSize - topMargin - cellSize * 0.5f;

            int colorIdx = Random.Range(0, _colorCount);
            BubbleType btype = BubbleType.Normal;
            Sprite sprite = GetColorSprite(colorIdx);

            var cellGo = new GameObject($"Bubble_{col}_{row}");
            cellGo.transform.SetParent(_gridRoot);
            cellGo.transform.position = new Vector3(startX + col * cellSize, startY - row * cellSize, 0f);

            var col2d = cellGo.AddComponent<CircleCollider2D>();
            col2d.radius = cellSize * 0.45f;

            var cell = cellGo.AddComponent<BubbleCell>();
            cell.GridCol = col;
            cell.GridRow = row;

            float scale = cellSize * 0.9f;
            float spriteSize = sprite != null ? sprite.rect.width / sprite.pixelsPerUnit : 1f;
            float scaleF = spriteSize > 0 ? scale / spriteSize : 1f;
            cellGo.transform.localScale = new Vector3(scaleF, scaleF, 1f);

            cell.Setup(colorIdx, btype, sprite, col, row);
            _grid[col, row] = cell;
            _allCells.Add(cell);
        }

        void OnTimerBubbleExpired(BubbleCell cell)
        {
            if (!_isActive || _isProcessing) return;
            cell.PlayTimerExpireFlash();
            int newColor = Random.Range(0, _colorCount);
            cell.SetColorIndex(newColor, GetColorSprite(newColor));
        }

        bool IsSortComplete()
        {
            // All non-fixed bubbles of same color must be grouped together
            // Simple check: each color occupies contiguous region
            // For casual check: count how many normal cells are "correctly placed"
            // A bubble is correct if all its same-color neighbors form a connected component
            // Simplified: check if all cells in each color group are connected
            int[] colorCounts = new int[_colorCount];
            for (int col = 0; col < _cols; col++)
                for (int row = 0; row < _rows; row++)
                {
                    var c = _grid[col, row];
                    if (c != null && c.BubbleType == BubbleType.Normal && c.ColorIndex >= 0 && c.ColorIndex < _colorCount)
                        colorCounts[c.ColorIndex]++;
                }

            // Each color must form a single connected component
            bool[] visited = new bool[_cols * _rows];
            for (int ci = 0; ci < _colorCount; ci++)
            {
                if (colorCounts[ci] == 0) continue;

                // Find first cell of this color
                int startCol = -1, startRow = -1;
                for (int col = 0; col < _cols && startCol < 0; col++)
                    for (int row = 0; row < _rows && startCol < 0; row++)
                    {
                        var c = _grid[col, row];
                        if (c != null && c.ColorIndex == ci) { startCol = col; startRow = row; }
                    }

                if (startCol < 0) continue;

                // BFS to count connected
                int connected = 0;
                var queue = new Queue<(int, int)>();
                queue.Enqueue((startCol, startRow));
                var bfsVisited = new HashSet<int>();
                bfsVisited.Add(startCol * _rows + startRow);

                while (queue.Count > 0)
                {
                    var (cc, cr) = queue.Dequeue();
                    connected++;
                    int[][] dirs = { new[]{1,0}, new[]{-1,0}, new[]{0,1}, new[]{0,-1} };
                    foreach (var d in dirs)
                    {
                        int nc = cc + d[0];
                        int nr = cr + d[1];
                        if (nc < 0 || nc >= _cols || nr < 0 || nr >= _rows) continue;
                        var cell = _grid[nc, nr];
                        if (cell == null || cell.ColorIndex != ci) continue;
                        int key = nc * _rows + nr;
                        if (!bfsVisited.Contains(key))
                        {
                            bfsVisited.Add(key);
                            queue.Enqueue((nc, nr));
                        }
                    }
                }

                if (connected < colorCounts[ci]) return false;
            }
            return true;
        }

        public void UndoLastSwap()
        {
            // Undo is tracked externally; not implemented in grid (moves not refunded per spec)
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        public void ClearGrid()
        {
            foreach (var cell in _allCells)
            {
                if (cell != null) Destroy(cell.gameObject);
            }
            _allCells.Clear();
            _grid = null;
            _selectedCell = null;
        }

        void OnDestroy()
        {
            ClearGrid();
        }
    }
}
