using UnityEngine;

namespace Game010_GearSync
{
    public class GearController : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public int CurrentRotation { get; private set; } // 0,1,2,3 = up,right,down,left
        public int TargetRotation { get; private set; }

        public void Initialize(Vector2Int gridPos, int startRotation, int targetRotation)
        {
            GridPosition = gridPos;
            CurrentRotation = startRotation;
            TargetRotation = targetRotation;
            UpdateVisual();
        }

        public void RotateCW()
        {
            CurrentRotation = (CurrentRotation + 1) % 4;
            UpdateVisual();
        }

        public void RotateCCW()
        {
            CurrentRotation = (CurrentRotation + 3) % 4;
            UpdateVisual();
        }

        public bool IsAligned()
        {
            return CurrentRotation == TargetRotation;
        }

        private void UpdateVisual()
        {
            transform.rotation = Quaternion.Euler(0, 0, -CurrentRotation * 90);
        }
    }
}
