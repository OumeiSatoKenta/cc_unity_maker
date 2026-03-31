using UnityEngine;

namespace Game014_MagnetPath
{
    public enum Polarity { North, South }

    public class MagnetController : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public Polarity CurrentPolarity { get; private set; }

        private SpriteRenderer _spriteRenderer;
        private Sprite _northSprite;
        private Sprite _southSprite;

        public void Initialize(Vector2Int gridPos, Polarity polarity, Sprite northSprite, Sprite southSprite)
        {
            GridPosition = gridPos;
            CurrentPolarity = polarity;
            _northSprite = northSprite;
            _southSprite = southSprite;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateVisual();
        }

        public void TogglePolarity()
        {
            CurrentPolarity = CurrentPolarity == Polarity.North ? Polarity.South : Polarity.North;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_spriteRenderer == null) return;
            _spriteRenderer.sprite = CurrentPolarity == Polarity.North ? _northSprite : _southSprite;
        }
    }
}
