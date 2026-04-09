using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game094v2_GravityPainter
{
    public enum GravityDir { Up, Down, Left, Right }

    public enum CellType { Empty, Wall, Absorb }

    public class PaintManager : MonoBehaviour
    {
        [SerializeField] GravityPainterGameManager _gameManager;
        [SerializeField] GravityPainterUI _ui;
        [SerializeField] Sprite _sprCellEmpty;
        [SerializeField] Sprite _sprCellWall;
        [SerializeField] Sprite _sprCellAbsorb;
        [SerializeField] Sprite _sprTargetOverlay;
        [SerializeField] Sprite[] _sprPaintDrops;   // index 0..3 = red, blue, green, yellow

        // Grid state
        int _gridSize;
        int _colorCount;
        int _paintBudget;
        bool _hasWalls;
        bool _hasAbsorb;
        bool _hasMix;
        float _timeLimit;

        CellType[,] _specialCells;   // Wall / Absorb / Empty
        int[,] _paintGrid;           // 0=empty, 1..4=colorIndex
        int[,] _targetGrid;          // target pattern

        GameObject[,] _cellBgObjects;
        GameObject[,] _paintObjects;
        GameObject[,] _targetOverlays;

        int _selectedColor = 1;
        int _remainingPaint;
        int _gravityChangesThisStage;
        bool _isActive;
        float _remainingTime;
        bool _hasTimeLimit;

        static readonly Color[] PaintColors = new Color[]
        {
            Color.clear,
            new Color(0.86f, 0.2f, 0.2f, 1f),   // 1 red
            new Color(0.2f, 0.39f, 0.86f, 1f),   // 2 blue
            new Color(0.2f, 0.78f, 0.31f, 1f),   // 3 green
            new Color(0.9f, 0.78f, 0.12f, 1f),   // 4 yellow
        };

        // Stage-level mix table: colorA + colorB = colorC
        static readonly (int, int, int)[] MixRules = new (int, int, int)[]
        {
            (1, 2, 3), // red + blue = green
            (1, 3, 4), // red + green = yellow
            (2, 4, 1), // blue + yellow = red
        };

        public void SetActive(bool value) => _isActive = value;

        public void SelectColor(int colorIndex)
        {
            if (colorIndex < 1 || colorIndex > _colorCount) return;
            _selectedColor = colorIndex;
        }

        public int GetSelectedColor() => _selectedColor;


        public void ApplyGravity(GravityDir dir)
        {
            if (!_isActive) return;

            bool anyMoved = MovePaint(dir);
            if (anyMoved)
            {
                _gravityChangesThisStage++;
                if (_hasMix) ApplyMixRules();
                UpdateAllVisuals();
                UpdateMatchRate(checkGameOver: false);
                StartCoroutine(FlashMovedCells());
            }
        }

        bool MovePaint(GravityDir dir)
        {
            bool moved = false;
            switch (dir)
            {
                case GravityDir.Down:
                    for (int c = 0; c < _gridSize; c++)
                        for (int r = _gridSize - 2; r >= 0; r--)
                            moved |= SlideCell(r, c, 1, 0);
                    break;
                case GravityDir.Up:
                    for (int c = 0; c < _gridSize; c++)
                        for (int r = 1; r < _gridSize; r++)
                            moved |= SlideCell(r, c, -1, 0);
                    break;
                case GravityDir.Right:
                    for (int r = 0; r < _gridSize; r++)
                        for (int c = _gridSize - 2; c >= 0; c--)
                            moved |= SlideCell(r, c, 0, 1);
                    break;
                case GravityDir.Left:
                    for (int r = 0; r < _gridSize; r++)
                        for (int c = 1; c < _gridSize; c++)
                            moved |= SlideCell(r, c, 0, -1);
                    break;
            }
            return moved;
        }

        bool SlideCell(int row, int col, int dr, int dc)
        {
            if (_paintGrid[row, col] == 0) return false;

            int nr = row + dr;
            int nc = col + dc;

            // Out of bounds
            if (nr < 0 || nr >= _gridSize || nc < 0 || nc >= _gridSize) return false;

            // Wall: stop
            if (_specialCells[nr, nc] == CellType.Wall) return false;

            // Absorb: consume paint
            if (_specialCells[nr, nc] == CellType.Absorb)
            {
                _paintGrid[row, col] = 0;
                return true;
            }

            // Move into empty
            if (_paintGrid[nr, nc] == 0)
            {
                _paintGrid[nr, nc] = _paintGrid[row, col];
                _paintGrid[row, col] = 0;
                SlideCell(nr, nc, dr, dc); // keep sliding
                return true;
            }

            // Collide: stay (already occupied)
            return false;
        }

        void ApplyMixRules()
        {
            // For each cell, if paint exists, check neighbors for mix
            int[,] newGrid = (int[,])_paintGrid.Clone();
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    if (_paintGrid[r, c] == 0) continue;
                    int colorA = _paintGrid[r, c];
                    // Check 4 neighbors
                    int[] dr = { -1, 1, 0, 0 };
                    int[] dc = { 0, 0, -1, 1 };
                    for (int d = 0; d < 4; d++)
                    {
                        int nr = r + dr[d];
                        int nc = c + dc[d];
                        if (nr < 0 || nr >= _gridSize || nc < 0 || nc >= _gridSize) continue;
                        if (_paintGrid[nr, nc] == 0) continue;
                        int colorB = _paintGrid[nr, nc];
                        foreach (var rule in MixRules)
                        {
                            if ((rule.Item1 == colorA && rule.Item2 == colorB) ||
                                (rule.Item1 == colorB && rule.Item2 == colorA))
                            {
                                newGrid[r, c] = rule.Item3;
                                newGrid[nr, nc] = rule.Item3;
                                goto nextCell;
                            }
                        }
                    }
                    nextCell:;
                }
            }
            _paintGrid = newGrid;
        }

        void UpdateMatchRate(bool checkGameOver = true)
        {
            int total = 0;
            int matched = 0;
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    if (_targetGrid[r, c] == 0) continue;
                    total++;
                    if (_paintGrid[r, c] == _targetGrid[r, c]) matched++;
                }
            }
            float rate = total > 0 ? (float)matched / total : 0f;

            _ui?.UpdateMatchRate(rate);

            if (rate >= 0.5f)
            {
                _isActive = false;
                _gameManager?.OnStageClear(rate, _remainingPaint, _gravityChangesThisStage);
                return;
            }

            if (checkGameOver && _remainingPaint <= 0)
            {
                _isActive = false;
                _gameManager?.OnGameOver();
            }
        }

        #region Stage Setup


        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            ClearGrid();

            switch (stageIndex)
            {
                case 0: SetupStageData(6, 1, 8,  false, false, false, 0f); break;
                case 1: SetupStageData(6, 2, 10, false, false, false, 0f); break;
                case 2: SetupStageData(7, 3, 10, true,  false, false, 0f); break;
                case 3: SetupStageData(7, 3, 9,  true,  true,  false, 0f); break;
                case 4: SetupStageData(8, 4, 10, false, false, true,  60f); break;
                default: SetupStageData(6, 1, 8, false, false, false, 0f); break;
            }

            BuildTargetGrid(stageIndex);
            BuildCellVisuals();
            _selectedColor = 1;
            _gravityChangesThisStage = 0;
            _isActive = true;

            _ui?.UpdatePaintCount(_remainingPaint);
            _ui?.UpdateMatchRate(0f);
            _ui?.SetColorCount(_colorCount);
        }

        void SetupStageData(int gridSize, int colorCount, int paintBudget,
            bool hasWalls, bool hasAbsorb, bool hasMix, float timeLimit)
        {
            _gridSize = gridSize;
            _colorCount = colorCount;
            _paintBudget = paintBudget;
            _remainingPaint = paintBudget;
            _hasWalls = hasWalls;
            _hasAbsorb = hasAbsorb;
            _hasMix = hasMix;
            _timeLimit = timeLimit;
            _hasTimeLimit = timeLimit > 0f;
            _remainingTime = timeLimit;

            _specialCells = new CellType[gridSize, gridSize];
            _paintGrid = new int[gridSize, gridSize];
            _targetGrid = new int[gridSize, gridSize];

            if (hasWalls) PlaceWalls(gridSize);
            if (hasAbsorb) PlaceAbsorb(gridSize);
        }

        void PlaceWalls(int gridSize)
        {
            // Place walls along edges of inner region
            int mid = gridSize / 2;
            _specialCells[mid, 0] = CellType.Wall;
            _specialCells[mid, gridSize - 1] = CellType.Wall;
            _specialCells[0, mid] = CellType.Wall;
            _specialCells[gridSize - 1, mid] = CellType.Wall;
        }

        void PlaceAbsorb(int gridSize)
        {
            int mid = gridSize / 2;
            _specialCells[mid, mid] = CellType.Absorb;
            _specialCells[mid - 1, mid] = CellType.Absorb;
        }

        void BuildTargetGrid(int stageIndex)
        {
            int gs = _gridSize;
            _targetGrid = new int[gs, gs];

            // Generate simple target patterns per stage
            switch (stageIndex)
            {
                case 0:
                    // Fill inner 4×4 with color 1
                    for (int r = 1; r < gs - 1; r++)
                        for (int c = 1; c < gs - 1; c++)
                            _targetGrid[r, c] = 1;
                    break;
                case 1:
                    // Left half color1, right half color2
                    for (int r = 0; r < gs; r++)
                        for (int c = 0; c < gs; c++)
                            _targetGrid[r, c] = (c < gs / 2) ? 1 : 2;
                    break;
                case 2:
                    // Checkerboard of colors 1,2,3
                    int[] colors3 = { 1, 2, 3 };
                    for (int r = 0; r < gs; r++)
                        for (int c = 0; c < gs; c++)
                            if (_specialCells[r, c] == CellType.Empty)
                                _targetGrid[r, c] = colors3[(r + c) % 3];
                    break;
                case 3:
                    // Diagonal stripes
                    for (int r = 0; r < gs; r++)
                        for (int c = 0; c < gs; c++)
                            if (_specialCells[r, c] == CellType.Empty)
                                _targetGrid[r, c] = ((r + c) % 3) + 1;
                    break;
                case 4:
                    // Quadrant pattern 4 colors
                    int half = gs / 2;
                    for (int r = 0; r < gs; r++)
                        for (int c = 0; c < gs; c++)
                        {
                            int q = (r < half ? 0 : 2) + (c < half ? 0 : 1);
                            _targetGrid[r, c] = q + 1;
                        }
                    break;
            }
        }

        #endregion

        #region Visuals

        void ClearGrid()
        {
            if (_cellBgObjects != null)
                foreach (var go in _cellBgObjects)
                    if (go != null) Destroy(go);
            if (_paintObjects != null)
                foreach (var go in _paintObjects)
                    if (go != null) Destroy(go);
            if (_targetOverlays != null)
                foreach (var go in _targetOverlays)
                    if (go != null) Destroy(go);
        }

        void BuildCellVisuals()
        {
            float camSize = Camera.main != null ? Camera.main.orthographicSize : 6f;
            float camWidth = camSize * (Camera.main != null ? Camera.main.aspect : 0.5625f);
            float topMargin = 1.5f;
            float bottomMargin = 3.5f;
            float availH = camSize * 2f - topMargin - bottomMargin;
            float cellSize = Mathf.Min(availH / _gridSize, camWidth * 2f / _gridSize, 1.2f);
            float gridW = cellSize * _gridSize;
            float gridH = cellSize * _gridSize;
            float startX = -gridW / 2f + cellSize / 2f;
            float startY = (camSize - topMargin - gridH / 2f);

            _cellBgObjects = new GameObject[_gridSize, _gridSize];
            _paintObjects  = new GameObject[_gridSize, _gridSize];
            _targetOverlays = new GameObject[_gridSize, _gridSize];

            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    float x = startX + c * cellSize;
                    float y = startY - r * cellSize;
                    Vector3 pos = new Vector3(x, y, 0f);

                    // Cell background
                    var bgGo = new GameObject($"Cell_{r}_{c}");
                    bgGo.transform.SetParent(transform);
                    bgGo.transform.position = pos;
                    bgGo.transform.localScale = Vector3.one * cellSize * 0.95f;
                    var bgSr = bgGo.AddComponent<SpriteRenderer>();
                    bgSr.sortingOrder = 0;
                    bgSr.sprite = GetCellSprite(r, c);
                    _cellBgObjects[r, c] = bgGo;

                    // Target overlay
                    if (_targetGrid[r, c] > 0)
                    {
                        var tGo = new GameObject($"Target_{r}_{c}");
                        tGo.transform.SetParent(transform);
                        tGo.transform.position = pos + new Vector3(0, 0, -0.1f);
                        tGo.transform.localScale = Vector3.one * cellSize * 0.92f;
                        var tSr = tGo.AddComponent<SpriteRenderer>();
                        tSr.sortingOrder = 1;
                        tSr.sprite = _sprTargetOverlay;
                        int tcIdx = Mathf.Clamp(_targetGrid[r, c], 0, PaintColors.Length - 1);
                        Color tc = PaintColors[tcIdx];
                        tc.a = 0.45f;
                        tSr.color = tc;
                        _targetOverlays[r, c] = tGo;
                    }

                    // Paint object
                    var pGo = new GameObject($"Paint_{r}_{c}");
                    pGo.transform.SetParent(transform);
                    pGo.transform.position = pos + new Vector3(0, 0, -0.2f);
                    pGo.transform.localScale = Vector3.one * cellSize * 0.7f;
                    var pSr = pGo.AddComponent<SpriteRenderer>();
                    pSr.sortingOrder = 2;
                    pSr.color = Color.clear;
                    pSr.sprite = _sprPaintDrops != null && _sprPaintDrops.Length > 0 ? _sprPaintDrops[0] : null;
                    _paintObjects[r, c] = pGo;

                    // Add collider for click detection
                    var bc = bgGo.AddComponent<BoxCollider2D>();
                    bc.size = Vector2.one;
                    bgGo.name = $"Cell_{r}_{c}";
                }
            }
        }

        Sprite GetCellSprite(int r, int c)
        {
            switch (_specialCells[r, c])
            {
                case CellType.Wall:  return _sprCellWall;
                case CellType.Absorb: return _sprCellAbsorb;
                default: return _sprCellEmpty;
            }
        }

        void UpdateCellVisual(int row, int col)
        {
            if (_paintObjects == null) return;
            var go = _paintObjects[row, col];
            if (go == null) return;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) return;

            int colorIdx = _paintGrid[row, col];
            if (colorIdx == 0)
            {
                sr.color = Color.clear;
            }
            else
            {
                if (_sprPaintDrops == null || _sprPaintDrops.Length == 0) return;
                int sprIdx = Mathf.Clamp(colorIdx - 1, 0, _sprPaintDrops.Length - 1);
                sr.sprite = _sprPaintDrops[sprIdx];
                sr.color = Color.white;
            }
        }

        void UpdateAllVisuals()
        {
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    UpdateCellVisual(r, c);
        }

        IEnumerator PulseCell(int row, int col)
        {
            if (_paintObjects == null || _paintObjects[row, col] == null) yield break;
            var t = _paintObjects[row, col].transform;
            float cellSize = t.localScale.x;
            float dur = 0.2f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                if (t == null) yield break;
                float ratio = elapsed / dur;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(cellSize, cellSize * 1.3f, ratio * 2f)
                    : Mathf.Lerp(cellSize * 1.3f, cellSize, (ratio - 0.5f) * 2f);
                t.localScale = Vector3.one * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            t.localScale = Vector3.one * cellSize;
        }

        IEnumerator FlashMovedCells()
        {
            float dur = 0.1f;
            var srs = new List<SpriteRenderer>();
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    if (_paintGrid[r, c] > 0 && _paintObjects[r, c] != null)
                    {
                        var sr = _paintObjects[r, c].GetComponent<SpriteRenderer>();
                        if (sr != null) srs.Add(sr);
                    }

            Color bright = new Color(1f, 1f, 0.7f, 1f);
            foreach (var sr in srs) sr.color = bright;
            yield return new WaitForSeconds(dur);
            foreach (var sr in srs)
                if (sr != null) sr.color = Color.white;
        }

        #endregion

        #region Input

        void Update()
        {
            if (!_isActive) return;

            // Time limit countdown (Stage 5)
            if (_hasTimeLimit)
            {
                _remainingTime -= Time.deltaTime;
                _ui?.UpdateTimeRemaining(_remainingTime);
                if (_remainingTime <= 0f)
                {
                    _remainingTime = 0f;
                    _isActive = false;
                    _gameManager?.OnGameOver();
                    return;
                }
            }

            // Click input for paint drop
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                var cam = Camera.main;
                if (cam == null) return;
                var mousePos = mouse.position.ReadValue();
                var worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
                worldPos.z = 0f;

                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null && hit.gameObject.name.StartsWith("Cell_"))
                {
                    string[] parts = hit.gameObject.name.Split('_');
                    if (parts.Length == 3 &&
                        int.TryParse(parts[1], out int row) &&
                        int.TryParse(parts[2], out int col))
                    {
                        TryDropPaintAt(row, col);
                    }
                }
            }
        }

        void TryDropPaintAt(int row, int col)
        {
            if (!_isActive || _remainingPaint <= 0) return;
            if (_specialCells[row, col] == CellType.Wall) return;

            _paintGrid[row, col] = _selectedColor;
            _remainingPaint--;
            UpdateCellVisual(row, col);
            StartCoroutine(PulseCell(row, col));

            _ui?.UpdatePaintCount(_remainingPaint);
            UpdateMatchRate();
        }

        #endregion

        void OnDestroy()
        {
            ClearGrid();
        }
    }
}
