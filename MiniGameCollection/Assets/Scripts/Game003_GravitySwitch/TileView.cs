using UnityEngine;

namespace Game003_GravitySwitch
{
    /// <summary>
    /// グリッドタイルの位置・種別データを保持する。
    /// 表示はSpriteRendererで行い、このクラスは状態管理のみ担当する。
    /// </summary>
    public class TileView : MonoBehaviour
    {
        public int Row { get; private set; }
        public int Col { get; private set; }
        public int TileType { get; private set; } // 1=wall, 2=goal

        public void Init(int row, int col, int type)
        {
            Row = row;
            Col = col;
            TileType = type;
        }
    }
}
