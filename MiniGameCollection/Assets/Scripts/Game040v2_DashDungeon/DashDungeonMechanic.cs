using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game040v2_DashDungeon
{
    public enum TileType
    {
        Floor,
        Wall,
        Spike,
        Exit,
        Ice,
        WarpA,
        WarpB
    }

    public class DashDungeonMechanic : MonoBehaviour
    {
        [SerializeField] DashDungeonGameManager _gameManager;
        [SerializeField] Sprite _spriteFloor;
        [SerializeField] Sprite _spriteWall;
        [SerializeField] Sprite _spritePlayer;
        [SerializeField] Sprite _spriteEnemy;
        [SerializeField] Sprite _spriteSpike;
        [SerializeField] Sprite _spriteExit;
        [SerializeField] Sprite _spriteIce;
        [SerializeField] Sprite _spriteWarpA;
        [SerializeField] Sprite _spriteWarpB;

        // Grid state
        TileType[,] _grid;
        int _gridSize;
        float _cellSize;
        Vector3 _gridOrigin;

        // GameObjects
        GameObject[,] _tileObjects;
        GameObject _playerObj;
        List<GameObject> _enemyObjects = new List<GameObject>();
        Vector2Int _playerPos;
        Vector2Int _exitPos;
        Vector2Int _warpAPos;
        Vector2Int _warpBPos;
        List<Vector2Int> _enemyPositions = new List<Vector2Int>();

        bool _isActive;
        bool _isMoving;
        int _currentHp;
        int _maxHp = 3;
        int _moves;
        int _minMoves;

        // Stage config
        struct StageConfig
        {
            public int gridSize;
            public int spikeCount;
            public int enemyCount;
            public int iceCount;
            public bool hasWarp;
        }

        static readonly StageConfig[] Stages = new StageConfig[]
        {
            new StageConfig { gridSize = 5, spikeCount = 0, enemyCount = 0, iceCount = 0, hasWarp = false },
            new StageConfig { gridSize = 7, spikeCount = 2, enemyCount = 0, iceCount = 0, hasWarp = false },
            new StageConfig { gridSize = 7, spikeCount = 2, enemyCount = 1, iceCount = 0, hasWarp = false },
            new StageConfig { gridSize = 9, spikeCount = 3, enemyCount = 1, iceCount = 2, hasWarp = false },
            new StageConfig { gridSize = 9, spikeCount = 3, enemyCount = 2, iceCount = 2, hasWarp = true },
        };

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        public void SetupStage(int stageIndex)
        {
            // Clear existing (also stops coroutines)
            ClearGrid();

            int idx = Mathf.Clamp(stageIndex, 0, Stages.Length - 1);
            var cfg = Stages[idx];
            _gridSize = cfg.gridSize;
            _currentHp = _maxHp;
            _moves = 0;

            // Calculate cell size from camera
            var mainCam = Camera.main;
            if (mainCam == null) { Debug.LogError("[DashDungeon] Camera.main is null in SetupStage"); return; }
            float camSize = mainCam.orthographicSize;
            float camWidth = camSize * mainCam.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 3.0f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            _cellSize = Mathf.Min(availableHeight / _gridSize, camWidth * 2f / _gridSize, 0.9f);

            float gridH = _cellSize * _gridSize;
            float gridW = _cellSize * _gridSize;
            // Origin = bottom-left corner of grid
            _gridOrigin = new Vector3(
                -gridW / 2f + _cellSize / 2f,
                (-camSize + bottomMargin) + _cellSize / 2f,
                0f
            );

            GenerateGrid(cfg);
            PlaceTileObjects();
            PlacePlayer();
            PlaceEnemies(cfg.enemyCount);

            _minMoves = ComputeMinMoves();
            _isActive = true;
            _isMoving = false;

            _gameManager.OnHpChanged(_currentHp, _maxHp);
            _gameManager.OnMovesChanged(_moves, _minMoves);
        }

        void GenerateGrid(StageConfig cfg)
        {
            _grid = new TileType[_gridSize, _gridSize];
            _enemyPositions.Clear();

            // Initialize all floors
            for (int x = 0; x < _gridSize; x++)
                for (int y = 0; y < _gridSize; y++)
                    _grid[x, y] = TileType.Floor;

            // Place walls on border
            for (int x = 0; x < _gridSize; x++)
            {
                _grid[x, 0] = TileType.Wall;
                _grid[x, _gridSize - 1] = TileType.Wall;
            }
            for (int y = 0; y < _gridSize; y++)
            {
                _grid[0, y] = TileType.Wall;
                _grid[_gridSize - 1, y] = TileType.Wall;
            }

            // Add some internal walls for interesting paths
            var rng = new System.Random(cfg.gridSize * 137 + cfg.spikeCount * 31);
            int internalWalls = _gridSize - 3;
            int placed = 0;
            int attempts = 0;
            while (placed < internalWalls && attempts < 200)
            {
                attempts++;
                int wx = rng.Next(1, _gridSize - 1);
                int wy = rng.Next(1, _gridSize - 1);
                if (_grid[wx, wy] == TileType.Floor &&
                    !(wx == 1 && wy == 1) &&
                    !(wx == _gridSize - 2 && wy == _gridSize - 2) &&
                    !(wx == 1 && wy == _gridSize - 2))
                {
                    _grid[wx, wy] = TileType.Wall;
                    placed++;
                }
            }

            // Player start: bottom-left inner
            _playerPos = new Vector2Int(1, 1);
            _grid[_playerPos.x, _playerPos.y] = TileType.Floor;

            // Exit: top-right inner
            _exitPos = new Vector2Int(_gridSize - 2, _gridSize - 2);
            _grid[_exitPos.x, _exitPos.y] = TileType.Exit;

            // Spikes
            var usedPositions = new HashSet<string>();
            usedPositions.Add($"{_playerPos.x},{_playerPos.y}");
            usedPositions.Add($"{_exitPos.x},{_exitPos.y}");
            PlaceRandomTiles(cfg.spikeCount, TileType.Spike, usedPositions, rng);

            // Ice
            PlaceRandomTiles(cfg.iceCount, TileType.Ice, usedPositions, rng);

            // Warp
            if (cfg.hasWarp)
            {
                _warpAPos = PlaceOneTile(TileType.WarpA, usedPositions, rng);
                _warpBPos = PlaceOneTile(TileType.WarpB, usedPositions, rng);
                // If placement failed, disable warp for this run
                if (_warpAPos.x < 0 || _warpBPos.x < 0)
                {
                    _warpAPos = new Vector2Int(-1, -1);
                    _warpBPos = new Vector2Int(-1, -1);
                }
            }
        }

        void PlaceRandomTiles(int count, TileType type, HashSet<string> used, System.Random rng)
        {
            int placed = 0;
            int attempts = 0;
            while (placed < count && attempts < 200)
            {
                attempts++;
                int x = rng.Next(1, _gridSize - 1);
                int y = rng.Next(1, _gridSize - 1);
                string key = $"{x},{y}";
                if (_grid[x, y] == TileType.Floor && !used.Contains(key))
                {
                    _grid[x, y] = type;
                    used.Add(key);
                    placed++;
                }
            }
        }

        // Returns (-1,-1) sentinel on failure
        Vector2Int PlaceOneTile(TileType type, HashSet<string> used, System.Random rng)
        {
            int attempts = 0;
            while (attempts < 200)
            {
                attempts++;
                int x = rng.Next(1, _gridSize - 1);
                int y = rng.Next(1, _gridSize - 1);
                string key = $"{x},{y}";
                if (_grid[x, y] == TileType.Floor && !used.Contains(key))
                {
                    _grid[x, y] = type;
                    used.Add(key);
                    return new Vector2Int(x, y);
                }
            }
            return new Vector2Int(-1, -1); // sentinel: placement failed
        }

        void PlaceTileObjects()
        {
            _tileObjects = new GameObject[_gridSize, _gridSize];
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    var tileObj = new GameObject($"Tile_{x}_{y}");
                    tileObj.transform.SetParent(transform);
                    tileObj.transform.position = GridToWorld(x, y);
                    tileObj.transform.localScale = Vector3.one * _cellSize * 0.95f;

                    var sr = tileObj.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 0;

                    switch (_grid[x, y])
                    {
                        case TileType.Floor:  sr.sprite = _spriteFloor; break;
                        case TileType.Wall:   sr.sprite = _spriteWall; break;
                        case TileType.Spike:  sr.sprite = _spriteSpike; break;
                        case TileType.Exit:   sr.sprite = _spriteExit; break;
                        case TileType.Ice:    sr.sprite = _spriteIce; break;
                        case TileType.WarpA:  sr.sprite = _spriteWarpA; break;
                        case TileType.WarpB:  sr.sprite = _spriteWarpB; break;
                        default: sr.sprite = _spriteFloor; break;
                    }
                    _tileObjects[x, y] = tileObj;
                }
            }
        }

        void PlacePlayer()
        {
            if (_playerObj == null)
            {
                _playerObj = new GameObject("Player");
                _playerObj.transform.SetParent(transform);
                var sr = _playerObj.AddComponent<SpriteRenderer>();
                sr.sprite = _spritePlayer;
                sr.sortingOrder = 5;
            }
            _playerObj.transform.position = GridToWorld(_playerPos.x, _playerPos.y);
            _playerObj.transform.localScale = Vector3.one * _cellSize * 0.9f;
            _playerObj.SetActive(true);
        }

        void PlaceEnemies(int count)
        {
            // Destroy old enemies
            foreach (var e in _enemyObjects)
                if (e != null) Destroy(e);
            _enemyObjects.Clear();
            _enemyPositions.Clear();

            var rng = new System.Random(count * 97 + _gridSize * 13);
            var usedPositions = new HashSet<string>();
            usedPositions.Add($"{_playerPos.x},{_playerPos.y}");
            usedPositions.Add($"{_exitPos.x},{_exitPos.y}");

            for (int i = 0; i < count; i++)
            {
                int attempts = 0;
                while (attempts < 200)
                {
                    attempts++;
                    int x = rng.Next(1, _gridSize - 1);
                    int y = rng.Next(1, _gridSize - 1);
                    string key = $"{x},{y}";
                    if (_grid[x, y] == TileType.Floor && !usedPositions.Contains(key))
                    {
                        usedPositions.Add(key);
                        _enemyPositions.Add(new Vector2Int(x, y));

                        var enemyObj = new GameObject($"Enemy_{i}");
                        enemyObj.transform.SetParent(transform);
                        enemyObj.transform.position = GridToWorld(x, y);
                        enemyObj.transform.localScale = Vector3.one * _cellSize * 0.85f;
                        var sr = enemyObj.AddComponent<SpriteRenderer>();
                        sr.sprite = _spriteEnemy;
                        sr.sortingOrder = 4;
                        _enemyObjects.Add(enemyObj);
                        break;
                    }
                }
            }
        }

        void ClearGrid()
        {
            StopAllCoroutines();
            _isMoving = false;

            if (_tileObjects != null)
            {
                for (int x = 0; x < _tileObjects.GetLength(0); x++)
                    for (int y = 0; y < _tileObjects.GetLength(1); y++)
                        if (_tileObjects[x, y] != null)
                            Destroy(_tileObjects[x, y]);
            }
            foreach (var e in _enemyObjects)
                if (e != null) Destroy(e);
            _enemyObjects.Clear();
            _enemyPositions.Clear();
            if (_playerObj != null) _playerObj.SetActive(false);
        }

        // BFS to compute minimum moves from start to exit
        int ComputeMinMoves()
        {
            var queue = new Queue<(Vector2Int pos, int moves)>();
            var visited = new HashSet<string>();
            queue.Enqueue((_playerPos, 0));
            visited.Add($"{_playerPos.x},{_playerPos.y}");

            var dirs = new Vector2Int[] {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            while (queue.Count > 0)
            {
                var (pos, moves) = queue.Dequeue();
                if (pos == _exitPos) return moves;

                foreach (var dir in dirs)
                {
                    var landed = SimulateSlide(pos, dir);
                    string key = $"{landed.x},{landed.y}";
                    if (!visited.Contains(key) && _grid[landed.x, landed.y] != TileType.Wall)
                    {
                        visited.Add(key);
                        queue.Enqueue((landed, moves + 1));
                    }
                }
            }
            return 99; // unreachable (fallback)
        }

        // Simulate slide without actually moving
        Vector2Int SimulateSlide(Vector2Int startPos, Vector2Int dir)
        {
            var pos = startPos;
            int extraSlide = 0;
            bool onIce = false;

            while (true)
            {
                var next = pos + dir;
                if (next.x < 0 || next.x >= _gridSize || next.y < 0 || next.y >= _gridSize)
                    break;
                if (_grid[next.x, next.y] == TileType.Wall)
                    break;

                pos = next;

                if (onIce)
                {
                    extraSlide--;
                    if (extraSlide <= 0) onIce = false;
                }
                else if (_grid[pos.x, pos.y] == TileType.Ice)
                {
                    onIce = true;
                    extraSlide = 2;
                }
                else
                {
                    break;
                }
            }
            return pos;
        }

        public void OnDirectionInput(Vector2Int dir)
        {
            if (!_isActive || _isMoving) return;
            if (_gameManager.State != DashDungeonState.Playing) return;

            StartCoroutine(MovePlayer(dir));
        }

        IEnumerator MovePlayer(Vector2Int dir)
        {
            _isMoving = true;

            var pos = _playerPos;
            var path = new List<Vector2Int>();
            bool extraSliding = false;
            int extraLeft = 0;

            while (true)
            {
                var next = pos + dir;
                if (next.x < 0 || next.x >= _gridSize || next.y < 0 || next.y >= _gridSize)
                    break;
                if (_grid[next.x, next.y] == TileType.Wall)
                    break;

                path.Add(next);
                pos = next;

                if (extraSliding)
                {
                    extraLeft--;
                    if (extraLeft <= 0) extraSliding = false;
                    else continue; // keep sliding
                }

                if (_grid[pos.x, pos.y] == TileType.Ice)
                {
                    extraSliding = true;
                    extraLeft = 2;
                    continue;
                }
                break;
            }

            // Animate movement along path
            foreach (var step in path)
            {
                Vector3 targetPos = GridToWorld(step.x, step.y);
                float elapsed = 0f;
                float duration = 0.06f;
                Vector3 startPos3 = _playerObj.transform.position;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    _playerObj.transform.position = Vector3.Lerp(startPos3, targetPos, elapsed / duration);
                    yield return null;
                }
                _playerObj.transform.position = targetPos;
            }

            _playerPos = pos;
            _moves++;
            _gameManager.OnMovesChanged(_moves, _minMoves);

            // Land effect
            yield return StartCoroutine(HandleLanding());

            _isMoving = false;
        }

        IEnumerator HandleLanding(int warpDepth = 0)
        {
            var tile = _grid[_playerPos.x, _playerPos.y];

            // Check for enemy collision
            int enemyIdx = _enemyPositions.IndexOf(_playerPos);
            if (enemyIdx >= 0)
            {
                // Defeat enemy
                yield return StartCoroutine(PopScaleAndDestroy(_enemyObjects[enemyIdx]));
                _enemyObjects.RemoveAt(enemyIdx);
                _enemyPositions.RemoveAt(enemyIdx);
            }

            switch (tile)
            {
                case TileType.Spike:
                    yield return StartCoroutine(DamagePlayer());
                    break;

                case TileType.Exit:
                    yield return StartCoroutine(ExitReachedEffect());
                    int bonus = CalculateBonus();
                    _gameManager.OnStageClear(bonus);
                    break;

                case TileType.WarpA:
                    if (_warpBPos.x >= 0 && warpDepth < 2)
                    {
                        yield return StartCoroutine(WarpPlayer(_warpBPos));
                        yield return StartCoroutine(HandleLanding(warpDepth + 1));
                    }
                    break;

                case TileType.WarpB:
                    if (_warpAPos.x >= 0 && warpDepth < 2)
                    {
                        yield return StartCoroutine(WarpPlayer(_warpAPos));
                        yield return StartCoroutine(HandleLanding(warpDepth + 1));
                    }
                    break;
            }
        }

        IEnumerator DamagePlayer()
        {
            _currentHp--;
            _gameManager.OnHpChanged(_currentHp, _maxHp);

            // Red flash
            var sr = _playerObj.GetComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.2f, 0.2f, 1f);

            // Camera shake
            StartCoroutine(CameraShake(0.15f, 0.12f));

            yield return new WaitForSeconds(0.15f);

            // Restore color
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.15f;
                sr.color = Color.Lerp(new Color(1f, 0.2f, 0.2f), Color.white, t);
                yield return null;
            }
            sr.color = Color.white;

            if (_currentHp <= 0)
            {
                _gameManager.OnGameOver();
            }
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            var cam = Camera.main;
            Vector3 origPos = cam.transform.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                cam.transform.localPosition = origPos + new Vector3(x, y, 0f);
                yield return null;
            }
            cam.transform.localPosition = origPos;
        }

        IEnumerator ExitReachedEffect()
        {
            // Scale pop on player
            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 baseScale = Vector3.one * _cellSize * 0.9f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = t < 0.5f ? Mathf.Lerp(1f, 1.4f, t * 2f) : Mathf.Lerp(1.4f, 1f, (t - 0.5f) * 2f);
                _playerObj.transform.localScale = baseScale * scale;
                yield return null;
            }
            _playerObj.transform.localScale = baseScale;

            // Flash exit tile
            if (_tileObjects != null && _tileObjects[_exitPos.x, _exitPos.y] != null)
            {
                var exitSR = _tileObjects[_exitPos.x, _exitPos.y].GetComponent<SpriteRenderer>();
                if (exitSR != null)
                {
                    exitSR.color = new Color(1f, 1f, 0.3f);
                    yield return new WaitForSeconds(0.15f);
                    exitSR.color = Color.white;
                }
            }
        }

        IEnumerator PopScaleAndDestroy(GameObject obj)
        {
            if (obj == null) yield break;
            Vector3 baseScale = obj.transform.localScale;
            float elapsed = 0f;
            float duration = 0.2f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = t < 0.5f ? Mathf.Lerp(1f, 1.3f, t * 2f) : Mathf.Lerp(1.3f, 0f, (t - 0.5f) * 2f);
                if (obj != null) obj.transform.localScale = baseScale * scale;
                yield return null;
            }
            if (obj != null) Destroy(obj);
        }

        IEnumerator WarpPlayer(Vector2Int dest)
        {
            // Flash effect
            var sr = _playerObj.GetComponent<SpriteRenderer>();
            sr.color = new Color(0.5f, 0.5f, 1f, 0.3f);
            yield return new WaitForSeconds(0.15f);
            sr.color = Color.white;

            // Teleport
            _playerPos = dest;
            _playerObj.transform.position = GridToWorld(dest.x, dest.y);
        }

        int CalculateBonus()
        {
            int bonus = 100;
            bonus += _currentHp * 50;
            int moveDiff = _moves - _minMoves;
            bonus -= Mathf.Max(0, moveDiff) * 5;
            return Mathf.Max(bonus, 10);
        }

        Vector3 GridToWorld(int x, int y)
        {
            return _gridOrigin + new Vector3(x * _cellSize, y * _cellSize, 0f);
        }
    }
}
