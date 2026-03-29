using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game001_BlockFlow
{
    /// <summary>
    /// 盤面の生成・ブロック配置・移動処理・隣接グループ判定・入力処理を担当する。
    /// 入力は BoardManager が一元管理し、正しいブロック1つだけを操作する。
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [SerializeField, Tooltip("盤面の横マス数")]
        private int _boardWidth = 5;

        [SerializeField, Tooltip("盤面の縦マス数")]
        private int _boardHeight = 5;

        [SerializeField, Tooltip("使用する色の数（2〜5）")]
        private int _colorCount = 3;

        [SerializeField, Tooltip("各色のブロック数")]
        private int _blocksPerColor = 3;

        [SerializeField, Tooltip("ブロックのプレハブ")]
        private GameObject _blockPrefab;

        [SerializeField, Tooltip("セルのサイズ（ワールド座標）")]
        private float _cellSize = 1.2f;

        private BlockController[,] _grid;
        private readonly List<BlockController> _allBlocks = new List<BlockController>();
        private BlockFlowGameManager _gameManager;

        // 入力状態
        private BlockController _draggedBlock;
        private Vector2 _swipeStart;

        private void Start()
        {
            _gameManager = GetComponentInParent<BlockFlowGameManager>();
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // クリック開始 — どのブロックをクリックしたか判定
            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var block = hit.GetComponent<BlockController>();
                    if (block != null && _allBlocks.Contains(block))
                    {
                        _draggedBlock = block;
                        _swipeStart = mouse.position.ReadValue();
                    }
                }
            }

            // クリック終了 — スワイプ方向を判定して移動
            if (mouse.leftButton.wasReleasedThisFrame && _draggedBlock != null)
            {
                Vector2 swipeEnd = mouse.position.ReadValue();
                Vector2 swipeDelta = swipeEnd - _swipeStart;

                if (swipeDelta.magnitude >= 30f)
                {
                    Vector2Int direction;
                    if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                    {
                        direction = swipeDelta.x > 0 ? Vector2Int.right : Vector2Int.left;
                    }
                    else
                    {
                        direction = swipeDelta.y > 0 ? Vector2Int.up : Vector2Int.down;
                    }

                    if (TryMoveBlock(_draggedBlock, direction))
                    {
                        if (_gameManager != null) _gameManager.OnBlockMoved();
                    }
                }

                _draggedBlock = null;
            }
        }

        /// <summary>
        /// 盤面を生成してブロックをランダム配置する。
        /// </summary>
        public void GenerateBoard()
        {
            ClearBoard();
            _grid = new BlockController[_boardWidth, _boardHeight];

            // 各色のブロックを配置リストに追加
            var placements = new List<int>();
            for (int c = 0; c < _colorCount; c++)
            {
                for (int i = 0; i < _blocksPerColor; i++)
                {
                    placements.Add(c);
                }
            }

            // シャッフル
            for (int i = placements.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (placements[i], placements[j]) = (placements[j], placements[i]);
            }

            // 空きマスのリストを作成
            var emptyPositions = new List<Vector2Int>();
            for (int x = 0; x < _boardWidth; x++)
            {
                for (int y = 0; y < _boardHeight; y++)
                {
                    emptyPositions.Add(new Vector2Int(x, y));
                }
            }

            // シャッフル
            for (int i = emptyPositions.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (emptyPositions[i], emptyPositions[j]) = (emptyPositions[j], emptyPositions[i]);
            }

            // ブロックを配置
            for (int i = 0; i < placements.Count && i < emptyPositions.Count; i++)
            {
                var pos = emptyPositions[i];
                var block = CreateBlock(placements[i], pos);
                _grid[pos.x, pos.y] = block;
                _allBlocks.Add(block);
            }
        }

        private BlockController CreateBlock(int colorId, Vector2Int gridPos)
        {
            if (_blockPrefab == null)
            {
                Debug.LogError("[BoardManager] blockPrefab が設定されていません");
                return null;
            }

            var obj = Instantiate(_blockPrefab, transform);
            obj.SetActive(true);
            obj.name = $"Block_{colorId}_{gridPos.x}_{gridPos.y}";
            obj.transform.position = GridToWorld(gridPos);

            var block = obj.GetComponent<BlockController>();
            if (block != null)
            {
                block.Initialize(colorId, gridPos);
            }
            return block;
        }

        /// <summary>
        /// ブロックを指定方向に1マス移動させる。移動先が空なら成功。
        /// </summary>
        public bool TryMoveBlock(BlockController block, Vector2Int direction)
        {
            if (block == null) return false;

            Vector2Int newPos = block.GridPosition + direction;
            if (!IsInBounds(newPos)) return false;
            if (_grid[newPos.x, newPos.y] != null) return false;

            // 移動実行
            var oldPos = block.GridPosition;
            _grid[oldPos.x, oldPos.y] = null;
            _grid[newPos.x, newPos.y] = block;
            block.SetGridPosition(newPos);
            block.UpdateWorldPosition(GridToWorld(newPos));

            return true;
        }

        /// <summary>
        /// 全ての色について、同色ブロックが全て隣接しているかチェックする。
        /// </summary>
        public bool CheckAllGrouped()
        {
            var colorGroups = new Dictionary<int, List<Vector2Int>>();
            foreach (var block in _allBlocks)
            {
                if (block == null) continue;
                if (!colorGroups.ContainsKey(block.ColorId))
                {
                    colorGroups[block.ColorId] = new List<Vector2Int>();
                }
                colorGroups[block.ColorId].Add(block.GridPosition);
            }

            foreach (var group in colorGroups.Values)
            {
                if (!IsGroupConnected(group)) return false;
            }

            return true;
        }

        private bool IsGroupConnected(List<Vector2Int> positions)
        {
            if (positions.Count <= 1) return true;

            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            var posSet = new HashSet<Vector2Int>(positions);

            queue.Enqueue(positions[0]);
            visited.Add(positions[0]);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                Vector2Int[] neighbors =
                {
                    current + Vector2Int.up,
                    current + Vector2Int.down,
                    current + Vector2Int.left,
                    current + Vector2Int.right
                };

                foreach (var neighbor in neighbors)
                {
                    if (posSet.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return visited.Count == positions.Count;
        }

        private bool IsInBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _boardWidth && pos.y >= 0 && pos.y < _boardHeight;
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offsetX = (_boardWidth - 1) * _cellSize * 0.5f;
            float offsetY = (_boardHeight - 1) * _cellSize * 0.5f;
            return new Vector3(gridPos.x * _cellSize - offsetX, gridPos.y * _cellSize - offsetY, 0f);
        }

        private void ClearBoard()
        {
            foreach (var block in _allBlocks)
            {
                if (block != null) Destroy(block.gameObject);
            }
            _allBlocks.Clear();
            _grid = null;
        }
    }
}
