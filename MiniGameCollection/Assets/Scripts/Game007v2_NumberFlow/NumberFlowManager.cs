using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game007v2_NumberFlow
{
    public enum CellType { Normal, Wall, WarpA, WarpB, DirectionUp, DirectionDown, DirectionLeft, DirectionRight }

    public class CellData
    {
        public Vector2Int GridPos;
        public CellType Type;
        public int HintNumber; // 0 = no hint
        public int PathIndex;  // -1 = not in path
        public GameObject Obj;
        public SpriteRenderer Sr;
        public GameObject NumberLabel; // optional TextMeshPro for hint/path number
    }

    public class NumberFlowManager : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] Sprite _spNormal;
        [SerializeField] Sprite _spWall;
        [SerializeField] Sprite _spWarpA;
        [SerializeField] Sprite _spWarpB;
        [SerializeField] Sprite _spDirection;

        [Header("Refs")]
        [SerializeField] NumberFlowGameManager _gameManager;

        // Grid state
        int _gridN;
        CellData[,] _grid;
        List<Vector2Int> _path = new List<Vector2Int>();
        float _cellSize;
        Vector3 _gridOrigin;
        bool _isActive;
        int _totalCells;
        float _startTime;
        float _targetTime;
        int _undoCount;

        // Stage info
        int _stageIndex;

        // Warp pair
        Vector2Int _warpA = new Vector2Int(-1, -1);
        Vector2Int _warpB = new Vector2Int(-1, -1);

        // Cached colours
        static readonly Color ColNormal      = new Color(0.26f, 0.55f, 0.96f, 1f);
        static readonly Color ColSelected    = new Color(0.10f, 0.90f, 0.60f, 1f);
        static readonly Color ColWall        = new Color(0.38f, 0.38f, 0.44f, 1f);
        static readonly Color ColWarpA       = new Color(0.81f, 0.58f, 0.85f, 1f);
        static readonly Color ColWarpB       = new Color(1.00f, 0.72f, 0.25f, 1f);
        static readonly Color ColDirection   = new Color(0.30f, 0.82f, 0.88f, 1f);

        // Input tracking
        bool _pointerDown;
        Vector2Int _lastDragCell = new Vector2Int(-999, -999);

        // ───────── Setup ─────────

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = Mathf.Max(1, stageIndex);
            ClearGrid();

            // Determine grid size and features
            _gridN = stageIndex <= 1 ? 4 : stageIndex <= 3 ? 5 : 6;

            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.4f;
            float bottomMargin = 3.0f;
            float availableH = camSize * 2f - topMargin - bottomMargin;
            _cellSize = Mathf.Min(availableH / _gridN, camWidth * 2f / _gridN, 1.8f);

            float totalW = _cellSize * _gridN;
            float totalH = _cellSize * _gridN;
            float startY = (camSize - topMargin) - _cellSize * 0.5f;
            float startX = -totalW * 0.5f + _cellSize * 0.5f;
            _gridOrigin = new Vector3(startX, startY - totalH * 0.5f + _cellSize * 0.5f, 0f);

            _totalCells = _gridN * _gridN;

            // Determine special features based on stageIndex
            bool hasWall = stageIndex == 3;
            bool hasWarp = stageIndex == 4;
            bool hasDirection = stageIndex == 5;

            // Determine hint count
            int hintCount = config.countMultiplier >= 2f ? 5
                          : config.countMultiplier >= 1.5f ? 3
                          : 2;

            // Generate a valid Hamiltonian path for this grid
            List<Vector2Int> solution = GenerateSolution(_gridN);

            _grid = new CellData[_gridN, _gridN];
            _warpA = new Vector2Int(-1, -1);
            _warpB = new Vector2Int(-1, -1);

            // Place cells
            for (int y = 0; y < _gridN; y++)
            {
                for (int x = 0; x < _gridN; x++)
                {
                    var cell = new CellData();
                    cell.GridPos = new Vector2Int(x, y);
                    cell.Type = CellType.Normal;
                    cell.PathIndex = -1;
                    cell.HintNumber = 0;

                    var obj = new GameObject($"Cell_{x}_{y}");
                    obj.transform.SetParent(transform);
                    obj.transform.localPosition = GridToWorld(x, y);

                    var bc = obj.AddComponent<BoxCollider2D>();
                    bc.size = Vector2.one * _cellSize * 0.95f;

                    var sr = obj.AddComponent<SpriteRenderer>();
                    sr.sprite = _spNormal;
                    sr.color = ColNormal;
                    sr.sortingOrder = 0;
                    float scale = _cellSize * 0.92f / 1f;
                    obj.transform.localScale = new Vector3(scale, scale, 1f);

                    cell.Obj = obj;
                    cell.Sr = sr;
                    _grid[x, y] = cell;
                }
            }

            // Apply wall cells (stage 3)
            if (hasWall)
            {
                // Place 2 wall cells not at path endpoints
                var wallCandidates = new List<Vector2Int>();
                for (int i = 2; i < solution.Count - 2; i++)
                    wallCandidates.Add(solution[i]);
                if (wallCandidates.Count >= 2)
                {
                    // Remove from solution - but we need to regenerate.
                    // Simpler: mark mid-cells of a different path segment as wall display only
                    // For a clean implementation, mark 2 cells at specific positions as visual walls
                    // but keep them in the path. This is a visual hint "avoid these" is too complex.
                    // Instead, place walls at cells NOT in the solution path if possible.
                    var nonSolution = new List<Vector2Int>();
                    var solutionSet = new HashSet<Vector2Int>(solution);
                    for (int y = 0; y < _gridN; y++)
                        for (int x = 0; x < _gridN; x++)
                            if (!solutionSet.Contains(new Vector2Int(x, y)))
                                nonSolution.Add(new Vector2Int(x, y));
                    // If all cells are in solution (Hamiltonian), walls must replace solution cells
                    // Just pick 2 interior solution cells and regenerate around them — too complex.
                    // Use a pre-designed puzzle for wall stages instead.
                    ApplyWallStage();
                }
                else
                {
                    ApplyWallStage();
                }
            }
            else if (hasWarp)
            {
                ApplyWarpStage(solution);
            }
            else if (hasDirection)
            {
                ApplyDirectionStage(solution);
            }
            else
            {
                // Place hint numbers along solution path
                PlaceHints(solution, hintCount);
            }

            _path.Clear();
            _undoCount = 0;
            _startTime = Time.time;
            _targetTime = _totalCells * 3f; // 3 seconds per cell target
            _isActive = true;
        }

        void ApplyWallStage()
        {
            if (_gridN != 5) { Debug.LogError($"[NumberFlow] ApplyWallStage requires gridN=5, got {_gridN}"); return; }
            // Use a handcrafted 5x5 puzzle with 2 wall cells
            // Solution path avoids wall cells at (1,2) and (3,2)
            int[,] layout = {
                {1, 0, 0, 0,25},
                {0, 0, 0, 0, 0},
                {0,-1, 0,-1, 0},
                {0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0}
            };
            // Mark walls
            for (int y = 0; y < _gridN; y++)
                for (int x = 0; x < _gridN; x++)
                {
                    if (layout[y, x] == -1)
                    {
                        _grid[x, y].Type = CellType.Wall;
                        _grid[x, y].Sr.sprite = _spWall;
                        _grid[x, y].Sr.color = ColWall;
                    }
                    else if (layout[y, x] == 1 || layout[y, x] == 25)
                    {
                        _grid[x, y].HintNumber = layout[y, x];
                        SetHintLabel(_grid[x, y], layout[y, x]);
                    }
                }
            _totalCells = _gridN * _gridN - 2; // 2 wall cells
        }

        void ApplyWarpStage(List<Vector2Int> solution)
        {
            // Mark 2 warp cells (warpA teleports to warpB)
            // Use cells near 1/3 and 2/3 of solution
            int idxA = solution.Count / 3;
            int idxB = solution.Count * 2 / 3;
            _warpA = solution[idxA];
            _warpB = solution[idxB];
            _grid[_warpA.x, _warpA.y].Type = CellType.WarpA;
            _grid[_warpA.x, _warpA.y].Sr.sprite = _spWarpA;
            _grid[_warpA.x, _warpA.y].Sr.color = ColWarpA;
            _grid[_warpB.x, _warpB.y].Type = CellType.WarpB;
            _grid[_warpB.x, _warpB.y].Sr.sprite = _spWarpB;
            _grid[_warpB.x, _warpB.y].Sr.color = ColWarpB;

            PlaceHints(solution, 3);
        }

        void ApplyDirectionStage(List<Vector2Int> solution)
        {
            // Mark 2 direction-limited cells along solution
            int idxA = Mathf.Max(1, solution.Count / 4);
            int idxB = Mathf.Max(idxA + 1, solution.Count * 3 / 4);
            Vector2Int posA = solution[idxA];
            Vector2Int posB = solution[idxB];
            // Direction from previous to this cell in solution
            CellType dirA = GetDirectionType(solution[idxA - 1], solution[idxA]);
            CellType dirB = GetDirectionType(solution[idxB - 1], solution[idxB]);

            _grid[posA.x, posA.y].Type = dirA;
            _grid[posA.x, posA.y].Sr.sprite = _spDirection;
            _grid[posA.x, posA.y].Sr.color = ColDirection;
            _grid[posB.x, posB.y].Type = dirB;
            _grid[posB.x, posB.y].Sr.sprite = _spDirection;
            _grid[posB.x, posB.y].Sr.color = ColDirection;

            PlaceHints(solution, 2);
        }

        CellType GetDirectionType(Vector2Int from, Vector2Int to)
        {
            var delta = to - from;
            if (delta == Vector2Int.up) return CellType.DirectionUp;
            if (delta == Vector2Int.down) return CellType.DirectionDown;
            if (delta == Vector2Int.left) return CellType.DirectionLeft;
            return CellType.DirectionRight;
        }

        void PlaceHints(List<Vector2Int> solution, int hintCount)
        {
            // Always hint first and last
            _grid[solution[0].x, solution[0].y].HintNumber = 1;
            SetHintLabel(_grid[solution[0].x, solution[0].y], 1);
            _grid[solution[solution.Count - 1].x, solution[solution.Count - 1].y].HintNumber = solution.Count;
            SetHintLabel(_grid[solution[solution.Count - 1].x, solution[solution.Count - 1].y], solution.Count);

            if (hintCount <= 2) return;
            int placed = 2;
            int step = solution.Count / (hintCount - 1);
            for (int i = step; i < solution.Count - 1 && placed < hintCount; i += step)
            {
                var pos = solution[i];
                if (_grid[pos.x, pos.y].HintNumber == 0)
                {
                    _grid[pos.x, pos.y].HintNumber = i + 1;
                    SetHintLabel(_grid[pos.x, pos.y], i + 1);
                    placed++;
                }
            }
        }

        void SetHintLabel(CellData cell, int number)
        {
            if (cell.NumberLabel != null) Destroy(cell.NumberLabel);
            // Create a child TextMesh label
            var labelObj = new GameObject("HintLabel");
            labelObj.transform.SetParent(cell.Obj.transform);
            labelObj.transform.localPosition = Vector3.zero;
            labelObj.transform.localScale = Vector3.one;

            var tm = labelObj.AddComponent<TMPro.TextMeshPro>();
            tm.text = number.ToString();
            tm.fontSize = Mathf.Clamp(_cellSize * 2.5f, 3f, 8f);
            tm.alignment = TMPro.TextAlignmentOptions.Center;
            tm.color = Color.white;
            tm.sortingOrder = 2;
            cell.NumberLabel = labelObj;
        }

        // ───────── Hamiltonian path generation ─────────

        List<Vector2Int> GenerateSolution(int n)
        {
            // Use simple serpentine (snake) path as guaranteed Hamiltonian path
            var path = new List<Vector2Int>();
            for (int y = n - 1; y >= 0; y--)
            {
                if ((n - 1 - y) % 2 == 0)
                    for (int x = 0; x < n; x++)
                        path.Add(new Vector2Int(x, y));
                else
                    for (int x = n - 1; x >= 0; x--)
                        path.Add(new Vector2Int(x, y));
            }
            return path;
        }

        // ───────── World / Grid utils ─────────

        Vector3 GridToWorld(int x, int y)
        {
            float camSize = Camera.main.orthographicSize;
            float topMargin = 1.4f;
            float bottomMargin = 3.0f;
            float availableH = camSize * 2f - topMargin - bottomMargin;
            float cs = Mathf.Min(availableH / _gridN, camSize * Camera.main.aspect * 2f / _gridN, 1.8f);
            float totalW = cs * _gridN;
            float totalH = cs * _gridN;
            float ox = -totalW * 0.5f + cs * 0.5f;
            float oy = (camSize - topMargin) - totalH + cs * 0.5f;
            return new Vector3(ox + x * cs, oy + y * cs, 0f);
        }

        Vector2Int WorldToGrid(Vector2 world)
        {
            float camSize = Camera.main.orthographicSize;
            float topMargin = 1.4f;
            float bottomMargin = 3.0f;
            float availableH = camSize * 2f - topMargin - bottomMargin;
            float cs = Mathf.Min(availableH / _gridN, camSize * Camera.main.aspect * 2f / _gridN, 1.8f);
            float totalW = cs * _gridN;
            float totalH = cs * _gridN;
            float ox = -totalW * 0.5f;
            float oy = (camSize - topMargin) - totalH;
            int gx = Mathf.FloorToInt((world.x - ox) / cs);
            int gy = Mathf.FloorToInt((world.y - oy) / cs);
            return new Vector2Int(gx, gy);
        }

        bool InBounds(int x, int y) => x >= 0 && x < _gridN && y >= 0 && y < _gridN;

        // ───────── Input handling ─────────

        void Update()
        {
            if (!_isActive) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            bool pressed = mouse.leftButton.wasPressedThisFrame;
            if (pressed)
            {
                _pointerDown = true;
                _lastDragCell = new Vector2Int(-999, -999);
                HandlePointer(mouse.position.ReadValue(), pressed);
            }
            else if (mouse.leftButton.isPressed && _pointerDown)
            {
                HandlePointer(mouse.position.ReadValue(), false);
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                _pointerDown = false;
            }
        }

        void HandlePointer(Vector2 screenPos, bool isPress)
        {
            var worldPos = (Vector2)Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
            var gp = WorldToGrid(worldPos);
            if (gp == _lastDragCell) return;
            if (!InBounds(gp.x, gp.y)) return;

            _lastDragCell = gp;
            var cell = _grid[gp.x, gp.y];
            if (cell.Type == CellType.Wall) return;

            // Check if tapping last cell → undo
            if (_path.Count > 0 && _path[_path.Count - 1] == gp)
            {
                if (isPress)
                    UndoLastStep();
                return;
            }

            // Check if already in path (not last) → ignore
            if (cell.PathIndex >= 0) return;

            // Check adjacency (or warp)
            if (_path.Count > 0)
            {
                var prev = _path[_path.Count - 1];
                if (!IsValidMove(prev, gp)) return;
            }

            // Check hint constraint: if cell has a hint, it must match current path index+1
            if (cell.HintNumber > 0 && cell.HintNumber != _path.Count + 1) return;

            // Check direction constraint from previous cell
            if (_path.Count > 0)
            {
                var prev = _path[_path.Count - 1];
                var prevCell = _grid[prev.x, prev.y];
                if (!IsDirectionAllowed(prevCell, gp)) return;
            }

            // Add to path
            AddToPath(gp);
        }

        bool IsValidMove(Vector2Int from, Vector2Int to)
        {
            // Warp: stepping onto warpA or warpB from adjacent
            var delta = to - from;
            bool adjacent = (Mathf.Abs(delta.x) + Mathf.Abs(delta.y)) == 1;
            if (adjacent) return true;

            // Warp teleport: if previous is warpA and to is warpB or vice versa
            var fromCell = _grid[from.x, from.y];
            if (fromCell.Type == CellType.WarpA && to == _warpB) return true;
            if (fromCell.Type == CellType.WarpB && to == _warpA) return true;

            return false;
        }

        bool IsDirectionAllowed(CellData fromCell, Vector2Int to)
        {
            var delta = to - fromCell.GridPos;
            switch (fromCell.Type)
            {
                case CellType.DirectionUp:    return delta == Vector2Int.up;
                case CellType.DirectionDown:  return delta == Vector2Int.down;
                case CellType.DirectionLeft:  return delta == Vector2Int.left;
                case CellType.DirectionRight: return delta == Vector2Int.right;
                default: return true;
            }
        }

        void AddToPath(Vector2Int gp)
        {
            var cell = _grid[gp.x, gp.y];
            cell.PathIndex = _path.Count;
            _path.Add(gp);

            // Visual: highlight + pulse animation
            cell.Sr.color = ColSelected;
            StartCoroutine(PulseCell(cell.Obj.transform));

            // Update number label if no hint
            if (cell.HintNumber == 0)
                SetPathLabel(cell, _path.Count);

            // Check clear
            if (_path.Count == _totalCells)
                CheckClear();
        }

        void SetPathLabel(CellData cell, int number)
        {
            if (cell.NumberLabel == null)
            {
                var labelObj = new GameObject("PathLabel");
                labelObj.transform.SetParent(cell.Obj.transform);
                labelObj.transform.localPosition = Vector3.zero;
                labelObj.transform.localScale = Vector3.one;
                var tm = labelObj.AddComponent<TMPro.TextMeshPro>();
                tm.text = number.ToString();
                tm.fontSize = Mathf.Clamp(_cellSize * 2.5f, 3f, 8f);
                tm.alignment = TMPro.TextAlignmentOptions.Center;
                tm.color = Color.white;
                tm.sortingOrder = 2;
                cell.NumberLabel = labelObj;
            }
            else
            {
                cell.NumberLabel.GetComponent<TMPro.TextMeshPro>().text = number.ToString();
            }
        }

        void UndoLastStep()
        {
            var last = _path[_path.Count - 1];
            var cell = _grid[last.x, last.y];

            // Remove path label if not hint
            if (cell.HintNumber == 0 && cell.NumberLabel != null)
            {
                Destroy(cell.NumberLabel);
                cell.NumberLabel = null;
            }

            cell.PathIndex = -1;
            cell.Sr.color = GetDefaultColor(cell.Type);
            _path.RemoveAt(_path.Count - 1);
            _undoCount++;
        }

        Color GetDefaultColor(CellType t)
        {
            switch (t)
            {
                case CellType.Wall: return ColWall;
                case CellType.WarpA: return ColWarpA;
                case CellType.WarpB: return ColWarpB;
                case CellType.DirectionUp:
                case CellType.DirectionDown:
                case CellType.DirectionLeft:
                case CellType.DirectionRight: return ColDirection;
                default: return ColNormal;
            }
        }

        public void ResetPath()
        {
            for (int i = _path.Count - 1; i >= 0; i--)
            {
                var pos = _path[i];
                var cell = _grid[pos.x, pos.y];
                if (cell.HintNumber == 0 && cell.NumberLabel != null)
                {
                    Destroy(cell.NumberLabel);
                    cell.NumberLabel = null;
                }
                cell.PathIndex = -1;
                cell.Sr.color = GetDefaultColor(cell.Type);
            }
            _path.Clear();
            _undoCount = 0;
            StartCoroutine(CameraShake(0.1f, 0.08f));
        }

        void CheckClear()
        {
            _isActive = false;
            StartCoroutine(ClearFlash());
        }

        IEnumerator ClearFlash()
        {
            // Green flash across all cells
            for (int rep = 0; rep < 3; rep++)
            {
                foreach (var pos in _path)
                    _grid[pos.x, pos.y].Sr.color = new Color(0.2f, 0.95f, 0.4f);
                yield return new WaitForSeconds(0.1f);
                foreach (var pos in _path)
                    _grid[pos.x, pos.y].Sr.color = ColSelected;
                yield return new WaitForSeconds(0.1f);
            }

            float elapsed = Time.time - _startTime;
            bool noUndo = _undoCount == 0;
            bool fastClear = elapsed <= _targetTime * 0.5f;
            int baseScore = _totalCells * 100 * _stageIndex;
            _gameManager.OnStageClear(baseScore, noUndo, fastClear);
        }

        IEnumerator PulseCell(Transform t)
        {
            float duration = 0.1f;
            float baseScale = t.localScale.x;
            float targetScale = baseScale * 1.25f;
            float timer = 0f;
            while (timer < duration)
            {
                float s = Mathf.Lerp(baseScale, targetScale, timer / duration);
                t.localScale = new Vector3(s, s, 1f);
                timer += Time.deltaTime;
                yield return null;
            }
            timer = 0f;
            while (timer < duration)
            {
                float s = Mathf.Lerp(targetScale, baseScale, timer / duration);
                t.localScale = new Vector3(s, s, 1f);
                timer += Time.deltaTime;
                yield return null;
            }
            t.localScale = new Vector3(baseScale, baseScale, 1f);
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            var cam = Camera.main;
            var origPos = cam.transform.localPosition;
            float timer = 0f;
            while (timer < duration)
            {
                var offset = (Vector3)Random.insideUnitCircle * magnitude;
                cam.transform.localPosition = origPos + offset;
                timer += Time.deltaTime;
                yield return null;
            }
            cam.transform.localPosition = origPos;
        }

        // ───────── Cleanup ─────────

        void ClearGrid()
        {
            if (_grid == null) return;
            int prevN = _grid.GetLength(0);
            for (int y = 0; y < prevN; y++)
                for (int x = 0; x < prevN; x++)
                    if (_grid[x, y]?.Obj != null)
                        Destroy(_grid[x, y].Obj);
            _grid = null;
            _path.Clear();
        }

        void OnDestroy()
        {
            ClearGrid();
        }
    }
}
