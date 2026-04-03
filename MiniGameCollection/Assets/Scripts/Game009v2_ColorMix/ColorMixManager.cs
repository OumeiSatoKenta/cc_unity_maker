using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game009v2_ColorMix
{
    public class ColorMixManager : MonoBehaviour
    {
        [SerializeField] private ColorMixGameManager _gameManager;
        [SerializeField] private Image _targetColorImage;
        [SerializeField] private Image _mixColorImage;
        [SerializeField] private Slider _sliderR;
        [SerializeField] private Slider _sliderG;
        [SerializeField] private Slider _sliderB;
        [SerializeField] private Slider _sliderV; // brightness (stage 3+)
        [SerializeField] private Camera _mainCamera;

        // Stage config
        private float _allowedDeltaE;
        private int _maxJudgments;
        private bool _dynamicTarget;
        private int _currentStage;
        private bool _isActive;

        // Runtime state
        private Color _targetColor;
        private int _judgementsUsed;
        private Coroutine _dynamicTargetCoroutine;
        private Coroutine _feedbackCoroutine;
        private Coroutine _cameraShakeCoroutine;

        // Stage-specific target colors
        private static readonly Color[][] _stageColors = new Color[][]
        {
            // Stage 0: primary colors
            new Color[] {
                new Color(1f, 0.2f, 0.2f),
                new Color(0.2f, 0.4f, 1f),
                new Color(1f, 0.86f, 0f)
            },
            // Stage 1: secondary colors
            new Color[] {
                new Color(0.2f, 0.78f, 0.31f),
                new Color(0.63f, 0.2f, 0.78f),
                new Color(1f, 0.55f, 0f)
            },
            // Stage 2: pastel colors
            new Color[] {
                new Color(1f, 0.71f, 0.78f),
                new Color(0.59f, 0.9f, 0.78f),
                new Color(0.78f, 0.71f, 0.94f)
            },
            // Stage 3: complex colors (teal, magenta, gold)
            new Color[] {
                new Color(0f, 0.59f, 0.63f),
                new Color(0.9f, 0.2f, 0.6f),
                new Color(0.84f, 0.7f, 0.1f)
            },
            // Stage 4: dynamic target base colors
            new Color[] {
                new Color(0f, 0.8f, 0.8f),
                new Color(1f, 0.41f, 0.38f),
                new Color(0.49f, 0.86f, 0.2f)
            }
        };

        public int MaxJudgments => _maxJudgments;
        public int JudgmentsLeft => Mathf.Max(0, _maxJudgments - _judgementsUsed);

        private void Awake()
        {
            if (_sliderR != null) _sliderR.onValueChanged.AddListener(_ => OnSliderChanged());
            if (_sliderG != null) _sliderG.onValueChanged.AddListener(_ => OnSliderChanged());
            if (_sliderB != null) _sliderB.onValueChanged.AddListener(_ => OnSliderChanged());
            if (_sliderV != null) _sliderV.onValueChanged.AddListener(_ => OnSliderChanged());
        }

        public void SetupStage(int stageIndex)
        {
            _currentStage = stageIndex;
            _isActive = true;
            _judgementsUsed = 0;

            // Stage config
            switch (stageIndex)
            {
                case 0: _allowedDeltaE = 20f; _maxJudgments = -1; _dynamicTarget = false; break;
                case 1: _allowedDeltaE = 15f; _maxJudgments = -1; _dynamicTarget = false; break;
                case 2: _allowedDeltaE = 12f; _maxJudgments = -1; _dynamicTarget = false; break;
                case 3: _allowedDeltaE = 10f; _maxJudgments = 3; _dynamicTarget = false; break;
                case 4: _allowedDeltaE = 8f;  _maxJudgments = 3; _dynamicTarget = true; break;
                default: _allowedDeltaE = 10f; _maxJudgments = -1; _dynamicTarget = false; break;
            }

            // Pick target color
            int si = Mathf.Min(stageIndex, _stageColors.Length - 1);
            _targetColor = _stageColors[si][Random.Range(0, _stageColors[si].Length)];
            if (_targetColorImage != null) _targetColorImage.color = _targetColor;

            // Reset sliders
            ResetSliders();

            // Dynamic target
            if (_dynamicTargetCoroutine != null) StopCoroutine(_dynamicTargetCoroutine);
            if (_dynamicTarget)
                _dynamicTargetCoroutine = StartCoroutine(DynamicTargetRoutine());
        }

        private IEnumerator DynamicTargetRoutine()
        {
            float t = 0f;
            Color baseColor = _targetColor;
            float period = 5f;
            while (_isActive)
            {
                t += Time.deltaTime;
                float phase = Mathf.Sin(t * Mathf.PI * 2f / period) * 0.5f + 0.5f;
                _targetColor = Color.Lerp(baseColor,
                    new Color(1f - baseColor.r, 1f - baseColor.g, baseColor.b), phase * 0.4f);
                if (_targetColorImage != null) _targetColorImage.color = _targetColor;
                yield return null;
            }
        }

        private void OnSliderChanged()
        {
            if (!_isActive) return;
            UpdateMixPreview();
        }

        private void UpdateMixPreview()
        {
            float r = _sliderR != null ? _sliderR.value / 255f : 0f;
            float g = _sliderG != null ? _sliderG.value / 255f : 0f;
            float b = _sliderB != null ? _sliderB.value / 255f : 0f;
            float v = (_sliderV != null && _sliderV.gameObject.activeInHierarchy) ? _sliderV.value : 1f;

            Color mixed = new Color(r * v, g * v, b * v, 1f);
            if (_mixColorImage != null) _mixColorImage.color = mixed;
        }

        public void OnJudgePressed()
        {
            if (!_isActive) return;
            if (_gameManager == null || !_gameManager.IsPlaying) return;

            _judgementsUsed++;

            float r = _sliderR != null ? _sliderR.value / 255f : 0f;
            float g = _sliderG != null ? _sliderG.value / 255f : 0f;
            float b = _sliderB != null ? _sliderB.value / 255f : 0f;
            float v = (_sliderV != null && _sliderV.gameObject.activeInHierarchy) ? _sliderV.value : 1f;

            Color mixed = new Color(r * v, g * v, b * v, 1f);
            float deltaE = CalculateDeltaE(mixed, _targetColor);

            bool cleared = deltaE <= _allowedDeltaE;
            bool outOfJudges = _maxJudgments > 0 && _judgementsUsed >= _maxJudgments;

            // Lock input immediately
            _isActive = false;
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);

            if (cleared)
            {
                if (_dynamicTargetCoroutine != null) StopCoroutine(_dynamicTargetCoroutine);
                _feedbackCoroutine = StartCoroutine(SuccessFeedback());
            }
            else
            {
                _feedbackCoroutine = StartCoroutine(FailFeedback());
                // Re-enable input only if not out of judges and game continues
                if (!outOfJudges)
                    _isActive = true;
            }

            _gameManager.OnJudgeResult(deltaE, _allowedDeltaE, _judgementsUsed);
        }

        private float CalculateDeltaE(Color a, Color b)
        {
            float dr = a.r - b.r;
            float dg = a.g - b.g;
            float db = a.b - b.b;
            return Mathf.Sqrt(dr * dr + dg * dg + db * db) * 100f;
        }

        private IEnumerator SuccessFeedback()
        {
            if (_mixColorImage == null) yield break;
            var tr = _mixColorImage.transform;
            float duration = 0.3f;
            float elapsed = 0f;
            Color original = _mixColorImage.color;
            Color flashColor = new Color(0.5f, 1f, 0.5f, 1f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
                tr.localScale = Vector3.one * scale;
                _mixColorImage.color = Color.Lerp(original, flashColor, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
            tr.localScale = Vector3.one;
            _mixColorImage.color = original;
        }

        private IEnumerator FailFeedback()
        {
            if (_mixColorImage == null) yield break;
            Color original = _mixColorImage.color;
            Color red = new Color(1f, 0.2f, 0.2f, 1f);
            float duration = 0.25f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _mixColorImage.color = Color.Lerp(red, original, t);
                yield return null;
            }
            _mixColorImage.color = original;

            // Camera shake
            if (_mainCamera != null)
            {
                if (_cameraShakeCoroutine != null) StopCoroutine(_cameraShakeCoroutine);
                _cameraShakeCoroutine = StartCoroutine(CameraShake(0.15f, 0.25f));
            }
        }

        private IEnumerator CameraShake(float amplitude, float duration)
        {
            if (_mainCamera == null) yield break;
            Vector3 origin = _mainCamera.transform.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (_mainCamera == null) yield break;
                float progress = 1f - elapsed / duration;
                float x = Random.Range(-amplitude, amplitude) * progress;
                float y = Random.Range(-amplitude, amplitude) * progress;
                _mainCamera.transform.localPosition = new Vector3(origin.x + x, origin.y + y, origin.z);
                yield return null;
            }
            if (_mainCamera != null)
                _mainCamera.transform.localPosition = origin;
        }

        public void ResetSliders()
        {
            if (_sliderR != null) _sliderR.value = 0f;
            if (_sliderG != null) _sliderG.value = 0f;
            if (_sliderB != null) _sliderB.value = 0f;
            if (_sliderV != null) _sliderV.value = 1f;
            UpdateMixPreview();
        }

        public void ResetForRetry()
        {
            _judgementsUsed = 0;
            _isActive = true;
            ResetSliders();
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active && _dynamicTargetCoroutine != null)
                StopCoroutine(_dynamicTargetCoroutine);
        }

        private void OnDestroy()
        {
            if (_cameraShakeCoroutine != null) StopCoroutine(_cameraShakeCoroutine);
            if (_sliderR != null) _sliderR.onValueChanged.RemoveAllListeners();
            if (_sliderG != null) _sliderG.onValueChanged.RemoveAllListeners();
            if (_sliderB != null) _sliderB.onValueChanged.RemoveAllListeners();
            if (_sliderV != null) _sliderV.onValueChanged.RemoveAllListeners();
        }
    }
}
