using UnityEngine;

namespace Game034_DropZone
{
    public class DropItem : MonoBehaviour
    {
        public int ColorIndex { get; private set; }
        public bool IsCaught { get; set; }

        public void Initialize(int colorIndex) { ColorIndex = colorIndex; IsCaught = false; }

        private void Update()
        {
            if (!IsCaught) transform.position += Vector3.down * 3f * Time.deltaTime;
        }
    }
}
