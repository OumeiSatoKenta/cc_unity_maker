using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game010v2_GearSync
{
    public enum GearType { None, SmallGear, LargeGear, Belt, FixedObstacle }
    public enum RotDir { None, CW, CCW }

    [System.Serializable]
    public class GearCell
    {
        public GearType type = GearType.None;
        public bool isFixed = false;
        public bool isPowerSource = false;
        public bool isGoal = false;
        public RotDir requiredDir = RotDir.None;
        public RotDir currentDir = RotDir.None;
        public GameObject go;
        public SpriteRenderer sr;
    }

    [System.Serializable]
    public class BeltConnection
    {
        public int fromRow, fromCol, toRow, toCol;
    }

    public class GearSyncManager : MonoBehaviour
    {
        [SerializeField] GearSyncGameManager _gameManager;
        [SerializeField] GearSyncUI _ui;

        [SerializeField] Sprite _smallGearSprite;
        [SerializeField] Sprite _largeGearSprite;
        [SerializeField] Sprite _powerSourceSprite;
        [SerializeField] Sprite _goalGearSprite;
        [SerializeField] Sprite _fixedGearSprite;
        [SerializeField] Sprite _beltSprite;
        [SerializeField] Sprite _gridCellSprite;
        [SerializeField] Sprite _arrowCWSprite;
        [SerializeField] Sprite _arrowCCWSprite;

        int _gridSize;
        GearCell[,] _grid;
        List<BeltConnection> _belts = new List<BeltConnection>();
        List<GameObject> _cellObjects = new List<GameObject>();

        float _cellSize;
        Vector2 _gridOrigin;
        Camera _mainCamera;

        GearType _selectedType = GearType.None;
        bool _isActive = false;
        int _testCount = 0;
        int _partsMin = 1;
        bool _isAnimating = false;

        // Pending belt: first cell selected for belt connection
        int _beltFirstRow = -1, _beltFirstCol = -1;

        int _smallGearCount = 0;
        int _largeGearCount = 0;
        int _beltCount = 0;
        int _smallGearUsed = 0;
        int _largeGearUsed = 0;
        int _beltUsed = 0;

        void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void SetupStage(StageManager.StageConfig config, int stage)
        {
            _isActive = false;
            _testCount = 0;
            _selectedType = GearType.None;
            _beltFirstRow = -1;
            _beltFirstCol = -1;
            _mainCamera = Camera.main;

            ClearGrid();

            switch (stage)
            {
                case 1: SetupStage1(); break;
                case 2: SetupStage2(); break;
                case 3: SetupStage3(); break;
                case 4: SetupStage4(); break;
                case 5: SetupStage5(); break;
                default: SetupStage1(); break;
            }

            BuildGrid();
            _isActive = true;
            _ui.UpdateParts(_smallGearCount - _smallGearUsed, _largeGearCount - _largeGearUsed, _beltCount - _beltUsed);
            _ui.UpdateTestCount(_testCount);
        }

        void SetupStage1()
        {
            _gridSize = 4;
            _grid = new GearCell[_gridSize, _gridSize];
            InitGrid();
            // 動力源 (row=1, col=0)
            _grid[1, 0].type = GearType.SmallGear;
            _grid[1, 0].isPowerSource = true;
            _grid[1, 0].isFixed = true;
            // ゴール (row=1, col=3) - CCW方向
            _grid[1, 3].type = GearType.SmallGear;
            _grid[1, 3].isGoal = true;
            _grid[1, 3].isFixed = true;
            _grid[1, 3].requiredDir = RotDir.CCW;
            _smallGearCount = 2;
            _largeGearCount = 0;
            _beltCount = 0;
            _partsMin = 1;
        }

        void SetupStage2()
        {
            _gridSize = 4;
            _grid = new GearCell[_gridSize, _gridSize];
            InitGrid();
            // 動力源 (row=0, col=0)
            _grid[0, 0].type = GearType.SmallGear;
            _grid[0, 0].isPowerSource = true;
            _grid[0, 0].isFixed = true;
            // ゴール (row=2, col=3) - CW方向
            _grid[2, 3].type = GearType.SmallGear;
            _grid[2, 3].isGoal = true;
            _grid[2, 3].isFixed = true;
            _grid[2, 3].requiredDir = RotDir.CW;
            _smallGearCount = 3;
            _largeGearCount = 0;
            _beltCount = 0;
            _partsMin = 2;
        }

        void SetupStage3()
        {
            _gridSize = 5;
            _grid = new GearCell[_gridSize, _gridSize];
            InitGrid();
            // 動力源 (row=2, col=0)
            _grid[2, 0].type = GearType.SmallGear;
            _grid[2, 0].isPowerSource = true;
            _grid[2, 0].isFixed = true;
            // 大歯車固定 (row=2, col=2)
            _grid[2, 2].type = GearType.LargeGear;
            _grid[2, 2].isFixed = true;
            // ゴール (row=2, col=4) - CCW
            _grid[2, 4].type = GearType.SmallGear;
            _grid[2, 4].isGoal = true;
            _grid[2, 4].isFixed = true;
            _grid[2, 4].requiredDir = RotDir.CCW;
            _smallGearCount = 2;
            _largeGearCount = 1;
            _beltCount = 0;
            _partsMin = 1;
        }

        void SetupStage4()
        {
            _gridSize = 5;
            _grid = new GearCell[_gridSize, _gridSize];
            InitGrid();
            // 動力源 (row=0, col=0)
            _grid[0, 0].type = GearType.SmallGear;
            _grid[0, 0].isPowerSource = true;
            _grid[0, 0].isFixed = true;
            // 固定障害歯車
            _grid[2, 1].type = GearType.FixedObstacle;
            _grid[2, 1].isFixed = true;
            _grid[1, 2].type = GearType.FixedObstacle;
            _grid[1, 2].isFixed = true;
            // ゴール (row=0, col=4) - CW
            _grid[0, 4].type = GearType.SmallGear;
            _grid[0, 4].isGoal = true;
            _grid[0, 4].isFixed = true;
            _grid[0, 4].requiredDir = RotDir.CW;
            _smallGearCount = 3;
            _largeGearCount = 1;
            _beltCount = 0;
            _partsMin = 2;
        }

        void SetupStage5()
        {
            _gridSize = 6;
            _grid = new GearCell[_gridSize, _gridSize];
            InitGrid();
            // 動力源 (row=2, col=0)
            _grid[2, 0].type = GearType.SmallGear;
            _grid[2, 0].isPowerSource = true;
            _grid[2, 0].isFixed = true;
            // 固定大歯車 (row=2, col=2)
            _grid[2, 2].type = GearType.LargeGear;
            _grid[2, 2].isFixed = true;
            // ゴール1 (row=0, col=5) - CW
            _grid[0, 5].type = GearType.SmallGear;
            _grid[0, 5].isGoal = true;
            _grid[0, 5].isFixed = true;
            _grid[0, 5].requiredDir = RotDir.CW;
            // ゴール2 (row=4, col=5) - CCW
            _grid[4, 5].type = GearType.SmallGear;
            _grid[4, 5].isGoal = true;
            _grid[4, 5].isFixed = true;
            _grid[4, 5].requiredDir = RotDir.CCW;
            _smallGearCount = 3;
            _largeGearCount = 1;
            _beltCount = 1;
            _partsMin = 3;
            // Stage5 デフォルトベルト接続なし（プレイヤーがベルトを置いて2点を繋ぐ）
        }

        void InitGrid()
        {
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    _grid[r, c] = new GearCell();
        }

        void ClearGrid()
        {
            foreach (var obj in _cellObjects) if (obj) Destroy(obj);
            _cellObjects.Clear();
            _belts.Clear();
            _smallGearUsed = 0;
            _largeGearUsed = 0;
            _beltUsed = 0;
        }

        void BuildGrid()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            float camSize = _mainCamera.orthographicSize;
            float camWidth = camSize * _mainCamera.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 3.0f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            float availableWidth = camWidth * 2f - 1.0f;
            _cellSize = Mathf.Min(availableHeight / _gridSize, availableWidth / _gridSize, 1.3f);

            float totalW = _cellSize * _gridSize;
            _gridOrigin = new Vector2(-totalW / 2f + _cellSize * 0.5f, camSize - topMargin - _cellSize * 0.5f);

            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    Vector3 pos = GridToWorld(r, c);
                    var cell = _grid[r, c];

                    var bgObj = new GameObject($"Cell_{r}_{c}");
                    bgObj.transform.position = pos + Vector3.back * 0.1f;
                    var bgSr = bgObj.AddComponent<SpriteRenderer>();
                    bgSr.sprite = _gridCellSprite;
                    bgSr.color = new Color(1, 1, 1, 0.4f);
                    bgSr.transform.localScale = Vector3.one * _cellSize * 0.95f;
                    _cellObjects.Add(bgObj);

                    if (cell.type != GearType.None)
                        SpawnGearObject(r, c);
                }
            }
        }

        void SpawnGearObject(int r, int c)
        {
            var cell = _grid[r, c];
            if (cell.go != null) { Destroy(cell.go); cell.go = null; }

            var go = new GameObject($"Gear_{r}_{c}");
            go.transform.position = GridToWorld(r, c);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            sr.sprite = GetGearSprite(cell);
            sr.transform.localScale = Vector3.one * _cellSize * 0.9f;

            cell.go = go;
            cell.sr = sr;
            _cellObjects.Add(go);

            if (cell.isGoal && cell.requiredDir != RotDir.None)
            {
                var arrowGo = new GameObject($"Arrow_{r}_{c}");
                arrowGo.transform.position = GridToWorld(r, c) + Vector3.forward * -0.5f;
                var aSr = arrowGo.AddComponent<SpriteRenderer>();
                aSr.sprite = cell.requiredDir == RotDir.CW ? _arrowCWSprite : _arrowCCWSprite;
                aSr.sortingOrder = 2;
                aSr.transform.localScale = Vector3.one * _cellSize * 0.8f;
                _cellObjects.Add(arrowGo);
            }
        }

        Sprite GetGearSprite(GearCell cell)
        {
            if (cell.isPowerSource) return _powerSourceSprite;
            if (cell.isGoal) return _goalGearSprite;
            if (cell.type == GearType.FixedObstacle || cell.isFixed) return _fixedGearSprite;
            if (cell.type == GearType.LargeGear) return _largeGearSprite;
            if (cell.type == GearType.Belt) return _beltSprite;
            return _smallGearSprite;
        }

        Vector3 GridToWorld(int r, int c)
        {
            return new Vector3(_gridOrigin.x + c * _cellSize, _gridOrigin.y - r * _cellSize, 0f);
        }

        bool WorldToGrid(Vector3 world, out int r, out int c)
        {
            float dx = world.x - (_gridOrigin.x - _cellSize * 0.5f);
            float dy = (_gridOrigin.y + _cellSize * 0.5f) - world.y;
            c = Mathf.FloorToInt(dx / _cellSize);
            r = Mathf.FloorToInt(dy / _cellSize);
            return r >= 0 && r < _gridSize && c >= 0 && c < _gridSize;
        }

        bool InBounds(int r, int c) => r >= 0 && r < _gridSize && c >= 0 && c < _gridSize;

        void Update()
        {
            if (!_isActive || _isAnimating) return;
            if (Mouse.current == null) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (_mainCamera == null) return;
                Vector2 screenPos = Mouse.current.position.ReadValue();
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));

                if (WorldToGrid(worldPos, out int r, out int c))
                    HandleGridClick(r, c);
            }
        }

        void HandleGridClick(int r, int c)
        {
            var cell = _grid[r, c];

            if (_selectedType == GearType.Belt)
            {
                // ベルト: 2点を選んで接続
                if (_beltFirstRow < 0)
                {
                    // 1点目: ギアがある非空セルを選択
                    if (cell.type != GearType.None && cell.type != GearType.Belt)
                    {
                        _beltFirstRow = r;
                        _beltFirstCol = c;
                    }
                }
                else
                {
                    // 2点目: 別のギアセルを選択
                    if ((r != _beltFirstRow || c != _beltFirstCol) && cell.type != GearType.None && cell.type != GearType.Belt)
                    {
                        if (CanPlaceGear(GearType.Belt))
                        {
                            _belts.Add(new BeltConnection { fromRow = _beltFirstRow, fromCol = _beltFirstCol, toRow = r, toCol = c });
                            _beltUsed++;
                            _ui.UpdateParts(_smallGearCount - _smallGearUsed, _largeGearCount - _largeGearUsed, _beltCount - _beltUsed);
                        }
                    }
                    _beltFirstRow = -1;
                    _beltFirstCol = -1;
                }
                return;
            }

            if (_selectedType != GearType.None)
            {
                if (cell.type == GearType.None)
                    PlaceGear(r, c, _selectedType);
                else if (!cell.isFixed && cell.type != GearType.FixedObstacle)
                    RemoveGear(r, c);
            }
            else
            {
                if (!cell.isFixed && cell.type != GearType.None && cell.type != GearType.FixedObstacle)
                    RemoveGear(r, c);
            }
        }

        void PlaceGear(int r, int c, GearType type)
        {
            if (!CanPlaceGear(type)) return;
            _grid[r, c].type = type;
            if (type == GearType.SmallGear) _smallGearUsed++;
            else if (type == GearType.LargeGear) _largeGearUsed++;

            SpawnGearObject(r, c);
            var placedGo = _grid[r, c].go;
            if (placedGo != null) StartCoroutine(PlacePulse(placedGo));

            _ui.UpdateParts(_smallGearCount - _smallGearUsed, _largeGearCount - _largeGearUsed, _beltCount - _beltUsed);
        }

        void RemoveGear(int r, int c)
        {
            var cell = _grid[r, c];
            if (cell.go != null) { Destroy(cell.go); cell.go = null; }
            if (cell.type == GearType.SmallGear) _smallGearUsed--;
            else if (cell.type == GearType.LargeGear) _largeGearUsed--;
            cell.type = GearType.None;
            cell.sr = null;

            _ui.UpdateParts(_smallGearCount - _smallGearUsed, _largeGearCount - _largeGearUsed, _beltCount - _beltUsed);
        }

        bool CanPlaceGear(GearType type)
        {
            if (type == GearType.SmallGear) return (_smallGearCount - _smallGearUsed) > 0;
            if (type == GearType.LargeGear) return (_largeGearCount - _largeGearUsed) > 0;
            if (type == GearType.Belt) return (_beltCount - _beltUsed) > 0;
            return false;
        }

        public void SelectSmallGear()
        {
            if (!_isActive || _isAnimating) return;
            _selectedType = _selectedType == GearType.SmallGear ? GearType.None : GearType.SmallGear;
            _beltFirstRow = -1; _beltFirstCol = -1;
            _ui.UpdateSelection(_selectedType);
        }

        public void SelectLargeGear()
        {
            if (!_isActive || _isAnimating) return;
            _selectedType = _selectedType == GearType.LargeGear ? GearType.None : GearType.LargeGear;
            _beltFirstRow = -1; _beltFirstCol = -1;
            _ui.UpdateSelection(_selectedType);
        }

        public void SelectBelt()
        {
            if (!_isActive || _isAnimating) return;
            _selectedType = _selectedType == GearType.Belt ? GearType.None : GearType.Belt;
            _beltFirstRow = -1; _beltFirstCol = -1;
            _ui.UpdateSelection(_selectedType);
        }

        public void RunTest()
        {
            if (!_isActive || _isAnimating) return;
            _testCount++;
            _ui.UpdateTestCount(_testCount);

            SimulateGears();

            bool success = CheckGoals();
            if (success)
            {
                _isActive = false;
                StartCoroutine(SuccessAnimation());
            }
            else
            {
                StartCoroutine(FailAnimation());
                _gameManager.OnTestFailed();
            }
        }

        void SimulateGears()
        {
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    _grid[r, c].currentDir = RotDir.None;

            var queue = new Queue<(int r, int c, RotDir dir)>();
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    if (_grid[r, c].isPowerSource)
                    {
                        _grid[r, c].currentDir = RotDir.CW;
                        queue.Enqueue((r, c, RotDir.CW));
                    }

            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };

            while (queue.Count > 0)
            {
                var (cr, cc, dir) = queue.Dequeue();
                RotDir oppositeDir = dir == RotDir.CW ? RotDir.CCW : RotDir.CW;

                for (int d = 0; d < 4; d++)
                {
                    int nr = cr + dr[d];
                    int nc = cc + dc[d];
                    if (!InBounds(nr, nc)) continue;
                    var neighbor = _grid[nr, nc];
                    if (neighbor.type == GearType.None || neighbor.type == GearType.Belt) continue;
                    if (neighbor.currentDir != RotDir.None) continue;
                    neighbor.currentDir = oppositeDir;
                    queue.Enqueue((nr, nc, oppositeDir));
                }

                // ベルト接続（同方向伝達）
                foreach (var belt in _belts)
                {
                    int fr = belt.fromRow, fc = belt.fromCol;
                    int tr = belt.toRow, tc = belt.toCol;
                    if (!InBounds(fr, fc) || !InBounds(tr, tc)) continue;

                    if (fr == cr && fc == cc && _grid[tr, tc].type != GearType.None && _grid[tr, tc].currentDir == RotDir.None)
                    {
                        _grid[tr, tc].currentDir = dir;
                        queue.Enqueue((tr, tc, dir));
                    }
                    else if (tr == cr && tc == cc && _grid[fr, fc].type != GearType.None && _grid[fr, fc].currentDir == RotDir.None)
                    {
                        _grid[fr, fc].currentDir = dir;
                        queue.Enqueue((fr, fc, dir));
                    }
                }
            }
        }

        bool CheckGoals()
        {
            bool hasGoal = false;
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                {
                    var cell = _grid[r, c];
                    if (cell.isGoal && cell.requiredDir != RotDir.None)
                    {
                        hasGoal = true;
                        if (cell.currentDir != cell.requiredDir) return false;
                    }
                }
            return hasGoal;
        }

        IEnumerator SuccessAnimation()
        {
            _isAnimating = true;
            float elapsed = 0f;
            float duration = 1.5f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                for (int r = 0; r < _gridSize; r++)
                    for (int c = 0; c < _gridSize; c++)
                    {
                        var cell = _grid[r, c];
                        if (cell.go == null || cell.currentDir == RotDir.None) continue;
                        float sign = cell.currentDir == RotDir.CW ? -1f : 1f;
                        cell.go.transform.Rotate(0f, 0f, sign * 120f * Time.deltaTime);
                    }
                yield return null;
            }

            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                {
                    var cell = _grid[r, c];
                    if (cell.isGoal && cell.sr != null)
                        StartCoroutine(ColorFlash(cell.sr, Color.green));
                }

            yield return new WaitForSeconds(0.5f);
            _isAnimating = false;
            _gameManager.OnTestResult(true, _testCount, _smallGearUsed + _largeGearUsed + _beltUsed, _partsMin);
        }

        IEnumerator FailAnimation()
        {
            _isAnimating = true;
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                {
                    var cell = _grid[r, c];
                    if (cell.isGoal && cell.currentDir != cell.requiredDir && cell.sr != null)
                        StartCoroutine(ColorFlash(cell.sr, Color.red));
                }

            if (_mainCamera != null) StartCoroutine(CameraShake(0.3f, 0.15f));
            yield return new WaitForSeconds(0.6f);
            _isAnimating = false;
        }

        IEnumerator ColorFlash(SpriteRenderer sr, Color flashColor)
        {
            if (sr == null) yield break;
            Color orig = sr.color;
            sr.color = flashColor;
            yield return new WaitForSeconds(0.3f);
            float t = 0f;
            while (t < 0.2f)
            {
                if (sr == null) yield break;
                t += Time.deltaTime;
                sr.color = Color.Lerp(flashColor, orig, t / 0.2f);
                yield return null;
            }
            if (sr != null) sr.color = orig;
        }

        IEnumerator PlacePulse(GameObject go)
        {
            if (go == null) yield break;
            Vector3 orig = go.transform.localScale;
            Vector3 big = orig * 1.2f;
            float t = 0f;
            while (t < 0.1f)
            {
                if (go == null) yield break;
                t += Time.deltaTime;
                go.transform.localScale = Vector3.Lerp(orig, big, t / 0.1f);
                yield return null;
            }
            t = 0f;
            while (t < 0.1f)
            {
                if (go == null) yield break;
                t += Time.deltaTime;
                go.transform.localScale = Vector3.Lerp(big, orig, t / 0.1f);
                yield return null;
            }
            if (go != null) go.transform.localScale = orig;
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            if (_mainCamera == null) yield break;
            Vector3 orig = _mainCamera.transform.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (_mainCamera == null) yield break;
                elapsed += Time.deltaTime;
                _mainCamera.transform.localPosition = new Vector3(
                    orig.x + Random.Range(-1f, 1f) * magnitude,
                    orig.y + Random.Range(-1f, 1f) * magnitude,
                    orig.z);
                yield return null;
            }
            if (_mainCamera != null) _mainCamera.transform.localPosition = orig;
        }
    }
}
