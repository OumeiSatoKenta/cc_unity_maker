using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game002_MirrorMaze
{
    public class MazeManager : MonoBehaviour
    {
        [SerializeField] private int _gridWidth = 7;
        [SerializeField] private int _gridHeight = 7;
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private GameObject _mirrorPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _laserSourcePrefab;
        [SerializeField] private GameObject _goalPrefab;
        [SerializeField] private GameObject _laserBeamPrefab;
        [SerializeField] private GameObject _emptyCellPrefab;
        [SerializeField] private GameObject _mirrorSlotPrefab;

        private CellType[,] _grid;
        private CellType[,] _initialGrid;
        private MirrorController[,] _mirrorGrid;
        private readonly List<MirrorController> _allMirrors = new List<MirrorController>();
        private readonly List<GameObject> _laserSegments = new List<GameObject>();
        private readonly List<GameObject> _stageObjects = new List<GameObject>();

        private MirrorMazeGameManager _gameManager;
        private Camera _mainCamera;
        private MirrorController _selectedMirror;
        private Vector2 _dragStart;

        private Vector2Int _laserStartPos;
        private Vector2Int _laserStartDir;
        private Vector2Int _goalPos;
        private bool _goalReached;

        public static int StageCount => 3;

        public enum CellType
        {
            Empty,
            Wall,
            LaserSource,
            Goal,
            Mirror,
            MirrorSlot
        }

        private void Awake()
        {
            _gameManager = GetComponentInParent<MirrorMazeGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (_mainCamera == null) return;
                Vector3 screenPos = mouse.position.ReadValue();
                screenPos.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var mirror = hit.GetComponent<MirrorController>();
                    if (mirror != null && _allMirrors.Contains(mirror))
                    {
                        _selectedMirror = mirror;
                        _dragStart = mouse.position.ReadValue();
                    }
                }
            }

            if (mouse.leftButton.wasReleasedThisFrame && _selectedMirror != null)
            {
                Vector2 dragEnd = mouse.position.ReadValue();
                Vector2 dragDelta = dragEnd - _dragStart;

                if (dragDelta.magnitude < 30f)
                {
                    _selectedMirror.Rotate45();
                    if (_gameManager != null) _gameManager.OnMirrorRotated();
                }
                else
                {
                    Vector2Int direction;
                    if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
                        direction = dragDelta.x > 0 ? Vector2Int.right : Vector2Int.left;
                    else
                        direction = dragDelta.y > 0 ? Vector2Int.up : Vector2Int.down;

                    if (TryMoveMirror(_selectedMirror, direction))
                    {
                        if (_gameManager != null) _gameManager.OnMirrorMoved();
                    }
                }

                _selectedMirror = null;
            }
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            _grid = new CellType[_gridWidth, _gridHeight];
            _mirrorGrid = new MirrorController[_gridWidth, _gridHeight];

            var stageData = GetStageData(stageIndex);
            BuildStage(stageData);

            // 初期グリッド状態を保存（ミラー移動後の復元用）
            _initialGrid = new CellType[_gridWidth, _gridHeight];
            for (int x = 0; x < _gridWidth; x++)
                for (int y = 0; y < _gridHeight; y++)
                    _initialGrid[x, y] = _grid[x, y];

            UpdateLaser();
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _stageObjects.Clear();
            _allMirrors.Clear();
            ClearLaser();
        }

        private void ClearLaser()
        {
            foreach (var seg in _laserSegments)
            {
                if (seg != null) Destroy(seg);
            }
            _laserSegments.Clear();
        }

        private bool TryMoveMirror(MirrorController mirror, Vector2Int direction)
        {
            if (mirror == null) return false;

            Vector2Int newPos = mirror.GridPosition + direction;
            if (!IsInBounds(newPos)) return false;

            var targetCell = _grid[newPos.x, newPos.y];
            if (targetCell != CellType.Empty && targetCell != CellType.MirrorSlot) return false;
            if (_mirrorGrid[newPos.x, newPos.y] != null) return false;

            var oldPos = mirror.GridPosition;
            _mirrorGrid[oldPos.x, oldPos.y] = null;
            _grid[oldPos.x, oldPos.y] = _initialGrid[oldPos.x, oldPos.y];

            _mirrorGrid[newPos.x, newPos.y] = mirror;
            _grid[newPos.x, newPos.y] = CellType.Mirror;

            mirror.SetGridPosition(newPos);
            mirror.UpdateWorldPosition(GridToWorld(newPos));

            return true;
        }

        public void UpdateLaser()
        {
            ClearLaser();
            _goalReached = false;

            Vector2Int pos = _laserStartPos;
            Vector2Int dir = _laserStartDir;
            int maxSteps = 100;
            var visited = new HashSet<(Vector2Int, Vector2Int)>();

            for (int step = 0; step < maxSteps; step++)
            {
                var key = (pos, dir);
                if (visited.Contains(key)) break;
                visited.Add(key);
                Vector2Int nextPos = pos + dir;

                if (!IsInBounds(nextPos)) break;

                DrawLaserSegment(pos, nextPos);

                if (nextPos == _goalPos)
                {
                    _goalReached = true;
                    break;
                }

                if (_grid[nextPos.x, nextPos.y] == CellType.Wall)
                    break;

                if (_mirrorGrid[nextPos.x, nextPos.y] != null)
                {
                    var mirror = _mirrorGrid[nextPos.x, nextPos.y];
                    dir = mirror.Reflect(dir);
                    pos = nextPos;
                    continue;
                }

                if (_grid[nextPos.x, nextPos.y] == CellType.LaserSource && nextPos != _laserStartPos)
                    break;

                pos = nextPos;
            }
        }

        private void DrawLaserSegment(Vector2Int from, Vector2Int to)
        {
            if (_laserBeamPrefab == null) return;

            var segment = Instantiate(_laserBeamPrefab, transform);
            _laserSegments.Add(segment);

            Vector3 fromWorld = GridToWorld(from);
            Vector3 toWorld = GridToWorld(to);
            Vector3 midPoint = (fromWorld + toWorld) * 0.5f;

            segment.transform.position = midPoint;

            Vector2 delta = (Vector2)(toWorld - fromWorld);
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            segment.transform.rotation = Quaternion.Euler(0, 0, angle);

            float length = delta.magnitude / _cellSize;
            segment.transform.localScale = new Vector3(length * _cellSize, 0.12f, 1f);
        }

        public bool IsGoalReached()
        {
            return _goalReached;
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offsetX = (_gridWidth - 1) * _cellSize * 0.5f;
            float offsetY = (_gridHeight - 1) * _cellSize * 0.5f;
            return new Vector3(gridPos.x * _cellSize - offsetX, gridPos.y * _cellSize - offsetY, 0f);
        }

        private bool IsInBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _gridWidth && pos.y >= 0 && pos.y < _gridHeight;
        }

        private void BuildStage(StageData data)
        {
            _laserStartPos = data.laserPos;
            _laserStartDir = data.laserDir;
            _goalPos = data.goalPos;

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    _grid[x, y] = CellType.Empty;
                    if (_emptyCellPrefab != null)
                    {
                        var cell = Instantiate(_emptyCellPrefab, transform);
                        cell.transform.position = GridToWorld(new Vector2Int(x, y));
                        cell.name = $"Cell_{x}_{y}";
                        _stageObjects.Add(cell);
                    }
                }
            }

            foreach (var wallPos in data.walls)
            {
                if (IsInBounds(wallPos))
                {
                    _grid[wallPos.x, wallPos.y] = CellType.Wall;
                    if (_wallPrefab != null)
                    {
                        var wall = Instantiate(_wallPrefab, transform);
                        wall.transform.position = GridToWorld(wallPos);
                        wall.name = $"Wall_{wallPos.x}_{wallPos.y}";
                        _stageObjects.Add(wall);
                    }
                }
            }

            foreach (var slotPos in data.mirrorSlots)
            {
                if (IsInBounds(slotPos))
                {
                    _grid[slotPos.x, slotPos.y] = CellType.MirrorSlot;
                    if (_mirrorSlotPrefab != null)
                    {
                        var slot = Instantiate(_mirrorSlotPrefab, transform);
                        slot.transform.position = GridToWorld(slotPos);
                        slot.name = $"Slot_{slotPos.x}_{slotPos.y}";
                        _stageObjects.Add(slot);
                    }
                }
            }

            foreach (var mirrorData in data.mirrors)
            {
                if (IsInBounds(mirrorData.pos))
                {
                    _grid[mirrorData.pos.x, mirrorData.pos.y] = CellType.Mirror;
                    if (_mirrorPrefab != null)
                    {
                        var mirrorObj = Instantiate(_mirrorPrefab, transform);
                        mirrorObj.transform.position = GridToWorld(mirrorData.pos);
                        mirrorObj.name = $"Mirror_{mirrorData.pos.x}_{mirrorData.pos.y}";

                        var mirrorCtrl = mirrorObj.GetComponent<MirrorController>();
                        if (mirrorCtrl != null)
                            mirrorCtrl.Initialize(mirrorData.pos, mirrorData.angle);

                        _mirrorGrid[mirrorData.pos.x, mirrorData.pos.y] = mirrorCtrl;
                        _allMirrors.Add(mirrorCtrl);
                        _stageObjects.Add(mirrorObj);
                    }
                }
            }

            _grid[data.laserPos.x, data.laserPos.y] = CellType.LaserSource;
            if (_laserSourcePrefab != null)
            {
                var source = Instantiate(_laserSourcePrefab, transform);
                source.transform.position = GridToWorld(data.laserPos);
                source.name = "LaserSource";
                float angle = Mathf.Atan2(data.laserDir.y, data.laserDir.x) * Mathf.Rad2Deg;
                source.transform.rotation = Quaternion.Euler(0, 0, angle);
                _stageObjects.Add(source);
            }

            _grid[data.goalPos.x, data.goalPos.y] = CellType.Goal;
            if (_goalPrefab != null)
            {
                var goal = Instantiate(_goalPrefab, transform);
                goal.transform.position = GridToWorld(data.goalPos);
                goal.name = "Goal";
                _stageObjects.Add(goal);
            }
        }

        #region Stage Data

        private struct MirrorPlacement
        {
            public Vector2Int pos;
            public int angle;

            public MirrorPlacement(int x, int y, int a)
            {
                pos = new Vector2Int(x, y);
                angle = a;
            }
        }

        private struct StageData
        {
            public Vector2Int laserPos;
            public Vector2Int laserDir;
            public Vector2Int goalPos;
            public List<Vector2Int> walls;
            public List<Vector2Int> mirrorSlots;
            public List<MirrorPlacement> mirrors;
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

        private StageData GetStage1()
        {
            return new StageData
            {
                laserPos = new Vector2Int(0, 3),
                laserDir = Vector2Int.right,
                goalPos = new Vector2Int(3, 6),
                walls = new List<Vector2Int>
                {
                    new Vector2Int(2, 1), new Vector2Int(4, 1),
                    new Vector2Int(2, 5), new Vector2Int(4, 5),
                },
                mirrorSlots = new List<Vector2Int>
                {
                    new Vector2Int(1, 3), new Vector2Int(2, 3),
                    new Vector2Int(3, 3), new Vector2Int(4, 3), new Vector2Int(5, 3),
                    new Vector2Int(3, 2), new Vector2Int(3, 4), new Vector2Int(3, 5),
                },
                mirrors = new List<MirrorPlacement>
                {
                    new MirrorPlacement(3, 3, 0),
                    new MirrorPlacement(3, 5, 1),
                }
            };
        }

        private StageData GetStage2()
        {
            return new StageData
            {
                laserPos = new Vector2Int(0, 1),
                laserDir = Vector2Int.right,
                goalPos = new Vector2Int(6, 5),
                walls = new List<Vector2Int>
                {
                    new Vector2Int(3, 0), new Vector2Int(3, 1), new Vector2Int(3, 2),
                    new Vector2Int(3, 4), new Vector2Int(3, 5), new Vector2Int(3, 6),
                },
                mirrorSlots = new List<Vector2Int>
                {
                    new Vector2Int(1, 1), new Vector2Int(2, 1),
                    new Vector2Int(2, 3), new Vector2Int(2, 4),
                    new Vector2Int(4, 3), new Vector2Int(4, 5),
                    new Vector2Int(5, 5), new Vector2Int(5, 3),
                },
                mirrors = new List<MirrorPlacement>
                {
                    new MirrorPlacement(2, 1, 0),
                    new MirrorPlacement(2, 3, 1),
                    new MirrorPlacement(4, 3, 0),
                    new MirrorPlacement(4, 5, 1),
                }
            };
        }

        private StageData GetStage3()
        {
            return new StageData
            {
                laserPos = new Vector2Int(0, 6),
                laserDir = Vector2Int.right,
                goalPos = new Vector2Int(6, 0),
                walls = new List<Vector2Int>
                {
                    new Vector2Int(1, 4), new Vector2Int(2, 4),
                    new Vector2Int(4, 2), new Vector2Int(5, 2),
                    new Vector2Int(1, 0), new Vector2Int(1, 1),
                    new Vector2Int(5, 5), new Vector2Int(5, 6),
                },
                mirrorSlots = new List<Vector2Int>
                {
                    new Vector2Int(1, 6), new Vector2Int(2, 6), new Vector2Int(3, 6),
                    new Vector2Int(3, 5), new Vector2Int(3, 4), new Vector2Int(3, 3),
                    new Vector2Int(3, 2), new Vector2Int(3, 1), new Vector2Int(3, 0),
                    new Vector2Int(4, 3), new Vector2Int(5, 3), new Vector2Int(6, 3),
                    new Vector2Int(6, 2), new Vector2Int(6, 1),
                },
                mirrors = new List<MirrorPlacement>
                {
                    new MirrorPlacement(3, 6, 1),
                    new MirrorPlacement(3, 3, 0),
                    new MirrorPlacement(6, 3, 1),
                }
            };
        }

        #endregion
    }
}
