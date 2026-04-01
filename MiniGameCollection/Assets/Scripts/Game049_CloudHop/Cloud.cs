using UnityEngine;

namespace Game049_CloudHop
{
    public class Cloud : MonoBehaviour
    {
        private float _lifetime;
        private float _timer;
        private SpriteRenderer _sr;

        public void Initialize(float lifetime)
        {
            _lifetime = lifetime;
            _timer = 0f;
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            float ratio = _timer / _lifetime;

            // Start blinking at 70% lifetime
            if (ratio > 0.7f)
            {
                float blink = Mathf.PingPong((_timer - _lifetime * 0.7f) * 6f, 1f);
                if (_sr != null)
                {
                    var c = _sr.color;
                    c.a = Mathf.Lerp(0.9f, 0.2f, blink);
                    _sr.color = c;
                }
            }

            if (_timer >= _lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
