using UnityEngine;

namespace Game047_SpinBalance
{
    public class Piece : MonoBehaviour
    {
        private System.Action<Piece> _onFallen;
        private bool _hasFallen;

        public void Initialize(System.Action<Piece> onFallen)
        {
            _onFallen = onFallen;
            _hasFallen = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasFallen) return;
            if (other.gameObject.name == "FallZone")
            {
                _hasFallen = true;
                _onFallen?.Invoke(this);
            }
        }
    }
}
