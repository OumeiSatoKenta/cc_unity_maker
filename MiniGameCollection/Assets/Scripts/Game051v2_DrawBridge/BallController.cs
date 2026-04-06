using System.Collections;
using UnityEngine;

namespace Game051v2_DrawBridge
{
    public class BallController : MonoBehaviour
    {
        [SerializeField] DrawBridgeGameManager _gameManager;
        [SerializeField] DrawingManager _drawingManager;

        private Rigidbody2D _rb;
        private Vector3 _startPosition;
        private bool _isLaunched = false;
        private bool _hasReportedResult = false;
        private float _bottomLimit;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb != null)
            {
                _rb.simulated = false;
                _rb.gravityScale = 1f;
            }
            _startPosition = transform.position;
        }

        void Start()
        {
            if (Camera.main != null)
            {
                _bottomLimit = -Camera.main.orthographicSize - 1f;
            }
            else
            {
                _bottomLimit = -7f;
            }
        }

        public void ResetBall()
        {
            _isLaunched = false;
            _hasReportedResult = false;
            transform.position = _startPosition;
            if (_rb != null)
            {
                _rb.simulated = false;
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }
        }

        public void Launch()
        {
            if (_rb == null) return;
            _isLaunched = true;
            _rb.simulated = true;
        }

        void FixedUpdate()
        {
            if (!_isLaunched || _hasReportedResult) return;

            // Apply wind force if stage has wind
            if (_drawingManager != null && _drawingManager.HasWind)
            {
                _rb.AddForce(_drawingManager.GetWindForce() * Time.fixedDeltaTime * 10f);
            }

            // Check if ball fell off screen
            if (transform.position.y < _bottomLimit)
            {
                _hasReportedResult = true;
                _isLaunched = false;
                _rb.simulated = false;
                StartCoroutine(ReportFall());
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasReportedResult) return;
            if (other.CompareTag("Goal"))
            {
                _hasReportedResult = true;
                _isLaunched = false;
                _rb.simulated = false;
                StartCoroutine(ReportGoal(other.transform));
            }
        }

        private IEnumerator ReportGoal(Transform goalTransform)
        {
            // Goal pop animation
            yield return StartCoroutine(PopAnimation(goalTransform));
            _gameManager.OnBallReachedGoal();
        }

        private IEnumerator ReportFall()
        {
            // Camera shake
            yield return StartCoroutine(CameraShake());
            // Ball flash red
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.15f);
                sr.color = Color.white;
            }
            _gameManager.OnBallFell();
        }

        private IEnumerator PopAnimation(Transform target)
        {
            Vector3 original = target.localScale;
            float t = 0f;
            float duration = 0.3f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float ratio = t / duration;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(1f, 1.3f, ratio * 2f)
                    : Mathf.Lerp(1.3f, 1f, (ratio - 0.5f) * 2f);
                target.localScale = original * scale;
                yield return null;
            }
            target.localScale = original;
        }

        private IEnumerator CameraShake()
        {
            if (Camera.main == null) yield break;
            Transform camTr = Camera.main.transform;
            Vector3 origPos = camTr.position;
            float elapsed = 0f;
            float duration = 0.2f;
            float amplitude = 0.1f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-amplitude, amplitude);
                float y = Random.Range(-amplitude, amplitude);
                camTr.position = origPos + new Vector3(x, y, 0f);
                yield return null;
            }
            camTr.position = origPos;
        }
    }
}
