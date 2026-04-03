using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game008v2_IcePath
{
    public enum CellType { Ice, Wall, Rock, Crack, Hole, Redirect, Friction }

    [System.Serializable]
    public class CellState
    {
        public CellType type;
        public bool visited;
        public int crackCount; // ひび割れ氷の通過回数
        public Vector2Int redirectDir; // Redirect タイルの強制方向

        public CellState(CellType t) { type = t; }
    }

    public class IceBoardManager : MonoBehaviour
    {
        [SerializeField] private Sprite _iceSprite;
        [SerializeField] private Sprite _visitedSprite;
        [SerializeField] private Sprite _rockSprite;
        [SerializeField] private Sprite _crackSprite;
        [SerializeField] private Sprite _holeSprite;
        [SerializeField] private Sprite _redirectSprite;
        [SerializeField] private Sprite _frictionSprite;
        [SerializeField] private Sprite _playerSprite;

        private IcePathGameManager _gameManager;
        private CellState[,] _grid;
        private GameObject[,] _cellObjects;
        private GameObject _playerObject;
        private SpriteRenderer _playerSr;
        private int _gridSize;
        private float _cellSize;
        private float _startX;
        private float _startY;
        private Vector2Int _playerPos;
        private bool _isActive;
        private int _totalIceCells;
        private int _visitedCount;
        private int _minMoves;
        private int _currentStageIndex;

        // Undo stack
        private Stack<BoardSnapshot> _undoStack = new Stack<BoardSnapshot>();

        // Swipe detection
        private Vector2 _swipeStart;
        private bool _isSwiping;
        private const float SwipeThreshold = 0.5f; // world units

        // Stage layouts (0-based stage index)
        // W=Wall, I=Ice, R=Rock, C=Crack, D=Redirect, F=Friction, P=Player start
        // 0: free=traversable non-wall
        private static readonly string[][] StageLayouts = new string[][]
        {
            // Stage 0: 5x5, walls only
            new string[] {
                "WWWWW",
                "WIPWW",  // W=Wall, I=Ice, P=PlayerStart
                "WIIIIW",
                "WIIIIW",
                "WWWWW",
            },
            // Stage 1: 6x6, rocks added
            new string[] {
                "WWWWWW",
                "WIIIIIW",
                "WIRIIW",
                "WIIIIW",
                "WIIIPIW",
                "WWWWWW",
            },
            // Stage 2: 6x6, crack ice
            new string[] {
                "WWWWWW",
                "WIIIIW",
                "WICICIW",
                "WIIIIW",
                "WIICIIW",
                "WWWWWW",
            },
            // Stage 3: 7x7, redirect tiles
            new string[] {
                "WWWWWWW",
                "WIIIIIIW",
                "WIDIIIIW",
                "WIIIIIIW",
                "WIIDIIIIW",
                "WIIIIIIW",
                "WWWWWWW",
            },
            // Stage 4: 7x7, friction zones
            new string[] {
                "WWWWWWW",
                "WIIIIIIW",
                "WIFIFIW",
                "WIIIIIIW",
                "WIFIFIW",
                "WIIIIIIW",
                "WWWWWWW",
            },
        };

        // Proper stage data with correct player positions
        private static readonly int[][,] GridData;
        private static readonly Vector2Int[] PlayerStarts;
        private static readonly int[] MinMoves = { 7, 8, 9, 10, 11 };

        // CellType encoding: 0=Ice, 1=Wall, 2=Rock, 3=Crack, 4=Hole, 5=Redirect, 6=Friction
        static IceBoardManager()
        {
            GridData = new int[][,]
            {
                // Stage 0: 5x5
                new int[5, 5]
                {
                    {1,1,1,1,1},
                    {1,0,0,0,1},
                    {1,0,0,0,1},
                    {1,0,0,0,1},
                    {1,1,1,1,1},
                },
                // Stage 1: 6x6 with rocks
                new int[6, 6]
                {
                    {1,1,1,1,1,1},
                    {1,0,0,0,0,1},
                    {1,0,2,0,0,1},
                    {1,0,0,0,2,1},
                    {1,0,0,0,0,1},
                    {1,1,1,1,1,1},
                },
                // Stage 2: 6x6 with cracks
                new int[6, 6]
                {
                    {1,1,1,1,1,1},
                    {1,0,0,0,0,1},
                    {1,3,0,3,0,1},
                    {1,0,0,0,0,1},
                    {1,0,3,0,3,1},
                    {1,1,1,1,1,1},
                },
                // Stage 3: 7x7 with redirect tiles
                new int[7, 7]
                {
                    {1,1,1,1,1,1,1},
                    {1,0,0,0,0,0,1},
                    {1,5,0,0,0,0,1},
                    {1,0,0,0,0,0,1},
                    {1,0,0,5,0,0,1},
                    {1,0,0,0,0,0,1},
                    {1,1,1,1,1,1,1},
                },
                // Stage 4: 7x7 with friction zones
                new int[7, 7]
                {
                    {1,1,1,1,1,1,1},
                    {1,0,0,0,0,0,1},
                    {1,0,6,0,6,0,1},
                    {1,0,0,0,0,0,1},
                    {1,0,6,0,6,0,1},
                    {1,0,0,0,0,0,1},
                    {1,1,1,1,1,1,1},
                },
            };

            PlayerStarts = new Vector2Int[]
            {
                new Vector2Int(1, 1), // Stage 0
                new Vector2Int(1, 1), // Stage 1
                new Vector2Int(1, 1), // Stage 2
                new Vector2Int(1, 1), // Stage 3
                new Vector2Int(1, 1), // Stage 4
            };
        }

        private void Awake()
        {
            _gameManager = GetComponentInParent<IcePathGameManager>();
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        public void SetupStage(int stageIndex)
        {
            StopAllCoroutines();
            ClearBoard();
            _undoStack.Clear();

            _currentStageIndex = stageIndex;
            int idx = Mathf.Clamp(stageIndex, 0, GridData.Length - 1);
            var data = GridData[idx];
            _gridSize = data.GetLength(0);
            _minMoves = MinMoves[idx];

            _grid = new CellState[_gridSize, _gridSize];
            _cellObjects = new GameObject[_gridSize, _gridSize];

            float camSize = Camera.main != null ? Camera.main.orthographicSize : 6f;
            float camWidth = camSize * (Camera.main != null ? Camera.main.aspect : 0.5625f);
            float topMargin = 1.2f;
            float bottomMargin = 2.8f;
            float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
            float maxCellSize = 1.2f;
            _cellSize = Mathf.Min(availableHeight / _gridSize, camWidth * 2f / _gridSize, maxCellSize);

            float boardWidth = _cellSize * _gridSize;
            float boardHeight = _cellSize * _gridSize;
            _startX = -boardWidth / 2f + _cellSize / 2f;
            _startY = (camSize - topMargin) - _cellSize / 2f;
            float startX = _startX;
            float startY = _startY;

            _totalIceCells = 0;
            _visitedCount = 0;

            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    int v = data[row, col];
                    CellType ct = (CellType)v;
                    _grid[row, col] = new CellState(ct);

                    if (ct == CellType.Ice || ct == CellType.Crack ||
                        ct == CellType.Redirect || ct == CellType.Friction)
                        _totalIceCells++;

                    var cellGO = new GameObject($"Cell_{row}_{col}");
                    cellGO.transform.SetParent(transform);
                    float wx = startX + col * _cellSize;
                    float wy = startY - row * _cellSize;
                    cellGO.transform.position = new Vector3(wx, wy, 0f);
                    cellGO.transform.localScale = Vector3.one * (_cellSize * 0.92f);

                    var sr = cellGO.AddComponent<SpriteRenderer>();
                    sr.sprite = GetSprite(ct);
                    sr.sortingOrder = 1;
                    if (ct == CellType.Wall) sr.color = new Color(0.2f, 0.3f, 0.5f, 1f);
                    _cellObjects[row, col] = cellGO;
                }
            }

            // Player
            _playerPos = PlayerStarts[idx];
            CreatePlayer(startX, startY);

            // Mark start as visited
            VisitCell(_playerPos);

            _isActive = true;

            // Update UI remaining
            int remaining = _totalIceCells - _visitedCount;
            if (_gameManager != null) _gameManager.OnMoved(remaining);
        }

        private Sprite GetSprite(CellType ct)
        {
            return ct switch
            {
                CellType.Ice => _iceSprite,
                CellType.Rock => _rockSprite,
                CellType.Crack => _crackSprite,
                CellType.Hole => _holeSprite,
                CellType.Redirect => _redirectSprite,
                CellType.Friction => _frictionSprite,
                _ => null,
            };
        }

        private void CreatePlayer(float startX, float startY)
        {
            if (_playerObject != null) Destroy(_playerObject);
            _playerObject = new GameObject("Player");
            _playerObject.transform.SetParent(transform);
            _playerSr = _playerObject.AddComponent<SpriteRenderer>();
            _playerSr.sprite = _playerSprite;
            _playerSr.sortingOrder = 10;
            _playerObject.transform.localScale = Vector3.one * (_cellSize * 0.85f);
            UpdatePlayerPosition(startX, startY);
        }

        private void UpdatePlayerPosition(float startX, float startY)
        {
            if (_playerObject == null) return;
            float wx = startX + _playerPos.x * _cellSize;
            float wy = startY - _playerPos.y * _cellSize;
            _playerObject.transform.position = new Vector3(wx, wy, 0f);
        }

        private Vector3 GridToWorld(Vector2Int pos)
        {
            return new Vector3(_startX + pos.x * _cellSize, _startY - pos.y * _cellSize, 0f);
        }

        private void ClearBoard()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            _playerObject = null;
            _grid = null;
            _cellObjects = null;
        }

        public void ResetBoard()
        {
            SetupStage(_currentStageIndex);
        }

        private void VisitCell(Vector2Int pos)
        {
            var cell = _grid[pos.y, pos.x];
            if (cell.type == CellType.Crack)
            {
                cell.crackCount++;
                if (cell.crackCount >= 2)
                {
                    // Break the ice — also decrement _totalIceCells so clear condition stays reachable
                    // (Crack was counted once; visited once already. Hole is unreachable, so subtract.)
                    if (cell.visited) _totalIceCells--;
                    cell.type = CellType.Hole;
                    if (_cellObjects[pos.y, pos.x] != null)
                    {
                        var go = _cellObjects[pos.y, pos.x];
                        var sr = go.GetComponent<SpriteRenderer>();
                        if (sr != null) sr.sprite = _holeSprite;
                        StartCoroutine(CrackBreakEffect(go));
                    }
                    return; // Hole is not visitable
                }
            }

            if (!cell.visited && (cell.type == CellType.Ice || cell.type == CellType.Crack ||
                cell.type == CellType.Redirect || cell.type == CellType.Friction))
            {
                cell.visited = true;
                _visitedCount++;
                if (_cellObjects[pos.y, pos.x] != null)
                {
                    var sr = _cellObjects[pos.y, pos.x].GetComponent<SpriteRenderer>();
                    if (sr != null && cell.type != CellType.Redirect && cell.type != CellType.Friction)
                        sr.sprite = _visitedSprite;
                    StartCoroutine(CellVisitEffect(_cellObjects[pos.y, pos.x].transform));
                }
            }
        }

        private IEnumerator CellVisitEffect(Transform t)
        {
            if (t == null) yield break;
            Vector3 orig = t.localScale;
            t.localScale = orig * 1.25f;
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(orig * 1.25f, orig, elapsed / 0.15f);
                yield return null;
            }
            if (t != null) t.localScale = orig;
        }

        private IEnumerator CrackBreakEffect(GameObject go)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            Color orig = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = orig;
            yield return new WaitForSeconds(0.05f);
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = orig;
        }

        private IEnumerator PlayerMoveEffect()
        {
            if (_playerObject == null) yield break;
            Vector3 orig = _playerObject.transform.localScale;
            _playerObject.transform.localScale = orig * 1.3f;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                _playerObject.transform.localScale = Vector3.Lerp(orig * 1.3f, orig, elapsed / 0.2f);
                yield return null;
            }
            _playerObject.transform.localScale = orig;
        }

        private void Update()
        {
            if (!_isActive) return;
            HandleSwipeInput();
        }

        private void HandleSwipeInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _swipeStart = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
                _isSwiping = true;
            }
            else if (mouse.leftButton.wasReleasedThisFrame && _isSwiping)
            {
                _isSwiping = false;
                Vector2 swipeEnd = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
                Vector2 delta = swipeEnd - _swipeStart;
                if (delta.magnitude >= SwipeThreshold)
                {
                    Vector2Int dir = GetSwipeDirection(delta);
                    TryMove(dir);
                }
            }
        }

        private Vector2Int GetSwipeDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        // Public so UI buttons can call
        public void MoveUp() { if (_isActive) TryMove(Vector2Int.up); }
        public void MoveDown() { if (_isActive) TryMove(Vector2Int.down); }
        public void MoveLeft() { if (_isActive) TryMove(Vector2Int.left); }
        public void MoveRight() { if (_isActive) TryMove(Vector2Int.right); }

        private void TryMove(Vector2Int dir)
        {
            if (_gameManager != null && !_gameManager.IsPlaying) return;

            // Save snapshot for undo
            _undoStack.Push(TakeSnapshot());

            Vector2Int gridDir = new Vector2Int(dir.x, -dir.y); // Convert world up→grid up
            Vector2Int newPos = Slide(_playerPos, gridDir);

            if (newPos == _playerPos)
            {
                _undoStack.Pop(); // No movement, discard snapshot
                return;
            }

            _playerPos = newPos;
            _playerObject.transform.position = GridToWorld(_playerPos);
            StartCoroutine(PlayerMoveEffect());

            int remaining = _totalIceCells - _visitedCount;
            _gameManager?.OnMoved(remaining);

            // Check win
            if (_visitedCount >= _totalIceCells)
            {
                StartCoroutine(StageClearEffect());
            }
            else if (!HasAnyValidMove())
            {
                _gameManager?.OnGameOver();
            }
        }

        private Vector2Int Slide(Vector2Int from, Vector2Int dir)
        {
            Vector2Int pos = from;
            int maxSlide = _gridSize * _gridSize;
            int steps = 0;
            bool inFriction = false;
            int frictionSteps = 0;

            while (steps < maxSlide)
            {
                Vector2Int next = pos + dir;

                // Boundary / wall / rock / hole check
                if (!InBounds(next)) break;
                var nextCell = _grid[next.y, next.x];
                if (nextCell.type == CellType.Wall || nextCell.type == CellType.Rock || nextCell.type == CellType.Hole)
                    break;

                pos = next;
                VisitCell(pos);
                steps++;

                // Friction zone: limit to 2 steps
                if (_grid[pos.y, pos.x].type == CellType.Friction)
                {
                    inFriction = true;
                    frictionSteps++;
                    if (frictionSteps >= 2) break;
                }
                else if (inFriction)
                {
                    // Left friction zone — reset counter
                    inFriction = false;
                    frictionSteps = 0;
                }

                // Redirect tile: change direction
                if (_grid[pos.y, pos.x].type == CellType.Redirect)
                {
                    // Alternate between right and down based on entry direction
                    dir = dir.x != 0 ? new Vector2Int(0, 1) : new Vector2Int(1, 0);
                }
            }

            return pos;
        }

        private bool InBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _gridSize && pos.y >= 0 && pos.y < _gridSize;
        }

        private bool HasAnyValidMove()
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in dirs)
            {
                Vector2Int next = _playerPos + dir;
                if (!InBounds(next)) continue;
                var cell = _grid[next.y, next.x];
                if (cell.type != CellType.Wall && cell.type != CellType.Rock && cell.type != CellType.Hole)
                    return true;
            }
            return false;
        }

        private IEnumerator StageClearEffect()
        {
            _isActive = false; // Prevent further input during clear animation
            // Flash all visited cells green
            for (int row = 0; row < _gridSize; row++)
                for (int col = 0; col < _gridSize; col++)
                    if (_cellObjects[row, col] != null && _grid[row, col].visited)
                    {
                        var sr = _cellObjects[row, col].GetComponent<SpriteRenderer>();
                        if (sr != null) sr.color = new Color(0.5f, 1f, 0.5f, 1f);
                    }

            yield return new WaitForSeconds(0.4f);

            for (int row = 0; row < _gridSize; row++)
                for (int col = 0; col < _gridSize; col++)
                    if (_cellObjects[row, col] != null)
                    {
                        var sr = _cellObjects[row, col].GetComponent<SpriteRenderer>();
                        if (sr != null) sr.color = Color.white;
                    }

            _gameManager?.OnStageClear(_minMoves);
        }

        // Undo
        public void UndoMove()
        {
            if (_undoStack.Count == 0) return;
            if (_gameManager != null && !_gameManager.IsPlaying) return;
            RestoreSnapshot(_undoStack.Pop());
        }

        private BoardSnapshot TakeSnapshot()
        {
            var snap = new BoardSnapshot();
            snap.playerPos = _playerPos;
            snap.visitedCount = _visitedCount;
            snap.cells = new CellSnapshot[_gridSize, _gridSize];
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    snap.cells[r, c] = new CellSnapshot(_grid[r, c]);
            return snap;
        }

        private void RestoreSnapshot(BoardSnapshot snap)
        {
            _playerPos = snap.playerPos;
            _visitedCount = snap.visitedCount;

            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    var cell = _grid[r, c];
                    var s = snap.cells[r, c];
                    cell.type = s.type;
                    cell.visited = s.visited;
                    cell.crackCount = s.crackCount;

                    if (_cellObjects[r, c] != null)
                    {
                        var sr = _cellObjects[r, c].GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.sprite = cell.visited ?
                                (cell.type == CellType.Redirect || cell.type == CellType.Friction ? GetSprite(cell.type) : _visitedSprite) :
                                GetSprite(cell.type);
                            sr.color = Color.white;
                        }
                    }
                }
            }

            if (_playerObject != null)
                _playerObject.transform.position = GridToWorld(_playerPos);

            int remaining = _totalIceCells - _visitedCount;
            _gameManager?.OnMoved(remaining);
        }

        private struct BoardSnapshot
        {
            public Vector2Int playerPos;
            public int visitedCount;
            public CellSnapshot[,] cells;
        }

        private struct CellSnapshot
        {
            public CellType type;
            public bool visited;
            public int crackCount;

            public CellSnapshot(CellState s)
            {
                type = s.type;
                visited = s.visited;
                crackCount = s.crackCount;
            }
        }
    }
}
