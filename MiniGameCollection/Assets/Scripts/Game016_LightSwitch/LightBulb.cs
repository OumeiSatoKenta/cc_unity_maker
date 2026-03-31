using UnityEngine;

namespace Game016_LightSwitch
{
    public class LightBulb : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public bool IsOn { get; private set; }

        private SpriteRenderer _spriteRenderer;
        private Sprite _onSprite;
        private Sprite _offSprite;

        public void Initialize(Vector2Int gridPos, bool isOn, Sprite onSprite, Sprite offSprite)
        {
            GridPosition = gridPos;
            _onSprite = onSprite;
            _offSprite = offSprite;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            SetState(isOn);
        }

        public void Toggle()
        {
            SetState(!IsOn);
        }

        public void SetState(bool on)
        {
            IsOn = on;
            if (_spriteRenderer != null)
                _spriteRenderer.sprite = IsOn ? _onSprite : _offSprite;
        }
    }
}
