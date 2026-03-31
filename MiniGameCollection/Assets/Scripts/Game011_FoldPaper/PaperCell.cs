using UnityEngine;

namespace Game011_FoldPaper
{
    public class PaperCell : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public bool IsFolded { get; private set; }

        private SpriteRenderer _spriteRenderer;

        public void Initialize(Vector2Int gridPos, bool folded, Sprite whiteSprite, Sprite foldedSprite)
        {
            GridPosition = gridPos;
            IsFolded = folded;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateVisual(whiteSprite, foldedSprite);
        }

        public void Toggle(Sprite whiteSprite, Sprite foldedSprite)
        {
            IsFolded = !IsFolded;
            UpdateVisual(whiteSprite, foldedSprite);
        }

        private void UpdateVisual(Sprite whiteSprite, Sprite foldedSprite)
        {
            if (_spriteRenderer == null) return;
            _spriteRenderer.sprite = IsFolded ? foldedSprite : whiteSprite;
        }
    }
}
