using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game095v2_SilentWorld
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] SilentWorldGameManager _gameManager;
        [SerializeField] Sprite _floorSprite;
        [SerializeField] Sprite _wallSprite;
        [SerializeField] Sprite _trapSprite;
        [SerializeField] Sprite _exitSprite;
        [SerializeField] Sprite _itemSprite;
        [SerializeField] Sprite _characterSprite;
        [SerializeField] Sprite _hintGlowSprite;

        enum CellType { Floor, Wall, Trap, Exit, Item, HiddenPassage }

        struct Cell
        {
            public CellType type;
            public SpriteRenderer sr;
            public GameObject go;
            public Vector2Int gridPos;
            public bool isGateLinked;   // true if this is a gated item
            public int gateId;          // links item to hidden passage
            public bool isFakeHint;     // stage 5 only
        }

        int _gridCols;
        int _gridRows;
        float _cellSize;
        Vector3 _gridOrigin;

        Cell[,] _cells;
        GameObject _characterGo;
        SpriteRenderer _characterSr;
        Vector2Int _charPos;

        List<GameObject> _hintGlows = new List<GameObject>();
        List<GameObject> _itemObjects = new List<GameObject>();
        List<SpriteRenderer> _trapSrs = new List<SpriteRenderer>();

        bool _isActive;
        StageManager.StageConfig _currentConfig;
        int _currentStageIndex;
        bool _hasDarkArea;
        bool _hasFakeHints;
        bool _hasTimeLimit;
        float _timeRemaining;
        bool _timerRunning;
        int _totalItems;
        int _collectedItems;
        int _requiredItems;

        // Input state for long press
        bool _wasPressed;
        float _pressStartTime;
        const float LongPressThreshold = 0.5f;
        bool _longPressHandled;

        // Hidden passage map: gateId -> list of cell positions
        Dictionary<int, List<Vector2Int>> _gatePassages = new Dictionary<int, List<Vector2Int>>();

        // Trap damage cooldown
        float _trapCooldown;
        const float TrapCooldownDuration = 1.5f;

        Camera _mainCamera;

        public bool IsActive => _isActive;
        public float GetTimeRemaining() => _timeRemaining;

        void Awake()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) Debug.LogError("[WorldManager] Main camera not found.");
        }

        public void SetActive(bool value)
        {
            _isActive = value;
            if (!value) StopAllCoroutines();
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            _currentConfig = config;
            _currentStageIndex = stageIndex;

            ClearGrid();

            // Determine grid size from countMultiplier
            switch (config.countMultiplier)
            {
                case 1: _gridCols = 5; _gridRows = 7; break;
                case 2: _gridCols = 6; _gridRows = 8; break;
                case 3: _gridCols = 7; _gridRows = 9; break;
                default: _gridCols = 8; _gridRows = 10; break;
            }

            int trapCount = stageIndex; // stage 0=0, 1=2, 2=3, 3=4, 4=5
            if (stageIndex == 1) trapCount = 2;

            _hasDarkArea = config.complexityFactor >= 0.5f;
            _hasFakeHints = config.complexityFactor >= 0.9f;
            _hasTimeLimit = config.speedMultiplier >= 2.0f;
            _timeRemaining = 60f;
            _timerRunning = _hasTimeLimit;

            CalculateCellSize();
            BuildGrid(trapCount, stageIndex);
            PlaceCharacter();

            _isActive = true;
            _trapCooldown = 0f;
            _longPressHandled = false;
            _wasPressed = false;
        }

        void CalculateCellSize()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            float camSize = _mainCamera.orthographicSize;
            float camWidth = camSize * _mainCamera.aspect;
            float topMargin = 1.3f;
            float bottomMargin = 2.8f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            float availableWidth = camWidth * 2f - 0.4f;
            float csH = availableHeight / _gridRows;
            float csW = availableWidth / _gridCols;
            _cellSize = Mathf.Min(csH, csW, 1.2f);

            // Grid origin (bottom-left corner)
            float gridWidth = _cellSize * _gridCols;
            float gridHeight = _cellSize * _gridRows;
            float centerY = -bottomMargin / 2f + topMargin / 2f;
            _gridOrigin = new Vector3(-gridWidth / 2f + _cellSize / 2f, centerY - gridHeight / 2f + _cellSize / 2f, 0f);
        }

        void BuildGrid(int trapCount, int stageIndex)
        {
            _cells = new Cell[_gridCols, _gridRows];
            _gatePassages.Clear();
            _itemObjects.Clear();
            _trapSrs.Clear();

            var rng = new System.Random(stageIndex * 1000 + _gridCols);

            // Initialize all cells as Floor
            for (int x = 0; x < _gridCols; x++)
            {
                for (int y = 0; y < _gridRows; y++)
                {
                    _cells[x, y] = CreateCell(x, y, CellType.Floor);
                }
            }

            // Place walls (border + some inner walls)
            for (int x = 0; x < _gridCols; x++)
            {
                SetCellType(x, 0, CellType.Wall);
                SetCellType(x, _gridRows - 1, CellType.Wall);
            }
            for (int y = 0; y < _gridRows; y++)
            {
                SetCellType(0, y, CellType.Wall);
                SetCellType(_gridCols - 1, y, CellType.Wall);
            }

            // Inner walls (generate a simple maze feel)
            int innerWallCount = Mathf.RoundToInt(_gridCols * _gridRows * 0.1f);
            for (int i = 0; i < innerWallCount; i++)
            {
                int wx = rng.Next(1, _gridCols - 1);
                int wy = rng.Next(2, _gridRows - 2);
                if (!IsEdge(wx, wy)) SetCellType(wx, wy, CellType.Wall);
            }

            // Place exit (top-center area)
            int ex = _gridCols / 2;
            int ey = _gridRows - 2;
            SetCellType(ex, ey, CellType.Exit);

            // Place items
            _totalItems = Mathf.Max(2, stageIndex + 2);
            _collectedItems = 0;
            _requiredItems = stageIndex >= 2 ? 1 : 0; // stage 3+ has gate items

            int gateLinkedItemCount = (stageIndex >= 2) ? 1 : 0;

            for (int i = 0; i < _totalItems; i++)
            {
                Vector2Int ipos = GetRandomFloorPos(rng, ex, ey);
                bool isGate = (i == 0 && gateLinkedItemCount > 0);
                SetCellType(ipos.x, ipos.y, CellType.Item);
                _cells[ipos.x, ipos.y].isGateLinked = isGate;
                _cells[ipos.x, ipos.y].gateId = isGate ? 0 : -1;
                var itemGo = _cells[ipos.x, ipos.y].go;
                if (itemGo != null) _itemObjects.Add(itemGo);

                if (isGate)
                {
                    // Place corresponding hidden passage
                    Vector2Int hpos = GetRandomFloorPos(rng, ex, ey);
                    SetCellType(hpos.x, hpos.y, CellType.HiddenPassage);
                    if (!_gatePassages.ContainsKey(0)) _gatePassages[0] = new List<Vector2Int>();
                    _gatePassages[0].Add(hpos);
                }
            }

            // Place traps
            for (int i = 0; i < trapCount; i++)
            {
                Vector2Int tpos = GetRandomFloorPos(rng, ex, ey);
                SetCellType(tpos.x, tpos.y, CellType.Trap);
                bool isFake = _hasFakeHints && (i % 2 == 1);
                _cells[tpos.x, tpos.y].isFakeHint = isFake;
                if (_cells[tpos.x, tpos.y].sr != null)
                    _trapSrs.Add(_cells[tpos.x, tpos.y].sr);
            }

            // Apply dark area (stage 4+): make a region of cells have reduced alpha hints
            // (actual rendering logic is in hint system)
        }

        Cell CreateCell(int x, int y, CellType type)
        {
            var go = new GameObject($"Cell_{x}_{y}");
            go.transform.position = GridToWorld(x, y);
            go.transform.localScale = Vector3.one * _cellSize * 0.95f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSpriteForType(type);
            sr.sortingOrder = 0;

            var cell = new Cell { type = type, sr = sr, go = go, gridPos = new Vector2Int(x, y) };
            return cell;
        }

        void SetCellType(int x, int y, CellType type)
        {
            if (_cells[x, y].go != null)
            {
                _cells[x, y].type = type;
                _cells[x, y].sr.sprite = GetSpriteForType(type);
                if (type == CellType.HiddenPassage)
                {
                    // Hidden passage looks like wall until activated
                    _cells[x, y].sr.sprite = _wallSprite;
                }
            }
        }

        Sprite GetSpriteForType(CellType type)
        {
            switch (type)
            {
                case CellType.Wall: return _wallSprite;
                case CellType.Trap: return _trapSprite;
                case CellType.Exit: return _exitSprite;
                case CellType.Item: return _itemSprite;
                default: return _floorSprite;
            }
        }

        Vector3 GridToWorld(int x, int y) =>
            _gridOrigin + new Vector3(x * _cellSize, y * _cellSize, 0f);

        Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int gx = Mathf.RoundToInt((worldPos.x - _gridOrigin.x) / _cellSize);
            int gy = Mathf.RoundToInt((worldPos.y - _gridOrigin.y) / _cellSize);
            return new Vector2Int(gx, gy);
        }

        bool IsInGrid(int x, int y) => x >= 0 && x < _gridCols && y >= 0 && y < _gridRows;
        bool IsEdge(int x, int y) => x == 0 || y == 0 || x == _gridCols - 1 || y == _gridRows - 1;

        Vector2Int GetRandomFloorPos(System.Random rng, int exX, int exY)
        {
            for (int attempt = 0; attempt < 200; attempt++)
            {
                int rx = rng.Next(1, _gridCols - 1);
                int ry = rng.Next(1, _gridRows - 1);
                if (_cells[rx, ry].type == CellType.Floor && !(rx == 1 && ry == 1) && !(rx == exX && ry == exY))
                    return new Vector2Int(rx, ry);
            }
            return new Vector2Int(2, 2);
        }

        void PlaceCharacter()
        {
            _charPos = new Vector2Int(1, 1);

            if (_characterGo == null)
            {
                _characterGo = new GameObject("Character");
                _characterSr = _characterGo.AddComponent<SpriteRenderer>();
                _characterSr.sprite = _characterSprite;
                _characterSr.sortingOrder = 5;
            }
            _characterGo.transform.position = GridToWorld(_charPos.x, _charPos.y);
            _characterGo.transform.localScale = Vector3.one * _cellSize * 0.85f;
        }

        void ClearGrid()
        {
            if (_cells != null)
            {
                int cols = _cells.GetLength(0);
                int rows = _cells.GetLength(1);
                for (int x = 0; x < cols; x++)
                    for (int y = 0; y < rows; y++)
                        if (_cells[x, y].go != null) Destroy(_cells[x, y].go);
                _cells = null;
            }
            ClearHintGlows();
            _itemObjects.Clear();
            _trapSrs.Clear();
        }

        void ClearHintGlows()
        {
            foreach (var g in _hintGlows) if (g != null) Destroy(g);
            _hintGlows.Clear();
        }

        void Update()
        {
            if (!_isActive || _gameManager == null || !_gameManager.IsPlaying) return;

            _trapCooldown -= Time.deltaTime;

            if (_timerRunning)
            {
                _timeRemaining -= Time.deltaTime;
                if (_timeRemaining <= 0f)
                {
                    _timeRemaining = 0f;
                    _timerRunning = false;
                    _isActive = false;
                    _gameManager.OnTrapHit(); // treat timeout as losing a life
                    return;
                }
            }

            HandleInput();
        }

        void HandleInput()
        {
            if (Mouse.current == null) return;

            bool pressed = Mouse.current.leftButton.isPressed;
            bool wasPressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;

            if (wasPressedThisFrame)
            {
                _pressStartTime = Time.time;
                _longPressHandled = false;
                _wasPressed = true;
            }

            // Long press detection
            if (pressed && _wasPressed && !_longPressHandled)
            {
                float held = Time.time - _pressStartTime;
                if (held >= LongPressThreshold)
                {
                    _longPressHandled = true;
                    Vector2 mousePos2D = Mouse.current.position.ReadValue();
                    Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(mousePos2D.x, mousePos2D.y, 0f));
                    worldPos.z = 0f;
                    HandleObserve(worldPos);
                }
            }

            // Tap (release without long press)
            if (!pressed && _wasPressed && !_longPressHandled)
            {
                _wasPressed = false;
                Vector2 mousePos2D = Mouse.current.position.ReadValue();
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(mousePos2D.x, mousePos2D.y, 0f));
                worldPos.z = 0f;
                HandleTap(worldPos);
            }

            if (!pressed) _wasPressed = false;
        }

        void HandleTap(Vector3 worldPos)
        {
            Vector2Int tapGrid = WorldToGrid(worldPos);
            if (!IsInGrid(tapGrid.x, tapGrid.y)) return;

            // Only allow movement to adjacent cells (Manhattan distance = 1)
            int dist = Mathf.Abs(tapGrid.x - _charPos.x) + Mathf.Abs(tapGrid.y - _charPos.y);
            if (dist != 1) return;

            CellType targetType = _cells[tapGrid.x, tapGrid.y].type;

            if (targetType == CellType.Wall) return; // Can't move into walls

            MoveCharacter(tapGrid);
        }

        void MoveCharacter(Vector2Int targetPos)
        {
            _charPos = targetPos;
            _characterGo.transform.position = GridToWorld(_charPos.x, _charPos.y);

            // Pop animation
            StartCoroutine(PopCharacter());

            // Check cell interaction
            CellType cellType = _cells[_charPos.x, _charPos.y].type;

            switch (cellType)
            {
                case CellType.Item:
                    CollectItem(_charPos);
                    break;
                case CellType.Trap:
                    if (_trapCooldown <= 0f)
                        StartCoroutine(TrapHitEffect());
                    break;
                case CellType.Exit:
                    if (_collectedItems >= _requiredItems)
                        TriggerStageClear();
                    break;
            }
        }

        void CollectItem(Vector2Int pos)
        {
            var cell = _cells[pos.x, pos.y];
            bool isGate = cell.isGateLinked;
            int gateId = cell.gateId;

            // Remove item visual
            if (cell.go != null)
            {
                StartCoroutine(ItemCollectEffect(cell.go, cell.sr));
            }

            // Replace cell with floor
            _cells[pos.x, pos.y].type = CellType.Floor;
            _cells[pos.x, pos.y].sr.sprite = _floorSprite;
            _cells[pos.x, pos.y].isGateLinked = false;
            _collectedItems++;

            _gameManager.OnItemCollected(isGate);

            // Unlock gate passage if applicable
            if (isGate && _gatePassages.ContainsKey(gateId))
            {
                foreach (var hpos in _gatePassages[gateId])
                {
                    if (IsInGrid(hpos.x, hpos.y))
                    {
                        _cells[hpos.x, hpos.y].type = CellType.Floor;
                        _cells[hpos.x, hpos.y].sr.sprite = _floorSprite;
                        StartCoroutine(PassageOpenEffect(_cells[hpos.x, hpos.y].go));
                    }
                }
                _gatePassages.Remove(gateId);
            }
        }

        void TriggerStageClear()
        {
            _isActive = false;
            bool noHints = _gameManager.GetHintsUsed() == 0;
            bool noDamage = _gameManager.GetLives() == 3;
            StartCoroutine(ExitEffect(() => _gameManager.OnStageClear(noHints, noDamage)));
        }

        void HandleObserve(Vector3 worldPos)
        {
            // Check hint availability
            if (!_gameManager.TryUseHint(_currentConfig)) return;

            Vector2Int centerGrid = WorldToGrid(worldPos);
            ClearHintGlows();

            int radius = 3;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int gx = centerGrid.x + dx;
                    int gy = centerGrid.y + dy;
                    if (!IsInGrid(gx, gy)) continue;

                    CellType t = _cells[gx, gy].type;
                    bool isTrap = (t == CellType.Trap);
                    bool isFake = _cells[gx, gy].isFakeHint;

                    // Dark area: only show hints if not in dark zone or stage < 4
                    bool darkArea = _hasDarkArea && (gx > _gridCols * 0.5f);
                    if (darkArea && !IsInRadius(centerGrid, new Vector2Int(gx, gy), 1)) continue;

                    // Show hint glow: blue-white for safe, faint red for danger
                    Color hintColor = isTrap ? new Color(1f, 0.3f, 0.2f, 0.7f) : new Color(0.5f, 0.8f, 1f, 0.5f);
                    if (_hasFakeHints && isFake) hintColor = new Color(0.5f, 0.8f, 1f, 0.5f); // fake shows as safe

                    var glowGo = new GameObject($"HintGlow_{gx}_{gy}");
                    glowGo.transform.position = GridToWorld(gx, gy);
                    glowGo.transform.localScale = Vector3.one * _cellSize;
                    var glowSr = glowGo.AddComponent<SpriteRenderer>();
                    glowSr.sprite = _hintGlowSprite;
                    glowSr.color = hintColor;
                    glowSr.sortingOrder = 3;
                    _hintGlows.Add(glowGo);
                }
            }

            StartCoroutine(FadeHintGlows(0.8f));
        }

        bool IsInRadius(Vector2Int center, Vector2Int pos, int r) =>
            Mathf.Abs(pos.x - center.x) <= r && Mathf.Abs(pos.y - center.y) <= r;

        // ---- Coroutines (Visual Feedback) ----

        IEnumerator PopCharacter()
        {
            float t = 0f;
            Vector3 baseScale = Vector3.one * _cellSize * 0.85f;
            while (t < 0.2f)
            {
                float s = 1f + 0.3f * Mathf.Sin(Mathf.PI * t / 0.2f);
                _characterGo.transform.localScale = baseScale * s;
                t += Time.deltaTime;
                yield return null;
            }
            _characterGo.transform.localScale = baseScale;
        }

        IEnumerator TrapHitEffect()
        {
            _trapCooldown = TrapCooldownDuration;
            // Red flash on character
            Color orig = _characterSr.color;
            float t = 0f;
            while (t < 0.4f)
            {
                float lerp = Mathf.PingPong(t * 5f, 1f);
                _characterSr.color = Color.Lerp(Color.red, Color.white, lerp);
                t += Time.deltaTime;
                yield return null;
            }
            _characterSr.color = orig;

            // Camera shake
            StartCoroutine(CameraShake(0.2f, 0.08f));

            if (_isActive && _gameManager != null && _gameManager.IsPlaying)
                _gameManager.OnTrapHit();
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            var cam = _mainCamera != null ? _mainCamera : Camera.main;
            Vector3 origin = cam.transform.position;
            float t = 0f;
            while (t < duration)
            {
                cam.transform.position = origin + (Vector3)Random.insideUnitCircle * magnitude;
                t += Time.deltaTime;
                yield return null;
            }
            cam.transform.position = origin;
        }

        IEnumerator ItemCollectEffect(GameObject itemGo, SpriteRenderer sr)
        {
            float t = 0f;
            Vector3 baseScale = itemGo.transform.localScale;
            while (t < 0.3f)
            {
                float s = 1f + 0.4f * Mathf.Sin(Mathf.PI * t / 0.15f);
                itemGo.transform.localScale = baseScale * s;
                float a = 1f - (t / 0.3f);
                if (sr != null) sr.color = new Color(1f, 1f, 1f, a);
                t += Time.deltaTime;
                yield return null;
            }
            // Object will be reused as floor cell, so just reset scale/alpha
            itemGo.transform.localScale = baseScale;
            if (sr != null) sr.color = Color.white;
        }

        IEnumerator PassageOpenEffect(GameObject passageGo)
        {
            if (passageGo == null) yield break;
            var sr = passageGo.GetComponent<SpriteRenderer>();
            Vector3 baseScale = passageGo.transform.localScale;
            float t = 0f;
            while (t < 0.5f)
            {
                float s = 1f + 0.3f * Mathf.Sin(Mathf.PI * t / 0.5f);
                passageGo.transform.localScale = baseScale * s;
                if (sr != null)
                {
                    float lerp = t / 0.5f;
                    sr.color = Color.Lerp(new Color(0.3f, 0.3f, 0.3f), Color.white, lerp);
                }
                t += Time.deltaTime;
                yield return null;
            }
            passageGo.transform.localScale = baseScale;
            if (sr != null) sr.color = Color.white;
        }

        IEnumerator ExitEffect(System.Action onComplete)
        {
            var exitCell = GetExitCell();
            if (exitCell.go != null)
            {
                Vector3 baseScale = exitCell.go.transform.localScale;
                float t = 0f;
                while (t < 0.5f)
                {
                    float s = 1f + 0.3f * Mathf.Sin(Mathf.PI * t / 0.5f);
                    exitCell.go.transform.localScale = baseScale * s;
                    if (exitCell.sr != null)
                        exitCell.sr.color = Color.Lerp(new Color(0.5f, 0.2f, 1f), Color.yellow, t / 0.5f);
                    t += Time.deltaTime;
                    yield return null;
                }
            }
            yield return new WaitForSeconds(0.3f);
            onComplete?.Invoke();
        }

        IEnumerator FadeHintGlows(float duration)
        {
            var srs = new List<SpriteRenderer>();
            foreach (var g in _hintGlows) if (g != null) srs.Add(g.GetComponent<SpriteRenderer>());

            var startColors = new List<Color>();
            foreach (var sr in srs) startColors.Add(sr != null ? sr.color : Color.clear);

            float t = 0f;
            while (t < duration)
            {
                float a = 1f - (t / duration);
                for (int i = 0; i < srs.Count; i++)
                    if (srs[i] != null) srs[i].color = new Color(startColors[i].r, startColors[i].g, startColors[i].b, startColors[i].a * a);
                t += Time.deltaTime;
                yield return null;
            }
            ClearHintGlows();
        }

        Cell GetExitCell()
        {
            for (int x = 0; x < _gridCols; x++)
                for (int y = 0; y < _gridRows; y++)
                    if (_cells[x, y].type == CellType.Exit) return _cells[x, y];
            return default;
        }

        void OnDestroy()
        {
            ClearGrid();
            if (_characterGo != null) Destroy(_characterGo);
        }
    }
}
