using UnityEngine;

namespace Game005_PipeConnect
{
    /// <summary>
    /// パイプタイルの種別・回転・表示を管理する。入力処理は持たない。
    /// タイル種別: 0=空, 1=直管, 2=曲管, 3=T字, 4=十字, 5=水源, 6=ゴール
    /// 回転: 0=0°, 1=90°CW, 2=180°, 3=270°CW
    /// </summary>
    public class PipeTile : MonoBehaviour
    {
        public int TileType { get; private set; }
        public int Rotation { get; private set; }
        public bool IsFixed { get; private set; }
        public int Row { get; private set; }
        public int Col { get; private set; }

        private static readonly string[] SpriteNames =
        {
            "Sprites/Game005_PipeConnect/pipe_empty",
            "Sprites/Game005_PipeConnect/pipe_straight",
            "Sprites/Game005_PipeConnect/pipe_bend",
            "Sprites/Game005_PipeConnect/pipe_t",
            "Sprites/Game005_PipeConnect/pipe_cross",
            "Sprites/Game005_PipeConnect/pipe_source",
            "Sprites/Game005_PipeConnect/pipe_goal",
        };

        private SpriteRenderer _sr;

        private void Awake() => _sr = GetComponent<SpriteRenderer>();

        public void Init(int type, int rotation, int row, int col, bool isFixed)
        {
            TileType = type;
            Rotation = rotation;
            Row = row;
            Col = col;
            IsFixed = isFixed;
            UpdateVisual();
        }

        public void Rotate()
        {
            if (IsFixed) return;
            Rotation = (Rotation + 1) % 4;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_sr != null && TileType >= 0 && TileType < SpriteNames.Length)
            {
                var sp = Resources.Load<Sprite>(SpriteNames[TileType]);
                if (sp != null) _sr.sprite = sp;
            }
            // Rotate transform CW: rot0=0°, rot1=-90°, rot2=-180°, rot3=-270°
            transform.rotation = Quaternion.Euler(0f, 0f, -Rotation * 90f);
        }
    }
}
