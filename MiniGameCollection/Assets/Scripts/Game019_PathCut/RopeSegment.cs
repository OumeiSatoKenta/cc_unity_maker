using UnityEngine;

namespace Game019_PathCut
{
    public class RopeSegment : MonoBehaviour
    {
        public bool IsCut { get; private set; }
        public int RopeIndex { get; private set; }

        public void Initialize(int ropeIndex)
        {
            RopeIndex = ropeIndex;
            IsCut = false;
        }

        public void Cut()
        {
            IsCut = true;
            gameObject.SetActive(false);
        }
    }
}
