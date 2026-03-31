using UnityEngine;

namespace Game008_IcePath
{
    public enum IceCellType { Ice, Wall, Goal }

    public class IceCell : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public IceCellType CellType { get; private set; }
        public bool IsVisited { get; private set; }

        private SpriteRenderer _spriteRenderer;

        public void Initialize(Vector2Int gridPos, IceCellType cellType)
        {
            GridPosition = gridPos;
            CellType = cellType;
            IsVisited = false;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void MarkVisited(Sprite visitedSprite)
        {
            if (IsVisited) return;
            IsVisited = true;
            if (_spriteRenderer != null && visitedSprite != null)
                _spriteRenderer.sprite = visitedSprite;
        }

        public void ResetVisited(Sprite originalSprite)
        {
            IsVisited = false;
            if (_spriteRenderer != null && originalSprite != null)
                _spriteRenderer.sprite = originalSprite;
        }
    }
}
