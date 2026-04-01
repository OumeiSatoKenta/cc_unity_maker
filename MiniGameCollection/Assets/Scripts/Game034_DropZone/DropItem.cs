using UnityEngine;

namespace Game034_DropZone
{
    public class DropItem : MonoBehaviour
    {
        private int _category; // 0=フルーツ, 1=ゴミ, 2=リサイクル
        private float _fallSpeed;
        private bool _isActive;

        public void Initialize(Sprite sprite, int category, float fallSpeed)
        {
            _category = category;
            _fallSpeed = fallSpeed;
            _isActive = true;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && sprite != null) sr.sprite = sprite;
        }

        private void Update()
        {
            if (!_isActive) return;
            transform.position += new Vector3(0f, -_fallSpeed * Time.deltaTime, 0f);
        }

        public void SetXPosition(float x)
        {
            var pos = transform.position;
            pos.x = Mathf.Clamp(x, -4f, 4f);
            transform.position = pos;
        }

        public void AccelerateFall()
        {
            _fallSpeed = Mathf.Min(_fallSpeed * 3f, 30f);
        }

        public void Stop()
        {
            _isActive = false;
        }

        public int Category => _category;
        public bool IsActive => _isActive;
    }
}
