using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game005v2_PipeConnect
{
    /// <summary>
    /// グリッド管理・入力処理・水流BFS・5ステージレイアウト生成
    /// </summary>
    public class PipeManager : MonoBehaviour
    {
        [SerializeField] private GameObject _pipeCellPrefab;
        [SerializeField] private Sprite _spriteStraight;
        [SerializeField] private Sprite _spriteElbow;
        [SerializeField] private Sprite _spriteTJunction;
        [SerializeField] private Sprite _spriteSource;
        [SerializeField] private Sprite _spriteExit;
        [SerializeField] private Sprite _spriteLocked;
        [SerializeField] private Sprite _spriteValveOpen;
        [SerializeField] private Sprite _spriteValveClosed;

        public event Action<bool, int> OnFlowComplete; // (allConnected, pathLength)
        public event Action OnPipeTapped;

        private PipeCell[,] _grid;
        private int _gridSize;
        private Vector2Int _source;
        private List<Vector2Int> _exits = new List<Vector2Int>();
        private bool _isActive;
        private bool _flowRunning;
        private int _stageIndex;

        // ステージ別レイアウトデータ
        private struct CellData
        {
            public PipeType type;
            public int rot;
            public bool locked;
        }

        public void SetActive(bool active) { _isActive = active; }

        public void SetupStage(int stageIndex)
        {
            _stageIndex = stageIndex;
            _flowRunning = false;
            ClearGrid();
            GenerateStage(stageIndex);
            _isActive = true;
        }

        private void ClearGrid()
        {
            if (_grid == null) return;
            foreach (var cell in _grid)
            {
                if (cell != null) Destroy(cell.gameObject);
            }
            _grid = null;
            _exits.Clear();
        }

        private void GenerateStage(int stageIndex)
        {
            // ステージ定義（固定レイアウト）
            switch (stageIndex)
            {
                case 0: GenerateStage1(); break;
                case 1: GenerateStage2(); break;
                case 2: GenerateStage3(); break;
                case 3: GenerateStage4(); break;
                case 4: GenerateStage5(); break;
                default: GenerateStage1(); break;
            }
            SpawnGrid();
        }

        // Stage 1: 4×4, 直線・L字のみ, 出口1
        // 水源(0,1)→右に進む→出口(3,1)
        private CellData[,] _layoutData;

        private void GenerateStage1()
        {
            _gridSize = 4;
            _source = new Vector2Int(0, 1);
            _exits = new List<Vector2Int> { new Vector2Int(3, 1) };
            _layoutData = new CellData[4, 4];

            // row1: 水源→直線→直線→出口
            Set(0, 1, PipeType.Source, 0);
            Set(1, 1, PipeType.Straight, 1); // 左右
            Set(2, 1, PipeType.Straight, 1); // 左右
            Set(3, 1, PipeType.Exit, 0);
            // 残りはElbowで埋める（接続に使わないがビジュアル的に）
            Set(0, 0, PipeType.Elbow, 1); Set(1, 0, PipeType.Elbow, 2);
            Set(2, 0, PipeType.Elbow, 3); Set(3, 0, PipeType.Elbow, 0);
            Set(0, 2, PipeType.Elbow, 0); Set(1, 2, PipeType.Elbow, 1);
            Set(2, 2, PipeType.Elbow, 2); Set(3, 2, PipeType.Elbow, 3);
            Set(0, 3, PipeType.Straight, 0); Set(1, 3, PipeType.Straight, 0);
            Set(2, 3, PipeType.Straight, 1); Set(3, 3, PipeType.Elbow, 2);
        }

        // Stage 2: 5×5, T字追加, 出口1
        private void GenerateStage2()
        {
            _gridSize = 5;
            _source = new Vector2Int(0, 2);
            _exits = new List<Vector2Int> { new Vector2Int(4, 2) };
            _layoutData = new CellData[5, 5];

            Set(0, 2, PipeType.Source, 0);
            Set(1, 2, PipeType.Elbow, 0);  // 上右 → 上へ曲がる
            Set(1, 1, PipeType.Straight, 1); // 左右
            Set(2, 1, PipeType.TJunction, 2); // 右下左
            Set(2, 2, PipeType.Straight, 0); // 上下
            Set(2, 3, PipeType.Elbow, 3);   // 左下
            Set(3, 3, PipeType.Straight, 1);
            Set(3, 2, PipeType.Straight, 0);
            Set(3, 1, PipeType.Elbow, 1);   // 上右
            Set(4, 1, PipeType.Elbow, 2);   // 右下
            Set(4, 2, PipeType.Exit, 0);

            // 周辺の飾り
            Set(0, 1, PipeType.Elbow, 1); Set(0, 3, PipeType.Elbow, 0);
            Set(1, 0, PipeType.Straight, 1); Set(2, 0, PipeType.Straight, 1);
            Set(3, 0, PipeType.Straight, 1); Set(4, 0, PipeType.Elbow, 2);
            Set(0, 4, PipeType.Straight, 1); Set(1, 4, PipeType.Elbow, 1);
            Set(4, 4, PipeType.Elbow, 3); Set(3, 4, PipeType.Straight, 1);
            Set(2, 4, PipeType.Straight, 1); Set(1, 3, PipeType.Straight, 0);
            Set(4, 3, PipeType.Straight, 0);
        }

        // Stage 3: 5×5, 出口2箇所
        private void GenerateStage3()
        {
            _gridSize = 5;
            _source = new Vector2Int(0, 2);
            _exits = new List<Vector2Int> { new Vector2Int(4, 1), new Vector2Int(4, 3) };
            _layoutData = new CellData[5, 5];

            Set(0, 2, PipeType.Source, 0);
            Set(1, 2, PipeType.TJunction, 0); // 上右下
            Set(1, 1, PipeType.Elbow, 1);    // 上右
            Set(2, 1, PipeType.Straight, 1); // 左右
            Set(3, 1, PipeType.Straight, 1);
            Set(4, 1, PipeType.Exit, 0);
            Set(1, 3, PipeType.Elbow, 0);    // 右下... wait 上右はElbow rot=0
            // 修正: Elbow rot0=上右, rot1=右下, rot2=下左, rot3=左上
            Set(1, 3, PipeType.Elbow, 1);    // 右下
            Set(2, 3, PipeType.Straight, 1);
            Set(3, 3, PipeType.Straight, 1);
            Set(4, 3, PipeType.Exit, 0);

            // 残り
            Set(0, 1, PipeType.Straight, 0); Set(0, 3, PipeType.Straight, 0);
            Set(0, 0, PipeType.Elbow, 1); Set(1, 0, PipeType.Straight, 1);
            Set(2, 0, PipeType.Straight, 1); Set(3, 0, PipeType.Straight, 1);
            Set(4, 0, PipeType.Elbow, 2);
            Set(0, 4, PipeType.Elbow, 0); Set(1, 4, PipeType.Straight, 1);
            Set(2, 4, PipeType.Straight, 1); Set(3, 4, PipeType.Straight, 1);
            Set(4, 4, PipeType.Elbow, 3);
            Set(2, 2, PipeType.Straight, 0); Set(3, 2, PipeType.Straight, 0);
            Set(4, 2, PipeType.Straight, 0);
        }

        // Stage 4: 6×6, ロックパイプ追加
        private void GenerateStage4()
        {
            _gridSize = 6;
            _source = new Vector2Int(0, 3);
            _exits = new List<Vector2Int> { new Vector2Int(5, 3) };
            _layoutData = new CellData[6, 6];

            Set(0, 3, PipeType.Source, 0);
            Set(1, 3, PipeType.Elbow, 0);    // 上右
            Set(1, 2, PipeType.Straight, 1); // 左右
            Set(2, 2, PipeType.Straight, 1);
            Set(3, 2, PipeType.Locked, 1, true); // ロック・左右固定
            Set(4, 2, PipeType.Elbow, 2);    // 下左... rot2=下左
            Set(4, 3, PipeType.Straight, 1); // 左右
            Set(5, 3, PipeType.Exit, 0);

            // 追加のロックパイプ（経路外、障害）
            Set(2, 3, PipeType.Locked, 0, true); // 上下固定
            Set(3, 4, PipeType.Locked, 1, true); // 左右固定

            // 残り
            Set(0, 2, PipeType.Straight, 0); Set(0, 1, PipeType.Elbow, 1);
            Set(1, 1, PipeType.Straight, 1); Set(2, 1, PipeType.Straight, 1);
            Set(3, 1, PipeType.Straight, 1); Set(4, 1, PipeType.Elbow, 2);
            Set(5, 1, PipeType.Straight, 0); Set(5, 2, PipeType.Straight, 0);
            Set(0, 4, PipeType.Straight, 0); Set(0, 5, PipeType.Elbow, 0);
            Set(1, 5, PipeType.Straight, 1); Set(2, 5, PipeType.Straight, 1);
            Set(3, 5, PipeType.Straight, 1); Set(4, 5, PipeType.Straight, 1);
            Set(5, 5, PipeType.Elbow, 3); Set(5, 4, PipeType.Straight, 0);
            Set(1, 4, PipeType.Elbow, 3); Set(2, 4, PipeType.Straight, 1);
            Set(4, 4, PipeType.Elbow, 1); Set(3, 3, PipeType.Elbow, 2);
            Set(0, 0, PipeType.Elbow, 1); Set(1, 0, PipeType.Straight, 1);
            Set(2, 0, PipeType.Straight, 1); Set(3, 0, PipeType.Straight, 1);
            Set(4, 0, PipeType.Straight, 1); Set(5, 0, PipeType.Elbow, 2);
        }

        // Stage 5: 6×6, バルブ追加, 出口2
        private void GenerateStage5()
        {
            _gridSize = 6;
            _source = new Vector2Int(0, 3);
            _exits = new List<Vector2Int> { new Vector2Int(5, 2), new Vector2Int(5, 4) };
            _layoutData = new CellData[6, 6];

            Set(0, 3, PipeType.Source, 0);
            Set(1, 3, PipeType.TJunction, 0);  // 上右下
            Set(1, 2, PipeType.ValveClosed, 0); // バルブ(上下)→開けると上へ
            Set(1, 1, PipeType.Elbow, 1);        // 上右
            Set(2, 1, PipeType.Straight, 1);
            Set(3, 1, PipeType.Straight, 1);
            Set(4, 1, PipeType.Elbow, 2);        // 右下
            Set(4, 2, PipeType.Straight, 0);
            Set(5, 2, PipeType.Exit, 0);

            Set(1, 4, PipeType.ValveClosed, 0); // バルブ(上下)→開けると下へ
            Set(1, 5, PipeType.Elbow, 0);        // 上右
            Set(2, 5, PipeType.Straight, 1);
            Set(3, 5, PipeType.Straight, 1);
            Set(4, 5, PipeType.Elbow, 3);        // 左上
            Set(4, 4, PipeType.Straight, 0);
            Set(5, 4, PipeType.Exit, 0);

            // 残り
            Set(2, 3, PipeType.Straight, 0); Set(3, 3, PipeType.Elbow, 1);
            Set(3, 2, PipeType.Straight, 1); Set(2, 2, PipeType.Straight, 0);
            Set(3, 4, PipeType.Straight, 1); Set(2, 4, PipeType.Straight, 0);
            Set(0, 2, PipeType.Elbow, 1); Set(0, 4, PipeType.Elbow, 0);
            Set(0, 1, PipeType.Straight, 1); Set(0, 5, PipeType.Straight, 1);
            Set(0, 0, PipeType.Elbow, 1); Set(5, 0, PipeType.Elbow, 2);
            Set(5, 1, PipeType.Straight, 0); Set(5, 3, PipeType.Straight, 0);
            Set(5, 5, PipeType.Elbow, 3);
            Set(1, 0, PipeType.Straight, 1); Set(2, 0, PipeType.Straight, 1);
            Set(3, 0, PipeType.Straight, 1); Set(4, 0, PipeType.Straight, 1);
            Set(4, 3, PipeType.Straight, 1);
        }

        private void Set(int x, int y, PipeType t, int rot, bool locked = false)
        {
            if (_layoutData == null || x < 0 || y < 0 || x >= _gridSize || y >= _gridSize) return;
            _layoutData[x, y] = new CellData { type = t, rot = rot, locked = locked };
        }

        private void SpawnGrid()
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 3.0f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            float cellSize = Mathf.Min(availableHeight / _gridSize, camWidth * 2f / _gridSize, 1.4f);

            float totalW = cellSize * _gridSize;
            float totalH = cellSize * _gridSize;
            float startX = -totalW / 2f + cellSize / 2f;
            float startY = (camSize - topMargin) - cellSize / 2f;

            _grid = new PipeCell[_gridSize, _gridSize];

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    var data = _layoutData[x, y];
                    if (data.type == PipeType.Empty) continue;

                    var pos = new Vector3(startX + x * cellSize, startY - y * cellSize, 0);
                    var go = Instantiate(_pipeCellPrefab, pos, Quaternion.identity, transform);
                    go.transform.localScale = Vector3.one * cellSize;

                    var cell = go.GetComponent<PipeCell>();
                    cell.SpriteValveOpen = _spriteValveOpen;
                    cell.SpriteValveClosed = _spriteValveClosed;
                    cell.Initialize(data.type, data.rot, data.locked);

                    // スプライト設定
                    var sr = go.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null) sr.sprite = GetSprite(data.type);

                    _grid[x, y] = cell;
                }
            }
        }

        private Sprite GetSprite(PipeType t)
        {
            switch (t)
            {
                case PipeType.Straight: return _spriteStraight;
                case PipeType.Elbow: return _spriteElbow;
                case PipeType.TJunction: return _spriteTJunction;
                case PipeType.Source: return _spriteSource;
                case PipeType.Exit: return _spriteExit;
                case PipeType.Locked: return _spriteLocked;
                case PipeType.ValveOpen: return _spriteValveOpen;
                case PipeType.ValveClosed: return _spriteValveClosed;
                default: return null;
            }
        }

        private void Update()
        {
            if (!_isActive || _grid == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            var cell = hit.GetComponentInParent<PipeCell>();
            if (cell == null) return;

            if (cell.Type == PipeType.Locked)
            {
                cell.ShakeLocked();
                return;
            }
            if (cell.Type == PipeType.ValveOpen || cell.Type == PipeType.ValveClosed)
            {
                cell.ToggleValve();
                // バルブのスプライト更新
                var sr = cell.GetSpriteRenderer();
                if (sr != null) sr.sprite = GetSprite(cell.Type);
            }
            else
            {
                cell.Rotate();
            }
            OnPipeTapped?.Invoke();
        }

        public void StartWaterFlow()
        {
            if (_grid == null || _flowRunning) return;
            StartCoroutine(FlowCoroutine());
        }

        private IEnumerator FlowCoroutine()
        {
            _flowRunning = true;
            // まず全セルをリセット
            ResetFlowColors();

            // BFSで接続チェック
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(_source);
            visited.Add(_source);

            int pathLength = 0;
            var reachedExits = new HashSet<Vector2Int>();

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                var curCell = _grid[cur.x, cur.y];
                if (curCell == null) continue;

                pathLength++;
                curCell.SetFlowColor(true);
                yield return new WaitForSeconds(0.05f);

                if (_exits.Contains(cur))
                {
                    reachedExits.Add(cur);
                }

                var curConn = curCell.GetConnections();
                // 上下左右探索
                var neighbors = new (Vector2Int pos, int fromDir)[]
                {
                    (new Vector2Int(cur.x, cur.y - 1), 2), // 上 → 隣のdown=2
                    (new Vector2Int(cur.x + 1, cur.y), 3), // 右 → 隣のleft=3
                    (new Vector2Int(cur.x, cur.y + 1), 0), // 下 → 隣のup=0
                    (new Vector2Int(cur.x - 1, cur.y), 1), // 左 → 隣のright=1
                };

                for (int d = 0; d < 4; d++)
                {
                    if (!curConn[d]) continue;
                    var np = neighbors[d].pos;
                    int fromDir = neighbors[d].fromDir;
                    if (np.x < 0 || np.y < 0 || np.x >= _gridSize || np.y >= _gridSize) continue;
                    if (visited.Contains(np)) continue;
                    var nc = _grid[np.x, np.y];
                    if (nc == null) continue;
                    var ncConn = nc.GetConnections();
                    if (!ncConn[fromDir]) continue;
                    visited.Add(np);
                    queue.Enqueue(np);
                }
            }

            bool allConnected = reachedExits.Count == _exits.Count;
            if (!allConnected)
            {
                // 未接続セルをエラーフラッシュ
                for (int x = 0; x < _gridSize; x++)
                    for (int y = 0; y < _gridSize; y++)
                        if (_grid[x, y] != null && !visited.Contains(new Vector2Int(x, y)))
                            _grid[x, y].FlashError();
            }

            _flowRunning = false;
            OnFlowComplete?.Invoke(allConnected, pathLength);
        }

        private void ResetFlowColors()
        {
            if (_grid == null) return;
            foreach (var cell in _grid)
                if (cell != null) cell.SetFlowColor(false);
        }

        public void ResetPipes()
        {
            if (_grid == null) return;
            ResetFlowColors();
            // 初期レイアウトを再生成
            ClearGrid();
            GenerateStage(_stageIndex);
        }

        private void OnDestroy()
        {
            ClearGrid();
        }
    }
}
