using UnityEngine;

namespace Game054_FruitSlash
{
    public class FruitItem : MonoBehaviour
    {
        private bool _isBomb;
        private bool _isSlashed;
        private System.Action<FruitItem> _onFellOff;
        private bool _hasFallen;

        public void Initialize(bool isBomb, System.Action<FruitItem> onFellOff)
        {
            _isBomb = isBomb;
            _isSlashed = false;
            _hasFallen = false;
            _onFellOff = onFellOff;
        }

        public void Slash()
        {
            _isSlashed = true;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.3f);
            Destroy(gameObject, 0.3f);
        }

        private void Update()
        {
            if (_hasFallen) return;
            if (transform.position.y < -8f)
            {
                _hasFallen = true;
                _onFellOff?.Invoke(this);
                Destroy(gameObject);
            }
        }

        public bool IsBomb => _isBomb;
        public bool IsSlashed => _isSlashed;
    }
}
