using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game003v2_GravitySwitch
{
    public enum CellType { Empty, Wall, Hole, Goal }

    public class GravityManager : MonoBehaviour
    {
        public event System.Action<int, int> OnMovesChanged; // (movesUsed, moveLimit)

        [SerializeField] private GameObject _ballPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _goalPrefab;
        [SerializeField] private GameObject _holePrefab;

        private CellType[,] _grid;
        private int _gridSize;
        private float _cellSize;
        private Vector3 _gridOrigin;

        private BallController _ball;
        private BallController _ball2;
        private Vector2Int _ballStart;
        private Vector2Int _ball2Start;
        private Vector2Int _goalPos;
        private Vector2Int _goal2Pos;

        private bool _hasTwoBalls;
        private bool _hasHole;
        private int _moveLimit;
        private int _minMoves;
        private int _movesUsed;
        private bool _isMoving;

        private readonly List<GameObject> _stageObjects = new List<GameObject>();
        private GravitySwitchGameManager _gameManager;

        private void Awake()
        {
            _gameManager = GetComponentInParent<GravitySwitchGameManager>();
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            var data = GetStageData(stageIndex);
            _gridSize = data.gridSize;
            _hasHole = data.hasHole;
            _hasTwoBalls = data.hasTwoBalls;
            _moveLimit = data.moveLimit;
            _minMoves = data.minMoves;
            _movesUsed = 0;
            _ballStart = data.ballStart;
            _ball2Start = data.ball2Start;
            _goalPos = data.goalPos;
            _goal2Pos = data.goal2Pos;
            _isMoving = false;

            // レスポンシブ配置
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[GravityManager] Camera.main が見つかりません"); return; }
            float camSize = cam.orthographicSize;
            float camWidth = camSize * cam.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 3.5f;
            float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
            _cellSize = Mathf.Min(availableHeight / _gridSize, camWidth * 2f / _gridSize, 1.6f);
            float gridWorldSize = _cellSize * _gridSize;
            _gridOrigin = new Vector3(-gridWorldSize * 0.5f + _cellSize * 0.5f,
                                      -camSize + bottomMargin + _cellSize * 0.5f, 0f);

            _grid = new CellType[_gridSize, _gridSize];
            BuildStage(data);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
            _ball = null;
            _ball2 = null;
        }

        private void BuildStage(StageData data)
        {
            for (int x = 0; x < _gridSize; x++)
                for (int y = 0; y < _gridSize; y++)
                    _grid[x, y] = CellType.Empty;

            foreach (var wp in data.walls)
            {
                if (InBounds(wp))
                {
                    _grid[wp.x, wp.y] = CellType.Wall;
                    SpawnAt(_wallPrefab, wp, "Wall");
                }
            }

            if (data.hasHole)
            {
                foreach (var hp in data.holes)
                {
                    if (InBounds(hp))
                    {
                        _grid[hp.x, hp.y] = CellType.Hole;
                        SpawnAt(_holePrefab, hp, "Hole");
                    }
                }
            }

            _grid[data.goalPos.x, data.goalPos.y] = CellType.Goal;
            SpawnAt(_goalPrefab, data.goalPos, "Goal");

            if (data.hasTwoBalls)
            {
                _grid[data.goal2Pos.x, data.goal2Pos.y] = CellType.Goal;
                SpawnAt(_goalPrefab, data.goal2Pos, "Goal2");
            }

            if (_ballPrefab != null)
            {
                var b1 = Instantiate(_ballPrefab, GridToWorld(data.ballStart), Quaternion.identity, transform);
                b1.name = "Ball";
                b1.transform.localScale = Vector3.one * _cellSize * 0.8f;
                _ball = b1.GetComponent<BallController>();
                if (_ball != null) _ball.Initialize(data.ballStart);
                _stageObjects.Add(b1);

                if (data.hasTwoBalls)
                {
                    var b2 = Instantiate(_ballPrefab, GridToWorld(data.ball2Start), Quaternion.identity, transform);
                    b2.name = "Ball2";
                    b2.transform.localScale = Vector3.one * _cellSize * 0.8f;
                    _ball2 = b2.GetComponent<BallController>();
                    if (_ball2 != null) _ball2.Initialize(data.ball2Start);
                    _stageObjects.Add(b2);
                }
            }
        }

        private GameObject SpawnAt(GameObject prefab, Vector2Int gridPos, string name)
        {
            if (prefab == null) return null;
            var obj = Instantiate(prefab, GridToWorld(gridPos), Quaternion.identity, transform);
            obj.name = name;
            obj.transform.localScale = Vector3.one * _cellSize * 0.9f;
            _stageObjects.Add(obj);
            return obj;
        }

        public void ApplyGravity(GravityDirection dir)
        {
            if (_isMoving || _ball == null) return;
            if (_gameManager == null || !_gameManager.IsPlaying) return;

            Vector2Int d = DirToVec(dir);
            bool moved = false;

            // ボール1移動
            var (newPos1, result1) = SimulateMove(_ball.GridPosition, d);
            // ボール2移動（2ボール時）
            Vector2Int newPos2 = _hasTwoBalls && _ball2 != null ? _ball2.GridPosition : Vector2Int.zero;
            MoveResult result2 = MoveResult.Blocked;
            if (_hasTwoBalls && _ball2 != null)
            {
                (newPos2, result2) = SimulateMove(_ball2.GridPosition, d);
            }

            if (newPos1 != _ball.GridPosition || (_hasTwoBalls && _ball2 != null && newPos2 != _ball2.GridPosition))
                moved = true;

            if (!moved) return;

            _movesUsed++;
            _ball.SetGridPosition(newPos1);
            if (_hasTwoBalls && _ball2 != null) _ball2.SetGridPosition(newPos2);

            OnMovesChanged?.Invoke(_movesUsed, _moveLimit);

            StartCoroutine(AnimateMove(newPos1, newPos2, result1, result2));
        }

        private IEnumerator AnimateMove(Vector2Int newPos1, Vector2Int newPos2, MoveResult result1, MoveResult result2)
        {
            _isMoving = true;
            float t = 0f;
            float dur = 0.18f;

            Vector3 startW1 = _ball.transform.position;
            Vector3 endW1 = GridToWorld(newPos1);
            Vector3 startW2 = _hasTwoBalls && _ball2 != null ? _ball2.transform.position : Vector3.zero;
            Vector3 endW2 = _hasTwoBalls && _ball2 != null ? GridToWorld(newPos2) : Vector3.zero;

            while (t < dur)
            {
                t += Time.deltaTime;
                float ratio = Mathf.Clamp01(t / dur);
                _ball.transform.position = Vector3.Lerp(startW1, endW1, ratio);
                if (_hasTwoBalls && _ball2 != null) _ball2.transform.position = Vector3.Lerp(startW2, endW2, ratio);
                yield return null;
            }
            _ball.transform.position = endW1;
            if (_hasTwoBalls && _ball2 != null) _ball2.transform.position = endW2;

            _isMoving = false;

            // 結果処理
            if (result1 == MoveResult.Hole || (_hasTwoBalls && result2 == MoveResult.Hole))
            {
                StartCoroutine(PlayHoleEffect());
                yield return new WaitForSeconds(0.3f);
                _gameManager.OnFallIntoHole();
                yield break;
            }

            if (_moveLimit > 0 && _movesUsed >= _moveLimit)
            {
                if (CheckGoalReached(result1, result2))
                {
                    yield return StartCoroutine(PlayGoalEffect());
                    _gameManager.OnReachGoal(_movesUsed, _minMoves, _moveLimit);
                }
                else
                {
                    _gameManager.OnMoveLimitExceeded();
                }
                yield break;
            }

            if (CheckGoalReached(result1, result2))
            {
                yield return StartCoroutine(PlayGoalEffect());
                _gameManager.OnReachGoal(_movesUsed, _minMoves, _moveLimit);
            }
        }

        private bool CheckGoalReached(MoveResult r1, MoveResult r2)
        {
            if (_hasTwoBalls)
                return r1 == MoveResult.Goal && r2 == MoveResult.Goal;
            return r1 == MoveResult.Goal;
        }

        private enum MoveResult { Moved, Blocked, Goal, Hole }

        private (Vector2Int, MoveResult) SimulateMove(Vector2Int from, Vector2Int dir)
        {
            Vector2Int pos = from;
            while (true)
            {
                Vector2Int next = pos + dir;
                if (!InBounds(next)) break;
                CellType cell = _grid[next.x, next.y];
                if (cell == CellType.Wall) break;
                pos = next;
                if (cell == CellType.Goal) return (pos, MoveResult.Goal);
                if (cell == CellType.Hole) return (pos, MoveResult.Hole);
            }
            return (pos, pos == from ? MoveResult.Blocked : MoveResult.Moved);
        }

        public void ResetStage()
        {
            StopAllCoroutines();
            _isMoving = false;
            if (_ball != null)
            {
                _ball.SetGridPosition(_ballStart);
                _ball.transform.position = GridToWorld(_ballStart);
            }
            if (_hasTwoBalls && _ball2 != null)
            {
                _ball2.SetGridPosition(_ball2Start);
                _ball2.transform.position = GridToWorld(_ball2Start);
            }
            _movesUsed = 0;
            _isMoving = false;
        }

        private IEnumerator PlayGoalEffect()
        {
            // ゴール到達: スケールパルス
            if (_ball != null)
            {
                float t = 0f;
                Vector3 origScale = _ball.transform.localScale;
                while (t < 0.3f)
                {
                    t += Time.deltaTime;
                    float s = 1f + Mathf.Sin(t / 0.3f * Mathf.PI) * 0.5f;
                    _ball.transform.localScale = origScale * s;
                    yield return null;
                }
                _ball.transform.localScale = origScale;
            }
        }

        private IEnumerator PlayHoleEffect()
        {
            // 穴落下: 赤フラッシュ
            if (_ball != null)
            {
                var sr = _ball.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    float t = 0f;
                    while (t < 0.4f)
                    {
                        t += Time.deltaTime;
                        float s = Mathf.Sin(t * 20f) * 0.5f + 0.5f;
                        sr.color = Color.Lerp(Color.red, Color.white, s);
                        yield return null;
                    }
                    sr.color = Color.white;
                }
            }
        }

        private Vector3 GridToWorld(Vector2Int pos)
        {
            return _gridOrigin + new Vector3(pos.x * _cellSize, pos.y * _cellSize, 0f);
        }

        private bool InBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _gridSize && pos.y >= 0 && pos.y < _gridSize;
        }

        private Vector2Int DirToVec(GravityDirection dir)
        {
            return dir switch
            {
                GravityDirection.Up => Vector2Int.up,
                GravityDirection.Down => Vector2Int.down,
                GravityDirection.Left => Vector2Int.left,
                GravityDirection.Right => Vector2Int.right,
                _ => Vector2Int.down
            };
        }

        private void OnDestroy()
        {
            ClearStage();
        }

        #region Stage Data

        private struct StageData
        {
            public int gridSize;
            public Vector2Int ballStart;
            public Vector2Int ball2Start;
            public Vector2Int goalPos;
            public Vector2Int goal2Pos;
            public List<Vector2Int> walls;
            public List<Vector2Int> holes;
            public bool hasHole;
            public bool hasTwoBalls;
            public int moveLimit;
            public int minMoves;
        }

        private StageData GetStageData(int index)
        {
            return index switch
            {
                0 => GetStage1(),
                1 => GetStage2(),
                2 => GetStage3(),
                3 => GetStage4(),
                4 => GetStage5(),
                _ => GetStage1()
            };
        }

        // Stage 1: 5x5, 壁のみ, 基本操作
        // 解: 右→上 (2手)
        // Ball(0,0) → 右 → (4,0) → 上 → (4,4)=Goal
        private StageData GetStage1()
        {
            return new StageData
            {
                gridSize = 5,
                ballStart = new Vector2Int(0, 0),
                goalPos = new Vector2Int(4, 4),
                walls = new List<Vector2Int> {
                    new Vector2Int(1, 2),
                    new Vector2Int(3, 1),
                },
                holes = new List<Vector2Int>(),
                hasHole = false, hasTwoBalls = false, moveLimit = 0, minMoves = 2
            };
        }

        // Stage 2: 5x5, 壁で迂回必要
        // 解: 上→右→上 (3手)
        private StageData GetStage2()
        {
            return new StageData
            {
                gridSize = 5,
                ballStart = new Vector2Int(0, 0),
                goalPos = new Vector2Int(4, 4),
                walls = new List<Vector2Int> {
                    new Vector2Int(0, 4), new Vector2Int(1, 4), new Vector2Int(2, 4),
                    new Vector2Int(4, 0), new Vector2Int(4, 1), new Vector2Int(4, 2),
                },
                holes = new List<Vector2Int>(),
                hasHole = false, hasTwoBalls = false, moveLimit = 0, minMoves = 3
            };
        }

        // Stage 3: 6x6, 穴あり注意
        // 解: 上→右→上→右→上 (5手)
        // Ball(0,0)→上→(0,5)→右→(5,5)=Goal ※穴は経路外
        // 実際: 上→(0,2) ※(0,3)壁, 右→(4,2) ※(5,2)壁, 上→(4,5), 右→(5,5)=Goal (4手)
        private StageData GetStage3()
        {
            return new StageData
            {
                gridSize = 6,
                ballStart = new Vector2Int(0, 0),
                goalPos = new Vector2Int(5, 5),
                walls = new List<Vector2Int> {
                    new Vector2Int(0, 3),
                    new Vector2Int(5, 2),
                    new Vector2Int(2, 5),
                },
                holes = new List<Vector2Int> {
                    new Vector2Int(3, 0), new Vector2Int(0, 4),
                },
                hasHole = true, hasTwoBalls = false, moveLimit = 0, minMoves = 4
            };
        }

        // Stage 4: 6x6, 手数制限12
        // 解: 右→上→右→上 (4手)
        // Ball(0,0)→右→(1,0)[壁(2,0)]→上→(1,4)[壁(1,5)]→右→(5,4)→上→(5,5)=Goal
        // 穴は解の経路(x=1のy列、y=4のx列)を避けた位置に配置
        private StageData GetStage4()
        {
            return new StageData
            {
                gridSize = 6,
                ballStart = new Vector2Int(0, 0),
                goalPos = new Vector2Int(5, 5),
                walls = new List<Vector2Int> {
                    new Vector2Int(2, 0),
                    new Vector2Int(1, 5),
                },
                holes = new List<Vector2Int> {
                    new Vector2Int(0, 2), new Vector2Int(3, 1), new Vector2Int(4, 2),
                },
                hasHole = true, hasTwoBalls = false, moveLimit = 12, minMoves = 4
            };
        }

        // Stage 5: 6x6, 2ボール同時ゴール
        // Ball(0,0)→Goal(0,5), Ball2(5,0)→Goal2(5,5)
        // 解: 上 (1手) Ball→(0,5)=Goal, Ball2→(5,5)=Goal2
        // 壁は中央帯に置いて経路は両端のみ通る
        private StageData GetStage5()
        {
            return new StageData
            {
                gridSize = 6,
                ballStart = new Vector2Int(0, 0),
                ball2Start = new Vector2Int(5, 0),
                goalPos = new Vector2Int(0, 5),
                goal2Pos = new Vector2Int(5, 5),
                walls = new List<Vector2Int> {
                    new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1), new Vector2Int(4, 1),
                    new Vector2Int(1, 3), new Vector2Int(2, 3), new Vector2Int(3, 3), new Vector2Int(4, 3),
                },
                holes = new List<Vector2Int> {
                    new Vector2Int(2, 2), new Vector2Int(3, 2),
                },
                hasHole = true, hasTwoBalls = true, moveLimit = 15, minMoves = 1
            };
        }

        #endregion
    }
}
