using UnityEngine;

namespace Game012_BridgeBuilder
{
    public enum SlotType { Empty, Plank, Support, Cliff, Water }

    public class BridgeSlot : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public SlotType Type { get; private set; }
        public bool IsFixed { get; private set; }

        private SpriteRenderer _spriteRenderer;

        public void Initialize(Vector2Int gridPos, SlotType type, bool isFixed)
        {
            GridPosition = gridPos;
            Type = type;
            IsFixed = isFixed;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetType(SlotType type, Sprite sprite)
        {
            Type = type;
            if (_spriteRenderer != null && sprite != null)
                _spriteRenderer.sprite = sprite;
        }
    }
}
