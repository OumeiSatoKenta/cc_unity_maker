using UnityEngine;

namespace Game012_BridgeBuilder
{
    public class BridgePart : MonoBehaviour
    {
        public int PartType { get; private set; }
        public Vector2 Position { get; private set; }

        public void Init(int partType, Vector2 position)
        {
            PartType = partType;
            Position = position;
        }
    }
}
