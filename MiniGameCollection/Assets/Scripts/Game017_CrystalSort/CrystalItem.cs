using UnityEngine;

namespace Game017_CrystalSort
{
    public class CrystalItem : MonoBehaviour
    {
        public int ColorIndex { get; private set; }
        public bool IsSorted { get; private set; }

        public void Initialize(int colorIndex)
        {
            ColorIndex = colorIndex;
            IsSorted = false;
        }

        public void MarkSorted()
        {
            IsSorted = true;
            gameObject.SetActive(false);
        }
    }
}
