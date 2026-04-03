using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game001v2_BlockFlow
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private BlockFlowGameManager _gameManager;
        [SerializeField, Tooltip("ブロックスプライト(赤)")] private Sprite _blockRedSprite;
        [SerializeField, Tooltip("ブロックスプライト(青)")] private Sprite _blockBlueSprite;
        [SerializeField, Tooltip("ブロックスプライト(緑)")] private Sprite _blockGreenSprite;
        [SerializeField, Tooltip("ブロックスプライト(黄)")] private Sprite _blockYellowSprite;
        [SerializeField, Tooltip("固定ブロックスプライト")] private Sprite _fixedBlockSprite;
        [SerializeField, Tooltip("ワープタイルスプライト")] private Sprite _warpTileSprite;
        [SerializeField, Tooltip("氷ブロックスプライト")] private Sprite _iceBlockSprite;
        [SerializeField, Tooltip("盤面背景スプライト")] private Sprite _boardBgSprite;

        private const float CellSize = 0.85f;
        private const float MoveAnimSpeed = 0.08f;

        // セルタイプ
        private const int Empty = 0;
        private const int Fixed = -1;
        private const int WarpA = -2;
        private const int WarpB = -3;
        private const int Ice = -4;

        private int _boardSize;
        private int _colorCount;
        private int _moveLimit;
        private int[,] _grid;           // 色ブロック(1〜4), 0=空
        private int[,] _specialGrid;    // 特殊タイル(Fixed/Warp/Ice), 0=なし
        private int[,] _iceHP;          // 氷のHP
        private int[,] _initialGrid;    // リセット用
        private int[,] _initialSpecialGrid;
        private int[,] _initialIceHP;

        private readonly List<GameObject> _blockObjects = new List<GameObject>();
        private readonly List<GameObject> _specialObjects = new List<GameObject>();
        private GameObject _boardBgObj;
        private bool _isActive;
        private bool _isAnimating;
        private Camera _mainCamera;

        // スワイプ入力
        private Vector2 _dragStart;
        private bool _isDragging;
        private Vector2Int _selectedCell;
        private const float SwipeThreshold = 30f;

        // ワープペア管理
        private readonly List<(Vector2Int a, Vector2Int b)> _warpPairs = new List<(Vector2Int, Vector2Int)>();

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        public void SetupStage(int boardSize, int colorCount, int moveLimit,
            int fixedCount, int warpPairCount, int iceCount)
        {
            ClearBoard();
            _boardSize = boardSize;
            _colorCount = colorCount;
            _moveLimit = moveLimit;
            _isActive = true;
            _isAnimating = false;

            _grid = new int[boardSize, boardSize];
            _specialGrid = new int[boardSize, boardSize];
            _iceHP = new int[boardSize, boardSize];

            // 盤面生成
            GenerateBoard(fixedCount, warpPairCount, iceCount);

            // 初期状態を保存（リセット用）
            _initialGrid = (int[,])_grid.Clone();
            _initialSpecialGrid = (int[,])_specialGrid.Clone();
            _initialIceHP = (int[,])_iceHP.Clone();

            RenderBoard();
        }

        private void GenerateBoard(int fixedCount, int warpPairCount, int iceCount)
        {
            // 1. 特殊タイルを先に配置
            var usedCells = new HashSet<Vector2Int>();
            _warpPairs.Clear();

            // 固定ブロック
            for (int i = 0; i < fixedCount; i++)
            {
                var pos = GetRandomEmptyCell(usedCells);
                _specialGrid[pos.x, pos.y] = Fixed;
                usedCells.Add(pos);
            }

            // ワープタイル
            for (int i = 0; i < warpPairCount; i++)
            {
                var a = GetRandomEmptyCell(usedCells);
                usedCells.Add(a);
                var b = GetRandomEmptyCell(usedCells);
                usedCells.Add(b);
                _specialGrid[a.x, a.y] = WarpA;
                _specialGrid[b.x, b.y] = WarpB;
                _warpPairs.Add((a, b));
            }

            // 氷ブロック
            for (int i = 0; i < iceCount; i++)
            {
                var pos = GetRandomEmptyCell(usedCells);
                _specialGrid[pos.x, pos.y] = Ice;
                _iceHP[pos.x, pos.y] = 2;
                usedCells.Add(pos);
            }

            // 2. 色ブロックを配置（各色2個ずつ）
            var availableCells = new List<Vector2Int>();
            for (int x = 0; x < _boardSize; x++)
                for (int y = 0; y < _boardSize; y++)
                    if (_specialGrid[x, y] == 0)
                        availableCells.Add(new Vector2Int(x, y));

            // シャッフル
            for (int i = availableCells.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (availableCells[i], availableCells[j]) = (availableCells[j], availableCells[i]);
            }

            int cellIndex = 0;
            for (int color = 1; color <= _colorCount; color++)
            {
                for (int count = 0; count < 2 && cellIndex < availableCells.Count; count++)
                {
                    var pos = availableCells[cellIndex++];
                    _grid[pos.x, pos.y] = color;
                }
            }
        }

        private Vector2Int GetRandomEmptyCell(HashSet<Vector2Int> used)
        {
            var available = new List<Vector2Int>();
            for (int x = 0; x < _boardSize; x++)
                for (int y = 0; y < _boardSize; y++)
                    if (!used.Contains(new Vector2Int(x, y)))
                        available.Add(new Vector2Int(x, y));

            if (available.Count == 0)
            {
                Debug.LogWarning("[BoardManager] GetRandomEmptyCell: 空きセルがありません");
                return Vector2Int.zero;
            }
            return available[Random.Range(0, available.Count)];
        }

        private void ClearBoard()
        {
            foreach (var obj in _blockObjects) if (obj != null) Destroy(obj);
            _blockObjects.Clear();
            foreach (var obj in _specialObjects) if (obj != null) Destroy(obj);
            _specialObjects.Clear();
            if (_boardBgObj != null) { Destroy(_boardBgObj); _boardBgObj = null; }
            _warpPairs.Clear();
        }

        private void RenderBoard()
        {
            // 既存オブジェクトをクリア
            foreach (var obj in _blockObjects) if (obj != null) Destroy(obj);
            _blockObjects.Clear();
            foreach (var obj in _specialObjects) if (obj != null) Destroy(obj);
            _specialObjects.Clear();
            if (_boardBgObj != null) Destroy(_boardBgObj);

            float offset = (_boardSize - 1) * CellSize * 0.5f;

            // 盤面背景
            _boardBgObj = new GameObject("BoardBg");
            _boardBgObj.transform.SetParent(transform);
            var bgSr = _boardBgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = _boardBgSprite;
            bgSr.sortingOrder = 0;
            float bgScale = _boardSize * CellSize * 1.15f;
            _boardBgObj.transform.localScale = new Vector3(bgScale / 2.56f, bgScale / 2.56f, 1f);
            _boardBgObj.transform.localPosition = Vector3.zero;

            // グリッドセル背景
            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    Vector3 pos = CellToWorld(x, y, offset);

                    // 特殊タイル描画
                    if (_specialGrid[x, y] == Fixed)
                    {
                        var obj = CreateSpriteObj("Fixed", _fixedBlockSprite, pos, 1);
                        _specialObjects.Add(obj);
                    }
                    else if (_specialGrid[x, y] == WarpA || _specialGrid[x, y] == WarpB)
                    {
                        var obj = CreateSpriteObj("Warp", _warpTileSprite, pos, 1);
                        obj.transform.localScale = Vector3.one * (CellSize * 0.9f);
                        _specialObjects.Add(obj);
                    }
                    else if (_specialGrid[x, y] == Ice && _iceHP[x, y] > 0)
                    {
                        var obj = CreateSpriteObj("Ice", _iceBlockSprite, pos, 1);
                        if (_iceHP[x, y] == 1)
                            obj.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
                        _specialObjects.Add(obj);
                    }

                    // 色ブロック描画
                    if (_grid[x, y] > 0)
                    {
                        Sprite sprite = GetColorSprite(_grid[x, y]);
                        var obj = CreateSpriteObj($"Block_{x}_{y}", sprite, pos, 2);
                        obj.transform.localScale = Vector3.one * (CellSize * 0.85f);
                        _blockObjects.Add(obj);
                    }
                }
            }
        }

        private GameObject CreateSpriteObj(string name, Sprite sprite, Vector3 pos, int order)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = pos;
            obj.transform.localScale = Vector3.one * (CellSize * 0.9f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return obj;
        }

        private Sprite GetColorSprite(int color)
        {
            return color switch
            {
                1 => _blockRedSprite,
                2 => _blockBlueSprite,
                3 => _blockGreenSprite,
                4 => _blockYellowSprite,
                _ => _blockRedSprite
            };
        }

        private Vector3 CellToWorld(int x, int y, float offset)
        {
            return new Vector3(x * CellSize - offset, y * CellSize - offset, 0f);
        }

        private Vector2Int WorldToCell(Vector2 worldPos, float offset)
        {
            int x = Mathf.RoundToInt((worldPos.x + offset) / CellSize);
            int y = Mathf.RoundToInt((worldPos.y + offset) / CellSize);
            return new Vector2Int(
                Mathf.Clamp(x, 0, _boardSize - 1),
                Mathf.Clamp(y, 0, _boardSize - 1));
        }

        private void Update()
        {
            if (!_isActive || _isAnimating || _gameManager == null || !_gameManager.IsPlaying) return;
            HandleInput();
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _dragStart = Mouse.current.position.ReadValue();
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(_dragStart);
                float offset = (_boardSize - 1) * CellSize * 0.5f;
                _selectedCell = WorldToCell(worldPos, offset);

                if (_selectedCell.x >= 0 && _selectedCell.x < _boardSize &&
                    _selectedCell.y >= 0 && _selectedCell.y < _boardSize &&
                    _grid[_selectedCell.x, _selectedCell.y] > 0)
                {
                    _isDragging = true;
                }
            }

            if (_isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
                Vector2 dragEnd = Mouse.current.position.ReadValue();
                Vector2 delta = dragEnd - _dragStart;

                if (delta.magnitude > SwipeThreshold)
                {
                    Vector2Int dir;
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        dir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
                    else
                        dir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

                    TryMoveBlock(_selectedCell, dir);
                }
            }

            // タッチ入力（primaryTouchのみ使用）
            var primaryTouch = Touchscreen.current?.primaryTouch;
            if (primaryTouch != null)
            {
                if (primaryTouch.press.wasPressedThisFrame)
                {
                    _dragStart = primaryTouch.position.ReadValue();
                    Vector2 worldPos = _mainCamera.ScreenToWorldPoint(_dragStart);
                    float tOffset = (_boardSize - 1) * CellSize * 0.5f;
                    _selectedCell = WorldToCell(worldPos, tOffset);
                    if (_selectedCell.x >= 0 && _selectedCell.x < _boardSize &&
                        _selectedCell.y >= 0 && _selectedCell.y < _boardSize &&
                        _grid[_selectedCell.x, _selectedCell.y] > 0)
                        _isDragging = true;
                }
                if (_isDragging && primaryTouch.press.wasReleasedThisFrame)
                {
                    _isDragging = false;
                    Vector2 dragEnd = primaryTouch.position.ReadValue();
                    Vector2 delta = dragEnd - _dragStart;
                    if (delta.magnitude > SwipeThreshold)
                    {
                        Vector2Int dir;
                        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                            dir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
                        else
                            dir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
                        TryMoveBlock(_selectedCell, dir);
                    }
                }
            }
        }

        private void TryMoveBlock(Vector2Int from, Vector2Int dir)
        {
            if (_grid[from.x, from.y] <= 0) return;

            int color = _grid[from.x, from.y];
            Vector2Int current = from;
            Vector2Int next = current + dir;

            // スライド: 壁か障害物に当たるまで
            int warpHops = 0;
            int maxWarpHops = _warpPairs.Count * 2 + 1;
            while (IsInBounds(next))
            {
                // 氷ブロック: ぶつかってHPを減らし、ブロックは手前で止まる
                if (IsIceBlocking(next))
                {
                    _iceHP[next.x, next.y]--;
                    if (_iceHP[next.x, next.y] <= 0)
                        _specialGrid[next.x, next.y] = 0;
                    break;
                }

                if (!CanMoveTo(next)) break;

                // ワープチェック（無限ループ防止付き）
                if (_specialGrid[next.x, next.y] == WarpA || _specialGrid[next.x, next.y] == WarpB)
                {
                    if (++warpHops > maxWarpHops) break;
                    var partner = GetWarpPartner(next);
                    if (partner.HasValue)
                    {
                        Vector2Int warpExit = partner.Value + dir;
                        if (IsInBounds(warpExit) && CanMoveTo(warpExit) && !IsIceBlocking(warpExit))
                        {
                            current = partner.Value;
                            next = warpExit;
                            continue;
                        }
                    }
                }

                current = next;
                next = current + dir;
            }

            if (current != from)
            {
                _grid[from.x, from.y] = 0;
                _grid[current.x, current.y] = color;
                StartCoroutine(AnimateMove(from, current, color));
            }
        }

        private bool IsInBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _boardSize && pos.y >= 0 && pos.y < _boardSize;
        }

        private bool CanMoveTo(Vector2Int pos)
        {
            if (_specialGrid[pos.x, pos.y] == Fixed) return false;
            if (_grid[pos.x, pos.y] > 0) return false;
            // 氷ブロックは TryMoveBlock 側で「進入して止まる」特殊処理
            return true;
        }

        private bool IsIceBlocking(Vector2Int pos)
        {
            return _specialGrid[pos.x, pos.y] == Ice && _iceHP[pos.x, pos.y] > 0;
        }

        private Vector2Int? GetWarpPartner(Vector2Int pos)
        {
            foreach (var pair in _warpPairs)
            {
                if (pair.a == pos) return pair.b;
                if (pair.b == pos) return pair.a;
            }
            return null;
        }

        private IEnumerator AnimateMove(Vector2Int from, Vector2Int to, int color)
        {
            _isAnimating = true;
            float offset = (_boardSize - 1) * CellSize * 0.5f;

            // 移動ブロックオブジェクトを見つける
            Vector3 startPos = CellToWorld(from.x, from.y, offset);
            Vector3 endPos = CellToWorld(to.x, to.y, offset);

            GameObject movingBlock = null;
            foreach (var obj in _blockObjects)
            {
                if (obj != null && Vector3.Distance(obj.transform.localPosition, startPos) < 0.1f)
                {
                    movingBlock = obj;
                    break;
                }
            }

            if (movingBlock != null)
            {
                float elapsed = 0f;
                float distance = Vector3.Distance(startPos, endPos);
                float duration = distance / CellSize * MoveAnimSpeed;
                duration = Mathf.Max(duration, 0.1f);

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                    movingBlock.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                    yield return null;
                }
                movingBlock.transform.localPosition = endPos;

                // 到着エフェクト: スケールパルス
                StartCoroutine(ScalePulse(movingBlock.transform, 0.2f, 1.2f));
            }

            if (_gameManager == null) { _isAnimating = false; yield break; }
            _gameManager.OnMoveMade();

            // 盤面を再描画（特殊タイルの状態変化反映）
            RenderBoard();

            _isAnimating = false;

            // クリア判定
            if (_gameManager == null) yield break;
            if (CheckClear())
            {
                // クリア演出: 各色のブロックをフラッシュ
                for (int c = 1; c <= _colorCount; c++)
                    StartCoroutine(ColorFlashEffect(c));
                _gameManager.OnBoardCleared(_colorCount);
            }
        }

        public bool CheckClear()
        {
            // 各色のブロックが全て隣接しているか（BFS）
            for (int color = 1; color <= _colorCount; color++)
            {
                var cells = new List<Vector2Int>();
                for (int x = 0; x < _boardSize; x++)
                    for (int y = 0; y < _boardSize; y++)
                        if (_grid[x, y] == color) cells.Add(new Vector2Int(x, y));

                if (cells.Count == 0) continue;

                // BFSで連結チェック
                var visited = new HashSet<Vector2Int>();
                var queue = new Queue<Vector2Int>();
                queue.Enqueue(cells[0]);
                visited.Add(cells[0]);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                    foreach (var d in dirs)
                    {
                        var neighbor = current + d;
                        if (cells.Contains(neighbor) && !visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                if (visited.Count != cells.Count) return false;
            }
            return true;
        }

        public void ResetBoard()
        {
            if (!_isActive || _isAnimating || _gameManager == null || !_gameManager.IsPlaying) return;
            _grid = (int[,])_initialGrid.Clone();
            _specialGrid = (int[,])_initialSpecialGrid.Clone();
            _iceHP = (int[,])_initialIceHP.Clone();
            RenderBoard();
            _gameManager.OnResetUsed();
        }

        private IEnumerator ScalePulse(Transform target, float duration, float scale)
        {
            if (target == null) yield break;
            Vector3 orig = target.localScale;
            Vector3 peak = orig * scale;
            float elapsed = 0f;
            float half = duration * 0.5f;

            while (elapsed < half && target != null)
            {
                elapsed += Time.deltaTime;
                target.localScale = Vector3.Lerp(orig, peak, elapsed / half);
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < half && target != null)
            {
                elapsed += Time.deltaTime;
                target.localScale = Vector3.Lerp(peak, orig, elapsed / half);
                yield return null;
            }
            if (target != null) target.localScale = orig;
        }

        private IEnumerator ColorFlashEffect(int color)
        {
            float offset = (_boardSize - 1) * CellSize * 0.5f;
            var targets = new List<SpriteRenderer>();

            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    if (_grid[x, y] == color)
                    {
                        Vector3 pos = CellToWorld(x, y, offset);
                        foreach (var obj in _blockObjects)
                        {
                            if (obj != null && Vector3.Distance(obj.transform.localPosition, pos) < 0.1f)
                            {
                                var sr = obj.GetComponent<SpriteRenderer>();
                                if (sr != null) targets.Add(sr);
                            }
                        }
                    }
                }
            }

            // フラッシュ: 白→元色
            foreach (var sr in targets)
                if (sr != null) sr.color = Color.white;

            yield return new WaitForSeconds(0.15f);

            foreach (var sr in targets)
                if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f);
        }

        private void OnDestroy()
        {
            ClearBoard();
        }
    }
}
