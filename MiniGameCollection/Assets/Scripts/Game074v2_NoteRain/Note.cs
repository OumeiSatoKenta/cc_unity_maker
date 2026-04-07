using UnityEngine;

namespace Game074v2_NoteRain
{
    public enum NoteType { Normal, Fake, Accelerating, Curve }

    public class Note : MonoBehaviour
    {
        public NoteType noteType;
        public float fallSpeed;
        public float curveOffsetX;

        bool _accelerated;
        bool _active = true;
        SpriteRenderer _sr;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Initialize(NoteType type, float speed, float curveX = 0f)
        {
            noteType = type;
            fallSpeed = speed;
            curveOffsetX = curveX;
            _accelerated = false;
            _active = true;
        }

        void Update()
        {
            if (!_active) return;

            if (noteType == NoteType.Accelerating && !_accelerated && transform.position.y < 0f)
            {
                fallSpeed *= 1.5f;
                _accelerated = true;
                if (_sr != null) _sr.color = new Color(1f, 0.7f, 0f);
            }

            float dx = curveOffsetX;
            transform.position += new Vector3(dx, -fallSpeed, 0f) * Time.deltaTime;
        }

        public void SetActive(bool active)
        {
            _active = active;
        }

        public SpriteRenderer SpriteRenderer => _sr;
    }
}
