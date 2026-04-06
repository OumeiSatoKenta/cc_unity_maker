using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game048v2_GlassBall
{
    /// <summary>
    /// Controls the glass ball: waypoint-based movement along rail path,
    /// impact tracking, coin collection, goal detection.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class GlassBallController : MonoBehaviour
    {
        [SerializeField] GlassBallGameManager _gameManager;
        [SerializeField] GlassBallUI _ui;
        [SerializeField] SpriteRenderer _spriteRenderer;
        [SerializeField] RailManager _railManager;

        // Impact
        public float ImpactPercent { get; private set; }
        private float _impactMultiplier = 1f;
        private bool _isRolling;

        // Waypoints
        private List<Vector3> _waypoints;
        private int _waypointIndex;
        private float _speed = 4f;
        private float _baseSpeed = 4f;

        // Stage
        private bool _hasWind;
        private float _windForce;
        private bool _hasThinIce;

        // Crack visual
        private Color _baseColor = new Color(0.7f, 0.9f, 1f, 0.85f);
        private Coroutine _flashCoroutine;
        private Coroutine _impactFlashCoroutine;

        // Start position for reset
        private Vector3 _startPosition;

        private Rigidbody2D _rb;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _startPosition = transform.position;
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            StopAllCoroutines();
            _flashCoroutine = null;
            _impactFlashCoroutine = null;

            ImpactPercent = 0f;
            _impactMultiplier = 1f;
            _isRolling = false;
            _hasWind = stageNumber >= 5;
            _windForce = _hasWind ? 1.2f : 0f;
            _hasThinIce = stageNumber >= 5;
            _baseSpeed = 4f * config.speedMultiplier;
            _speed = _baseSpeed;

            if (_spriteRenderer != null)
                _spriteRenderer.color = _baseColor;
            transform.localScale = Vector3.one;

            transform.position = _startPosition;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;

            if (_ui != null) _ui.UpdateImpact(0f);
        }

        public void Launch(List<Vector3> waypoints, float speedMultiplier)
        {
            if (waypoints == null || waypoints.Count < 2) return;
            _waypoints = new List<Vector3>(waypoints);
            _waypointIndex = 0;
            _isRolling = true;
            _speed = _baseSpeed * speedMultiplier;
        }

        public void ResetBall()
        {
            StopAllCoroutines();
            _flashCoroutine = null;
            _impactFlashCoroutine = null;

            _isRolling = false;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            transform.position = _startPosition;
            transform.localScale = Vector3.one;
            ImpactPercent = 0f;
            _impactMultiplier = 1f;
            if (_spriteRenderer != null)
                _spriteRenderer.color = _baseColor;
            if (_ui != null) _ui.UpdateImpact(0f);
        }

        void Update()
        {
            if (!_isRolling) return;

            // Wind force (stage 5)
            if (_hasWind)
            {
                _rb.AddForce(new Vector2(_windForce * Mathf.Sin(Time.time * 1.5f), 0f), ForceMode2D.Force);
            }

            // Move toward next waypoint
            if (_waypoints != null && _waypointIndex < _waypoints.Count)
            {
                Vector3 target = _waypoints[_waypointIndex];
                Vector3 dir = (target - transform.position).normalized;
                _rb.linearVelocity = new Vector2(dir.x * _speed, dir.y * _speed);

                if (Vector3.Distance(transform.position, target) < 0.15f)
                {
                    _waypointIndex++;
                }
            }
            else if (_waypoints != null && _waypointIndex >= _waypoints.Count)
            {
                // Reached end of rail - ball stops; player must redraw
                _rb.linearVelocity = Vector2.zero;
            }

            // Impact flash when high
            if (ImpactPercent >= 80f && _flashCoroutine == null)
            {
                _flashCoroutine = StartCoroutine(DangerFlash());
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isRolling) return;

            if (other.CompareTag("Goal"))
            {
                _isRolling = false;
                _rb.linearVelocity = Vector2.zero;
                StartCoroutine(GoalEffect());
                float inkPercent = _railManager != null ? _railManager.GetInkPercent() : 50f;
                _gameManager.TriggerStageClear(ImpactPercent, inkPercent);
            }
            else if (other.CompareTag("Coin"))
            {
                StartCoroutine(CoinEffect(other.gameObject));
                _gameManager.OnCoinCollected();
            }
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (!_isRolling) return;

            float impact = col.relativeVelocity.magnitude * 10f * _impactMultiplier;
            if (_hasThinIce && col.gameObject.CompareTag("ThinIce"))
                impact *= 2f;

            AddImpact(impact);
        }

        public void AddImpactFromNail(float amount)
        {
            AddImpact(amount * _impactMultiplier);
        }

        void AddImpact(float amount)
        {
            if (!_isRolling) return;

            ImpactPercent = Mathf.Min(100f, ImpactPercent + amount);
            if (_ui != null) _ui.UpdateImpact(ImpactPercent / 100f);

            if (_impactFlashCoroutine != null)
            {
                StopCoroutine(_impactFlashCoroutine);
                _impactFlashCoroutine = null;
            }
            _impactFlashCoroutine = StartCoroutine(ImpactFlash());

            if (ImpactPercent >= 100f)
            {
                _isRolling = false;
                _rb.linearVelocity = Vector2.zero;
                StartCoroutine(BreakEffect());
                _gameManager.TriggerGameOver();
            }
        }

        IEnumerator ImpactFlash()
        {
            if (_spriteRenderer == null) { _impactFlashCoroutine = null; yield break; }
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            if (_spriteRenderer != null)
                _spriteRenderer.color = _baseColor;
            _impactFlashCoroutine = null;
        }

        IEnumerator DangerFlash()
        {
            while (ImpactPercent >= 80f && _isRolling)
            {
                if (_spriteRenderer != null)
                    _spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 0.9f);
                yield return new WaitForSeconds(0.2f);
                if (_spriteRenderer != null)
                    _spriteRenderer.color = _baseColor;
                yield return new WaitForSeconds(0.2f);
            }
            _flashCoroutine = null;
        }

        IEnumerator CoinEffect(GameObject coin)
        {
            float t = 0f;
            var sr = coin.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                while (t < 0.15f)
                {
                    t += Time.deltaTime;
                    float scale = Mathf.Lerp(1f, 0f, t / 0.15f);
                    coin.transform.localScale = Vector3.one * scale;
                    yield return null;
                }
            }
            coin.SetActive(false);
        }

        IEnumerator GoalEffect()
        {
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float s = 1f + 0.3f * Mathf.Sin(t / 0.3f * Mathf.PI);
                transform.localScale = Vector3.one * s;
                yield return null;
            }
            transform.localScale = Vector3.one;
        }

        IEnumerator BreakEffect()
        {
            float t = 0f;
            if (_spriteRenderer != null)
                _spriteRenderer.color = Color.red;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1f, 0f, t / 0.4f);
                transform.localScale = Vector3.one * s;
                yield return null;
            }
        }
    }
}
