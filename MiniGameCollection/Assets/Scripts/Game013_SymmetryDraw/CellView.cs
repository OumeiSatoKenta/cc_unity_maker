using UnityEngine;

namespace Game013_SymmetryDraw
{
    public class CellView : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public bool IsPainted { get; private set; }
        public bool IsTarget { get; private set; }

        private SpriteRenderer _spriteRenderer;

        public void Initialize(Vector2Int gridPos, bool isTarget)
        {
            GridPosition = gridPos;
            IsTarget = isTarget;
            IsPainted = false;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Paint(Sprite paintedSprite)
        {
            if (IsPainted) return;
            IsPainted = true;
            if (_spriteRenderer != null && paintedSprite != null)
                _spriteRenderer.sprite = paintedSprite;
        }

        public void ResetCell(Sprite emptySprite)
        {
            IsPainted = false;
            if (_spriteRenderer != null && emptySprite != null)
                _spriteRenderer.sprite = emptySprite;
        }
    }
}
