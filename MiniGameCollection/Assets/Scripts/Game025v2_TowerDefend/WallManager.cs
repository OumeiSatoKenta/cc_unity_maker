using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game025v2_TowerDefend
{
    public class WallManager : MonoBehaviour
    {
        [SerializeField] Sprite _wallSprite;

        // Grid
        HashSet<Vector2Int> _wallCells = new();
        Dictionary<Vector2Int, GameObject> _wallObjects = new();

        // Ink
        float _maxInk = 100f;
        float _currentInk;
        public float InkRatio => _currentInk / _maxInk;

        // Drawing state
        bool _drawingEnabled = true;
        bool _isDragging;
        Vector2Int _lastCellDragged = new(-9999, -9999);

        // Double-tap detection
        float _lastTapTime = -1f;
        Vector2 _lastTapPos;
        const float DoubleTapInterval = 0.35f;
        const float DoubleTapRadius = 40f; // pixels

        // Grid params
        float _cellSize = 0.5f;
        Vector3 _gridOrigin;
        int _gridWidth = 10;
        int _gridHeight = 12;

        // Staged params
        float _inkMultiplier = 1f;

        Camera _mainCam;

        void Awake()
        {
            _mainCam = Camera.main;
        }

        public void SetupStage(StageManager.StageConfig config, int stage)
        {
            _inkMultiplier = stage switch
            {
                1 => 1.0f,
                2 => 1.0f,
                3 => 0.8f,
                4 => 0.7f,
                5 => 0.6f,
                _ => 1.0f
            };
            _maxInk = 100f * _inkMultiplier;
            _currentInk = _maxInk;
            _drawingEnabled = true;
        }

        public void SetGridOrigin(Vector3 origin, float cellSize)
        {
            _gridOrigin = origin;
            _cellSize = cellSize;
            // Recalculate grid dimensions from camera
            var cam = Camera.main;
            if (cam != null)
            {
                float camSize = cam.orthographicSize;
                float camWidth = camSize * cam.aspect;
                float topMargin = 1.2f;
                float bottomMargin = 2.8f;
                float availH = (camSize * 2f) - topMargin - bottomMargin;
                _gridWidth  = Mathf.FloorToInt(camWidth * 2f / cellSize);
                _gridHeight = Mathf.FloorToInt(availH / cellSize);
            }
        }

        public void SetDrawingEnabled(bool enabled)
        {
            _drawingEnabled = enabled;
            if (!enabled) _isDragging = false;
        }

        public void RefillInkPartial(float ratio)
        {
            _currentInk = Mathf.Min(_maxInk, _currentInk + _maxInk * ratio);
        }

        public void ClearAllWalls()
        {
            foreach (var obj in _wallObjects.Values)
                if (obj != null) Destroy(obj);
            _wallCells.Clear();
            _wallObjects.Clear();
            _currentInk = _maxInk;
        }

        void Update()
        {
            if (!_drawingEnabled) return;
            if (_mainCam == null) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 mousePos = mouse.position.ReadValue();

            // Double-tap detection for wall removal
            if (mouse.leftButton.wasPressedThisFrame)
            {
                float now = Time.time;
                float dist = Vector2.Distance(mousePos, _lastTapPos);
                if (now - _lastTapTime < DoubleTapInterval && dist < DoubleTapRadius)
                {
                    // Double tap - remove wall
                    Vector3 worldPos = _mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -_mainCam.transform.position.z));
                    var cell = WorldToCell(worldPos);
                    if (_wallCells.Contains(cell))
                        RemoveWall(cell);
                    _lastTapTime = -1f;
                    _isDragging = false;
                    return;
                }
                _lastTapTime = now;
                _lastTapPos = mousePos;
                _isDragging = true;
                _lastCellDragged = new Vector2Int(-9999, -9999);
            }

            if (mouse.leftButton.isPressed && _isDragging)
            {
                Vector3 worldPos = _mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -_mainCam.transform.position.z));
                var cell = WorldToCell(worldPos);
                if (cell != _lastCellDragged)
                {
                    TryPlaceWall(cell);
                    _lastCellDragged = cell;
                }
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }
        }

        void TryPlaceWall(Vector2Int cell)
        {
            if (_wallCells.Contains(cell)) return;
            if (_currentInk < 1f) return;

            _wallCells.Add(cell);
            _currentInk -= 1f;

            var obj = new GameObject($"Wall_{cell.x}_{cell.y}");
            obj.transform.SetParent(transform);
            obj.transform.position = CellToWorld(cell);
            obj.transform.localScale = Vector3.one * _cellSize * 0.95f;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _wallSprite;
            sr.sortingOrder = 1;
            sr.color = new Color(0.13f, 0.59f, 0.95f, 0.9f);

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            _wallObjects[cell] = obj;

            // Flash effect
            StartCoroutine(FlashWall(sr));
        }

        System.Collections.IEnumerator FlashWall(SpriteRenderer sr)
        {
            if (sr == null) yield break;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            if (sr != null)
                sr.color = new Color(0.13f, 0.59f, 0.95f, 0.9f);
        }

        void RemoveWall(Vector2Int cell)
        {
            _wallCells.Remove(cell);
            if (_wallObjects.TryGetValue(cell, out var obj))
            {
                if (obj != null) Destroy(obj);
                _wallObjects.Remove(cell);
            }
            // Recover 50% ink cost
            _currentInk = Mathf.Min(_maxInk, _currentInk + 0.5f);
        }

        public bool TryBreakWallAt(Vector3 worldPos)
        {
            var cell = WorldToCell(worldPos);
            if (_wallCells.Contains(cell))
            {
                StartCoroutine(BreakWallEffect(cell));
                return true;
            }
            return false;
        }

        System.Collections.IEnumerator BreakWallEffect(Vector2Int cell)
        {
            if (!_wallObjects.TryGetValue(cell, out var obj) || obj == null) yield break;
            var sr = obj.GetComponent<SpriteRenderer>();
            // Pop animation
            float t = 0f;
            Vector3 startScale = obj.transform.localScale;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.2f;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(1f, 1.5f, ratio * 2f)
                    : Mathf.Lerp(1.5f, 0f, (ratio - 0.5f) * 2f);
                if (obj != null) obj.transform.localScale = startScale * scale;
                if (sr != null) sr.color = new Color(1f, 0.5f, 0f, 1f - ratio);
                yield return null;
            }
            _wallCells.Remove(cell);
            _wallObjects.Remove(cell);
            if (obj != null) Destroy(obj);
        }

        public bool IsWallAt(Vector2Int cell) => _wallCells.Contains(cell);
        public bool IsWallAt(Vector3 worldPos) => IsWallAt(WorldToCell(worldPos));

        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt((worldPos.x - _gridOrigin.x) / _cellSize);
            int y = Mathf.FloorToInt((worldPos.y - _gridOrigin.y) / _cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 CellToWorld(Vector2Int cell)
        {
            return _gridOrigin + new Vector3(
                (cell.x + 0.5f) * _cellSize,
                (cell.y + 0.5f) * _cellSize,
                0f);
        }

        // BFS pathfinding from world pos to goal, avoiding wall cells
        public List<Vector3> CalculatePath(Vector3 startWorld, Vector3 goalWorld)
        {
            var startCell = WorldToCell(startWorld);
            var goalCell = WorldToCell(goalWorld);
            return BFSPath(startCell, goalCell);
        }

        List<Vector3> BFSPath(Vector2Int start, Vector2Int goal)
        {
            int maxSearch = 500;
            var queue = new Queue<Vector2Int>();
            var came = new Dictionary<Vector2Int, Vector2Int>();
            queue.Enqueue(start);
            came[start] = start;

            var dirs = new Vector2Int[]
            {
                Vector2Int.right, Vector2Int.left,
                Vector2Int.up, Vector2Int.down
            };

            int searched = 0;
            while (queue.Count > 0 && searched < maxSearch)
            {
                searched++;
                var cur = queue.Dequeue();
                if (cur == goal)
                {
                    // Reconstruct path
                    var path = new List<Vector3>();
                    var c = goal;
                    while (c != start)
                    {
                        path.Add(CellToWorld(c));
                        c = came[c];
                    }
                    path.Reverse();
                    return path;
                }
                foreach (var d in dirs)
                {
                    var next = cur + d;
                    if (next.x < 0 || next.x >= _gridWidth || next.y < 0 || next.y >= _gridHeight) continue;
                    if (!came.ContainsKey(next) && !_wallCells.Contains(next))
                    {
                        came[next] = cur;
                        queue.Enqueue(next);
                    }
                }
            }
            // No path found - go direct
            return new List<Vector3> { CellToWorld(goal) };
        }

        void OnDestroy()
        {
            foreach (var obj in _wallObjects.Values)
                if (obj != null) Destroy(obj);
        }
    }
}
