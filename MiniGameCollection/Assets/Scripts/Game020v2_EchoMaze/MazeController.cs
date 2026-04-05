using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game020v2_EchoMaze
{
    public enum FloorType { Stone, Wood, Water }
    public enum Direction { Up, Down, Left, Right }

    public class MazeController : MonoBehaviour
    {
        [SerializeField] EchoMazeGameManager _gameManager;
        [SerializeField] EchoMazeUI _ui;
        [SerializeField] Sprite _playerSprite;
        [SerializeField] Sprite _cellSprite;
        [SerializeField] Sprite _goalSprite;
        [SerializeField] Sprite _portalSprite;

        // Grid data: true = wall, false = floor
        bool[,] _walls;
        FloorType[,] _floorTypes;
        bool[,] _isDisturbZone;
        bool[,] _isMovingWallCandidate;
        bool[,] _visited;
        Vector2Int _playerPos;
        Vector2Int _goalPos;
        Vector2Int _portalPos;
        bool _isOnSecondFloor;

        int _gridSize;
        int _moveLimit;
        int _movesUsed;
        bool _hasFloorTypes;
        bool _hasMovingWalls;
        bool _hasEchoDisturb;
        bool _hasTwoFloors;
        bool _isActive;
        int _movingWallTimer;
        const int MovingWallInterval = 5;

        GameObject _playerObj;
        GameObject[,] _cellObjs;
        GameObject _goalObj;
        GameObject _portalObj;

        // Explored cell visuals
        SpriteRenderer[,] _cellRenderers;

        void Awake()
        {
            _isActive = false;
        }

        public void SetupStage(int stage, StageManager.StageConfig config)
        {
            ClearAll();

            switch (stage)
            {
                case 1: _gridSize = 5; _moveLimit = 30; _hasFloorTypes = false; _hasMovingWalls = false; _hasEchoDisturb = false; _hasTwoFloors = false; break;
                case 2: _gridSize = 7; _moveLimit = 50; _hasFloorTypes = true; _hasMovingWalls = false; _hasEchoDisturb = false; _hasTwoFloors = false; break;
                case 3: _gridSize = 9; _moveLimit = 70; _hasFloorTypes = true; _hasMovingWalls = true; _hasEchoDisturb = false; _hasTwoFloors = false; break;
                case 4: _gridSize = 9; _moveLimit = 60; _hasFloorTypes = true; _hasMovingWalls = true; _hasEchoDisturb = true; _hasTwoFloors = false; break;
                default: _gridSize = 11; _moveLimit = 80; _hasFloorTypes = true; _hasMovingWalls = true; _hasEchoDisturb = true; _hasTwoFloors = true; break;
            }

            _movesUsed = 0;
            _isOnSecondFloor = false;
            _movingWallTimer = 0;

            GenerateMaze(_gridSize);
            PlaceObjects();
            _ui.SetMoveLimit(_moveLimit, _moveLimit - _movesUsed);
            UpdateEchoDisplay();
            _isActive = true;
        }

        void GenerateMaze(int size)
        {
            _walls = new bool[size, size];
            _floorTypes = new FloorType[size, size];
            _isDisturbZone = new bool[size, size];
            _isMovingWallCandidate = new bool[size, size];
            _visited = new bool[size, size];

            // Initialize all as walls
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    _walls[x, y] = true;

            // Recursive DFS maze generation
            var rng = new System.Random();
            GenerateDFS(1, 1, rng, size);

            // Ensure start (1,1) and end area are open
            _walls[1, 1] = false;
            _walls[size - 2, size - 2] = false;

            // Assign floor types (Stage 2+)
            if (_hasFloorTypes)
            {
                FloorType[] types = { FloorType.Stone, FloorType.Wood, FloorType.Water };
                for (int x = 0; x < size; x++)
                    for (int y = 0; y < size; y++)
                        if (!_walls[x, y])
                            _floorTypes[x, y] = types[rng.Next(3)];
            }

            // Mark disturb zones (Stage 4+) - ~20% of floor cells
            if (_hasEchoDisturb)
            {
                for (int x = 0; x < size; x++)
                    for (int y = 0; y < size; y++)
                        if (!_walls[x, y] && (x != 1 || y != 1) && rng.Next(5) == 0)
                            _isDisturbZone[x, y] = true;
            }

            // Mark moving wall candidates (Stage 3+)
            if (_hasMovingWalls)
            {
                int count = size / 3;
                int placed = 0;
                int attempts = 0;
                while (placed < count && attempts < 100)
                {
                    int wx = rng.Next(1, size - 1);
                    int wy = rng.Next(1, size - 1);
                    if (_walls[wx, wy] && !IsEssentialWall(wx, wy, size))
                    {
                        _isMovingWallCandidate[wx, wy] = true;
                        placed++;
                    }
                    attempts++;
                }
            }
        }

        void GenerateDFS(int x, int y, System.Random rng, int size)
        {
            _walls[x, y] = false;
            int[] dirs = { 0, 1, 2, 3 };
            // Fisher-Yates shuffle
            for (int i = 3; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int tmp = dirs[i]; dirs[i] = dirs[j]; dirs[j] = tmp;
            }
            foreach (int d in dirs)
            {
                int dx = d == 2 ? -2 : d == 3 ? 2 : 0;
                int dy = d == 0 ? 2 : d == 1 ? -2 : 0;
                int nx = x + dx;
                int ny = y + dy;
                if (nx > 0 && nx < size - 1 && ny > 0 && ny < size - 1 && _walls[nx, ny])
                {
                    _walls[x + dx / 2, y + dy / 2] = false;
                    GenerateDFS(nx, ny, rng, size);
                }
            }
        }

        bool IsEssentialWall(int x, int y, int size)
        {
            // Don't mark border walls or walls around start/goal
            if (x == 0 || x == size - 1 || y == 0 || y == size - 1) return true;
            if (Mathf.Abs(x - 1) <= 1 && Mathf.Abs(y - 1) <= 1) return true;
            if (Mathf.Abs(x - (size - 2)) <= 1 && Mathf.Abs(y - (size - 2)) <= 1) return true;
            return false;
        }

        void PlaceObjects()
        {
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[MazeController] Camera.main is null"); return; }
            float camSize = cam.orthographicSize;
            float camWidth = camSize * cam.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 3.0f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            float cellSize = Mathf.Min(availableHeight / _gridSize, camWidth * 2f / _gridSize, 0.8f);

            float totalW = cellSize * _gridSize;
            float totalH = cellSize * _gridSize;
            float startX = -totalW / 2f + cellSize / 2f;
            float startY = camSize - topMargin - cellSize / 2f;

            _cellObjs = new GameObject[_gridSize, _gridSize];
            _cellRenderers = new SpriteRenderer[_gridSize, _gridSize];

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    if (_walls[x, y]) continue;
                    var cell = new GameObject($"Cell_{x}_{y}");
                    cell.transform.SetParent(transform);
                    cell.transform.position = new Vector3(
                        startX + x * cellSize,
                        startY - y * cellSize,
                        0f
                    );
                    cell.transform.localScale = Vector3.one * cellSize * 0.95f;
                    var sr = cell.AddComponent<SpriteRenderer>();
                    sr.sprite = _cellSprite;
                    sr.color = new Color(0.15f, 0.25f, 0.55f, 0f); // hidden initially
                    sr.sortingOrder = 0;
                    _cellObjs[x, y] = cell;
                    _cellRenderers[x, y] = sr;
                }
            }

            // Player at (1,1)
            _playerPos = new Vector2Int(1, 1);
            _playerObj = new GameObject("Player");
            _playerObj.transform.SetParent(transform);
            _playerObj.transform.position = GridToWorld(1, 1, startX, startY, cellSize);
            _playerObj.transform.localScale = Vector3.one * cellSize * 0.85f;
            var psr = _playerObj.AddComponent<SpriteRenderer>();
            psr.sprite = _playerSprite;
            psr.sortingOrder = 2;
            // Mark start cell as visited without triggering combo counter
            _visited[1, 1] = true;
            if (_cellRenderers[1, 1] != null)
            {
                Color c = GetFloorColor(1, 1);
                c.a = 0.7f;
                _cellRenderers[1, 1].color = c;
            }

            // Goal at (size-2, size-2)
            _goalPos = new Vector2Int(_gridSize - 2, _gridSize - 2);
            _goalObj = new GameObject("Goal");
            _goalObj.transform.SetParent(transform);
            _goalObj.transform.position = GridToWorld(_gridSize - 2, _gridSize - 2, startX, startY, cellSize);
            _goalObj.transform.localScale = Vector3.one * cellSize * 0.85f;
            var gsr = _goalObj.AddComponent<SpriteRenderer>();
            gsr.sprite = _goalSprite;
            gsr.sortingOrder = 1;
            // Goal is hidden initially too
            gsr.color = new Color(1f, 1f, 1f, 0f);

            // Portal for Stage 5
            if (_hasTwoFloors)
            {
                _portalPos = new Vector2Int(_gridSize / 2, _gridSize / 2);
                if (_walls[_portalPos.x, _portalPos.y])
                {
                    bool found = false;
                    for (int px = 1; px < _gridSize - 1 && !found; px++)
                        for (int py = 1; py < _gridSize - 1 && !found; py++)
                            if (!_walls[px, py] && !(px == 1 && py == 1) && !(px == _gridSize - 2 && py == _gridSize - 2))
                            { _portalPos = new Vector2Int(px, py); found = true; }
                }
                _portalObj = new GameObject("Portal");
                _portalObj.transform.SetParent(transform);
                _portalObj.transform.position = GridToWorld(_portalPos.x, _portalPos.y, startX, startY, cellSize);
                _portalObj.transform.localScale = Vector3.one * cellSize * 0.85f;
                var portsr = _portalObj.AddComponent<SpriteRenderer>();
                portsr.sprite = _portalSprite;
                portsr.sortingOrder = 1;
                portsr.color = new Color(1f, 1f, 1f, 0f); // hidden initially
            }

            // Store for animation
            _stageStartX = startX;
            _stageStartY = startY;
            _stageCellSize = cellSize;
        }

        float _stageStartX, _stageStartY, _stageCellSize;

        Vector3 GridToWorld(int x, int y, float startX, float startY, float cellSize)
        {
            return new Vector3(startX + x * cellSize, startY - y * cellSize, 0f);
        }

        void MarkVisited(int x, int y)
        {
            if (_visited[x, y]) return;
            _visited[x, y] = true;
            _gameManager.OnExploredNewCell();
            if (_cellRenderers[x, y] != null)
            {
                Color c = GetFloorColor(x, y);
                c.a = 0.7f;
                _cellRenderers[x, y].color = c;
            }
        }

        Color GetFloorColor(int x, int y)
        {
            if (!_hasFloorTypes) return new Color(0.2f, 0.35f, 0.7f);
            switch (_floorTypes[x, y])
            {
                case FloorType.Stone: return new Color(0.3f, 0.5f, 0.8f);  // blue
                case FloorType.Wood:  return new Color(0.3f, 0.7f, 0.4f);  // green
                case FloorType.Water: return new Color(0.7f, 0.3f, 0.3f);  // red
                default: return new Color(0.25f, 0.4f, 0.6f);
            }
        }

        public void TryMove(Direction dir)
        {
            if (!_isActive) return;
            int dx = dir == Direction.Left ? -1 : dir == Direction.Right ? 1 : 0;
            int dy = dir == Direction.Up ? -1 : dir == Direction.Down ? 1 : 0;
            int nx = _playerPos.x + dx;
            int ny = _playerPos.y + dy;

            if (nx < 0 || nx >= _gridSize || ny < 0 || ny >= _gridSize || _walls[nx, ny])
            {
                StartCoroutine(BumpAnimation());
                return;
            }

            // Check moving wall state
            if (_hasMovingWalls && _isMovingWallCandidate[nx, ny])
            {
                StartCoroutine(BumpAnimation());
                return;
            }

            _playerPos = new Vector2Int(nx, ny);
            _playerObj.transform.position = GridToWorld(nx, ny, _stageStartX, _stageStartY, _stageCellSize);
            bool wasNew = !_visited[nx, ny];
            MarkVisited(nx, ny);
            _movesUsed++;

            if (wasNew) StartCoroutine(ExploreAnimation());
            else StartCoroutine(MoveAnimation());

            _movingWallTimer++;
            if (_hasMovingWalls && _movingWallTimer >= MovingWallInterval)
            {
                _movingWallTimer = 0;
                ToggleMovingWalls();
            }

            _ui.SetMoveLimit(_moveLimit, _moveLimit - _movesUsed);
            UpdateEchoDisplay();

            // Portal check
            if (_hasTwoFloors && _portalPos == _playerPos)
            {
                _isOnSecondFloor = !_isOnSecondFloor;
                _ui.ShowFloorIndicator(_isOnSecondFloor);
            }

            // Goal check
            if (_goalPos == _playerPos)
            {
                if (!_hasTwoFloors || _isOnSecondFloor)
                {
                    ReachGoal();
                    return;
                }
            }

            if (_movesUsed >= _moveLimit)
            {
                _isActive = false;
                _gameManager.OnGameOver();
            }
        }

        void ToggleMovingWalls()
        {
            for (int x = 0; x < _gridSize; x++)
                for (int y = 0; y < _gridSize; y++)
                    if (_isMovingWallCandidate[x, y])
                    {
                        bool isOccupied = (x == _playerPos.x && y == _playerPos.y)
                                       || (x == _goalPos.x && y == _goalPos.y);
                        if (!isOccupied)
                            _walls[x, y] = !_walls[x, y];
                    }
        }

        // Button callback methods for SceneSetup wiring
        public void OnUpButton()    { TryMove(Direction.Up); }
        public void OnDownButton()  { TryMove(Direction.Down); }
        public void OnLeftButton()  { TryMove(Direction.Left); }
        public void OnRightButton() { TryMove(Direction.Right); }

        public void EmitEcho()
        {
            if (!_isActive) return;
            _gameManager.OnEchoUsed();
            UpdateEchoDisplay();
            StartCoroutine(EchoAnimation());
        }

        public void ToggleMap()
        {
            if (!_isActive) return;
            _gameManager.OnMapUsed();
            _ui.ToggleMapPanel(_visited, _gridSize, _playerPos, _goalPos);
        }

        void UpdateEchoDisplay()
        {
            int px = _playerPos.x;
            int py = _playerPos.y;

            // Disturb zone check
            if (_hasEchoDisturb && _isDisturbZone[px, py])
            {
                float[] randomEcho = {
                    Random.Range(0f, 1f), Random.Range(0f, 1f),
                    Random.Range(0f, 1f), Random.Range(0f, 1f)
                };
                FloorType ft = _hasFloorTypes ? _floorTypes[px, py] : FloorType.Stone;
                _ui.UpdateEchoIndicator(randomEcho[0], randomEcho[1], randomEcho[2], randomEcho[3], ft, true);
                return;
            }

            float north = GetEchoStrength(px, py, 0, -1);
            float south = GetEchoStrength(px, py, 0, 1);
            float west  = GetEchoStrength(px, py, -1, 0);
            float east  = GetEchoStrength(px, py, 1, 0);
            FloorType floorType = _hasFloorTypes ? _floorTypes[px, py] : FloorType.Stone;
            _ui.UpdateEchoIndicator(north, south, west, east, floorType, false);
        }

        float GetEchoStrength(int x, int y, int dx, int dy)
        {
            int dist = 0;
            int cx = x + dx;
            int cy = y + dy;
            while (cx >= 0 && cx < _gridSize && cy >= 0 && cy < _gridSize && !_walls[cx, cy])
            {
                dist++;
                cx += dx;
                cy += dy;
            }
            // dist=0 means wall right next to -> max echo (1.0)
            // dist increases -> echo weakens
            float maxDist = _gridSize / 2f;
            return Mathf.Clamp01(1f - dist / maxDist);
        }

        void ReachGoal()
        {
            _isActive = false;
            // Reveal goal
            if (_goalObj != null)
            {
                var sr = _goalObj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;
            }
            StartCoroutine(GoalAnimation());
            _gameManager.OnGoalReached(_moveLimit - _movesUsed, _moveLimit);
        }

        void ClearAll()
        {
            StopAllCoroutines();
            _isActive = false;
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            _playerObj = null;
            _goalObj = null;
            _portalObj = null;
            _cellObjs = null;
            _cellRenderers = null;
        }

        IEnumerator BumpAnimation()
        {
            if (_playerObj == null) yield break;
            Vector3 orig = _playerObj.transform.localScale;
            var sr = _playerObj.GetComponent<SpriteRenderer>();
            Color origColor = sr != null ? sr.color : Color.white;

            if (sr != null) sr.color = new Color(1f, 0.3f, 0.3f);
            float t = 0;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float shake = Mathf.Sin(t * 80f) * 0.05f;
                _playerObj.transform.localScale = orig + new Vector3(shake, 0, 0);
                yield return null;
            }
            _playerObj.transform.localScale = orig;
            if (sr != null) sr.color = origColor;
        }

        IEnumerator MoveAnimation()
        {
            if (_playerObj == null) yield break;
            Vector3 orig = _playerObj.transform.localScale;
            float t = 0;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float scale = 1f + 0.1f * Mathf.Sin(t / 0.15f * Mathf.PI);
                _playerObj.transform.localScale = orig * scale;
                yield return null;
            }
            _playerObj.transform.localScale = orig;
        }

        IEnumerator ExploreAnimation()
        {
            if (_playerObj == null) yield break;
            Vector3 orig = _playerObj.transform.localScale;
            float t = 0;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float scale = 1f + 0.3f * Mathf.Sin(t / 0.2f * Mathf.PI);
                _playerObj.transform.localScale = orig * scale;
                yield return null;
            }
            _playerObj.transform.localScale = orig;
        }

        IEnumerator EchoAnimation()
        {
            if (_playerObj == null) yield break;
            var sr = _playerObj.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            Color orig = sr.color;
            float t = 0;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float pulse = 0.5f + 0.5f * Mathf.Sin(t / 0.3f * Mathf.PI * 3f);
                sr.color = Color.Lerp(orig, new Color(0.5f, 0.8f, 1f), pulse * 0.5f);
                yield return null;
            }
            sr.color = orig;
        }

        IEnumerator GoalAnimation()
        {
            if (_playerObj == null) yield break;
            Vector3 orig = _playerObj.transform.localScale;
            float t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float scale = 1f + 0.5f * Mathf.Sin(t / 0.5f * Mathf.PI);
                _playerObj.transform.localScale = orig * scale;
                yield return null;
            }
            _playerObj.transform.localScale = orig;
        }
    }
}
