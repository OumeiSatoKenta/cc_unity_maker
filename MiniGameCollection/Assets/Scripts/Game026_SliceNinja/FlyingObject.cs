using UnityEngine;

namespace Game026_SliceNinja
{
    public class FlyingObject : MonoBehaviour
    {
        public bool IsBomb { get; private set; }
        public bool IsSliced { get; private set; }
        public Vector2 Velocity { get; set; }

        public void Initialize(bool isBomb, Vector2 velocity)
        {
            IsBomb = isBomb;
            Velocity = velocity;
            IsSliced = false;
        }

        public void Slice()
        {
            IsSliced = true;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!IsSliced)
            {
                Velocity += Vector2.down * 6f * Time.deltaTime;
                transform.position += (Vector3)Velocity * Time.deltaTime;
                transform.Rotate(0, 0, 200f * Time.deltaTime);
            }
        }
    }
}
