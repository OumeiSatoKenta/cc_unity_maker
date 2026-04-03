using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game006v2_ShadowMatch
{
    public class ShadowObjectController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _objectRenderer;
        [SerializeField] private SpriteRenderer _shadowRenderer;
        [SerializeField] private SpriteRenderer _targetRenderer;
        [SerializeField] private GameObject _hintArrow;
        [SerializeField] private Camera _mainCamera;

        [SerializeField] private Sprite _spriteObjectCube;
        [SerializeField] private Sprite _spriteObjectLShape;
        [SerializeField] private Sprite _spriteShadow;
        [SerializeField] private Sprite _spriteTarget;
        [SerializeField] private Sprite _spriteHintArrow;

        // Stage config
        private bool _lockX;      // When true, X-axis rotation is disabled
        private bool _lockYRot;   // When true, Y-axis rotation is disabled
        private bool _dualObject;
        private bool _dualLight;
        private int _hintCount;
        private float _matchThreshold;
        private Vector2 _targetRotation;

        // Current rotation (euler x, y)
        private Vector2 _currentRotation;
        private const float RotationSensitivity = 0.4f;

        // Drag state
        private bool _isDragging;
        private Vector2 _lastDragPos;
        private bool _isActive;

        // Coroutine guard
        private bool _missFeedbackRunning;

        // Callback for hint count update
        public System.Action<int> OnHintCountChanged;

        // Stage configs: (lockX, lockYRot, dualObject, dualLight, hints, threshold, targetX, targetY)
        private static readonly (bool lockX, bool lockYRot, bool dualObject, bool dualLight, int hints, float threshold, float targetX, float targetY)[]
            StageConfigs = new[]
            {
                (true,  false, false, false, 3, 0.80f, 0f,  45f),   // Stage 1: Y-only rotation
                (false, false, false, false, 3, 0.80f, 30f, 45f),   // Stage 2: X+Y rotation
                (false, false, true,  false, 2, 0.80f, 20f, 60f),   // Stage 3: dual object
                (false, false, false, true,  2, 0.80f, 25f, 50f),   // Stage 4: dual light
                (false, false, false, false, 1, 0.80f, 35f, 70f),   // Stage 5: high precision
            };

        private void Awake()
        {
            System.Diagnostics.Debug.Assert(StageConfigs.Length == 5, "StageConfigs must have 5 entries");
        }

        public void SetupStage(int stageIndex)
        {
            _isActive = true;
            int idx = Mathf.Clamp(stageIndex, 0, StageConfigs.Length - 1);
            var cfg = StageConfigs[idx];
            _lockX = cfg.lockX;
            _lockYRot = cfg.lockYRot;
            _dualObject = cfg.dualObject;
            _dualLight = cfg.dualLight;
            _hintCount = cfg.hints;
            _matchThreshold = cfg.threshold;
            _targetRotation = new Vector2(cfg.targetX, cfg.targetY);

            // Reset current rotation
            _currentRotation = Vector2.zero;

            // Setup sprites
            if (_objectRenderer != null)
                _objectRenderer.sprite = (stageIndex >= 1) ? _spriteObjectLShape : _spriteObjectCube;
            if (_shadowRenderer != null)
                _shadowRenderer.sprite = _spriteShadow;
            if (_targetRenderer != null)
                _targetRenderer.sprite = _spriteTarget;
            if (_hintArrow != null)
                _hintArrow.SetActive(false);

            OnHintCountChanged?.Invoke(_hintCount);
            UpdateObjectVisual();
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        private void Update()
        {
            if (!_isActive) return;
            HandleDragInput();
        }

        private void HandleDragInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _lastDragPos = mouse.position.ReadValue();
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }

            if (_isDragging && mouse.leftButton.isPressed)
            {
                Vector2 currentPos = mouse.position.ReadValue();
                Vector2 delta = currentPos - _lastDragPos;
                _lastDragPos = currentPos;

                if (!_lockX)
                    _currentRotation.x -= delta.y * RotationSensitivity;
                if (!_lockYRot)
                    _currentRotation.y += delta.x * RotationSensitivity;

                // Clamp x rotation to -90..90
                _currentRotation.x = Mathf.Clamp(_currentRotation.x, -89f, 89f);

                UpdateObjectVisual();
            }
        }

        private void UpdateObjectVisual()
        {
            if (_objectRenderer == null) return;

            float normY = _currentRotation.y / 180f;
            float normX = _currentRotation.x / 90f;

            float scaleX = Mathf.Cos(normY * Mathf.PI);
            float scaleY = 1f - Mathf.Abs(normX) * 0.3f;
            _objectRenderer.transform.localScale = new Vector3(
                Mathf.Max(0.1f, scaleX),
                Mathf.Max(0.5f, scaleY),
                1f
            );

            UpdateShadowVisual();
        }

        private void UpdateShadowVisual()
        {
            if (_shadowRenderer == null) return;

            float diffX = _lockX ? 0f : Mathf.Abs(_currentRotation.x - _targetRotation.x);
            float diffY = Mathf.Abs(NormalizeAngle(_currentRotation.y) - NormalizeAngle(_targetRotation.y));
            if (diffY > 180f) diffY = 360f - diffY;

            float totalDiff = (diffX + diffY) / 180f;

            float shadowScaleX = 0.6f + 0.4f * Mathf.Cos((_currentRotation.y - _targetRotation.y) * Mathf.Deg2Rad);
            float shadowScaleY = 0.6f + 0.4f * Mathf.Cos((_currentRotation.x - _targetRotation.x) * Mathf.Deg2Rad);
            _shadowRenderer.transform.localScale = new Vector3(
                Mathf.Clamp(shadowScaleX, 0.3f, 1.3f),
                Mathf.Clamp(shadowScaleY, 0.3f, 1.3f),
                1f
            );

            float brightness = Mathf.Lerp(0.3f, 0.8f, 1f - Mathf.Clamp01(totalDiff));
            _shadowRenderer.color = new Color(brightness * 0.5f, brightness * 0.5f, brightness * 0.7f, 0.9f);
        }

        private static float NormalizeAngle(float a) => ((a % 360f) + 360f) % 360f;

        public float CalculateMatch()
        {
            float diffX = _lockX ? 0f : Mathf.Abs(_currentRotation.x - _targetRotation.x);
            float diffY = Mathf.Abs(NormalizeAngle(_currentRotation.y) - NormalizeAngle(_targetRotation.y));
            if (diffY > 180f) diffY = 360f - diffY;

            float angleError = _lockX ? diffY : (diffX + diffY) / 2f;

            // Gaussian scoring: 0° → 1.0, ~15° → ~0.83, ~30° → ~0.50
            float matchRate = Mathf.Exp(-angleError * angleError / (2f * 15f * 15f));
            return Mathf.Clamp01(matchRate);
        }

        public void ShowHint()
        {
            if (_hintCount <= 0) return;
            _hintCount--;
            OnHintCountChanged?.Invoke(_hintCount);
            StartCoroutine(ShowHintRoutine());
        }

        private IEnumerator ShowHintRoutine()
        {
            if (_hintArrow == null) yield break;

            float diffY = _targetRotation.y - NormalizeAngle(_currentRotation.y);
            float arrowAngle = diffY > 0 ? 0f : 180f;
            _hintArrow.transform.rotation = Quaternion.Euler(0, 0, arrowAngle);
            _hintArrow.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            if (_hintArrow != null) _hintArrow.SetActive(false);
        }

        public void PlayClearFeedback()
        {
            StartCoroutine(ClearFeedbackRoutine());
        }

        private IEnumerator ClearFeedbackRoutine()
        {
            if (_objectRenderer == null) yield break;
            Vector3 originalScale = _objectRenderer.transform.localScale;

            float t = 0f;
            while (t < 0.2f)
            {
                if (this == null || !gameObject.activeInHierarchy) yield break;
                t += Time.deltaTime;
                float ratio = t / 0.2f;
                float scaleMult = ratio < 0.5f ? Mathf.Lerp(1f, 1.3f, ratio * 2f) : Mathf.Lerp(1.3f, 1f, (ratio - 0.5f) * 2f);
                _objectRenderer.transform.localScale = originalScale * scaleMult;
                yield return null;
            }
            if (_objectRenderer != null) _objectRenderer.transform.localScale = originalScale;

            if (_shadowRenderer != null)
            {
                _shadowRenderer.color = new Color(1f, 0.85f, 0.2f, 0.9f);
                yield return new WaitForSeconds(0.15f);
                if (_shadowRenderer != null) _shadowRenderer.color = new Color(0.5f, 0.5f, 0.7f, 0.9f);
            }
        }

        public void PlayMissFeedback()
        {
            // Prevent multiple simultaneous shake coroutines
            if (_missFeedbackRunning) return;
            StartCoroutine(MissFeedbackRoutine());
        }

        private IEnumerator MissFeedbackRoutine()
        {
            _missFeedbackRunning = true;

            if (_shadowRenderer != null)
            {
                _shadowRenderer.color = new Color(1f, 0.2f, 0.2f, 0.9f);
                yield return new WaitForSeconds(0.15f);
                if (_shadowRenderer != null) _shadowRenderer.color = new Color(0.5f, 0.5f, 0.7f, 0.9f);
            }

            if (_mainCamera != null)
            {
                Vector3 origPos = _mainCamera.transform.localPosition;
                float duration = 0.2f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    if (this == null || !gameObject.activeInHierarchy)
                    {
                        if (_mainCamera != null) _mainCamera.transform.localPosition = origPos;
                        _missFeedbackRunning = false;
                        yield break;
                    }
                    elapsed += Time.deltaTime;
                    float strength = 0.15f * (1f - elapsed / duration);
                    _mainCamera.transform.localPosition = origPos + new Vector3(
                        Random.Range(-strength, strength),
                        Random.Range(-strength, strength),
                        0f
                    );
                    yield return null;
                }
                if (_mainCamera != null) _mainCamera.transform.localPosition = origPos;
            }

            _missFeedbackRunning = false;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
