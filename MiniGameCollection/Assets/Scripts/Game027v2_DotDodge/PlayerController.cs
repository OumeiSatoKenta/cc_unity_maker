using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Game027v2_DotDodge
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] DotDodgeGameManager _gameManager;
        [SerializeField] Camera _mainCamera;

        SpriteRenderer _sr;
        bool _isActive = false;
        bool _isDragging = false;
        Color _defaultColor;
        Coroutine _flashCoroutine;

        const float NearMissMultiplier = 1.8f;
        const float HitRadius = 0.25f;

        public float CurrentRadius => HitRadius;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _defaultColor = _sr.color;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active) _isDragging = false;
        }

        void Update()
        {
            if (!_isActive) return;

            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    _isDragging = true;
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                    _isDragging = false;

                if (_isDragging)
                {
                    Vector2 screenPos = Mouse.current.position.ReadValue();
                    MoveToScreen(screenPos);
                }
            }
        }

        void MoveToScreen(Vector2 screenPos)
        {
            if (_mainCamera == null) return;
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(_mainCamera.transform.position.z)));

            float camSize = _mainCamera.orthographicSize;
            float camWidth = camSize * _mainCamera.aspect;
            float margin = 0.4f;
            worldPos.x = Mathf.Clamp(worldPos.x, -camWidth + margin, camWidth - margin);
            worldPos.y = Mathf.Clamp(worldPos.y, -camSize + margin, camSize - margin);
            worldPos.z = 0f;
            transform.position = worldPos;
        }

        public void TriggerNearMissFlash()
        {
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(NearMissFlash());
        }

        IEnumerator NearMissFlash()
        {
            if (_sr == null) yield break;
            // Scale pulse
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float scale = 1f + 0.4f * Mathf.Sin(Mathf.PI * t / 0.2f);
                transform.localScale = Vector3.one * scale;
                _sr.color = Color.Lerp(_defaultColor, Color.yellow, Mathf.Sin(Mathf.PI * t / 0.2f));
                yield return null;
            }
            transform.localScale = Vector3.one;
            _sr.color = _defaultColor;
        }

        public void TriggerHitFlash()
        {
            if (_sr != null) _sr.color = Color.red;
        }
    }
}
