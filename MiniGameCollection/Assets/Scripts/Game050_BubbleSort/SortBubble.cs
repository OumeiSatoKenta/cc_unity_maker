using UnityEngine;

namespace Game050_BubbleSort
{
    public class SortBubble : MonoBehaviour
    {
        private int _colorIndex;
        private SpriteRenderer _sr;
        private Color _baseColor;

        public void Initialize(int colorIndex)
        {
            _colorIndex = colorIndex;
            _sr = GetComponent<SpriteRenderer>();
            _baseColor = _sr.color;
        }

        public void SetSelected(bool selected)
        {
            if (_sr == null) return;
            if (selected)
            {
                _sr.color = Color.Lerp(_baseColor, Color.white, 0.4f);
                transform.localScale = Vector3.one * 1.2f;
            }
            else
            {
                _sr.color = _baseColor;
                transform.localScale = Vector3.one;
            }
        }

        public int ColorIndex => _colorIndex;
    }
}
