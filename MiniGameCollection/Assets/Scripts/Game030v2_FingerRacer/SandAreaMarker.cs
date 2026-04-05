using UnityEngine;

namespace Game030v2_FingerRacer
{
    public class SandAreaMarker : MonoBehaviour
    {
        public CarController CarController;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CarController>() == null) return;
            CarController?.SetSandMode(true);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<CarController>() == null) return;
            CarController?.SetSandMode(false);
        }
    }
}
