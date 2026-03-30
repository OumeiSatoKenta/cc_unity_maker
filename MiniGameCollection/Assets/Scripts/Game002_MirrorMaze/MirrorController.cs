using UnityEngine;

namespace Game002_MirrorMaze
{
    public class MirrorController : MonoBehaviour
    {
        [SerializeField] private int _angleType;

        public int AngleType => _angleType;
        public Vector2Int GridPosition { get; private set; }

        public void Initialize(Vector2Int gridPos, int angleType)
        {
            GridPosition = gridPos;
            _angleType = angleType;
            UpdateVisual();
        }

        public void Rotate45()
        {
            _angleType = (_angleType + 1) % 2;
            UpdateVisual();
        }

        /// <summary>
        /// 入射方向から反射方向を計算する。
        /// angleType 0 (/) : (dx,dy) → (dy,dx)
        /// angleType 1 (\) : (dx,dy) → (-dy,-dx)
        /// </summary>
        public Vector2Int Reflect(Vector2Int inDir)
        {
            if (_angleType == 0)
                return new Vector2Int(inDir.y, inDir.x);
            else
                return new Vector2Int(-inDir.y, -inDir.x);
        }

        public void SetGridPosition(Vector2Int newPos)
        {
            GridPosition = newPos;
        }

        public void UpdateWorldPosition(Vector3 worldPos)
        {
            transform.position = worldPos;
        }

        private void UpdateVisual()
        {
            if (_angleType == 0)
                transform.rotation = Quaternion.Euler(0, 0, 0);
            else
                transform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }
}
