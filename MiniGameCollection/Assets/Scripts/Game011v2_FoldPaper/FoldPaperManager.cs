using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game011v2_FoldPaper
{
    public enum FoldLineType { Horizontal, Vertical, Diagonal45, Diagonal135 }

    public struct FoldLineData
    {
        public FoldLineType type;
        public int index; // row/col index (0-based)
        // For diagonal: index is not used (single diagonal for now)
    }

    public class FoldPaperManager : MonoBehaviour
    {
        [SerializeField] FoldPaperGameManager _gameManager;
        [SerializeField] FoldPaperUI _ui;

        [SerializeField] Sprite _paperCellSprite;
        [SerializeField] Sprite _targetCellSprite;
        [SerializeField] Sprite _selectedCellSprite;
        [SerializeField] Sprite _foldLineHSprite;
        [SerializeField] Sprite _foldLineVSprite;

        // Stage state
        int _gridSize;
        int _maxFolds;
        int _minFolds;
        int _movesLeft;
        int _undoLeft;
        int _undoUsed;
        bool _hasOverlapRule;
        bool _hasDiagonal;
        bool _hasFlip;
        int _timeLimit;
        float _timeRemaining;
        bool _timerActive;

        bool[,] _grid;
        bool[,] _targetGrid;
        bool[,] _initialGrid;
        Stack<bool[,]> _history = new Stack<bool[,]>();

        // GameObjects
        List<GameObject> _cellObjects = new List<GameObject>();
        List<GameObject> _targetObjects = new List<GameObject>();
        List<(FoldLineData data, GameObject go)> _foldLineObjects = new List<(FoldLineData, GameObject)>();

        FoldLineData? _selectedFoldLine = null;
        bool _isActive;

        float _cellSize;
        Vector2 _gridOrigin;

        // Stage config per stage index
        static readonly int[] GridSizes       = { 4, 4, 5, 5, 6 };
        static readonly int[] MaxFolds        = { 4, 6, 8, 10, 12 };
        static readonly int[] MinFolds        = { 2, 3, 4, 5, 6 };
        static readonly bool[] HasOverlap     = { false, true, true, true, true };
        static readonly bool[] HasDiagonal    = { false, false, true, true, true };
        static readonly bool[] HasFlip        = { false, false, false, true, true };
        static readonly int[] TimeLimits      = { 0, 0, 0, 0, 60 };

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            ClearAll();
            _isActive = true;

            int si = Mathf.Clamp(stageIndex, 0, 4);
            _gridSize = GridSizes[si];
            _maxFolds = MaxFolds[si];
            _minFolds = MinFolds[si];
            _movesLeft = _maxFolds;
            _undoLeft = 3;
            _undoUsed = 0;
            _hasOverlapRule = HasOverlap[si];
            _hasDiagonal = HasDiagonal[si];
            _hasFlip = HasFlip[si];
            _timeLimit = TimeLimits[si];
            _history.Clear();

            // Calculate cell positions
            var mainCam = Camera.main;
            if (mainCam == null) { Debug.LogError("[FoldPaper] Camera.main is null"); return; }
            float camSize = mainCam.orthographicSize;
            float camWidth = camSize * mainCam.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 3.2f;
            float available = camSize * 2f - topMargin - bottomMargin;
            _cellSize = Mathf.Min(available / _gridSize, camWidth * 2f / _gridSize, 1.0f);
            float totalW = _cellSize * _gridSize;
            float totalH = _cellSize * _gridSize;
            float centerY = camSize - topMargin - totalH / 2f;
            _gridOrigin = new Vector2(-totalW / 2f + _cellSize / 2f, centerY + totalH / 2f - _cellSize / 2f);

            // Init grids
            _grid = new bool[_gridSize, _gridSize];
            _initialGrid = new bool[_gridSize, _gridSize];
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                {
                    _grid[r, c] = true;
                    _initialGrid[r, c] = true;
                }

            _targetGrid = BuildTargetGrid(si);
            SpawnCells();
            SpawnFoldLines(si);
            SpawnTargetDisplay();

            if (_timeLimit > 0)
            {
                _timeRemaining = _timeLimit;
                _timerActive = true;
            }

            _ui.UpdateMoves(_movesLeft, _maxFolds);
            _ui.UpdateUndo(_undoLeft);
        }

        bool[,] BuildTargetGrid(int si)
        {
            bool[,] tg = new bool[_gridSize, _gridSize];
            // Stage-specific target shapes (hand-crafted patterns)
            switch (si)
            {
                case 0: // 4x4 → fold bottom half up → 4x2
                    for (int r = 0; r < 2; r++)
                        for (int c = 0; c < 4; c++)
                            tg[r, c] = true;
                    break;
                case 1: // 4x4 → fold right side → 4x2 + overlap cleared
                    for (int r = 0; r < 4; r++)
                        for (int c = 0; c < 2; c++)
                            tg[r, c] = true;
                    break;
                case 2: // 5x5 → L-shape via diagonal
                    for (int r = 0; r < 5; r++) tg[r, 0] = true;
                    for (int c = 0; c < 5; c++) tg[4, c] = true;
                    break;
                case 3: // 5x5 → T-shape
                    for (int c = 0; c < 5; c++) tg[0, c] = true;
                    for (int r = 0; r < 5; r++) tg[r, 2] = true;
                    break;
                case 4: // 6x6 → cross / plus shape
                    for (int c = 0; c < 6; c++) tg[2, c] = tg[3, c] = true;
                    for (int r = 0; r < 6; r++) tg[r, 2] = tg[r, 3] = true;
                    break;
            }
            return tg;
        }

        void SpawnCells()
        {
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    var go = new GameObject($"Cell_{r}_{c}");
                    go.transform.SetParent(transform);
                    go.transform.position = CellToWorld(r, c);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = _paperCellSprite;
                    sr.sortingOrder = 1;
                    go.transform.localScale = Vector3.one * _cellSize * 0.9f;
                    go.AddComponent<BoxCollider2D>();
                    _cellObjects.Add(go);
                }
            }
        }

        void SpawnTargetDisplay()
        {
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    if (!_targetGrid[r, c]) continue;
                    var go = new GameObject($"Target_{r}_{c}");
                    go.transform.SetParent(transform);
                    go.transform.position = CellToWorld(r, c) + Vector3.back * 0.1f;
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = _targetCellSprite;
                    sr.sortingOrder = 0;
                    go.transform.localScale = Vector3.one * _cellSize * 0.85f;
                    _targetObjects.Add(go);
                }
            }
        }

        void SpawnFoldLines(int si)
        {
            // Horizontal fold lines between rows
            int halfR = _gridSize / 2;
            var hData = new FoldLineData { type = FoldLineType.Horizontal, index = halfR };
            var hGo = CreateFoldLineGO(hData);
            _foldLineObjects.Add((hData, hGo));

            // Vertical fold lines between cols
            int halfC = _gridSize / 2;
            var vData = new FoldLineData { type = FoldLineType.Vertical, index = halfC };
            var vGo = CreateFoldLineGO(vData);
            _foldLineObjects.Add((vData, vGo));

            // Diagonal for stage 3+
            if (HasDiagonal[si])
            {
                var d45Data = new FoldLineData { type = FoldLineType.Diagonal45, index = 0 };
                var d45Go = CreateFoldLineGO(d45Data);
                _foldLineObjects.Add((d45Data, d45Go));
            }
        }

        GameObject CreateFoldLineGO(FoldLineData data)
        {
            var go = new GameObject($"FoldLine_{data.type}_{data.index}");
            go.transform.SetParent(transform);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            var col = go.AddComponent<BoxCollider2D>();

            float totalLen = _cellSize * _gridSize;
            float lineThick = 0.15f;

            switch (data.type)
            {
                case FoldLineType.Horizontal:
                {
                    float worldY = _gridOrigin.y - (data.index - 0.5f) * _cellSize;
                    float worldX = _gridOrigin.x - _cellSize / 2f + totalLen / 2f;
                    go.transform.position = new Vector3(worldX, worldY, -0.5f);
                    go.transform.localScale = new Vector3(totalLen, lineThick, 1f);
                    sr.sprite = _foldLineHSprite;
                    sr.color = new Color(0.4f, 0.8f, 1f, 0.7f);
                    col.size = new Vector2(1f, 1.5f / lineThick);
                    break;
                }
                case FoldLineType.Vertical:
                {
                    float worldX = _gridOrigin.x + (data.index - 0.5f) * _cellSize;
                    float worldY = _gridOrigin.y - totalLen / 2f + _cellSize / 2f;
                    go.transform.position = new Vector3(worldX, worldY, -0.5f);
                    go.transform.localScale = new Vector3(lineThick, totalLen, 1f);
                    sr.sprite = _foldLineVSprite;
                    sr.color = new Color(0.4f, 0.8f, 1f, 0.7f);
                    col.size = new Vector2(1.5f / lineThick, 1f);
                    break;
                }
                case FoldLineType.Diagonal45:
                {
                    float cx = _gridOrigin.x - _cellSize / 2f + totalLen / 2f;
                    float cy = _gridOrigin.y + _cellSize / 2f - totalLen / 2f;
                    go.transform.position = new Vector3(cx, cy, -0.5f);
                    go.transform.localScale = new Vector3(totalLen * 1.4f, lineThick, 1f);
                    go.transform.rotation = Quaternion.Euler(0, 0, 45f);
                    sr.sprite = _foldLineHSprite;
                    sr.color = new Color(1f, 0.8f, 0.2f, 0.7f);
                    col.size = new Vector2(1f, 1.5f / lineThick);
                    break;
                }
            }
            return go;
        }

        Vector3 CellToWorld(int r, int c)
        {
            return new Vector3(
                _gridOrigin.x + c * _cellSize,
                _gridOrigin.y - r * _cellSize,
                0f
            );
        }

        void Update()
        {
            if (!_isActive) return;

            if (_timerActive)
            {
                _timeRemaining -= Time.deltaTime;
                _ui.UpdateTimer(_timeRemaining);
                if (_timeRemaining <= 0f)
                {
                    _timerActive = false;
                    HandleMovesExhausted();
                    return;
                }
            }

            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            var cam = Camera.main;
            if (cam == null) return;
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

            // Check fold line tap first
            foreach (var (data, go) in _foldLineObjects)
            {
                var col = go.GetComponent<BoxCollider2D>();
                if (col.OverlapPoint(worldPos2D))
                {
                    SelectFoldLine(data, go);
                    return;
                }
            }

            // If fold line selected, tap on paper area to fold
            if (_selectedFoldLine.HasValue)
            {
                bool tapAboveLine = IsTapAboveFoldLine(worldPos2D, _selectedFoldLine.Value);
                ExecuteFold(_selectedFoldLine.Value, tapAboveLine);
                DeselectFoldLine();
            }
        }

        bool IsTapAboveFoldLine(Vector2 tapWorld, FoldLineData fl)
        {
            switch (fl.type)
            {
                case FoldLineType.Horizontal:
                {
                    float lineY = _gridOrigin.y - (fl.index - 0.5f) * _cellSize;
                    return tapWorld.y > lineY;
                }
                case FoldLineType.Vertical:
                {
                    float lineX = _gridOrigin.x + (fl.index - 0.5f) * _cellSize;
                    return tapWorld.x < lineX; // "above" = left
                }
                case FoldLineType.Diagonal45:
                {
                    float cx = _gridOrigin.x - _cellSize / 2f + _gridSize * _cellSize / 2f;
                    float cy = _gridOrigin.y + _cellSize / 2f - _gridSize * _cellSize / 2f;
                    // above diagonal: tapWorld.y - cy > tapWorld.x - cx
                    return (tapWorld.y - cy) > (tapWorld.x - cx);
                }
                default:
                    return true;
            }
        }

        void SelectFoldLine(FoldLineData data, GameObject go)
        {
            // Deselect previous
            DeselectFoldLine();
            _selectedFoldLine = data;
            go.GetComponent<SpriteRenderer>().color = new Color(1f, 0.9f, 0.2f, 1f);
        }

        void DeselectFoldLine()
        {
            if (!_selectedFoldLine.HasValue) return;
            foreach (var (d, go) in _foldLineObjects)
            {
                if (d.type == _selectedFoldLine.Value.type && d.index == _selectedFoldLine.Value.index)
                {
                    var c = d.type == FoldLineType.Diagonal45 ? new Color(1f, 0.8f, 0.2f, 0.7f) : new Color(0.4f, 0.8f, 1f, 0.7f);
                    go.GetComponent<SpriteRenderer>().color = c;
                    break;
                }
            }
            _selectedFoldLine = null;
        }

        void ExecuteFold(FoldLineData fl, bool foldTopOrLeft)
        {
            if (!_isActive || _movesLeft <= 0) return;

            bool[,] prev = CopyGrid(_grid);
            _history.Push(prev);

            bool[,] newGrid = new bool[_gridSize, _gridSize];

            switch (fl.type)
            {
                case FoldLineType.Horizontal:
                    ApplyHorizontalFold(fl.index, foldTopOrLeft, newGrid);
                    break;
                case FoldLineType.Vertical:
                    ApplyVerticalFold(fl.index, foldTopOrLeft, newGrid);
                    break;
                case FoldLineType.Diagonal45:
                    ApplyDiagonalFold(foldTopOrLeft, newGrid);
                    break;
            }

            _grid = newGrid;
            _movesLeft--;
            _ui.UpdateMoves(_movesLeft, _maxFolds);
            RefreshCellDisplay();

            if (CheckGoal())
            {
                _isActive = false;
                _timerActive = false;
                int movesUsed = _maxFolds - _movesLeft;
                StartCoroutine(ClearAnimation(() =>
                    _gameManager.OnFoldResult(true, movesUsed, _maxFolds, _undoUsed, _minFolds)));
            }
            else if (_movesLeft <= 0)
            {
                HandleMovesExhausted();
            }
        }

        void ApplyHorizontalFold(int splitRow, bool foldTopDown, bool[,] newGrid)
        {
            // Keep bottom portion, fold top down (or vice versa)
            if (foldTopDown)
            {
                // Keep rows >= splitRow, fold rows < splitRow onto >= splitRow
                for (int r = splitRow; r < _gridSize; r++)
                    for (int c = 0; c < _gridSize; c++)
                        newGrid[r, c] = _grid[r, c];
                for (int r = 0; r < splitRow; r++)
                {
                    int mirrorR = 2 * splitRow - r - 1;
                    if (mirrorR < _gridSize)
                    {
                        for (int c = 0; c < _gridSize; c++)
                        {
                            if (_grid[r, c])
                                newGrid[mirrorR, c] = _hasOverlapRule ? (newGrid[mirrorR, c] ^ true) : true;
                        }
                    }
                }
            }
            else
            {
                // Keep rows < splitRow, fold rows >= splitRow up
                for (int r = 0; r < splitRow; r++)
                    for (int c = 0; c < _gridSize; c++)
                        newGrid[r, c] = _grid[r, c];
                for (int r = splitRow; r < _gridSize; r++)
                {
                    int mirrorR = 2 * splitRow - r - 1;
                    if (mirrorR >= 0)
                    {
                        for (int c = 0; c < _gridSize; c++)
                        {
                            if (_grid[r, c])
                                newGrid[mirrorR, c] = _hasOverlapRule ? (newGrid[mirrorR, c] ^ true) : true;
                        }
                    }
                }
            }
        }

        void ApplyVerticalFold(int splitCol, bool foldLeftRight, bool[,] newGrid)
        {
            if (foldLeftRight)
            {
                // Keep cols >= splitCol, fold left part rightward
                for (int r = 0; r < _gridSize; r++)
                    for (int c = splitCol; c < _gridSize; c++)
                        newGrid[r, c] = _grid[r, c];
                for (int r = 0; r < _gridSize; r++)
                    for (int c = 0; c < splitCol; c++)
                    {
                        int mirrorC = 2 * splitCol - c - 1;
                        if (mirrorC < _gridSize && _grid[r, c])
                            newGrid[r, mirrorC] = _hasOverlapRule ? (newGrid[r, mirrorC] ^ true) : true;
                    }
            }
            else
            {
                // Keep cols < splitCol, fold right part leftward
                for (int r = 0; r < _gridSize; r++)
                    for (int c = 0; c < splitCol; c++)
                        newGrid[r, c] = _grid[r, c];
                for (int r = 0; r < _gridSize; r++)
                    for (int c = splitCol; c < _gridSize; c++)
                    {
                        int mirrorC = 2 * splitCol - c - 1;
                        if (mirrorC >= 0 && _grid[r, c])
                            newGrid[r, mirrorC] = _hasOverlapRule ? (newGrid[r, mirrorC] ^ true) : true;
                    }
            }
        }

        void ApplyDiagonalFold(bool foldUpperLeft, bool[,] newGrid)
        {
            // Diagonal 45: reflect along main diagonal (r == c)
            if (foldUpperLeft)
            {
                // Keep lower-right (r >= c), fold upper-left onto it
                for (int r = 0; r < _gridSize; r++)
                    for (int c = 0; c < _gridSize; c++)
                        if (r >= c) newGrid[r, c] = _grid[r, c];

                for (int r = 0; r < _gridSize; r++)
                    for (int c = r + 1; c < _gridSize; c++)
                        if (_grid[r, c] && c < _gridSize && r < _gridSize)
                            newGrid[c, r] = _hasOverlapRule ? (newGrid[c, r] ^ true) : true;
            }
            else
            {
                // Keep upper-left (r <= c), fold lower-right onto it
                for (int r = 0; r < _gridSize; r++)
                    for (int c = 0; c < _gridSize; c++)
                        if (r <= c) newGrid[r, c] = _grid[r, c];

                for (int r = 1; r < _gridSize; r++)
                    for (int c = 0; c < r; c++)
                        if (_grid[r, c])
                            newGrid[c, r] = _hasOverlapRule ? (newGrid[c, r] ^ true) : true;
            }
        }

        void RefreshCellDisplay()
        {
            int idx = 0;
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    if (idx < _cellObjects.Count)
                        _cellObjects[idx].SetActive(_grid[r, c]);
                    idx++;
                }
            }
        }

        bool CheckGoal()
        {
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    if (_grid[r, c] != _targetGrid[r, c]) return false;
            return true;
        }

        public void Undo()
        {
            if (!_isActive || _undoLeft <= 0 || _history.Count == 0) return;
            _grid = _history.Pop();
            _undoLeft--;
            _undoUsed++;
            _movesLeft = Mathf.Min(_movesLeft + 1, _maxFolds);
            _ui.UpdateMoves(_movesLeft, _maxFolds);
            _ui.UpdateUndo(_undoLeft);
            RefreshCellDisplay();
        }

        public void ResetStage()
        {
            _isActive = true;
            _grid = CopyGrid(_initialGrid);
            _movesLeft = _maxFolds;
            _undoLeft = 3;
            _undoUsed = 0;
            _history.Clear();
            _selectedFoldLine = null;
            DeselectAllFoldLines();
            RefreshCellDisplay();
            _ui.UpdateMoves(_movesLeft, _maxFolds);
            _ui.UpdateUndo(_undoLeft);
            if (_timeLimit > 0) { _timeRemaining = _timeLimit; _timerActive = true; }
        }

        void DeselectAllFoldLines()
        {
            foreach (var (d, go) in _foldLineObjects)
            {
                var c = d.type == FoldLineType.Diagonal45 ? new Color(1f, 0.8f, 0.2f, 0.7f) : new Color(0.4f, 0.8f, 1f, 0.7f);
                go.GetComponent<SpriteRenderer>().color = c;
            }
        }

        void HandleMovesExhausted()
        {
            _isActive = false;
            StartCoroutine(FailShake());
            _gameManager.OnMoveFailed();
            _ui.ShowRetryMessage();
        }

        IEnumerator ClearAnimation(System.Action onComplete)
        {
            int idx = 0;
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    if (idx < _cellObjects.Count && _cellObjects[idx] != null && _cellObjects[idx].activeSelf)
                        StartCoroutine(PulseScale(_cellObjects[idx].transform));
                    idx++;
                    yield return new WaitForSeconds(0.03f);
                }
            }
            yield return new WaitForSeconds(0.4f);
            onComplete?.Invoke();
        }

        IEnumerator PulseScale(Transform t)
        {
            if (t == null) yield break;
            Vector3 orig = t.localScale;
            float elapsed = 0f;
            float dur = 0.2f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float s = elapsed < dur * 0.5f
                    ? Mathf.Lerp(1f, 1.3f, elapsed / (dur * 0.5f))
                    : Mathf.Lerp(1.3f, 1f, (elapsed - dur * 0.5f) / (dur * 0.5f));
                t.localScale = orig * s;
                yield return null;
            }
            t.localScale = orig;
        }

        IEnumerator FailShake()
        {
            var shakeCam = Camera.main;
            if (shakeCam == null) yield break;
            Vector3 origCam = shakeCam.transform.position;
            float duration = 0.3f;
            float elapsed = 0f;
            float amp = 0.2f;

            // Red flash
            foreach (var go in _cellObjects)
            {
                if (go.activeSelf)
                    go.GetComponent<SpriteRenderer>().color = new Color(1f, 0.3f, 0.3f, 1f);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float ox = Mathf.Sin(elapsed * 60f) * amp * (1f - elapsed / duration);
                if (shakeCam == null) yield break;
                shakeCam.transform.position = origCam + new Vector3(ox, 0, 0);
                yield return null;
            }
            if (shakeCam != null) shakeCam.transform.position = origCam;

            yield return new WaitForSeconds(0.1f);
            foreach (var go in _cellObjects)
            {
                if (go.activeSelf)
                    go.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        bool[,] CopyGrid(bool[,] src)
        {
            int sz = src.GetLength(0);
            bool[,] dst = new bool[sz, sz];
            for (int r = 0; r < sz; r++)
                for (int c = 0; c < sz; c++)
                    dst[r, c] = src[r, c];
            return dst;
        }

        void ClearAll()
        {
            _isActive = false;
            _timerActive = false;
            _selectedFoldLine = null;

            foreach (var go in _cellObjects) if (go) Destroy(go);
            foreach (var go in _targetObjects) if (go) Destroy(go);
            foreach (var (_, go) in _foldLineObjects) if (go) Destroy(go);

            _cellObjects.Clear();
            _targetObjects.Clear();
            _foldLineObjects.Clear();
            _history.Clear();
        }

        void OnDestroy()
        {
            ClearAll();
        }
    }
}
