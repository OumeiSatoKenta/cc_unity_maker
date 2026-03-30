using UnityEngine;
using System.Collections.Generic;

namespace Game013_SymmetryDraw
{
    /// <summary>
    /// ステージのお手本パターンを管理する。
    /// 各パターンはグリッド上の塗りつぶしセルの座標リスト。
    /// 左半分のみ定義し、右半分は対称に自動生成される。
    /// </summary>
    public class StageData : MonoBehaviour
    {
        // グリッドサイズ（偶数幅を想定: 左半分 + 右半分）
        public const int GridWidth = 10;
        public const int GridHeight = 10;

        /// <summary>
        /// ステージインデックスに対応するお手本パターン（左半分の座標リスト）を返す。
        /// </summary>
        public List<Vector2Int> GetPattern(int stageIndex)
        {
            switch (stageIndex)
            {
                case 0: return GetHeartPattern();
                case 1: return GetArrowPattern();
                case 2: return GetDiamondPattern();
                default: return GetHeartPattern();
            }
        }

        /// <summary>
        /// ハート型パターン（左半分）
        /// </summary>
        private List<Vector2Int> GetHeartPattern()
        {
            return new List<Vector2Int>
            {
                // ハートの左半分（x: 0-4, y: 0-9）
                new Vector2Int(1, 7), new Vector2Int(2, 7), new Vector2Int(3, 7),
                new Vector2Int(0, 6), new Vector2Int(1, 6), new Vector2Int(2, 6), new Vector2Int(3, 6), new Vector2Int(4, 6),
                new Vector2Int(0, 5), new Vector2Int(1, 5), new Vector2Int(2, 5), new Vector2Int(3, 5), new Vector2Int(4, 5),
                new Vector2Int(0, 4), new Vector2Int(1, 4), new Vector2Int(2, 4), new Vector2Int(3, 4), new Vector2Int(4, 4),
                new Vector2Int(1, 3), new Vector2Int(2, 3), new Vector2Int(3, 3), new Vector2Int(4, 3),
                new Vector2Int(2, 2), new Vector2Int(3, 2), new Vector2Int(4, 2),
                new Vector2Int(3, 1), new Vector2Int(4, 1),
                new Vector2Int(4, 0),
            };
        }

        /// <summary>
        /// 矢印型パターン（左半分）
        /// </summary>
        private List<Vector2Int> GetArrowPattern()
        {
            return new List<Vector2Int>
            {
                new Vector2Int(4, 9),
                new Vector2Int(3, 8), new Vector2Int(4, 8),
                new Vector2Int(2, 7), new Vector2Int(4, 7),
                new Vector2Int(1, 6), new Vector2Int(4, 6),
                new Vector2Int(0, 5), new Vector2Int(4, 5),
                new Vector2Int(3, 4), new Vector2Int(4, 4),
                new Vector2Int(3, 3), new Vector2Int(4, 3),
                new Vector2Int(3, 2), new Vector2Int(4, 2),
                new Vector2Int(3, 1), new Vector2Int(4, 1),
                new Vector2Int(3, 0), new Vector2Int(4, 0),
            };
        }

        /// <summary>
        /// ダイヤ型パターン（左半分）
        /// </summary>
        private List<Vector2Int> GetDiamondPattern()
        {
            return new List<Vector2Int>
            {
                new Vector2Int(4, 9),
                new Vector2Int(3, 8), new Vector2Int(4, 8),
                new Vector2Int(2, 7), new Vector2Int(3, 7), new Vector2Int(4, 7),
                new Vector2Int(1, 6), new Vector2Int(2, 6), new Vector2Int(3, 6), new Vector2Int(4, 6),
                new Vector2Int(0, 5), new Vector2Int(1, 5), new Vector2Int(2, 5), new Vector2Int(3, 5), new Vector2Int(4, 5),
                new Vector2Int(1, 4), new Vector2Int(2, 4), new Vector2Int(3, 4), new Vector2Int(4, 4),
                new Vector2Int(2, 3), new Vector2Int(3, 3), new Vector2Int(4, 3),
                new Vector2Int(3, 2), new Vector2Int(4, 2),
                new Vector2Int(4, 1),
            };
        }

        /// <summary>
        /// 左半分の座標リストから、右半分にミラーした座標リストを生成する。
        /// </summary>
        public static List<Vector2Int> MirrorPattern(List<Vector2Int> leftPattern)
        {
            var full = new List<Vector2Int>(leftPattern);
            int halfWidth = GridWidth / 2;
            foreach (var pos in leftPattern)
            {
                int mirrorX = GridWidth - 1 - pos.x;
                var mirrorPos = new Vector2Int(mirrorX, pos.y);
                if (!full.Contains(mirrorPos))
                {
                    full.Add(mirrorPos);
                }
            }
            return full;
        }
    }
}
