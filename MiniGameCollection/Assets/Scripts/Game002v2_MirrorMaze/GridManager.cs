using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game002v2_MirrorMaze
{
    public enum CellType { Empty, Wall, Emitter, Goal, Prism, MovingWall, Mirror }
    public enum Direction { Right, Up, Left, Down }

    public class GridManager : MonoBehaviour
    {
        [SerializeField] private MirrorMazeGameManager _gameManager;

        private bool _isActive;
        private int _currentStage;

        // Grid data
        private int _gridSize;
        private CellType[,] _grid;
        private GameObject[,] _cellObjects;
        private float _cellSize = 0.8f;
        private Vector2 _gridOrigin;

        // Stage elements
        private Vector2Int _emitterPos;
        private Direction _emitterDir;
        private List<Vector2Int> _goalPositions = new List<Vector2Int>();
        private List<GameObject> _goalObjects = new List<GameObject>();
        private List<MovingWallData> _movingWalls = new List<MovingWallData>();

        // Mirror management
        private int _totalMirrors;
        private int _placedMirrors;
        private List<MirrorData> _mirrors = new List<MirrorData>();
        private List<GameObject> _mirrorSlots = new List<GameObject>();

        // Drag state
        private bool _isDragging;
        private GameObject _dragObject;
        private int _dragMirrorIndex = -1;

        // Laser visualization
        private List<LineRenderer> _laserLines = new List<LineRenderer>();
        private List<Material> _laserMaterials = new List<Material>();
        private bool _laserVisible;

        // Sprites (loaded from Resources)
        private Sprite _gridCellSprite;
        private Sprite _mirrorSprite;
        private Sprite _emitterSprite;
        private Sprite _goalSprite;
        private Sprite _wallSprite;
        private Sprite _prismSprite;
        private Sprite _movingWallSprite;

        private struct MirrorData
        {
            public Vector2Int gridPos;
            public int angle; // 0, 45, 90, 135
            public bool placed;
            public GameObject obj;
        }

        private struct MovingWallData
        {
            public Vector2Int gridPos;
            public GameObject obj;
            public float period;
            public bool isOpen;
        }

        #region Stage Layouts

        private static readonly StageLayout[] Stages = new StageLayout[]
        {
            // Stage 1: 5x5, 1 mirror, simple L-path
            new StageLayout {
                gridSize = 5, mirrorCount = 1,
                emitterPos = new Vector2Int(0, 2), emitterDir = Direction.Right,
                goals = new[] { new Vector2Int(2, 4) },
                walls = new Vector2Int[0],
                prismPos = new Vector2Int(-1, -1),
                movingWalls = new Vector2Int[0]
            },
            // Stage 2: 6x6, 2 mirrors, double reflection
            new StageLayout {
                gridSize = 6, mirrorCount = 2,
                emitterPos = new Vector2Int(0, 1), emitterDir = Direction.Right,
                goals = new[] { new Vector2Int(5, 5) },
                walls = new Vector2Int[0],
                prismPos = new Vector2Int(-1, -1),
                movingWalls = new Vector2Int[0]
            },
            // Stage 3: 7x7, 3 mirrors, walls block direct path
            new StageLayout {
                gridSize = 7, mirrorCount = 3,
                emitterPos = new Vector2Int(0, 3), emitterDir = Direction.Right,
                goals = new[] { new Vector2Int(6, 3) },
                walls = new[] { new Vector2Int(3, 2), new Vector2Int(3, 3), new Vector2Int(3, 4) },
                prismPos = new Vector2Int(-1, -1),
                movingWalls = new Vector2Int[0]
            },
            // Stage 4: 7x7, 3 mirrors + prism, 2 goals
            new StageLayout {
                gridSize = 7, mirrorCount = 3,
                emitterPos = new Vector2Int(0, 3), emitterDir = Direction.Right,
                goals = new[] { new Vector2Int(6, 1), new Vector2Int(6, 5) },
                walls = new[] { new Vector2Int(2, 0), new Vector2Int(2, 6) },
                prismPos = new Vector2Int(3, 3),
                movingWalls = new Vector2Int[0]
            },
            // Stage 5: 8x8, 4 mirrors + prism + moving walls, 2 goals
            new StageLayout {
                gridSize = 8, mirrorCount = 4,
                emitterPos = new Vector2Int(0, 4), emitterDir = Direction.Right,
                goals = new[] { new Vector2Int(7, 1), new Vector2Int(7, 6) },
                walls = new[] { new Vector2Int(3, 2), new Vector2Int(3, 5), new Vector2Int(5, 3) },
                prismPos = new Vector2Int(4, 4),
                movingWalls = new[] { new Vector2Int(2, 4), new Vector2Int(6, 4) }
            }
        };

        private struct StageLayout
        {
            public int gridSize, mirrorCount;
            public Vector2Int emitterPos;
            public Direction emitterDir;
            public Vector2Int[] goals;
            public Vector2Int[] walls;
            public Vector2Int prismPos;
            public Vector2Int[] movingWalls;
        }

        #endregion

        private void Awake()
        {
            _gridCellSprite = Resources.Load<Sprite>("Sprites/Game002_MirrorMaze/grid_cell");
            _mirrorSprite = Resources.Load<Sprite>("Sprites/Game002_MirrorMaze/mirror");
            _emitterSprite = Resources.Load<Sprite>("Sprites/Game002_MirrorMaze/emitter");
            _goalSprite = Resources.Load<Sprite>("Sprites/Game002_MirrorMaze/goal");
            _wallSprite = Resources.Load<Sprite>("Sprites/Game002_MirrorMaze/wall");
            _prismSprite = Resources.Load<Sprite>("Sprites/Game002_MirrorMaze/prism");
            _movingWallSprite = Resources.Load<Sprite>("Sprites/Game002_MirrorMaze/moving_wall");
        }

        public void SetupStage(int stageIndex)
        {
            _currentStage = stageIndex;
            ClearGrid();
            ClearLaser();

            if (stageIndex < 0 || stageIndex >= Stages.Length) return;

            var layout = Stages[stageIndex];
            _gridSize = layout.gridSize;
            _totalMirrors = layout.mirrorCount;
            _placedMirrors = 0;
            _grid = new CellType[_gridSize, _gridSize];
            _cellObjects = new GameObject[_gridSize, _gridSize];

            // カメラサイズに基づいてレイアウトを動的計算
            // 上部: HUD(ステージ・スコア) 約1.0u
            // 中央: グリッド
            // 下部: ミラースロット + UIボタン用に2.5u確保
            float camSize = Camera.main != null ? Camera.main.orthographicSize : 6f;
            float topMargin = 1.2f;   // HUD用
            float bottomMargin = 2.8f; // スロット+ボタン用
            float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
            float availableWidth = (camSize * Camera.main.aspect * 2f) - 0.5f;

            // グリッドサイズに合わせてセルサイズを動的調整
            float maxCellByHeight = availableHeight / _gridSize;
            float maxCellByWidth = availableWidth / _gridSize;
            _cellSize = Mathf.Min(maxCellByHeight, maxCellByWidth, 0.8f);

            float totalWidth = _gridSize * _cellSize;
            float totalHeight = _gridSize * _cellSize;
            float gridCenterY = camSize - topMargin - totalHeight / 2f;
            _gridOrigin = new Vector2(-totalWidth / 2f, gridCenterY - totalHeight / 2f);

            // Grid cells
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    _grid[x, y] = CellType.Empty;
                    var cellObj = CreateCellObject(x, y, _gridCellSprite, "Cell");
                    cellObj.transform.localScale = Vector3.one * _cellSize * 0.95f;
                    _cellObjects[x, y] = cellObj;
                }
            }

            // Emitter
            _emitterPos = layout.emitterPos;
            _emitterDir = layout.emitterDir;
            if (IsInGrid(_emitterPos))
            {
                _grid[_emitterPos.x, _emitterPos.y] = CellType.Emitter;
                var emitterObj = CreateCellObject(_emitterPos.x, _emitterPos.y, _emitterSprite, "Emitter");
                emitterObj.transform.localScale = Vector3.one * _cellSize * 0.9f;
                emitterObj.transform.rotation = Quaternion.Euler(0, 0, DirectionToAngle(_emitterDir));
                ReplaceCellObject(_emitterPos.x, _emitterPos.y, emitterObj);
            }

            // Goals
            _goalPositions.Clear();
            _goalObjects.Clear();
            foreach (var gp in layout.goals)
            {
                if (!IsInGrid(gp)) continue;
                _goalPositions.Add(gp);
                _grid[gp.x, gp.y] = CellType.Goal;
                var goalObj = CreateCellObject(gp.x, gp.y, _goalSprite, "Goal");
                goalObj.transform.localScale = Vector3.one * _cellSize * 0.9f;
                _goalObjects.Add(goalObj);
                ReplaceCellObject(gp.x, gp.y, goalObj);
            }

            // Walls
            foreach (var wp in layout.walls)
            {
                if (!IsInGrid(wp)) continue;
                _grid[wp.x, wp.y] = CellType.Wall;
                var wallObj = CreateCellObject(wp.x, wp.y, _wallSprite, "Wall");
                wallObj.transform.localScale = Vector3.one * _cellSize * 0.95f;
                ReplaceCellObject(wp.x, wp.y, wallObj);
            }

            // Prism
            if (layout.prismPos.x >= 0 && IsInGrid(layout.prismPos))
            {
                _grid[layout.prismPos.x, layout.prismPos.y] = CellType.Prism;
                var prismObj = CreateCellObject(layout.prismPos.x, layout.prismPos.y, _prismSprite, "Prism");
                prismObj.transform.localScale = Vector3.one * _cellSize * 0.9f;
                ReplaceCellObject(layout.prismPos.x, layout.prismPos.y, prismObj);
            }

            // Moving walls
            _movingWalls.Clear();
            foreach (var mw in layout.movingWalls)
            {
                if (!IsInGrid(mw)) continue;
                _grid[mw.x, mw.y] = CellType.MovingWall;
                var mwObj = CreateCellObject(mw.x, mw.y, _movingWallSprite, "MovingWall");
                mwObj.transform.localScale = Vector3.one * _cellSize * 0.95f;
                _movingWalls.Add(new MovingWallData { gridPos = mw, obj = mwObj, period = 2f, isOpen = false });
                ReplaceCellObject(mw.x, mw.y, mwObj);
            }

            CreateMirrorSlots();
            _isActive = true;
            _laserVisible = false;
        }

        private void CreateMirrorSlots()
        {
            foreach (var slot in _mirrorSlots) if (slot != null) Destroy(slot);
            _mirrorSlots.Clear();
            _mirrors.Clear();

            // ミラースロットをグリッド下端とボタンの間に中央配置
            float slotsWidth = _totalMirrors * _cellSize * 1.3f - _cellSize * 0.3f;
            float slotStartX = -slotsWidth / 2f;
            float slotY = _gridOrigin.y - _cellSize * 1.2f;

            for (int i = 0; i < _totalMirrors; i++)
            {
                float x = slotStartX + i * _cellSize * 1.3f;
                var slotObj = new GameObject($"MirrorSlot_{i}");
                slotObj.transform.SetParent(transform);
                slotObj.transform.position = new Vector3(x, slotY, 0);

                var sr = slotObj.AddComponent<SpriteRenderer>();
                sr.sprite = _mirrorSprite;
                sr.sortingOrder = 5;
                slotObj.transform.localScale = Vector3.one * _cellSize * 0.8f;

                var col = slotObj.AddComponent<BoxCollider2D>();
                col.size = Vector2.one;

                _mirrorSlots.Add(slotObj);
                _mirrors.Add(new MirrorData
                {
                    gridPos = new Vector2Int(-1, -1),
                    angle = 45,
                    placed = false,
                    obj = slotObj
                });
            }
        }

        private void Update()
        {
            if (!_isActive || _gameManager == null || !_gameManager.IsPlaying) return;
            HandleInput();
            UpdateMovingWalls();
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            worldPos.z = 0;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnPointerDown(worldPos);
            }
            else if (Mouse.current.leftButton.isPressed && _isDragging)
            {
                OnPointerDrag(worldPos);
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                OnPointerUp(worldPos);
            }
        }

        private void OnPointerDown(Vector3 worldPos)
        {
            if (_laserVisible) { ClearLaser(); _laserVisible = false; }

            // Check placed mirrors (rotate on tap)
            for (int i = 0; i < _mirrors.Count; i++)
            {
                var m = _mirrors[i];
                if (m.placed && m.obj != null)
                {
                    Vector3 mPos = GridToWorld(m.gridPos.x, m.gridPos.y);
                    if (Vector2.Distance(worldPos, mPos) < _cellSize * 0.5f)
                    {
                        m.angle = (m.angle + 45) % 180;
                        m.obj.transform.rotation = Quaternion.Euler(0, 0, -m.angle);
                        _mirrors[i] = m;
                        StartCoroutine(FlashEffect(m.obj));
                        return;
                    }
                }
            }

            // Check unplaced mirrors (start drag)
            for (int i = 0; i < _mirrors.Count; i++)
            {
                var m = _mirrors[i];
                if (!m.placed && m.obj != null)
                {
                    if (Vector2.Distance(worldPos, m.obj.transform.position) < _cellSize * 0.6f)
                    {
                        _isDragging = true;
                        _dragMirrorIndex = i;
                        _dragObject = m.obj;
                        _dragObject.GetComponent<SpriteRenderer>().sortingOrder = 20;
                        return;
                    }
                }
            }
        }

        private void OnPointerDrag(Vector3 worldPos)
        {
            if (_dragObject != null) _dragObject.transform.position = worldPos;
        }

        private void OnPointerUp(Vector3 worldPos)
        {
            if (!_isDragging || _dragMirrorIndex < 0) return;

            Vector2Int gridPos = WorldToGrid(worldPos);
            bool validDrop = false;

            if (IsInGrid(gridPos) && _grid[gridPos.x, gridPos.y] == CellType.Empty)
            {
                var m = _mirrors[_dragMirrorIndex];
                m.gridPos = gridPos;
                m.placed = true;
                m.obj.transform.position = GridToWorld(gridPos.x, gridPos.y);
                m.obj.transform.rotation = Quaternion.Euler(0, 0, -m.angle);
                m.obj.GetComponent<SpriteRenderer>().sortingOrder = 10;
                _grid[gridPos.x, gridPos.y] = CellType.Mirror;
                _mirrors[_dragMirrorIndex] = m;
                _placedMirrors++;
                validDrop = true;
                StartCoroutine(ScalePulse(m.obj, 1.0f, 1.3f, 0.2f));
            }

            if (!validDrop)
            {
                ReturnMirrorToSlot(_dragMirrorIndex);
            }

            _isDragging = false;
            _dragMirrorIndex = -1;
            _dragObject = null;
        }

        private void ReturnMirrorToSlot(int index)
        {
            var m = _mirrors[index];
            float slotsWidth = _totalMirrors * _cellSize * 1.3f - _cellSize * 0.3f;
            float slotStartX = -slotsWidth / 2f;
            float slotX = slotStartX + index * _cellSize * 1.3f;
            float slotY = _gridOrigin.y - _cellSize * 1.2f;
            m.obj.transform.position = new Vector3(slotX, slotY, 0);
            m.obj.GetComponent<SpriteRenderer>().sortingOrder = 5;
            m.obj.transform.rotation = Quaternion.identity;
        }

        #region Laser Simulation

        public void FireLaser()
        {
            if (!_isActive || _gameManager == null || !_gameManager.IsPlaying) return;

            ClearLaser();
            var result = SimulateLaser();
            DrawLaserPaths(result.paths);
            _laserVisible = true;

            bool allGoalsHit = true;
            foreach (var gp in _goalPositions)
            {
                if (!result.hitGoals.Contains(gp)) { allGoalsHit = false; break; }
            }

            // Animate goals
            for (int i = 0; i < _goalObjects.Count; i++)
            {
                if (result.hitGoals.Contains(_goalPositions[i]))
                    StartCoroutine(GoalHitEffect(_goalObjects[i]));
            }

            int unusedMirrors = _totalMirrors - _placedMirrors;
            _gameManager.OnLaserResult(allGoalsHit, result.reflections, unusedMirrors);
        }

        private struct LaserResult
        {
            public List<List<Vector3>> paths;
            public HashSet<Vector2Int> hitGoals;
            public int reflections;
        }

        private LaserResult SimulateLaser()
        {
            var result = new LaserResult
            {
                paths = new List<List<Vector3>>(),
                hitGoals = new HashSet<Vector2Int>(),
                reflections = 0
            };
            var currentPath = new List<Vector3>();
            currentPath.Add(GridToWorld(_emitterPos.x, _emitterPos.y));
            TraceLaser(_emitterPos, _emitterDir, result, currentPath, 0);
            return result;
        }

        private void TraceLaser(Vector2Int startPos, Direction dir, LaserResult result, List<Vector3> path, int depth)
        {
            if (depth > 20) { result.paths.Add(path); return; }

            Vector2Int pos = startPos;
            Vector2Int delta = DirectionToDelta(dir);

            for (int step = 0; step < 30; step++)
            {
                Vector2Int nextPos = pos + delta;

                if (!IsInGrid(nextPos))
                {
                    path.Add(GridToWorld(nextPos.x, nextPos.y));
                    result.paths.Add(path);
                    return;
                }

                CellType cell = _grid[nextPos.x, nextPos.y];

                if (cell == CellType.Wall)
                {
                    path.Add(GridToWorld(nextPos.x, nextPos.y));
                    result.paths.Add(path);
                    return;
                }

                if (cell == CellType.MovingWall)
                {
                    bool isOpen = false;
                    foreach (var mw in _movingWalls)
                        if (mw.gridPos == nextPos) { isOpen = mw.isOpen; break; }
                    if (!isOpen)
                    {
                        path.Add(GridToWorld(nextPos.x, nextPos.y));
                        result.paths.Add(path);
                        return;
                    }
                }

                if (cell == CellType.Goal)
                {
                    result.hitGoals.Add(nextPos);
                    path.Add(GridToWorld(nextPos.x, nextPos.y));
                    pos = nextPos;
                    continue;
                }

                if (cell == CellType.Mirror)
                {
                    path.Add(GridToWorld(nextPos.x, nextPos.y));
                    result.reflections++;
                    int mirrorAngle = 45;
                    foreach (var m in _mirrors)
                        if (m.placed && m.gridPos == nextPos) { mirrorAngle = m.angle; break; }

                    Direction newDir = ReflectDirection(dir, mirrorAngle);
                    result.paths.Add(path);
                    var newPath = new List<Vector3> { GridToWorld(nextPos.x, nextPos.y) };
                    TraceLaser(nextPos, newDir, result, newPath, depth + 1);
                    return;
                }

                if (cell == CellType.Prism)
                {
                    path.Add(GridToWorld(nextPos.x, nextPos.y));
                    result.reflections++;
                    result.paths.Add(path);

                    Direction splitA, splitB;
                    GetPrismSplitDirections(dir, out splitA, out splitB);

                    var pathA = new List<Vector3> { GridToWorld(nextPos.x, nextPos.y) };
                    var pathB = new List<Vector3> { GridToWorld(nextPos.x, nextPos.y) };
                    TraceLaser(nextPos, splitA, result, pathA, depth + 1);
                    TraceLaser(nextPos, splitB, result, pathB, depth + 1);
                    return;
                }

                pos = nextPos;
            }

            path.Add(GridToWorld(pos.x, pos.y));
            result.paths.Add(path);
        }

        private Direction ReflectDirection(Direction incoming, int mirrorAngle)
        {
            switch (mirrorAngle)
            {
                case 45: // "/" mirror
                    switch (incoming)
                    {
                        case Direction.Right: return Direction.Up;
                        case Direction.Left: return Direction.Down;
                        case Direction.Up: return Direction.Right;
                        case Direction.Down: return Direction.Left;
                    }
                    break;
                case 0: // "-" horizontal mirror
                    switch (incoming)
                    {
                        case Direction.Up: return Direction.Down;
                        case Direction.Down: return Direction.Up;
                        default: return incoming;
                    }
                case 90: // "|" vertical mirror
                    switch (incoming)
                    {
                        case Direction.Right: return Direction.Left;
                        case Direction.Left: return Direction.Right;
                        default: return incoming;
                    }
                case 135: // "\" mirror
                    switch (incoming)
                    {
                        case Direction.Right: return Direction.Down;
                        case Direction.Left: return Direction.Up;
                        case Direction.Up: return Direction.Left;
                        case Direction.Down: return Direction.Right;
                    }
                    break;
            }
            return incoming;
        }

        private void GetPrismSplitDirections(Direction incoming, out Direction a, out Direction b)
        {
            switch (incoming)
            {
                case Direction.Right:
                case Direction.Left:
                    a = Direction.Up; b = Direction.Down; return;
                default:
                    a = Direction.Right; b = Direction.Left; return;
            }
        }

        private void DrawLaserPaths(List<List<Vector3>> paths)
        {
            foreach (var path in paths)
            {
                if (path.Count < 2) continue;
                var lineObj = new GameObject("LaserLine");
                lineObj.transform.SetParent(transform);
                var lr = lineObj.AddComponent<LineRenderer>();
                lr.positionCount = path.Count;
                lr.SetPositions(path.ToArray());
                lr.startWidth = 0.06f;
                lr.endWidth = 0.06f;
                var mat = new Material(Shader.Find("Sprites/Default"));
                lr.material = mat;
                lr.startColor = new Color(1f, 0.2f, 0.1f, 0.9f);
                lr.endColor = new Color(1f, 0.4f, 0.1f, 0.7f);
                lr.sortingOrder = 15;
                _laserLines.Add(lr);
                _laserMaterials.Add(mat);
            }
        }

        private void ClearLaser()
        {
            foreach (var lr in _laserLines)
                if (lr != null) Destroy(lr.gameObject);
            foreach (var mat in _laserMaterials)
                if (mat != null) Destroy(mat);
            _laserLines.Clear();
            _laserMaterials.Clear();
        }

        #endregion

        public void ResetMirrors()
        {
            for (int i = 0; i < _mirrors.Count; i++)
            {
                var m = _mirrors[i];
                if (m.placed)
                {
                    _grid[m.gridPos.x, m.gridPos.y] = CellType.Empty;
                    m.placed = false;
                    m.gridPos = new Vector2Int(-1, -1);
                    m.angle = 45;
                    _mirrors[i] = m;
                    ReturnMirrorToSlot(i);
                }
            }
            _placedMirrors = 0;
            ClearLaser();
            _laserVisible = false;
        }

        public void OnLaserFailed()
        {
            StartCoroutine(CameraShake(0.1f, 0.3f));
        }

        private void UpdateMovingWalls()
        {
            for (int i = 0; i < _movingWalls.Count; i++)
            {
                var mw = _movingWalls[i];
                float t = Time.time % mw.period;
                bool shouldBeOpen = t > mw.period * 0.5f;
                if (shouldBeOpen != mw.isOpen)
                {
                    mw.isOpen = shouldBeOpen;
                    if (mw.obj != null)
                    {
                        var sr = mw.obj.GetComponent<SpriteRenderer>();
                        if (sr != null) sr.color = shouldBeOpen ? new Color(1, 1, 1, 0.3f) : Color.white;
                    }
                    _movingWalls[i] = mw;
                }
            }
        }

        #region Grid Helpers

        private void ClearGrid()
        {
            _isActive = false;
            if (_cellObjects != null)
            {
                for (int x = 0; x < _cellObjects.GetLength(0); x++)
                    for (int y = 0; y < _cellObjects.GetLength(1); y++)
                        if (_cellObjects[x, y] != null) Destroy(_cellObjects[x, y]);
            }
            foreach (var slot in _mirrorSlots)
                if (slot != null) Destroy(slot);
            _mirrorSlots.Clear();
            _mirrors.Clear();
            _goalObjects.Clear();
            _movingWalls.Clear();
            ClearLaser();
        }

        private GameObject CreateCellObject(int x, int y, Sprite sprite, string namePrefix)
        {
            var obj = new GameObject($"{namePrefix}_{x}_{y}");
            obj.transform.SetParent(transform);
            obj.transform.position = GridToWorld(x, y);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;
            return obj;
        }

        private void ReplaceCellObject(int x, int y, GameObject newObj)
        {
            if (_cellObjects[x, y] != null) Destroy(_cellObjects[x, y]);
            _cellObjects[x, y] = newObj;
        }

        private Vector3 GridToWorld(int x, int y)
        {
            return new Vector3(
                _gridOrigin.x + x * _cellSize + _cellSize / 2f,
                _gridOrigin.y + y * _cellSize + _cellSize / 2f, 0);
        }

        private Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt((worldPos.x - _gridOrigin.x) / _cellSize);
            int y = Mathf.FloorToInt((worldPos.y - _gridOrigin.y) / _cellSize);
            return new Vector2Int(x, y);
        }

        private bool IsInGrid(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _gridSize && pos.y >= 0 && pos.y < _gridSize;
        }

        private Vector2Int DirectionToDelta(Direction dir)
        {
            switch (dir)
            {
                case Direction.Right: return new Vector2Int(1, 0);
                case Direction.Left: return new Vector2Int(-1, 0);
                case Direction.Up: return new Vector2Int(0, 1);
                case Direction.Down: return new Vector2Int(0, -1);
                default: return Vector2Int.zero;
            }
        }

        private float DirectionToAngle(Direction dir)
        {
            switch (dir)
            {
                case Direction.Right: return 0;
                case Direction.Up: return 90;
                case Direction.Left: return 180;
                case Direction.Down: return 270;
                default: return 0;
            }
        }

        #endregion

        #region Visual Effects

        private IEnumerator ScalePulse(GameObject obj, float from, float to, float duration)
        {
            if (obj == null) yield break;
            float half = duration / 2f;
            float baseScale = _cellSize * 0.9f;
            float elapsed = 0;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(from, to, elapsed / half);
                if (obj != null) obj.transform.localScale = Vector3.one * baseScale * s;
                yield return null;
            }
            elapsed = 0;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(to, from, elapsed / half);
                if (obj != null) obj.transform.localScale = Vector3.one * baseScale * s;
                yield return null;
            }
            if (obj != null) obj.transform.localScale = Vector3.one * baseScale;
        }

        private IEnumerator GoalHitEffect(GameObject goalObj)
        {
            if (goalObj == null) yield break;
            var sr = goalObj.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            Color orig = sr.color;
            sr.color = Color.green;
            yield return StartCoroutine(ScalePulse(goalObj, 1.0f, 1.5f, 0.3f));
            if (sr != null) sr.color = orig;
        }

        private IEnumerator FlashEffect(GameObject obj)
        {
            if (obj == null) yield break;
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            Color orig = sr.color;
            sr.color = Color.white;
            float elapsed = 0;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                if (sr != null) sr.color = Color.Lerp(Color.white, orig, elapsed / 0.15f);
                yield return null;
            }
            if (sr != null) sr.color = orig;
        }

        private IEnumerator CameraShake(float magnitude, float duration)
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            Vector3 origPos = cam.transform.position;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                cam.transform.position = origPos + new Vector3(x, y, 0);
                yield return null;
            }
            cam.transform.position = origPos;
        }

        #endregion

        private void OnDestroy()
        {
            ClearLaser();
        }
    }
}
