using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Game029v2_MeteorShield
{
    public class ShieldController : MonoBehaviour
    {
        [SerializeField] MeteorShieldGameManager _gameManager;
        [SerializeField] SpriteRenderer _shieldSr;
        [SerializeField] BoxCollider2D _shieldCollider;
        [SerializeField] Transform _shieldTransform;

        bool _isActive;
        float _targetX;
        float _shieldHalfWidth;
        Camera _cam;

        void Start()
        {
            _cam = Camera.main;
            _shieldHalfWidth = _shieldCollider != null ? _shieldCollider.size.x * 0.5f : 1.5f;
            _targetX = 0f;
            _isActive = false;
        }

        void Update()
        {
            if (!_isActive) return;
            if (_cam == null) { _cam = Camera.main; if (_cam == null) return; }

            float camHalfWidth = _cam.orthographicSize * _cam.aspect;

            // マウス/タッチ入力でX座標追従
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Vector3 worldPos = _cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
                _targetX = worldPos.x;
            }

            // X座標のクランプ
            float maxX = camHalfWidth - _shieldHalfWidth - 0.3f;
            _targetX = Mathf.Clamp(_targetX, -maxX, maxX);

            // シールドGameObjectの位置を更新
            if (_shieldTransform != null)
            {
                Vector3 pos = _shieldTransform.position;
                pos.x = Mathf.Lerp(pos.x, _targetX, Time.deltaTime * 15f);
                _shieldTransform.position = pos;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isActive) return;

            var meteor = other.GetComponent<MeteorObject>();
            if (meteor == null) return;

            Vector3 shieldPos = _shieldTransform != null ? _shieldTransform.position : transform.position;
            bool deflected = meteor.OnShieldHit(shieldPos, _shieldHalfWidth * 2f);
            if (deflected)
            {
                StartCoroutine(ShieldPulse());
                _gameManager?.OnMeteorDeflected(false);
            }
        }

        IEnumerator ShieldPulse()
        {
            if (_shieldTransform == null) yield break;
            Vector3 orig = _shieldTransform.localScale;
            Vector3 big = new Vector3(orig.x, orig.y * 1.2f, orig.z);
            float t = 0f;
            Color origColor = _shieldSr != null ? _shieldSr.color : Color.white;
            if (_shieldSr != null) _shieldSr.color = Color.white;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.075f;
                _shieldTransform.localScale = ratio <= 1f ? Vector3.Lerp(orig, big, ratio) : Vector3.Lerp(big, orig, ratio - 1f);
                yield return null;
            }
            _shieldTransform.localScale = orig;
            if (_shieldSr != null) _shieldSr.color = origColor;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        public float GetShieldWidth()
        {
            return _shieldHalfWidth * 2f;
        }
    }
}
