using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game060v2_MeltIce
{
    public class MeltIceGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] MeltIceUI _ui;
        [SerializeField] Transform _boardContainer;
        [SerializeField] LightRaySystem _lightRaySystem;

        // Sprites assigned in SceneSetup
        [SerializeField] Sprite _mirrorSprite;
        [SerializeField] Sprite _iceSprite;
        [SerializeField] Sprite _forbiddenIceSprite;
        [SerializeField] Sprite _wallSprite;
        [SerializeField] Sprite _prismSprite;
        [SerializeField] Sprite _gridCellSprite;

        public enum GameState { Idle, Playing, StageClear, GameClear, GameOver }

        GameState _state = GameState.Idle;
        bool _isActive;

        int _currentStageIndex;
        int _usedMirrors;
        int _totalScore;
        int _comboMultiplier = 1;

        // Board data
        public int GridSize { get; private set; }
        public float CellSize { get; private set; }
        public Vector3 BoardOrigin { get; private set; }

        MirrorController[] _placedMirrors;
        int _availableMirrorCount;
        List<IceBlockController> _iceBlocks = new List<IceBlockController>();
        List<GameObject> _wallObjects = new List<GameObject>();
        List<GameObject> _prismObjects = new List<GameObject>();

        // Input state
        MirrorController _draggingMirror;
        MirrorController _longPressMirror;
        float _pressTime;
        bool _isDragging;
        Vector2 _pressStartPos;
        const float LongPressDuration = 0.5f;
        const float DragThreshold = 0.3f;

        // Stage definitions
        static readonly StageData[] Stages = new StageData[]
        {
            // Stage 1: tutorial, 5x5, sun from top
            new StageData {
                GridSize = 5,
                SunDir = Vector2Int.down,
                SunGridPos = new Vector2Int(2, -1),
                IceTargets = new[] { new Vector2Int(2, 3) },
                IceForbidden = new Vector2Int[0],
                Walls = new Vector2Int[0],
                Prisms = new Vector2Int[0],
                MirrorCount = 2,
                MinMirrors = 1
            },
            // Stage 2: walls, 5x5
            new StageData {
                GridSize = 5,
                SunDir = Vector2Int.down,
                SunGridPos = new Vector2Int(1, -1),
                IceTargets = new[] { new Vector2Int(1, 3), new Vector2Int(3, 3) },
                IceForbidden = new Vector2Int[0],
                Walls = new[] { new Vector2Int(1, 1), new Vector2Int(3, 1) },
                Prisms = new Vector2Int[0],
                MirrorCount = 3,
                MinMirrors = 2
            },
            // Stage 3: forbidden ice, 6x6
            new StageData {
                GridSize = 6,
                SunDir = Vector2Int.right,
                SunGridPos = new Vector2Int(-1, 2),
                IceTargets = new[] { new Vector2Int(2, 2), new Vector2Int(4, 2), new Vector2Int(4, 4) },
                IceForbidden = new[] { new Vector2Int(2, 4) },
                Walls = new[] { new Vector2Int(1, 2), new Vector2Int(3, 3) },
                Prisms = new Vector2Int[0],
                MirrorCount = 3,
                MinMirrors = 3
            },
            // Stage 4: prism, 6x6
            new StageData {
                GridSize = 6,
                SunDir = Vector2Int.down,
                SunGridPos = new Vector2Int(2, -1),
                IceTargets = new[] { new Vector2Int(0, 4), new Vector2Int(4, 2), new Vector2Int(5, 4) },
                IceForbidden = new[] { new Vector2Int(2, 4), new Vector2Int(0, 2) },
                Walls = new[] { new Vector2Int(2, 2), new Vector2Int(3, 4) },
                Prisms = new[] { new Vector2Int(2, 1) },
                MirrorCount = 3,
                MinMirrors = 3
            },
            // Stage 5: mobile ice, 7x7
            new StageData {
                GridSize = 7,
                SunDir = Vector2Int.down,
                SunGridPos = new Vector2Int(3, -1),
                IceTargets = new[] { new Vector2Int(1, 4), new Vector2Int(3, 3), new Vector2Int(5, 4), new Vector2Int(3, 5) },
                IceForbidden = new[] { new Vector2Int(1, 2), new Vector2Int(5, 2) },
                Walls = new[] { new Vector2Int(2, 2), new Vector2Int(4, 2), new Vector2Int(3, 1) },
                Prisms = new[] { new Vector2Int(3, 2) },
                MirrorCount = 4,
                MinMirrors = 3,
                HasMobileIce = true
            }
        };

        public struct StageData
        {
            public int GridSize;
            public Vector2Int SunDir;
            public Vector2Int SunGridPos;
            public Vector2Int[] IceTargets;
            public Vector2Int[] IceForbidden;
            public Vector2Int[] Walls;
            public Vector2Int[] Prisms;
            public int MirrorCount;
            public int MinMirrors;
            public bool HasMobileIce;
        }

        public StageData CurrentStageData => Stages[_currentStageIndex];

        void Start()
        {
            _ui.Init(this);
            _instructionPanel.Show(
                "060",
                "MeltIce",
                "鏡で太陽光を反射させて氷を溶かそう！",
                "鏡をドラッグして配置、タップで45度回転、長押しで回収できるよ",
                "全ての青い氷ブロックに光を当ててステージクリア！赤い氷には絶対当てないで！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
            if (_instructionPanel != null)
                _instructionPanel.OnDismissed -= StartGame;
        }

        void StartGame()
        {
            _comboMultiplier = 1;
            _totalScore = 0;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            _state = GameState.Playing;
            _isActive = true;
            _currentStageIndex = stage;
            _usedMirrors = 0;

            BuildBoard(stage);
            _lightRaySystem.SetBoard(this);
            _lightRaySystem.RecalculateLightPath();
            _ui.OnStageChanged(stage + 1, Stages[stage].MirrorCount - _usedMirrors);
        }

        void OnAllStagesCleared()
        {
            _state = GameState.GameClear;
            _isActive = false;
            _ui.ShowGameClear(_totalScore);
        }

        void BuildBoard(int stageIndex)
        {
            // Clear existing board
            foreach (Transform child in _boardContainer)
                Destroy(child.gameObject);
            _iceBlocks.Clear();
            _wallObjects.Clear();
            _prismObjects.Clear();

            var data = Stages[stageIndex];
            GridSize = data.GridSize;

            // Calculate cell size
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.4f;
            float bottomMargin = 3.2f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            CellSize = Mathf.Min(availableHeight / GridSize, camWidth * 2f / GridSize, 1.05f);

            float boardW = CellSize * GridSize;
            float boardH = CellSize * GridSize;
            BoardOrigin = new Vector3(
                -boardW / 2f + CellSize / 2f,
                camSize - topMargin - CellSize / 2f,
                0f
            );

            // Grid cells (visual)
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (_gridCellSprite != null)
                    {
                        var cellObj = new GameObject($"Cell_{x}_{y}");
                        cellObj.transform.SetParent(_boardContainer);
                        cellObj.transform.position = GridToWorld(new Vector2Int(x, y));
                        var sr = cellObj.AddComponent<SpriteRenderer>();
                        sr.sprite = _gridCellSprite;
                        sr.sortingOrder = 0;
                        float scale = CellSize * 0.95f;
                        cellObj.transform.localScale = new Vector3(scale, scale, 1f);
                    }
                }
            }

            // Walls
            foreach (var pos in data.Walls)
            {
                var wallObj = new GameObject($"Wall_{pos.x}_{pos.y}");
                wallObj.transform.SetParent(_boardContainer);
                wallObj.transform.position = GridToWorld(pos);
                var sr = wallObj.AddComponent<SpriteRenderer>();
                sr.sprite = _wallSprite;
                sr.sortingOrder = 2;
                wallObj.transform.localScale = new Vector3(CellSize, CellSize, 1f);
                var col = wallObj.AddComponent<BoxCollider2D>();
                col.size = Vector2.one;
                wallObj.tag = "Wall";
                _wallObjects.Add(wallObj);
            }

            // Prisms
            foreach (var pos in data.Prisms)
            {
                var prismObj = new GameObject($"Prism_{pos.x}_{pos.y}");
                prismObj.transform.SetParent(_boardContainer);
                prismObj.transform.position = GridToWorld(pos);
                var sr = prismObj.AddComponent<SpriteRenderer>();
                sr.sprite = _prismSprite;
                sr.sortingOrder = 2;
                prismObj.transform.localScale = new Vector3(CellSize, CellSize, 1f);
                var col = prismObj.AddComponent<BoxCollider2D>();
                col.size = Vector2.one;
                prismObj.tag = "Prism";
                _prismObjects.Add(prismObj);
            }

            // Ice targets
            for (int i = 0; i < data.IceTargets.Length; i++)
            {
                var pos = data.IceTargets[i];
                var iceObj = new GameObject($"IceTarget_{i}");
                iceObj.transform.SetParent(_boardContainer);
                iceObj.transform.position = GridToWorld(pos);
                var ice = iceObj.AddComponent<IceBlockController>();
                ice.Setup(pos, false, _iceSprite, CellSize,
                    data.HasMobileIce && i < 2,  // first 2 ice blocks move in stage 5
                    new Vector2Int(data.GridSize - 1, data.GridSize - 1));
                _iceBlocks.Add(ice);
            }

            // Forbidden ice
            for (int i = 0; i < data.IceForbidden.Length; i++)
            {
                var pos = data.IceForbidden[i];
                var iceObj = new GameObject($"ForbiddenIce_{i}");
                iceObj.transform.SetParent(_boardContainer);
                iceObj.transform.position = GridToWorld(pos);
                var ice = iceObj.AddComponent<IceBlockController>();
                ice.Setup(pos, true, _forbiddenIceSprite, CellSize, false, Vector2Int.zero);
                _iceBlocks.Add(ice);
            }

            // Prepare mirror slots (available mirrors stored as count)
            _availableMirrorCount = data.MirrorCount;
            _placedMirrors = new MirrorController[data.MirrorCount];
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            return BoardOrigin + new Vector3(gridPos.x * CellSize, -gridPos.y * CellSize, 0f);
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            Vector3 local = worldPos - BoardOrigin;
            int gx = Mathf.RoundToInt(local.x / CellSize);
            int gy = Mathf.RoundToInt(-local.y / CellSize);
            return new Vector2Int(gx, gy);
        }

        public bool IsValidGridPos(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < GridSize && pos.y >= 0 && pos.y < GridSize;
        }

        public bool IsCellOccupied(Vector2Int pos)
        {
            var data = Stages[_currentStageIndex];
            foreach (var w in data.Walls) if (w == pos) return true;
            foreach (var p in data.Prisms) if (p == pos) return true;
            foreach (var t in data.IceTargets) if (t == pos) return true;
            foreach (var f in data.IceForbidden) if (f == pos) return true;
            foreach (var m in _placedMirrors)
                if (m != null && m.GridPosition == pos) return true;
            return false;
        }

        public MirrorController GetMirrorAt(Vector2Int pos)
        {
            foreach (var m in _placedMirrors)
                if (m != null && m.GridPosition == pos) return m;
            return null;
        }

        public List<IceBlockController> GetIceBlocks() => _iceBlocks;

        public bool HasWallAt(Vector2Int pos)
        {
            var data = Stages[_currentStageIndex];
            foreach (var w in data.Walls) if (w == pos) return true;
            return false;
        }

        public bool HasPrismAt(Vector2Int pos)
        {
            var data = Stages[_currentStageIndex];
            foreach (var p in data.Prisms) if (p == pos) return true;
            return false;
        }

        void Update()
        {
            if (!_isActive) return;
            if (_state != GameState.Playing) return;

            HandleInput();
        }

        void HandleInput()
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _pressStartPos = worldPos;
                _pressTime = Time.time;
                _isDragging = false;
                _longPressMirror = null;

                var gridPos = WorldToGrid(worldPos);
                _longPressMirror = GetMirrorAt(gridPos);
            }

            if (Mouse.current.leftButton.isPressed)
            {
                float dist = Vector2.Distance(worldPos, _pressStartPos);
                if (!_isDragging && dist > DragThreshold)
                {
                    _isDragging = true;
                    var startGrid = WorldToGrid(_pressStartPos);
                    var existingMirror = GetMirrorAt(startGrid);
                    if (existingMirror != null)
                    {
                        // Pick up mirror to drag
                        _draggingMirror = existingMirror;
                        RemoveMirrorFromPlaced(_draggingMirror);
                        _availableMirrorCount++;
                        _usedMirrors = Mathf.Max(0, _usedMirrors - 1);
                        _lightRaySystem.RecalculateLightPath();
                        _ui.UpdateMirrorCount(_availableMirrorCount);
                    }
                    else if (_availableMirrorCount > 0)
                    {
                        // Create new mirror to drag
                        _draggingMirror = CreateMirror(new Vector2Int(-1, -1));
                    }
                    _longPressMirror = null;
                }

                if (_isDragging && _draggingMirror != null)
                {
                    _draggingMirror.transform.position = new Vector3(worldPos.x, worldPos.y, -0.5f);
                }

                // Long press to remove mirror
                if (!_isDragging && _longPressMirror != null &&
                    Time.time - _pressTime >= LongPressDuration)
                {
                    RemoveMirrorFromPlaced(_longPressMirror);
                    Destroy(_longPressMirror.gameObject);
                    _availableMirrorCount++;
                    _usedMirrors = Mathf.Max(0, _usedMirrors - 1);
                    _lightRaySystem.RecalculateLightPath();
                    _ui.UpdateMirrorCount(_availableMirrorCount);
                    _longPressMirror = null;
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (_isDragging && _draggingMirror != null)
                {
                    var gridPos = WorldToGrid(worldPos);
                    if (IsValidGridPos(gridPos) && !IsCellOccupied(gridPos) && _availableMirrorCount > 0)
                    {
                        PlaceMirror(_draggingMirror, gridPos);
                        _availableMirrorCount--;
                        _usedMirrors++;
                        _lightRaySystem.RecalculateLightPath();
                        _ui.UpdateMirrorCount(_availableMirrorCount);
                        CheckWinCondition();
                    }
                    else
                    {
                        // Return to pool or discard
                        Destroy(_draggingMirror.gameObject);
                    }
                    _draggingMirror = null;
                }
                else if (!_isDragging)
                {
                    // Tap: rotate mirror
                    var gridPos = WorldToGrid(worldPos);
                    var mirror = GetMirrorAt(gridPos);
                    if (mirror != null)
                    {
                        mirror.Rotate45();
                        _lightRaySystem.RecalculateLightPath();
                        CheckWinCondition();
                    }
                }
                _isDragging = false;
                _longPressMirror = null;
            }
        }

        MirrorController CreateMirror(Vector2Int gridPos)
        {
            var obj = new GameObject("Mirror");
            obj.transform.SetParent(_boardContainer);
            var mc = obj.AddComponent<MirrorController>();
            mc.Setup(gridPos, _mirrorSprite, CellSize);
            return mc;
        }

        void PlaceMirror(MirrorController mirror, Vector2Int gridPos)
        {
            mirror.GridPosition = gridPos;
            mirror.transform.position = GridToWorld(gridPos);
            mirror.transform.localScale = new Vector3(CellSize, CellSize, 1f);
            // Add to placed list
            for (int i = 0; i < _placedMirrors.Length; i++)
            {
                if (_placedMirrors[i] == null)
                {
                    _placedMirrors[i] = mirror;
                    break;
                }
            }
            StartCoroutine(mirror.PlaceBounceAnimation());
        }

        void RemoveMirrorFromPlaced(MirrorController mirror)
        {
            for (int i = 0; i < _placedMirrors.Length; i++)
            {
                if (_placedMirrors[i] == mirror)
                {
                    _placedMirrors[i] = null;
                    break;
                }
            }
        }

        void CheckWinCondition()
        {
            bool allTargetsMelted = true;
            bool forbiddenHit = false;

            foreach (var ice in _iceBlocks)
            {
                if (ice == null) continue;
                if (ice.IsForbidden)
                {
                    if (ice.IsHitByLight)
                    {
                        forbiddenHit = true;
                        break;
                    }
                }
                else
                {
                    if (!ice.IsHitByLight)
                        allTargetsMelted = false;
                }
            }

            if (forbiddenHit)
            {
                StartCoroutine(GameOverCoroutine());
            }
            else if (allTargetsMelted)
            {
                StartCoroutine(StageClearCoroutine());
            }
        }

        IEnumerator StageClearCoroutine()
        {
            _state = GameState.StageClear;
            _isActive = false;

            // Melt animation for all target ice
            foreach (var ice in _iceBlocks)
                if (ice != null && !ice.IsForbidden && ice.IsHitByLight)
                    StartCoroutine(ice.MeltAnimation());

            yield return new WaitForSeconds(0.8f);

            var data = Stages[_currentStageIndex];
            int baseScore = (data.MirrorCount - _usedMirrors) * 200;
            baseScore = Mathf.Max(baseScore, 50);
            float optBonus = (_usedMirrors <= data.MinMirrors) ? 2.0f : 1.0f;
            float combo = 1.0f + (_comboMultiplier - 1) * 0.1f;
            combo = Mathf.Clamp(combo, 1.0f, 1.5f);
            int score = Mathf.RoundToInt(baseScore * optBonus * combo);
            _totalScore += score;
            _comboMultiplier++;

            _ui.ShowStageClear(score);
        }

        IEnumerator GameOverCoroutine()
        {
            _state = GameState.GameOver;
            _isActive = false;
            StartCoroutine(CameraShake());
            // Red flash on forbidden ice
            foreach (var ice in _iceBlocks)
                if (ice != null && ice.IsForbidden && ice.IsHitByLight)
                    StartCoroutine(ice.ForbiddenHitFlash());
            yield return new WaitForSeconds(0.5f);
            _ui.ShowGameOver();
        }

        public void OnNextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void OnBackToMenu()
        {
            SceneLoader.BackToMenu();
        }

        IEnumerator CameraShake()
        {
            if (Camera.main == null) yield break;
            var cam = Camera.main.transform;
            Vector3 origin = cam.position;
            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                cam.position = origin + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cam.position = origin;
        }

        // Called by LightRaySystem to update ice hit states
        public void UpdateIceHitStates(HashSet<IceBlockController> hitIces)
        {
            foreach (var ice in _iceBlocks)
            {
                if (ice == null) continue;
                ice.IsHitByLight = hitIces.Contains(ice);
                ice.UpdateVisual();
            }
        }
    }
}
