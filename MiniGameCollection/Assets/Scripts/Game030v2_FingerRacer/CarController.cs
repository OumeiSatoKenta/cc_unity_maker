using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Game030v2_FingerRacer
{
    public class CarController : MonoBehaviour
    {
        [SerializeField] FingerRacerGameManager _gameManager;
        [SerializeField] SpriteRenderer _carSr;
        [SerializeField] Transform _goalMarker;

        Vector3[] _coursePoints;
        int _currentPointIndex;
        float _baseSpeed = 3.5f;
        float _speedMultiplier = 1f;
        bool _isRacing;
        bool _isBoosting;
        bool _isSandMode;
        bool _isSpinning;
        int _boostCount = 3;
        int _maxBoostCount = 3;
        float _boostTimer;
        float _courseRadius;
        int _stageIndex;

        const float BoostDuration = 0.5f;
        const float BoostSpeedMultiplier = 1.8f;
        const float SandSpeedMultiplier = 0.7f;
        const float GoalReachDistance = 0.6f;

        Coroutine _boostCo;
        Coroutine _spinCo;
        Coroutine _pulseCo;

        void Update()
        {
            if (!_isRacing) return;

            HandleBoostInput();
            MoveCar();
        }

        void HandleBoostInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (_boostCount <= 0) return;
            if (mouse.leftButton.wasPressedThisFrame)
            {
                TriggerBoost();
            }
        }

        public void TriggerBoost()
        {
            if (!_isRacing || _boostCount <= 0 || _isSpinning) return;
            _boostCount--;

            // Check if currently in a curve
            bool inCurve = IsCurveSegment();
            if (inCurve)
            {
                _gameManager.OnBoostFail();
                if (_spinCo != null) StopCoroutine(_spinCo);
                _spinCo = StartCoroutine(SpinOut());
            }
            else
            {
                _gameManager.OnBoostSuccess();
                if (_boostCo != null) StopCoroutine(_boostCo);
                _boostCo = StartCoroutine(BoostEffect());
            }
            _gameManager?.NotifyBoostUpdate(_boostCount, _maxBoostCount);
        }

        bool IsCurveSegment()
        {
            if (_coursePoints == null || _currentPointIndex < 1) return false;
            int prev = Mathf.Max(0, _currentPointIndex - 2);
            int curr = _currentPointIndex;
            int next = Mathf.Min(_coursePoints.Length - 1, _currentPointIndex + 2);
            Vector3 a = (_coursePoints[curr] - _coursePoints[prev]).normalized;
            Vector3 b = (_coursePoints[next] - _coursePoints[curr]).normalized;
            float angle = Vector3.Angle(a, b);
            return angle > 30f;
        }

        IEnumerator BoostEffect()
        {
            _isBoosting = true;
            // Scale pulse
            if (_pulseCo != null) StopCoroutine(_pulseCo);
            _pulseCo = StartCoroutine(ScalePulse(1.2f, BoostDuration));
            if (_carSr != null) _carSr.color = new Color(1f, 0.85f, 0.1f);
            yield return new WaitForSeconds(BoostDuration);
            _isBoosting = false;
            if (_carSr != null) _carSr.color = Color.white;
        }

        IEnumerator SpinOut()
        {
            _isSpinning = true;
            float elapsed = 0f;
            float spinDuration = 0.8f;
            if (_carSr != null) _carSr.color = new Color(1f, 0.3f, 0.3f);
            while (elapsed < spinDuration)
            {
                elapsed += Time.deltaTime;
                transform.Rotate(0, 0, 720f * Time.deltaTime);
                yield return null;
            }
            _isSpinning = false;
            if (_carSr != null) _carSr.color = Color.white;
            _gameManager.OnCourseOut();
        }

        IEnumerator ScalePulse(float maxScale, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float s = t < 0.5f ? Mathf.Lerp(1f, maxScale, t * 2f) : Mathf.Lerp(maxScale, 1f, (t - 0.5f) * 2f);
                transform.localScale = Vector3.one * s;
                yield return null;
            }
            transform.localScale = Vector3.one;
        }

        void MoveCar()
        {
            if (_coursePoints == null || _currentPointIndex >= _coursePoints.Length) return;

            float speed = _baseSpeed * _speedMultiplier;
            if (_isBoosting) speed *= BoostSpeedMultiplier;
            if (_isSandMode) speed *= SandSpeedMultiplier;
            if (_isSpinning) speed = 0f;

            Vector3 target = _coursePoints[_currentPointIndex];
            Vector3 dir = (target - transform.position).normalized;

            // Rotate towards movement direction
            if (dir.magnitude > 0.01f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 10f);
            }

            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                _currentPointIndex++;
                if (_currentPointIndex >= _coursePoints.Length)
                {
                    OnReachGoal();
                }
            }
        }

        void OnReachGoal()
        {
            _isRacing = false;
            if (_pulseCo != null) StopCoroutine(_pulseCo);
            StartCoroutine(GoalCelebration());
            _gameManager.OnGoalReached();
        }

        IEnumerator GoalCelebration()
        {
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 1f;
                Color c = Color.HSVToRGB(t, 1f, 1f);
                if (_carSr != null) _carSr.color = c;
                float s = 1f + Mathf.Sin(elapsed * Mathf.PI * 4f) * 0.2f;
                transform.localScale = Vector3.one * s;
                yield return null;
            }
            if (_carSr != null) _carSr.color = Color.white;
            transform.localScale = Vector3.one;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _speedMultiplier = config.speedMultiplier;
            _stageIndex = stageIndex;
            _boostCount = _maxBoostCount;
            _isRacing = false;
            _isBoosting = false;
            _isSandMode = false;
            _isSpinning = false;
            _coursePoints = null;
            _currentPointIndex = 0;
            if (_carSr != null) _carSr.color = Color.white;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
        }

        public void StartRace(Vector3[] coursePoints)
        {
            _coursePoints = coursePoints;
            _currentPointIndex = 0;
            _isRacing = true;
            _isBoosting = false;
            _isSandMode = false;
            _isSpinning = false;
            _boostCount = _maxBoostCount;
            if (coursePoints.Length > 0)
                transform.position = coursePoints[0];
            _gameManager?.NotifyBoostUpdate(_boostCount, _maxBoostCount);
        }

        public void StopRace()
        {
            _isRacing = false;
        }

        public void SetSandMode(bool on)
        {
            _isSandMode = on;
        }
    }
}
