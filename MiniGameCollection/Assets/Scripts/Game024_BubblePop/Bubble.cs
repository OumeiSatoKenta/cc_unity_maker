using UnityEngine;

namespace Game024_BubblePop
{
    public class Bubble : MonoBehaviour
    {
        public bool IsPopped { get; private set; }
        public Vector2 Velocity { get; set; }
        public int ColorIndex { get; private set; }

        public void Initialize(int colorIndex, Vector2 velocity)
        {
            ColorIndex = colorIndex;
            Velocity = velocity;
            IsPopped = false;
        }

        public void Pop()
        {
            IsPopped = true;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!IsPopped)
                transform.position += (Vector3)Velocity * Time.deltaTime;
        }
    }
}
