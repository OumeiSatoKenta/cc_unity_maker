using UnityEngine;

namespace Game038v2_FlyBird
{
    public class PipePair : MonoBehaviour
    {
        public bool Scored { get; set; }
        public int CurrentStage { get; set; }

        bool _isMoving;
        float _moveSpeed;
        float _moveRange;
        float _moveOffset;
        Vector3 _basePosition;

        bool _isRotating;

        public void SetMoving(bool moving, float speed = 1.0f, float range = 1.0f, float offset = 0f)
        {
            _isMoving = moving;
            _moveSpeed = speed;
            _moveRange = range;
            _moveOffset = offset;
            _basePosition = transform.position;
        }

        public void SetRotating(bool rotating)
        {
            _isRotating = rotating;
        }

        void Update()
        {
            if (_isMoving)
            {
                float newY = _basePosition.y + Mathf.Sin(Time.time * _moveSpeed + _moveOffset) * _moveRange;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }

            if (_isRotating)
            {
                transform.Rotate(0, 0, 30f * Time.deltaTime);
            }
        }
    }
}
