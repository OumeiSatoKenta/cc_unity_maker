using UnityEngine;

namespace Game008_IcePath
{
    /// <summary>
    /// 盤面の1マスを表すコンポーネント。
    /// 状態（通常・通過済み・壁）と表示を担当。
    /// </summary>
    public class IceCell : MonoBehaviour
    {
        public enum CellType { Ice, Wall }

        public int Row { get; private set; }
        public int Col { get; private set; }
        public CellType Type { get; private set; }
        public bool IsVisited { get; private set; }

        [SerializeField] private Sprite _sprIce;
        [SerializeField] private Sprite _sprVisited;
        [SerializeField] private Sprite _sprWall;

        private SpriteRenderer _sr;

        public void Init(int row, int col, CellType type, Sprite sprIce, Sprite sprVisited, Sprite sprWall)
        {
            Row = row;
            Col = col;
            Type = type;
            IsVisited = false;

            _sprIce = sprIce;
            _sprVisited = sprVisited;
            _sprWall = sprWall;

            _sr = GetComponent<SpriteRenderer>();
            UpdateSprite();
        }

        public void SetVisited(bool visited)
        {
            if (Type == CellType.Wall) return;
            IsVisited = visited;
            UpdateSprite();
        }

        public void Reset()
        {
            IsVisited = false;
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            if (_sr == null) return;
            _sr.sprite = Type == CellType.Wall ? _sprWall :
                         IsVisited ? _sprVisited : _sprIce;
        }
    }
}
