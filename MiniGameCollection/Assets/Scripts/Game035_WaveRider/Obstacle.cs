using UnityEngine;

namespace Game035_WaveRider
{
    public class Obstacle : MonoBehaviour
    {
        private float _speed;

        public void Initialize(Sprite sprite, float speed)
        {
            _speed = speed;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && sprite != null) sr.sprite = sprite;
        }

        private void Update()
        {
            transform.position += new Vector3(-_speed * Time.deltaTime, 0f, 0f);
            if (transform.position.x < -6f) Destroy(gameObject);
        }
    }
}
