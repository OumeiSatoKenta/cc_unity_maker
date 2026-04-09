using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game092v2_MirrorWorld
{
    public enum TileType
    {
        Empty = 0,
        Wall = 1,
        Goal = 2,
        Trap = 3,
        Switch = 4,
        Door = 5,
        Start = 6,
    }

    public class MirrorPuzzleManager : MonoBehaviour
    {
        [SerializeField] MirrorWorldGameManager _gameManager;
        [SerializeField] MirrorWorldUI _ui;

        [SerializeField] Sprite _sprWall;
        [SerializeField] Sprite _sprTrap;
        [SerializeField] Sprite _sprGoalTop;
        [SerializeField] Sprite _sprGoalBot;
        [SerializeField] Sprite _sprSwitch;
        [SerializeField] Sprite _sprDoor;
        [SerializeField] Sprite _sprPlayerTop;
        [SerializeField] Sprite _sprPlayerBot;

        bool _isActive;
        int _gridSize;
        int _movesLimit;
        int _movesUsed;
        int _currentStageIndex;
        int _bounceCount;

        // Player positions
        Vector2Int _topPos;
        Vector2Int _botPos;
        // Goal positions
        Vector2Int _topGoal;
        Vector2Int _botGoal;

        TileType[,] _topMap;
        TileType[,] _botMap;

        // Moving obstacles (Stage 5)
        List<MovingObstacle> _movingObstacles = new List<MovingObstacle>();

        SpriteRenderer[,] _topTileRenderers;
        SpriteRenderer[,] _botTileRenderers;
        GameObject _topPlayerObj;
        GameObject _botPlayerObj;

        float _cellSize;
        float _topZoneCenter;
        float _botZoneCenter;

        Coroutine _moveCoroutine;

        Camera _mainCamera;

        struct MovingObstacle
        {
            public bool isTop;
            public Vector2Int pos;
            public Vector2Int dir;
        }

        class GridObjects
        {
            public List<GameObject> objects = new List<GameObject>();
        }
        GridObjects _topGridObjects = new GridObjects();
        GridObjects _botGridObjects = new GridObjects();
        List<SpriteRenderer> _doorRenderers = new List<SpriteRenderer>();

        // --- Stage Data ---
        // Each stage: (gridSize, movesLimit, topStart, botStart, topGoal, botGoal, topMap, botMap)
        // topMap/botMap indexed [row, col]

        static readonly int[] GridSizes  = { 5, 5, 6, 6, 7 };
        static readonly int[] MovesLimits = { 0, 15, 12, 10, 8 }; // 0 = no limit

        // Stage 1: 5x5, simple symmetric
        static readonly TileType[,] Stage1Top = {
            { TileType.Start, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Wall  },
            { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Goal  },
        };
        static readonly TileType[,] Stage1Bot = {
            { TileType.Goal,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Start },
            { TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall  },
            { TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Empty },
            { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Empty },
        };

        // Stage 2: 5x5, asymmetric walls, moves limit
        static readonly TileType[,] Stage2Top = {
            { TileType.Start, TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Goal  },
        };
        static readonly TileType[,] Stage2Bot = {
            { TileType.Goal,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Start },
            { TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall  },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Empty },
        };

        // Stage 3: 6x6, traps
        static readonly TileType[,] Stage3Top = {
            { TileType.Start, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Wall,  TileType.Trap,  TileType.Empty, TileType.Wall,  TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Trap,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Trap,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Trap,  TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Goal  },
        };
        static readonly TileType[,] Stage3Bot = {
            { TileType.Goal,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Start },
            { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Trap,  TileType.Wall,  TileType.Empty },
            { TileType.Empty, TileType.Trap,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Trap,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Wall,  TileType.Trap,  TileType.Wall,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
        };

        // Stage 4: 6x6, switch & door
        static readonly TileType[,] Stage4Top = {
            { TileType.Start, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Door,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Trap,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Switch,TileType.Empty, TileType.Wall,  TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Goal  },
        };
        static readonly TileType[,] Stage4Bot = {
            { TileType.Goal,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Start },
            { TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty },
            { TileType.Empty, TileType.Door,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Trap,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Switch,TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
        };

        // Stage 5: 7x7, moving obstacles
        static readonly TileType[,] Stage5Top = {
            { TileType.Start, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Trap,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Door,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Trap,  TileType.Empty, TileType.Switch,TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Trap,  TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Goal  },
        };
        static readonly TileType[,] Stage5Bot = {
            { TileType.Goal,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Start },
            { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Trap,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Door,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Switch,TileType.Empty, TileType.Trap,  TileType.Empty, TileType.Empty },
            { TileType.Empty, TileType.Trap,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty },
            { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
        };

        void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _gridSize = GridSizes[stageIndex];
            _movesLimit = MovesLimits[stageIndex];
            _movesUsed = 0;
            _bounceCount = 0;
            _movingObstacles.Clear();
            _doorRenderers.Clear();

            // Clear previous objects
            foreach (var go in _topGridObjects.objects) if (go != null) Destroy(go);
            foreach (var go in _botGridObjects.objects) if (go != null) Destroy(go);
            _topGridObjects.objects.Clear();
            _botGridObjects.objects.Clear();
            if (_topPlayerObj != null) Destroy(_topPlayerObj);
            if (_botPlayerObj != null) Destroy(_botPlayerObj);

            // Load map data
            _topMap = GetTopMap(stageIndex);
            _botMap = GetBotMap(stageIndex);

            // Calculate responsive positions
            float camSize = _mainCamera.orthographicSize;
            float camWidth = camSize * _mainCamera.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 2.8f;
            float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
            float zoneHeight = availableHeight / 2f;
            float maxCell = Mathf.Min(camWidth * 2f / _gridSize, zoneHeight / _gridSize, 0.85f);
            _cellSize = maxCell;

            _topZoneCenter = camSize - topMargin - zoneHeight / 2f;
            _botZoneCenter = -(camSize - bottomMargin - zoneHeight / 2f);

            // Find start positions
            _topPos = FindTile(_topMap, TileType.Start);
            _botPos = FindTile(_botMap, TileType.Start);
            _topGoal = FindTile(_topMap, TileType.Goal);
            _botGoal = FindTile(_botMap, TileType.Goal);

            // Build tile visuals
            _topTileRenderers = new SpriteRenderer[_gridSize, _gridSize];
            _botTileRenderers = new SpriteRenderer[_gridSize, _gridSize];

            BuildGridVisuals(true, _topMap, _topTileRenderers, _topGridObjects);
            BuildGridVisuals(false, _botMap, _botTileRenderers, _botGridObjects);

            // Create player objects
            _topPlayerObj = CreateSpriteObj("TopPlayer", _sprPlayerTop, TopCellToWorld(_topPos.x, _topPos.y), 5);
            _botPlayerObj = CreateSpriteObj("BotPlayer", _sprPlayerBot, BotCellToWorld(_botPos.x, _botPos.y), 5);

            // Stage 5: setup moving obstacle
            if (stageIndex == 4)
            {
                _movingObstacles.Add(new MovingObstacle { isTop = true, pos = new Vector2Int(2, 5), dir = new Vector2Int(0, -1) });
                _movingObstacles.Add(new MovingObstacle { isTop = false, pos = new Vector2Int(2, 1), dir = new Vector2Int(0, 1) });
            }

            _ui.UpdateMoves(_movesUsed, _movesLimit);
            _isActive = true;
        }

        void BuildGridVisuals(bool isTop, TileType[,] map, SpriteRenderer[,] renderers, GridObjects container)
        {
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    TileType t = map[r, c];
                    Sprite spr = null;
                    int order = 1;
                    switch (t)
                    {
                        case TileType.Wall: spr = _sprWall; break;
                        case TileType.Trap: spr = _sprTrap; break;
                        case TileType.Goal: spr = isTop ? _sprGoalTop : _sprGoalBot; break;
                        case TileType.Switch: spr = _sprSwitch; break;
                        case TileType.Door: spr = _sprDoor; break;
                        default: break;
                    }

                    if (spr != null)
                    {
                        Vector3 pos = isTop ? TopCellToWorld(r, c) : BotCellToWorld(r, c);
                        var go = CreateSpriteObj($"Tile_{(isTop?"T":"B")}_{r}_{c}", spr, pos, order);
                        float sc = _cellSize * 0.9f;
                        go.transform.localScale = Vector3.one * sc;
                        var sr = go.GetComponent<SpriteRenderer>();
                        renderers[r, c] = sr;
                        container.objects.Add(go);

                        if (t == TileType.Door)
                            _doorRenderers.Add(sr);
                    }
                }
            }
        }

        Vector2Int FindTile(TileType[,] map, TileType target)
        {
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    if (map[r, c] == target) return new Vector2Int(r, c);
            return Vector2Int.zero;
        }

        GameObject CreateSpriteObj(string name, Sprite sprite, Vector3 pos, int order)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            float sc = _cellSize * 0.9f;
            go.transform.localScale = Vector3.one * sc;
            return go;
        }

        Vector3 TopCellToWorld(int r, int c)
        {
            float startX = -(_gridSize * _cellSize) / 2f + _cellSize / 2f;
            float startY = (_gridSize * _cellSize) / 2f - _cellSize / 2f;
            return new Vector3(startX + c * _cellSize, _topZoneCenter + startY - r * _cellSize, 0f);
        }

        Vector3 BotCellToWorld(int r, int c)
        {
            float startX = -(_gridSize * _cellSize) / 2f + _cellSize / 2f;
            float startY = (_gridSize * _cellSize) / 2f - _cellSize / 2f;
            return new Vector3(startX + c * _cellSize, _botZoneCenter + startY - r * _cellSize, 0f);
        }

        void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            // Detect swipe direction by comparing press position to player position
            Vector3 mouseScreen = Mouse.current.position.ReadValue();
            mouseScreen.z = -_mainCamera.transform.position.z;
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(mouseScreen);

            // Use top player position as reference
            Vector2 playerWorld = TopCellToWorld(_topPos.x, _topPos.y);
            Vector2 diff = worldPos - playerWorld;

            if (diff.magnitude < _cellSize * 0.3f) return;

            int dr = 0, dc = 0;
            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
                dc = diff.x > 0 ? 1 : -1;
            else
                dr = diff.y > 0 ? -1 : 1;

            TryMove(dr, dc);
        }

        void TryMove(int dr, int dc)
        {
            // Calculate new positions
            int newTopR = _topPos.x + dr;
            int newTopC = _topPos.y + dc;
            int newBotR = _botPos.x - dr; // mirrored vertically
            int newBotC = _botPos.y + dc;

            bool topBounced = false;
            bool botBounced = false;

            // Clamp to grid
            if (newTopR < 0 || newTopR >= _gridSize) { newTopR = _topPos.x; topBounced = true; }
            if (newTopC < 0 || newTopC >= _gridSize) { newTopC = _topPos.y; topBounced = true; }
            if (newBotR < 0 || newBotR >= _gridSize) { newBotR = _botPos.x; botBounced = true; }
            if (newBotC < 0 || newBotC >= _gridSize) { newBotC = _botPos.y; botBounced = true; }

            // Check walls
            if (IsWall(_topMap, newTopR, newTopC)) { newTopR = _topPos.x; newTopC = _topPos.y; topBounced = true; }
            if (IsWall(_botMap, newBotR, newBotC)) { newBotR = _botPos.x; newBotC = _botPos.y; botBounced = true; }

            if (topBounced || botBounced) _bounceCount++;

            _topPos = new Vector2Int(newTopR, newTopC);
            _botPos = new Vector2Int(newBotR, newBotC);

            // Update visuals
            _topPlayerObj.transform.position = TopCellToWorld(_topPos.x, _topPos.y);
            _botPlayerObj.transform.position = BotCellToWorld(_botPos.x, _botPos.y);

            // Bounce animation
            if (topBounced) StartCoroutine(BounceAnim(_topPlayerObj.transform));
            if (botBounced) StartCoroutine(BounceAnim(_botPlayerObj.transform));

            // Count move
            _movesUsed++;
            _ui.UpdateMoves(_movesUsed, _movesLimit);

            // Check switch
            CheckSwitch(_topMap, _topPos, true);
            CheckSwitch(_botMap, _botPos, false);

            // Stage 5: advance moving obstacles
            if (_currentStageIndex == 4)
            {
                if (AdvanceMovingObstacles()) return; // trap hit by moving obstacle
            }

            // Check trap
            if (_topMap[_topPos.x, _topPos.y] == TileType.Trap || _botMap[_botPos.x, _botPos.y] == TileType.Trap)
            {
                StartCoroutine(TrapFlash());
                return;
            }

            // Check moves exceeded
            if (_movesLimit > 0 && _movesUsed >= _movesLimit)
            {
                // Check goals first
                bool topGoal = (_topPos == _topGoal);
                bool botGoal = (_botPos == _botGoal);
                if (topGoal && botGoal)
                {
                    StartCoroutine(SuccessAnim());
                    return;
                }
                _gameManager.OnMovesExceeded();
                return;
            }

            // Check goals
            if (_topPos == _topGoal && _botPos == _botGoal)
            {
                StartCoroutine(SuccessAnim());
            }
        }

        bool IsWall(TileType[,] map, int r, int c)
        {
            if (r < 0 || r >= _gridSize || c < 0 || c >= _gridSize) return true;
            var t = map[r, c];
            return t == TileType.Wall || t == TileType.Door;
        }

        void CheckSwitch(TileType[,] map, Vector2Int pos, bool isTop)
        {
            if (map[pos.x, pos.y] == TileType.Switch)
            {
                // Open corresponding door
                OpenDoors(isTop);
                // Mark switch as used
                map[pos.x, pos.y] = TileType.Empty;
            }
        }

        void OpenDoors(bool triggeredByTop)
        {
            // Open doors in the opposite map (top switch opens bot door, vice versa)
            TileType[,] targetMap = triggeredByTop ? _botMap : _topMap;
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    if (targetMap[r, c] == TileType.Door)
                        targetMap[r, c] = TileType.Empty;

            // Hide door visuals
            foreach (var sr in _doorRenderers)
                if (sr != null)
                    StartCoroutine(FadeOutDoor(sr));
        }

        IEnumerator FadeOutDoor(SpriteRenderer sr)
        {
            float t = 0f;
            Color c = sr.color;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                sr.color = new Color(c.r, c.g, c.b, 1f - t / 0.3f);
                yield return null;
            }
            sr.gameObject.SetActive(false);
        }

        // Returns true if a moving obstacle hit the player (caller should handle trap)
        bool AdvanceMovingObstacles()
        {
            for (int i = 0; i < _movingObstacles.Count; i++)
            {
                var obs = _movingObstacles[i];
                Vector2Int newPos = obs.pos + obs.dir;
                var map = obs.isTop ? _topMap : _botMap;
                if (newPos.x < 0 || newPos.x >= _gridSize || newPos.y < 0 || newPos.y >= _gridSize
                    || map[newPos.x, newPos.y] == TileType.Wall)
                {
                    obs.dir = -obs.dir;
                    newPos = obs.pos + obs.dir;
                    // If still blocked after reversal, stay in place
                    if (newPos.x < 0 || newPos.x >= _gridSize || newPos.y < 0 || newPos.y >= _gridSize
                        || map[newPos.x, newPos.y] == TileType.Wall)
                    {
                        newPos = obs.pos;
                    }
                }
                // Check if player is at new obstacle position
                Vector2Int playerPos = obs.isTop ? _topPos : _botPos;
                if (newPos == playerPos)
                {
                    StartCoroutine(TrapFlash());
                    return true;
                }
                obs.pos = newPos;
                _movingObstacles[i] = obs;
            }
            return false;
        }

        IEnumerator SuccessAnim()
        {
            _isActive = false;
            // Pop animation on both players
            float t = 0f;
            Vector3 baseScale = Vector3.one * _cellSize * 0.9f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.3f;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(1f, 1.4f, ratio * 2f)
                    : Mathf.Lerp(1.4f, 1.0f, (ratio - 0.5f) * 2f);
                if (_topPlayerObj != null) _topPlayerObj.transform.localScale = baseScale * scale;
                if (_botPlayerObj != null) _botPlayerObj.transform.localScale = baseScale * scale;

                // Yellow flash
                float flash = Mathf.Sin(ratio * Mathf.PI);
                if (_topPlayerObj != null) _topPlayerObj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.yellow, flash);
                if (_botPlayerObj != null) _botPlayerObj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.yellow, flash);
                yield return null;
            }
            if (_topPlayerObj != null) _topPlayerObj.transform.localScale = baseScale;
            if (_botPlayerObj != null) _botPlayerObj.transform.localScale = baseScale;
            _gameManager.OnBothReachedGoal(_movesUsed, _movesLimit, _bounceCount);
        }

        IEnumerator TrapFlash()
        {
            _isActive = false;
            // Red flash + camera shake
            float t = 0f;
            Vector3 camBase = _mainCamera.transform.position;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.4f;
                float flash = Mathf.Sin(ratio * Mathf.PI * 3f);
                if (_topPlayerObj != null) _topPlayerObj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.red, Mathf.Abs(flash));
                if (_botPlayerObj != null) _botPlayerObj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.red, Mathf.Abs(flash));
                // Shake
                float shake = (1f - ratio) * 0.15f;
                _mainCamera.transform.position = camBase + new Vector3(Random.Range(-shake, shake), Random.Range(-shake, shake), 0f);
                yield return null;
            }
            _mainCamera.transform.position = camBase;
            _gameManager.OnTrapHit();
        }

        IEnumerator BounceAnim(Transform t)
        {
            if (t == null) yield break;
            float elapsed = 0f;
            Vector3 baseScale = t.localScale;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                if (t == null) yield break;
                float ratio = elapsed / 0.15f;
                float sc = ratio < 0.5f ? Mathf.Lerp(1f, 0.85f, ratio * 2f) : Mathf.Lerp(0.85f, 1f, (ratio - 0.5f) * 2f);
                t.localScale = baseScale * sc;
                yield return null;
            }
            if (t != null) t.localScale = baseScale;
        }

        TileType[,] GetTopMap(int stageIndex)
        {
            switch (stageIndex)
            {
                case 0: return CopyMap(Stage1Top);
                case 1: return CopyMap(Stage2Top);
                case 2: return CopyMap(Stage3Top);
                case 3: return CopyMap(Stage4Top);
                case 4: return CopyMap(Stage5Top);
                default: return CopyMap(Stage1Top);
            }
        }

        TileType[,] GetBotMap(int stageIndex)
        {
            switch (stageIndex)
            {
                case 0: return CopyMap(Stage1Bot);
                case 1: return CopyMap(Stage2Bot);
                case 2: return CopyMap(Stage3Bot);
                case 3: return CopyMap(Stage4Bot);
                case 4: return CopyMap(Stage5Bot);
                default: return CopyMap(Stage1Bot);
            }
        }

        TileType[,] CopyMap(TileType[,] src)
        {
            int rows = src.GetLength(0);
            int cols = src.GetLength(1);
            var dst = new TileType[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    dst[r, c] = src[r, c];
            return dst;
        }
    }
}
