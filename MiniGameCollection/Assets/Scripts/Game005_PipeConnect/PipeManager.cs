using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Game005_PipeConnect
{
    /// <summary>
    /// 5x5グリッドのパイプタイルを管理し、クリック入力と接続判定を担当する。
    /// 方向定数: 0=UP, 1=RIGHT, 2=DOWN, 3=LEFT
    /// </summary>
    public class PipeManager : MonoBehaviour
    {
        [SerializeField] private PipeConnectGameManager _gameManager;
        [SerializeField] private PipeTile[] _tiles; // 25 tiles, index = row*5+col

        public UnityEvent OnSolved = new();

        private static readonly int[] DRow = { -1, 0, 1, 0 };
        private static readonly int[] DCol = { 0, 1, 0, -1 };

        // Base openings for each type at rotation 0
        private static readonly int[][] BaseOpenings =
        {
            new int[0],             // 0: empty
            new[] { 0, 2 },         // 1: straight  (U+D)
            new[] { 0, 1 },         // 2: bend      (U+R)
            new[] { 0, 1, 2 },      // 3: T         (U+R+D)
            new[] { 0, 1, 2, 3 },   // 4: cross     (all)
            new[] { 1 },            // 5: source    (R)
            new[] { 3 },            // 6: goal      (L)
        };

        public void LoadLevel(int[,] types, int[,] rotations)
        {
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    int t = types[r, c];
                    int rot = rotations[r, c];
                    bool isFixed = t == 0 || t == 5 || t == 6;
                    _tiles[r * 5 + c].Init(t, rot, r, c, isFixed);
                }
            }
        }

        private void Update()
        {
            if (_gameManager == null || !_gameManager.IsPlaying) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(
                Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            var tile = hit.GetComponent<PipeTile>();
            if (tile == null || tile.IsFixed) return;

            tile.Rotate();
            _gameManager.OnTileMoved();

            if (CheckSolved()) OnSolved?.Invoke();
        }

        private bool CheckSolved()
        {
            // Find source position
            Vector2Int src = Vector2Int.zero;
            bool found = false;
            for (int r = 0; r < 5 && !found; r++)
                for (int c = 0; c < 5 && !found; c++)
                    if (_tiles[r * 5 + c].TileType == 5) { src = new Vector2Int(r, c); found = true; }

            if (!found) return false;

            // BFS from source
            var visited = new bool[5, 5];
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(src);
            visited[src.x, src.y] = true;

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                var tile = _tiles[pos.x * 5 + pos.y];

                if (tile.TileType == 6) return true; // goal reached

                foreach (int dir in GetOpenings(tile.TileType, tile.Rotation))
                {
                    int nr = pos.x + DRow[dir];
                    int nc = pos.y + DCol[dir];
                    if (nr < 0 || nr >= 5 || nc < 0 || nc >= 5) continue;
                    if (visited[nr, nc]) continue;

                    var neighbor = _tiles[nr * 5 + nc];
                    if (neighbor.TileType == 0) continue;

                    int opp = (dir + 2) % 4;
                    foreach (int nd in GetOpenings(neighbor.TileType, neighbor.Rotation))
                    {
                        if (nd == opp)
                        {
                            visited[nr, nc] = true;
                            queue.Enqueue(new Vector2Int(nr, nc));
                            break;
                        }
                    }
                }
            }
            return false;
        }

        private static int[] GetOpenings(int type, int rotation)
        {
            if (type < 0 || type >= BaseOpenings.Length) return new int[0];
            var base_ = BaseOpenings[type];
            var result = new int[base_.Length];
            for (int i = 0; i < base_.Length; i++)
                result[i] = (base_[i] + rotation) % 4;
            return result;
        }
    }
}
