using UnityEngine;

namespace Game037_ZapChain
{
    public class ZapNode : MonoBehaviour
    {
        private bool _isZapped;
        private Sprite _normalSprite;
        private Sprite _zappedSprite;
        private SpriteRenderer _sr;

        public void Initialize(Sprite normal, Sprite zapped)
        {
            _normalSprite = normal;
            _zappedSprite = zapped;
            _isZapped = false;
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _sr.sprite = _normalSprite;
        }

        public void Zap()
        {
            if (_isZapped) return;
            _isZapped = true;
            if (_sr != null && _zappedSprite != null) _sr.sprite = _zappedSprite;
        }

        public bool IsZapped => _isZapped;
    }
}
