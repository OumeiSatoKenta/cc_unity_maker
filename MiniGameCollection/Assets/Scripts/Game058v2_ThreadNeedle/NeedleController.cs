using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Game058v2_ThreadNeedle
{
    public class NeedleController : MonoBehaviour
    {
        [SerializeField] ThreadNeedleGameManager _gameManager;
        [SerializeField] ThreadNeedleUI _ui;

        [SerializeField] Transform _needleTransform;
        [SerializeField] Transform _threadLaunchPoint;
        [SerializeField] SpriteRenderer _needleHoleRenderer;

        // Stage config
        float _swingSpeed = 1.0f;
        float _maxAngle = 30f;
        float _holeSize = 0.8f;
        bool _rotationEnabled = false;
        float _rotSpeed = 0f;
        bool _dualHoleMode = false;
        bool _irregularSwing = false;
        int _totalRounds = 3;
        int _currentRound = 0;

        // Dual hole state
        bool _waitingForSecondHole = false;

        bool _isActive = false;
        bool _isShooting = false;
        float _needleRotation = 0f;

        // Thread visual
        [SerializeField] Transform _threadVisual;
        [SerializeField] SpriteRenderer _threadRenderer;

        // Needle Y position (computed in SetupStage)
        float _needleY = 0f;
        float _launchY = 0f;

        void Update()
        {
            if (!_isActive) return;
            if (_isShooting) return;

            // Swing needle
            float angle;
            if (_irregularSwing)
            {
                // Composite wave for irregular movement
                angle = Mathf.Sin(Time.time * _swingSpeed) * _maxAngle
                      + Mathf.Sin(Time.time * _swingSpeed * 0.7f) * (_maxAngle * 0.4f)
                      + Mathf.Sin(Time.time * _swingSpeed * 1.3f) * (_maxAngle * 0.2f);
            }
            else
            {
                angle = Mathf.Sin(Time.time * _swingSpeed) * _maxAngle;
            }

            if (_needleTransform != null)
            {
                Vector3 eulerAngles = _needleTransform.eulerAngles;
                eulerAngles.z = angle;
                _needleTransform.eulerAngles = eulerAngles;
            }

            // Hole rotation
            if (_rotationEnabled && _needleHoleRenderer != null)
            {
                _needleRotation += _rotSpeed * Time.deltaTime;
                Vector3 holeEuler = _needleHoleRenderer.transform.eulerAngles;
                holeEuler.z = _needleRotation;
                _needleHoleRenderer.transform.eulerAngles = holeEuler;
            }

            // Input detection
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnTap();
            }
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            _isActive = false;
            _isShooting = false;
            _currentRound = 0;
            _waitingForSecondHole = false;
            _needleRotation = 0f;

            // Stage-specific parameters
            switch (stageNumber)
            {
                case 1:
                    _swingSpeed = 1.0f; _maxAngle = 30f; _holeSize = 0.8f;
                    _rotationEnabled = false; _dualHoleMode = false; _irregularSwing = false;
                    _totalRounds = 3;
                    break;
                case 2:
                    _swingSpeed = 1.5f; _maxAngle = 40f; _holeSize = 0.6f;
                    _rotationEnabled = false; _dualHoleMode = false; _irregularSwing = false;
                    _totalRounds = 5;
                    break;
                case 3:
                    _swingSpeed = 2.0f; _maxAngle = 35f; _holeSize = 0.45f;
                    _rotationEnabled = true; _rotSpeed = 45f; _dualHoleMode = false; _irregularSwing = false;
                    _totalRounds = 5;
                    break;
                case 4:
                    _swingSpeed = 2.5f; _maxAngle = 40f; _holeSize = 0.35f;
                    _rotationEnabled = false; _dualHoleMode = true; _irregularSwing = false;
                    _totalRounds = 5;
                    break;
                case 5:
                    _swingSpeed = 3.0f; _maxAngle = 50f; _holeSize = 0.25f;
                    _rotationEnabled = true; _rotSpeed = 90f; _dualHoleMode = false; _irregularSwing = true;
                    _totalRounds = 7;
                    break;
                default:
                    _swingSpeed = config.speedMultiplier; _maxAngle = 30f; _holeSize = 0.5f;
                    _rotationEnabled = false; _dualHoleMode = false; _irregularSwing = false;
                    _totalRounds = 3;
                    break;
            }

            // Apply StageConfig speed multiplier on top
            _swingSpeed *= config.speedMultiplier * 0.3f + 0.7f;

            // Responsive positioning
            if (Camera.main != null)
            {
                float camSize = Camera.main.orthographicSize;
                _needleY = camSize * 0.3f;
                _launchY = -camSize * 0.5f;
                if (_needleTransform != null)
                    _needleTransform.position = new Vector3(0, _needleY, 0);
                if (_threadLaunchPoint != null)
                    _threadLaunchPoint.position = new Vector3(0, _launchY, 0);
            }

            // Update hole scale
            if (_needleHoleRenderer != null)
                _needleHoleRenderer.transform.localScale = new Vector3(_holeSize, _holeSize, 1f);

            // Hide thread
            if (_threadVisual != null)
                _threadVisual.gameObject.SetActive(false);

            _ui?.UpdateRound(0, _totalRounds);
            _isActive = true;
        }

        public void StopNeedle()
        {
            _isActive = false;
        }

        void OnTap()
        {
            if (!_isActive || _isShooting) return;
            if (!_gameManager.IsPlaying()) return;

            StartCoroutine(ShootThread());
        }

        IEnumerator ShootThread()
        {
            _isShooting = true;

            // Compute needle hole world position
            Vector3 holePos = _needleHoleRenderer != null
                ? _needleHoleRenderer.transform.position
                : (_needleTransform != null ? _needleTransform.position : Vector3.zero);

            // Show thread visual extending from launch point to needle
            if (_threadVisual != null && _threadLaunchPoint != null)
            {
                _threadVisual.gameObject.SetActive(true);
                Vector3 launchPos = _threadLaunchPoint.position;
                float dist = Vector3.Distance(launchPos, holePos);
                Vector3 mid = (launchPos + holePos) * 0.5f;
                _threadVisual.position = mid;
                float angle = Mathf.Atan2(holePos.y - launchPos.y, holePos.x - launchPos.x) * Mathf.Rad2Deg - 90f;
                _threadVisual.rotation = Quaternion.Euler(0, 0, angle);

                // Animate thread growing
                float elapsed = 0f;
                float duration = 0.15f;
                if (_threadRenderer != null)
                    _threadRenderer.size = new Vector2(0.1f, 0f);

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    if (_threadRenderer != null)
                        _threadRenderer.size = new Vector2(0.1f, dist * t);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.05f);

            // Judge using actual world position of the needle hole
            float holeWorldX = _needleHoleRenderer != null
                ? _needleHoleRenderer.transform.position.x
                : (_needleTransform != null ? _needleTransform.position.x : 0f);
            float launchX = _threadLaunchPoint != null ? _threadLaunchPoint.position.x : 0f;
            float deviation = Mathf.Abs(holeWorldX - launchX);

            float holeSizeWorld = _holeSize * 0.5f;
            float centerThreshold = holeSizeWorld * 0.2f;
            float successThreshold = holeSizeWorld * 0.5f;

            bool isCenter = deviation <= centerThreshold;
            bool isSuccess = deviation <= successThreshold;

            if (isCenter)
            {
                _ui?.ShowJudgement("CENTER!", new Color(1f, 0.85f, 0f));
                _gameManager.AddScore(200, true);
                StartCoroutine(HolePop());
                if (_dualHoleMode && !_waitingForSecondHole)
                {
                    _waitingForSecondHole = true;
                    _isShooting = false;
                    yield break;
                }
                AdvanceRound();
            }
            else if (isSuccess)
            {
                _ui?.ShowJudgement("SUCCESS", new Color(0.2f, 0.9f, 0.3f));
                _gameManager.AddScore(100, false);
                StartCoroutine(HolePop());
                if (_dualHoleMode && !_waitingForSecondHole)
                {
                    _waitingForSecondHole = true;
                    _isShooting = false;
                    yield break;
                }
                AdvanceRound();
            }
            else
            {
                _ui?.ShowJudgement("MISS", new Color(0.9f, 0.2f, 0.2f));
                _waitingForSecondHole = false;
                StartCoroutine(HoleMissFlash());
                _isShooting = false;
                if (_threadVisual != null) _threadVisual.gameObject.SetActive(false);
                _gameManager.OnMiss();
                yield break;
            }

            yield return new WaitForSeconds(0.4f);

            if (_threadVisual != null)
                _threadVisual.gameObject.SetActive(false);

            _isShooting = false;
        }

        void AdvanceRound()
        {
            _waitingForSecondHole = false;
            _currentRound++;
            _ui?.UpdateRound(_currentRound, _totalRounds);
            if (_currentRound >= _totalRounds)
            {
                _isActive = false;
                _gameManager.OnStageClear();
            }
        }

        IEnumerator HolePop()
        {
            if (_needleHoleRenderer == null) yield break;
            Vector3 baseScale = new Vector3(_holeSize, _holeSize, 1f);
            Vector3 bigScale = baseScale * 1.4f;
            float elapsed = 0f;
            float half = 0.1f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                _needleHoleRenderer.transform.localScale = Vector3.Lerp(baseScale, bigScale, elapsed / half);
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                _needleHoleRenderer.transform.localScale = Vector3.Lerp(bigScale, baseScale, elapsed / half);
                yield return null;
            }
            _needleHoleRenderer.transform.localScale = baseScale;
        }

        IEnumerator HoleMissFlash()
        {
            if (_needleHoleRenderer == null) yield break;
            Color original = _needleHoleRenderer.color;
            _needleHoleRenderer.color = new Color(1f, 0.2f, 0.2f, 1f);
            yield return new WaitForSeconds(0.15f);
            _needleHoleRenderer.color = original;
        }
    }
}
