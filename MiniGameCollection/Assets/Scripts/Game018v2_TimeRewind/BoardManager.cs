using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game018v2_TimeRewind
{
    public enum CellType
    {
        Empty,
        Wall,
        Goal,
        Switch,
        Ice,
        Bomb,
        Ghost
    }

    public class BoardSnapshot
    {
        public Vector2Int PlayerPos;
        public CellType[,] Cells;
        public Vector2Int? GhostPos;
        public int BombCountdown;
        public bool BombActive;

        public BoardSnapshot(Vector2Int pos, CellType[,] cells, Vector2Int? ghostPos, int bombCD, bool bombActive)
        {
            PlayerPos = pos;
            int r = cells.GetLength(0);
            int c = cells.GetLength(1);
            Cells = new CellType[r, c];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    Cells[i, j] = cells[i, j];
            GhostPos = ghostPos;
            BombCountdown = bombCD;
            BombActive = bombActive;
        }
    }

    public class BoardManager : MonoBehaviour
    {
        [SerializeField] Sprite _playerSprite;
        [SerializeField] Sprite _goalSprite;
        [SerializeField] Sprite _wallSprite;
        [SerializeField] Sprite _floorSprite;
        [SerializeField] Sprite _switchSprite;
        [SerializeField] Sprite _iceSprite;
        [SerializeField] Sprite _bombSprite;
        [SerializeField] Sprite _ghostSprite;

        TimeRewindGameManager _gm;
        TimeRewindUI _ui;

        int _gridSize;
        int _rewindsAllowed;
        int _rewindsUsed;
        int _moveCount;
        int _optimalMoves;
        bool _hasGhost;
        bool _hasBomb;
        int _bombN;
        int _bombCountdown;
        bool _bombActive;

        CellType[,] _cells;
        Vector2Int _playerPos;
        Vector2Int? _ghostPos;
        List<BoardSnapshot> _history = new List<BoardSnapshot>();

        GameObject[,] _cellObjects;
        GameObject _playerObj;
        GameObject _ghostObj;
        float _cellSize;
        Vector3 _boardOrigin;

        bool _isActive;
        bool _inputLocked;
        Vector2 _dragStart;
        bool _isDragging;

        static readonly Vector2Int[] Dirs = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        void Awake()
        {
            _gm = GetComponentInParent<TimeRewindGameManager>();
            _ui = FindFirstObjectByType<TimeRewindUI>();
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            _isActive = true;
            _inputLocked = false;
            _rewindsUsed = 0;
            _moveCount = 0;
            _bombActive = false;
            _isDragging = false;
            _history.Clear();

            // Determine grid size from speedMultiplier (1.0=5, 1.2=6, 1.4=7, 1.6=8)
            _gridSize = stageNumber <= 1 ? 5 : stageNumber == 2 ? 6 : stageNumber == 3 ? 7 : 8;
            _rewindsAllowed = stageNumber == 5 ? 2 : 3;
            _hasGhost = stageNumber == 5;
            _hasBomb = stageNumber >= 4;
            _bombN = 5;

            GenerateBoard(stageNumber);
            RenderBoard();

            _optimalMoves = CalculateOptimalMoves();

            if (_ui != null)
            {
                _ui.UpdateRewindCount(_rewindsAllowed - _rewindsUsed, _rewindsAllowed);
                _ui.UpdateMoveCount(_moveCount);
                _ui.HideBombCountdown();
            }
        }

        void GenerateBoard(int stageNumber)
        {
            _cells = new CellType[_gridSize, _gridSize];
            // Default all empty
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    _cells[r, c] = CellType.Empty;

            _playerPos = new Vector2Int(0, 0);
            Vector2Int goalPos = new Vector2Int(_gridSize - 1, _gridSize - 1);
            _cells[goalPos.x, goalPos.y] = CellType.Goal;
            _ghostPos = null;

            // Place walls based on stage
            PlaceWalls(stageNumber);

            // Place special cells
            if (stageNumber >= 2)
                PlaceSwitches();
            if (stageNumber >= 3)
                PlaceIce();
            if (stageNumber >= 4)
                PlaceBombs();
        }

        void PlaceWalls(int stageNumber)
        {
            int wallCount = stageNumber == 1 ? 2 :
                            stageNumber == 2 ? 4 :
                            stageNumber == 3 ? 6 :
                            stageNumber == 4 ? 7 : 7;

            System.Random rng = new System.Random(stageNumber * 137 + _gridSize);
            int placed = 0;
            int tries = 0;
            while (placed < wallCount && tries < 200)
            {
                tries++;
                int r = rng.Next(0, _gridSize);
                int c = rng.Next(0, _gridSize);
                if ((r == 0 && c == 0) || (r == _gridSize - 1 && c == _gridSize - 1)) continue;
                if (_cells[r, c] != CellType.Empty) continue;
                _cells[r, c] = CellType.Wall;
                if (!IsPathExists())
                {
                    _cells[r, c] = CellType.Empty;
                    continue;
                }
                placed++;
            }
        }

        void PlaceSwitches()
        {
            // Place 1-2 switch tiles on valid empty cells
            System.Random rng = new System.Random(42);
            int count = 0;
            for (int r = 1; r < _gridSize - 1 && count < 2; r++)
            {
                for (int c = 1; c < _gridSize - 1 && count < 2; c++)
                {
                    if (_cells[r, c] == CellType.Empty && rng.Next(0, 4) == 0)
                    {
                        _cells[r, c] = CellType.Switch;
                        count++;
                    }
                }
            }
        }

        void PlaceIce()
        {
            System.Random rng = new System.Random(99);
            int count = 0;
            for (int r = 0; r < _gridSize && count < 3; r++)
            {
                for (int c = 0; c < _gridSize && count < 3; c++)
                {
                    if (_cells[r, c] == CellType.Empty && !(r == 0 && c == 0) && rng.Next(0, 5) == 0)
                    {
                        _cells[r, c] = CellType.Ice;
                        count++;
                    }
                }
            }
        }

        void PlaceBombs()
        {
            // Place 1 bomb tile not adjacent to start/goal
            int margin = Mathf.Max(1, _gridSize / 4);
            for (int r = margin; r < _gridSize - margin; r++)
            {
                for (int c = margin; c < _gridSize - margin; c++)
                {
                    if (_cells[r, c] == CellType.Empty)
                    {
                        _cells[r, c] = CellType.Bomb;
                        return;
                    }
                }
            }
            // Fallback: place anywhere empty except start/goal
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    if (_cells[r, c] == CellType.Empty && !(r == 0 && c == 0) && !(r == _gridSize - 1 && c == _gridSize - 1))
                    { _cells[r, c] = CellType.Bomb; return; }
        }

        bool IsPathExists()
        {
            bool[,] visited = new bool[_gridSize, _gridSize];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(_playerPos);
            visited[_playerPos.x, _playerPos.y] = true;

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (_cells[cur.x, cur.y] == CellType.Goal) return true;
                foreach (var d in Dirs)
                {
                    var next = cur + d;
                    if (!InBounds(next)) continue;
                    if (visited[next.x, next.y]) continue;
                    if (_cells[next.x, next.y] == CellType.Wall) continue;
                    visited[next.x, next.y] = true;
                    queue.Enqueue(next);
                }
            }
            return false;
        }

        int CalculateOptimalMoves()
        {
            // BFS for shortest path
            int[,] dist = new int[_gridSize, _gridSize];
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    dist[r, c] = int.MaxValue;

            dist[_playerPos.x, _playerPos.y] = 0;
            Queue<Vector2Int> q = new Queue<Vector2Int>();
            q.Enqueue(_playerPos);

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                if (_cells[cur.x, cur.y] == CellType.Goal) return dist[cur.x, cur.y];
                foreach (var d in Dirs)
                {
                    var next = cur + d;
                    if (!InBounds(next) || dist[next.x, next.y] != int.MaxValue) continue;
                    if (_cells[next.x, next.y] == CellType.Wall) continue;
                    dist[next.x, next.y] = dist[cur.x, cur.y] + 1;
                    q.Enqueue(next);
                }
            }
            return _gridSize * 2;
        }

        void RenderBoard()
        {
            // Cleanup existing
            if (_cellObjects != null)
            {
                for (int r = 0; r < _cellObjects.GetLength(0); r++)
                    for (int c = 0; c < _cellObjects.GetLength(1); c++)
                        if (_cellObjects[r, c] != null) Destroy(_cellObjects[r, c]);
            }
            if (_playerObj != null) Destroy(_playerObj);
            if (_ghostObj != null) Destroy(_ghostObj);

            if (Camera.main == null) return;
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.3f;
            float bottomMargin = 2.8f;
            float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
            _cellSize = Mathf.Min(availableHeight / _gridSize, camWidth * 2f / _gridSize, 0.9f);

            float totalWidth = _cellSize * _gridSize;
            float totalHeight = _cellSize * _gridSize;
            _boardOrigin = new Vector3(
                -totalWidth / 2f + _cellSize / 2f,
                -camSize + bottomMargin + _cellSize / 2f,
                0f
            );

            _cellObjects = new GameObject[_gridSize, _gridSize];
            float spriteSize = 128f;

            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    var pos = CellToWorld(new Vector2Int(r, c));
                    var obj = new GameObject($"Cell_{r}_{c}");
                    obj.transform.position = pos;
                    obj.transform.localScale = Vector3.one * (_cellSize / spriteSize * 100f);

                    var sr = obj.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 0;

                    Sprite cellSprite = GetCellSprite(_cells[r, c]);
                    sr.sprite = cellSprite;

                    _cellObjects[r, c] = obj;
                }
            }

            SpawnPlayer();
        }

        void SpawnPlayer()
        {
            if (_playerObj != null) Destroy(_playerObj);
            _playerObj = new GameObject("Player");
            _playerObj.transform.position = CellToWorld(_playerPos);
            float spriteSize = 128f;
            _playerObj.transform.localScale = Vector3.one * (_cellSize / spriteSize * 100f);
            var sr = _playerObj.AddComponent<SpriteRenderer>();
            sr.sprite = _playerSprite;
            sr.sortingOrder = 2;
        }

        Sprite GetCellSprite(CellType type)
        {
            return type switch
            {
                CellType.Wall => _wallSprite,
                CellType.Goal => _goalSprite,
                CellType.Switch => _switchSprite,
                CellType.Ice => _iceSprite,
                CellType.Bomb => _bombSprite,
                CellType.Ghost => _ghostSprite,
                _ => _floorSprite
            };
        }

        Vector3 CellToWorld(Vector2Int cell)
        {
            return _boardOrigin + new Vector3(cell.y * _cellSize, cell.x * _cellSize, 0f);
        }

        bool InBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _gridSize && pos.y >= 0 && pos.y < _gridSize;
        }

        void Update()
        {
            if (!_isActive || _inputLocked) return;
            if (_gm.State != TimeRewindGameManager.GameState.Playing) return;

            HandleSwipeInput();
        }

        void HandleSwipeInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _dragStart = mouse.position.ReadValue();
                _isDragging = true;
            }

            if (_isDragging && mouse.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
                Vector2 dragEnd = mouse.position.ReadValue();
                Vector2 delta = dragEnd - _dragStart;

                if (delta.magnitude < 30f) return;

                Vector2Int dir;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    dir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
                else
                    dir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

                TryMove(dir);
            }
        }

        void TryMove(Vector2Int dir)
        {
            var newPos = _playerPos + dir;
            if (!InBounds(newPos)) return;
            if (_cells[newPos.x, newPos.y] == CellType.Wall) return;

            // Save snapshot before move
            _history.Add(new BoardSnapshot(_playerPos, _cells, _ghostPos, _bombCountdown, _bombActive));

            _playerPos = newPos;
            _moveCount++;

            // Handle bomb countdown
            if (_bombActive)
            {
                _bombCountdown--;
                if (_bombCountdown <= 0)
                {
                    OnBombExplode();
                    return;
                }
                if (_ui != null) _ui.UpdateBombCountdown(_bombCountdown);
            }

            // Apply cell effects
            var cellType = _cells[_playerPos.x, _playerPos.y];

            if (cellType == CellType.Ice)
                StartCoroutine(SlideOnIce(dir));
            else
            {
                ApplyCellEffect(_playerPos);
                UpdateVisuals();
                CheckWinCondition();
            }

            if (_ui != null) _ui.UpdateMoveCount(_moveCount);
        }

        void ApplyCellEffect(Vector2Int pos)
        {
            var cellType = _cells[pos.x, pos.y];

            if (cellType == CellType.Switch)
                ToggleWalls();

            if (cellType == CellType.Bomb && !_bombActive)
            {
                _bombActive = true;
                _bombCountdown = _bombN;
                if (_ui != null) _ui.UpdateBombCountdown(_bombCountdown);
            }

            // Goal effect handled solely by CheckWinCondition to avoid double-trigger
        }

        void ToggleWalls()
        {
            // Toggle all Wall cells: walls become Empty, empty non-special cells near original walls are set back
            // Simple approach: remove all walls temporarily (make passable for next few moves)
            // Implementation: flip Wall <-> a hidden state using a separate toggle flag
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    if (_cells[r, c] == CellType.Wall)
                        _cells[r, c] = CellType.Empty;
            // Re-render board to reflect wall removal
            RenderBoard();
        }

        IEnumerator SlideOnIce(Vector2Int dir)
        {
            _inputLocked = true;
            while (true)
            {
                var next = _playerPos + dir;
                if (!InBounds(next) || _cells[next.x, next.y] == CellType.Wall)
                    break;

                _playerPos = next;
                UpdatePlayerVisual();
                yield return new WaitForSeconds(0.1f);

                var type = _cells[_playerPos.x, _playerPos.y];
                if (type == CellType.Goal || type == CellType.Switch || type == CellType.Bomb)
                {
                    ApplyCellEffect(_playerPos);
                    break;
                }
                if (type != CellType.Ice)
                    break;
            }
            _inputLocked = false;
            UpdateVisuals();
            CheckWinCondition();
        }

        void UpdateVisuals()
        {
            UpdatePlayerVisual();
            if (_ghostObj != null && _ghostPos.HasValue)
                _ghostObj.transform.position = CellToWorld(_ghostPos.Value);
        }

        void UpdatePlayerVisual()
        {
            if (_playerObj != null)
                _playerObj.transform.position = CellToWorld(_playerPos);
        }

        IEnumerator PlayGoalEffect()
        {
            _isActive = false;
            // Scale pulse
            if (_playerObj != null)
            {
                float t = 0f;
                Vector3 origScale = _playerObj.transform.localScale;
                while (t < 0.3f)
                {
                    t += Time.deltaTime;
                    float s = 1f + 0.4f * Mathf.Sin(t / 0.3f * Mathf.PI);
                    _playerObj.transform.localScale = origScale * s;
                    yield return null;
                }
                _playerObj.transform.localScale = origScale;
            }
            _gm.OnStageClear(_rewindsUsed, _rewindsAllowed, _moveCount, _optimalMoves);
        }

        void CheckWinCondition()
        {
            if (_cells[_playerPos.x, _playerPos.y] != CellType.Goal) return;
            if (_hasGhost && _ghostPos.HasValue && _ghostPos.Value != _playerPos) return;

            StartCoroutine(PlayGoalEffect());
        }

        void OnBombExplode()
        {
            StartCoroutine(PlayBombEffect());
        }

        IEnumerator PlayBombEffect()
        {
            _isActive = false;
            // Red flash
            if (_playerObj != null)
            {
                var sr = _playerObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.red;
                    yield return new WaitForSeconds(0.3f);
                }
            }
            _gm.OnGameOver();
        }

        public void RequestRewind()
        {
            if (_history.Count == 0) return;
            if (_ui != null)
                _ui.ShowTimelinePanel(_history.Count);
        }

        public void RewindTo(int historyIndex)
        {
            if (historyIndex < 0 || historyIndex >= _history.Count) return;
            if (_rewindsUsed >= _rewindsAllowed)
            {
                if (_ui != null) _ui.HideTimelinePanel();
                return;
            }

            // Stop any running ice-slide coroutine
            StopAllCoroutines();
            _inputLocked = false;

            var snapshot = _history[historyIndex];

            // Stage 5: save ghost position before rewind
            if (_hasGhost)
                _ghostPos = _playerPos;

            _playerPos = snapshot.PlayerPos;
            // Restore cells
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    _cells[r, c] = snapshot.Cells[r, c];
            _bombCountdown = snapshot.BombCountdown;
            _bombActive = snapshot.BombActive;

            // Trim history
            _history.RemoveRange(historyIndex, _history.Count - historyIndex);

            _rewindsUsed++;

            if (_ui != null)
            {
                _ui.UpdateRewindCount(_rewindsAllowed - _rewindsUsed, _rewindsAllowed);
                _ui.HideTimelinePanel();
                _ui.ShowRewindEffect();
            }

            // Update ghost object
            UpdateGhostObject();
            UpdateVisuals();
        }

        void UpdateGhostObject()
        {
            if (!_hasGhost) return;

            if (_ghostPos.HasValue)
            {
                if (_ghostObj == null)
                {
                    _ghostObj = new GameObject("Ghost");
                    float spriteSize = 128f;
                    _ghostObj.transform.localScale = Vector3.one * (_cellSize / spriteSize * 100f);
                    var sr = _ghostObj.AddComponent<SpriteRenderer>();
                    sr.sprite = _ghostSprite;
                    sr.sortingOrder = 1;
                }
                _ghostObj.transform.position = CellToWorld(_ghostPos.Value);
            }
        }

        bool HasAnyMove()
        {
            foreach (var d in Dirs)
            {
                var next = _playerPos + d;
                if (InBounds(next) && _cells[next.x, next.y] != CellType.Wall)
                    return true;
            }
            return false;
        }

        public void CancelRewind()
        {
            if (_ui != null) _ui.HideTimelinePanel();
        }

        public bool CanRewind() => _rewindsUsed < _rewindsAllowed && _history.Count > 0;

        void OnDestroy()
        {
            // Cleanup dynamically created GameObjects handled by Unity scene lifecycle
        }
    }
}
