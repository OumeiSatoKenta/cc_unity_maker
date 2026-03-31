using UnityEngine;

namespace Game003_GravitySwitch
{
    public class BallController : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }

        public void Initialize(Vector2Int gridPos)
        {
            GridPosition = gridPos;
        }

        public void SetGridPosition(Vector2Int newPos)
        {
            GridPosition = newPos;
        }

        public void UpdateWorldPosition(Vector3 worldPos)
        {
            transform.position = worldPos;
        }
    }
}
