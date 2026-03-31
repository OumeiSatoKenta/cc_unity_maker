using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game014_MagnetPath
{
    public class MagnetManager : MonoBehaviour
    {
        [SerializeField] private int _gridWidth = 7;
        [SerializeField] private int _gridHeight = 7;
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private GameObject _magnetPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _goalPrefab;
        [SerializeField] private GameObject _ballPrefab;
        [SerializeField] private GameObject _cellBgPrefab;

        private CellType[,] _grid;
        private readonly List<MagnetController> _magnets = new List<MagnetController>();
        private readonly Dictionary<Vector2Int, MagnetController> _magnetMap = new Dictionary<Vector2Int, MagnetController>();
        private readonly List<GameObject> _stageObjects = new List<GameObject>();

        private GameObject _ballObj;
        private Vector2Int _ballPos;
        private Vector2Int _goalPos;

        private MagnetPathGameManager _gameManager;
        private Camera _mainCamera;

        private Sprite _northSprite;
        private Sprite _southSprite;

        public static int StageCount => 3;

        public enum CellType { Empty, Wall, Magnet, Goal }

        private void Awake()
        {
            _gameManager = GetComponentInParent<MagnetPathGameManager>();
            _mainCamera = Camera.main;
            _northSprite = Resources.Load<Sprite>("Sprites/Game014_MagnetPath/magnet_n");
            _southSprite = Resources.Load<Sprite>("Sprites/Game014_MagnetPath/magnet_s");
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
                    var magnet = hit.GetComponent<MagnetController>();
                    if (magnet != null)
                    {
                        magnet.TogglePolarity();
                        MoveBall();
                        if (_gameManager != null)
                        {
                            _gameManager.OnMagnetToggled();
                            if (_ballPos == _goalPos)
                                _gameManager.OnPuzzleSolved();
                        }
                    }
                }
            }
        }

        private void MoveBall()
        {
            // Calculate net force from all magnets
            Vector2 netForce = Vector2.zero;
            foreach (var magnet in _magnets)
            {
                Vector2 diff = (Vector2)(magnet.GridPosition - _ballPos);
                float dist = diff.magnitude;
                if (dist < 0.1f) continue;

                Vector2 dir = diff.normalized;
                // North attracts, South repels
                float sign = magnet.CurrentPolarity == Polarity.North ? 1f : -1f;
                netForce += dir * sign / dist;
            }

            // Determine dominant direction
            if (netForce.magnitude < 0.01f) return;

            Vector2Int moveDir;
            if (Mathf.Abs(netForce.x) > Mathf.Abs(netForce.y))
                moveDir = netForce.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                moveDir = netForce.y > 0 ? Vector2Int.up : Vector2Int.down;

            // Move ball one step in dominant direction
            Vector2Int newPos = _ballPos + moveDir;
            if (IsInBounds(newPos) && _grid[newPos.x, newPos.y] != CellType.Wall)
            {
                _ballPos = newPos;
                if (_ballObj != null)
                    _ballObj.transform.position = GridToWorld(_ballPos);
            }
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            var data = GetStageData(stageIndex);
            _gridWidth = data.width;
            _gridHeight = data.height;
            _grid = new CellType[_gridWidth, _gridHeight];
            _goalPos = data.goalPos;
            BuildStage(data);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
            _magnets.Clear();
            _magnetMap.Clear();
            _ballObj = null;
        }

        private void BuildStage(StageData data)
        {
            // Background cells
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    _grid[x, y] = CellType.Empty;
                    if (_cellBgPrefab != null)
                    {
                        var bg = Instantiate(_cellBgPrefab, transform);
                        bg.transform.position = GridToWorld(new Vector2Int(x, y));
                        _stageObjects.Add(bg);
                    }
                }
            }

            // Walls
            foreach (var wp in data.walls)
            {
                if (!IsInBounds(wp)) continue;
                _grid[wp.x, wp.y] = CellType.Wall;
                if (_wallPrefab != null)
                {
                    var obj = Instantiate(_wallPrefab, transform);
                    obj.transform.position = GridToWorld(wp);
                    _stageObjects.Add(obj);
                }
            }

            // Goal
            _grid[data.goalPos.x, data.goalPos.y] = CellType.Goal;
            if (_goalPrefab != null)
            {
                var obj = Instantiate(_goalPrefab, transform);
                obj.transform.position = GridToWorld(data.goalPos);
                _stageObjects.Add(obj);
            }

            // Magnets
            foreach (var md in data.magnets)
            {
                if (!IsInBounds(md.pos)) continue;
                _grid[md.pos.x, md.pos.y] = CellType.Magnet;
                if (_magnetPrefab != null)
                {
                    var obj = Instantiate(_magnetPrefab, transform);
                    obj.transform.position = GridToWorld(md.pos);
                    var ctrl = obj.GetComponent<MagnetController>();
                    if (ctrl != null)
                    {
                        ctrl.Initialize(md.pos, md.polarity, _northSprite, _southSprite);
                        _magnets.Add(ctrl);
                        _magnetMap[md.pos] = ctrl;
                    }
                    _stageObjects.Add(obj);
                }
            }

            // Ball
            _ballPos = data.ballStartPos;
            if (_ballPrefab != null)
            {
                _ballObj = Instantiate(_ballPrefab, transform);
                _ballObj.transform.position = GridToWorld(_ballPos);
                _stageObjects.Add(_ballObj);
            }
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

        #region Stage Data

        private struct MagnetData
        {
            public Vector2Int pos;
            public Polarity polarity;
            public MagnetData(int x, int y, Polarity p) { pos = new Vector2Int(x, y); polarity = p; }
        }

        private struct StageData
        {
            public int width, height;
            public Vector2Int ballStartPos;
            public Vector2Int goalPos;
            public List<Vector2Int> walls;
            public List<MagnetData> magnets;
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
            var walls = new List<Vector2Int>();
            for (int x = 0; x < 5; x++) { walls.Add(new Vector2Int(x, 0)); walls.Add(new Vector2Int(x, 4)); }
            for (int y = 1; y < 4; y++) { walls.Add(new Vector2Int(0, y)); walls.Add(new Vector2Int(4, y)); }
            return new StageData
            {
                width = 5, height = 5,
                ballStartPos = new Vector2Int(1, 1),
                goalPos = new Vector2Int(3, 3),
                walls = walls,
                magnets = new List<MagnetData> {
                    new MagnetData(2, 1, Polarity.North),
                    new MagnetData(2, 3, Polarity.South),
                }
            };
        }

        private StageData GetStage2()
        {
            var walls = new List<Vector2Int>();
            for (int x = 0; x < 6; x++) { walls.Add(new Vector2Int(x, 0)); walls.Add(new Vector2Int(x, 5)); }
            for (int y = 1; y < 5; y++) { walls.Add(new Vector2Int(0, y)); walls.Add(new Vector2Int(5, y)); }
            walls.Add(new Vector2Int(3, 2));
            return new StageData
            {
                width = 6, height = 6,
                ballStartPos = new Vector2Int(1, 1),
                goalPos = new Vector2Int(4, 4),
                walls = walls,
                magnets = new List<MagnetData> {
                    new MagnetData(2, 2, Polarity.North),
                    new MagnetData(4, 2, Polarity.South),
                    new MagnetData(2, 4, Polarity.North),
                }
            };
        }

        private StageData GetStage3()
        {
            var walls = new List<Vector2Int>();
            for (int x = 0; x < 7; x++) { walls.Add(new Vector2Int(x, 0)); walls.Add(new Vector2Int(x, 6)); }
            for (int y = 1; y < 6; y++) { walls.Add(new Vector2Int(0, y)); walls.Add(new Vector2Int(6, y)); }
            walls.Add(new Vector2Int(3, 3));
            walls.Add(new Vector2Int(2, 4));
            return new StageData
            {
                width = 7, height = 7,
                ballStartPos = new Vector2Int(1, 1),
                goalPos = new Vector2Int(5, 5),
                walls = walls,
                magnets = new List<MagnetData> {
                    new MagnetData(2, 2, Polarity.North),
                    new MagnetData(4, 2, Polarity.South),
                    new MagnetData(4, 4, Polarity.North),
                    new MagnetData(2, 5, Polarity.South),
                }
            };
        }

        #endregion
    }
}
