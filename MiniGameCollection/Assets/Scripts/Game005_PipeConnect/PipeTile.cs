using UnityEngine;

namespace Game005_PipeConnect
{
    public enum PipeType
    {
        Empty,
        Straight,  // connects 2 opposite sides (top-bottom)
        Bend,      // connects 2 adjacent sides (top-right)
        Cross,     // connects all 4 sides
        TJunction, // connects 3 sides (top-left-right)
        Source,
        Goal
    }

    public class PipeTile : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public PipeType Type { get; private set; }
        public int Rotation { get; private set; } // 0, 1, 2, 3 (x90 degrees)

        public void Initialize(Vector2Int gridPos, PipeType type, int rotation)
        {
            GridPosition = gridPos;
            Type = type;
            Rotation = rotation;
            UpdateVisual();
        }

        public void RotateCW()
        {
            if (Type == PipeType.Source || Type == PipeType.Goal || Type == PipeType.Cross)
                return;
            Rotation = (Rotation + 1) % 4;
            UpdateVisual();
        }

        /// <summary>
        /// Returns which directions this pipe connects to.
        /// 0=up, 1=right, 2=down, 3=left
        /// </summary>
        public bool[] GetConnections()
        {
            bool[] conn = new bool[4];
            switch (Type)
            {
                case PipeType.Straight:
                    conn[0] = true; conn[2] = true; // up-down
                    break;
                case PipeType.Bend:
                    conn[0] = true; conn[1] = true; // up-right
                    break;
                case PipeType.TJunction:
                    conn[0] = true; conn[1] = true; conn[3] = true; // up-right-left
                    break;
                case PipeType.Cross:
                    conn[0] = true; conn[1] = true; conn[2] = true; conn[3] = true;
                    break;
                case PipeType.Source:
                    conn[0] = true; conn[1] = true; conn[2] = true; conn[3] = true;
                    break;
                case PipeType.Goal:
                    conn[0] = true; conn[1] = true; conn[2] = true; conn[3] = true;
                    break;
            }
            // Apply rotation
            bool[] rotated = new bool[4];
            for (int i = 0; i < 4; i++)
            {
                rotated[(i + Rotation) % 4] = conn[i];
            }
            return rotated;
        }

        private void UpdateVisual()
        {
            transform.rotation = Quaternion.Euler(0, 0, -Rotation * 90);
        }
    }
}
