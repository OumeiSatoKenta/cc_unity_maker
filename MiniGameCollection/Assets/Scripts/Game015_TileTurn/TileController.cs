using UnityEngine;

namespace Game015_TileTurn
{
    public class TileController : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public int CurrentRotation { get; private set; } // 0=correct, 1,2,3=rotated
        public int TileIndex { get; private set; }

        public void Initialize(Vector2Int gridPos, int tileIndex, int startRotation)
        {
            GridPosition = gridPos;
            TileIndex = tileIndex;
            CurrentRotation = startRotation;
            UpdateVisual();
        }

        public void RotateCW()
        {
            CurrentRotation = (CurrentRotation + 1) % 4;
            UpdateVisual();
        }

        public bool IsCorrect()
        {
            return CurrentRotation == 0;
        }

        private void UpdateVisual()
        {
            transform.rotation = Quaternion.Euler(0, 0, -CurrentRotation * 90);
        }
    }
}
