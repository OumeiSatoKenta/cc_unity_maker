using UnityEngine;

namespace Game006_ShadowMatch
{
    public class ShapeController : MonoBehaviour
    {
        public float CurrentAngle { get; private set; }

        public void SetAngle(float angle)
        {
            CurrentAngle = angle % 360f;
            if (CurrentAngle < 0) CurrentAngle += 360f;
            transform.rotation = Quaternion.Euler(0, 0, -CurrentAngle);
        }

        public void AddAngle(float delta)
        {
            SetAngle(CurrentAngle + delta);
        }
    }
}
