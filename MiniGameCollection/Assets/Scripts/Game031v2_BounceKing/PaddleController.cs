using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Game031v2_BounceKing
{
    public class PaddleController : MonoBehaviour
    {
        [SerializeField] BounceKingGameManager _gameManager;
        SpriteRenderer _sr;
        BoxCollider2D _col;

        float _halfWidth;
        float _leftBound;
        float _rightBound;
        float _baseWidth = 2.0f;
        bool _isActive;

        const float BasePaddleHeight = 0.25f;
        Coroutine _expandCo;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _col = GetComponent<BoxCollider2D>();
        }

        public void Initialize()
        {
            float camW = Camera.main.orthographicSize * Camera.main.aspect;
            _leftBound = -camW + _baseWidth / 2f + 0.2f;
            _rightBound = camW - _baseWidth / 2f - 0.2f;
            SetWidth(_baseWidth);
            _isActive = true;
        }

        public void SetActive(bool active) => _isActive = active;

        public float PaddleHalfWidth => _halfWidth;

        void Update()
        {
            if (!_isActive) return;

            float screenX = Mouse.current.position.ReadValue().x;
            float worldX = Camera.main.ScreenToWorldPoint(new Vector3(screenX, 0f, 0f)).x;
            float clampedX = Mathf.Clamp(worldX, _leftBound, _rightBound);
            transform.position = new Vector3(clampedX, transform.position.y, 0f);
        }

        public Vector2 GetReflectDirection(Vector2 ballPos)
        {
            float hitOffset = (ballPos.x - transform.position.x) / _halfWidth;
            hitOffset = Mathf.Clamp(hitOffset, -1f, 1f);
            float angleDeg = hitOffset * 65f;
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)).normalized;
        }

        void SetWidth(float width)
        {
            _halfWidth = width / 2f;
            transform.localScale = new Vector3(width / _baseWidth, 1f, 1f);
            _leftBound = -(Camera.main.orthographicSize * Camera.main.aspect) + _halfWidth + 0.2f;
            _rightBound = (Camera.main.orthographicSize * Camera.main.aspect) - _halfWidth - 0.2f;
        }

        public void ApplyExpand(float duration)
        {
            if (_expandCo != null) StopCoroutine(_expandCo);
            _expandCo = StartCoroutine(ExpandEffect(duration, 1.5f));
        }

        public void ApplyShrink(float duration)
        {
            if (_expandCo != null) StopCoroutine(_expandCo);
            _expandCo = StartCoroutine(ExpandEffect(duration, 0.7f));
        }

        IEnumerator ExpandEffect(float duration, float widthMultiplier)
        {
            SetWidth(_baseWidth * widthMultiplier);
            yield return new WaitForSeconds(duration);
            SetWidth(_baseWidth);
            _expandCo = null;
        }

        public IEnumerator HitPulse()
        {
            if (_sr != null)
            {
                _sr.color = new Color(0.8f, 0.95f, 1f, 1f);
                yield return new WaitForSeconds(0.08f);
                _sr.color = Color.white;
            }
        }
    }
}
