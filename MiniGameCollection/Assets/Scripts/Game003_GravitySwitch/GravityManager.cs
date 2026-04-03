using System.Collections.Generic;
using UnityEngine;

namespace Game003_GravitySwitch
{
    public class GravityManager : MonoBehaviour
    {
        [SerializeField] private int _gridWidth = 7;
        [SerializeField] private int _gridHeight = 7;
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private GameObject _ballPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _goalPrefab;
        [SerializeField] private GameObject _floorPrefab;

        private CellType[,] _grid;
        private BallController _ball;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();

        private GravitySwitchGameManager _gameManager;
        private Vector2Int _goalPos;

        public static int StageCount => 3;

        public enum CellType
        {
            Floor,
            Wall,
            Goal
        }

        private void Awake()
        {
            _gameManager = GetComponentInParent<GravitySwitchGameManager>();
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            _grid = new CellType[_gridWidth, _gridHeight];
            BuildStage(GetStageData(stageIndex));
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _stageObjects.Clear();
            _ball = null;
        }

        public bool ApplyGravity(Vector2Int direction)
        {
            if (_ball == null) return false;

            Vector2Int currentPos = _ball.GridPosition;
            Vector2Int nextPos = currentPos;

            while (true)
            {
                Vector2Int candidate = nextPos + direction;
                if (!IsInBounds(candidate)) break;
                if (_grid[candidate.x, candidate.y] == CellType.Wall) break;
                nextPos = candidate;
                if (_grid[nextPos.x, nextPos.y] == CellType.Goal) break;
            }

            if (nextPos == currentPos) return false;

            _ball.SetGridPosition(nextPos);
            _ball.UpdateWorldPosition(GridToWorld(nextPos));
            return true;
        }

        public bool IsGoalReached()
        {
            return _ball != null && _ball.GridPosition == _goalPos;
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
            _goalPos = data.goalPos;

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    _grid[x, y] = CellType.Floor;
                    if (_floorPrefab != null)
                    {
                        var floor = Instantiate(_floorPrefab, transform);
                        floor.transform.position = GridToWorld(new Vector2Int(x, y));
                        floor.name = $"Floor_{x}_{y}";
                        _stageObjects.Add(floor);
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

            _grid[data.goalPos.x, data.goalPos.y] = CellType.Goal;
            if (_goalPrefab != null)
            {
                var goal = Instantiate(_goalPrefab, transform);
                goal.transform.position = GridToWorld(data.goalPos);
                goal.name = "Goal";
                _stageObjects.Add(goal);
            }

            if (_ballPrefab != null)
            {
                var ballObj = Instantiate(_ballPrefab, transform);
                ballObj.transform.position = GridToWorld(data.ballStartPos);
                ballObj.name = "Ball";
                _ball = ballObj.GetComponent<BallController>();
                if (_ball != null) _ball.Initialize(data.ballStartPos);
                _stageObjects.Add(ballObj);
            }
        }

        #region Stage Data

        private struct StageData
        {
            public Vector2Int ballStartPos;
            public Vector2Int goalPos;
            public List<Vector2Int> walls;
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
                ballStartPos = new Vector2Int(1, 1),
                goalPos = new Vector2Int(5, 5),
                walls = new List<Vector2Int>
                {
                    new Vector2Int(0, 6), new Vector2Int(1, 6), new Vector2Int(2, 6),
                    new Vector2Int(3, 6), new Vector2Int(4, 6), new Vector2Int(5, 6), new Vector2Int(6, 6),
                    new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                    new Vector2Int(3, 0), new Vector2Int(4, 0), new Vector2Int(5, 0), new Vector2Int(6, 0),
                    new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3),
                    new Vector2Int(0, 4), new Vector2Int(0, 5),
                    new Vector2Int(6, 1), new Vector2Int(6, 2), new Vector2Int(6, 3),
                    new Vector2Int(6, 4), new Vector2Int(6, 5),
                    new Vector2Int(3, 2), new Vector2Int(3, 3), new Vector2Int(4, 4),
                }
            };
        }

        private StageData GetStage2()
        {
            return new StageData
            {
                ballStartPos = new Vector2Int(1, 5),
                goalPos = new Vector2Int(5, 1),
                walls = new List<Vector2Int>
                {
                    new Vector2Int(0, 6), new Vector2Int(1, 6), new Vector2Int(2, 6),
                    new Vector2Int(3, 6), new Vector2Int(4, 6), new Vector2Int(5, 6), new Vector2Int(6, 6),
                    new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                    new Vector2Int(3, 0), new Vector2Int(4, 0), new Vector2Int(5, 0), new Vector2Int(6, 0),
                    new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3),
                    new Vector2Int(0, 4), new Vector2Int(0, 5),
                    new Vector2Int(6, 1), new Vector2Int(6, 2), new Vector2Int(6, 3),
                    new Vector2Int(6, 4), new Vector2Int(6, 5),
                    new Vector2Int(2, 4), new Vector2Int(3, 4),
                    new Vector2Int(4, 3), new Vector2Int(4, 2),
                    new Vector2Int(2, 2), new Vector2Int(5, 4),
                }
            };
        }

        private StageData GetStage3()
        {
            return new StageData
            {
                ballStartPos = new Vector2Int(1, 1),
                goalPos = new Vector2Int(3, 3),
                walls = new List<Vector2Int>
                {
                    new Vector2Int(0, 6), new Vector2Int(1, 6), new Vector2Int(2, 6),
                    new Vector2Int(3, 6), new Vector2Int(4, 6), new Vector2Int(5, 6), new Vector2Int(6, 6),
                    new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                    new Vector2Int(3, 0), new Vector2Int(4, 0), new Vector2Int(5, 0), new Vector2Int(6, 0),
                    new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3),
                    new Vector2Int(0, 4), new Vector2Int(0, 5),
                    new Vector2Int(6, 1), new Vector2Int(6, 2), new Vector2Int(6, 3),
                    new Vector2Int(6, 4), new Vector2Int(6, 5),
                    new Vector2Int(2, 1), new Vector2Int(2, 2),
                    new Vector2Int(3, 4), new Vector2Int(3, 5),
                    new Vector2Int(4, 2), new Vector2Int(4, 3),
                    new Vector2Int(1, 3),
                    new Vector2Int(5, 4), new Vector2Int(5, 5),
                }
            };
        }

        #endregion
    }
}
