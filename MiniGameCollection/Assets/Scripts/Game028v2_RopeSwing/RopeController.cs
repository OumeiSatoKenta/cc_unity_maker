using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game028v2_RopeSwing
{
    public class RopeController : MonoBehaviour
    {
        [SerializeField] RopeSwingGameManager _gameManager;
        [SerializeField] PlatformManager _platformManager;
        [SerializeField] SpriteRenderer _playerSr;

        bool _isActive;
        bool _isGripping;    // holding rope
        bool _isFlying;      // released, in air
        bool _isOnPlatform;  // standing on platform

        // Pendulum state
        Vector2 _anchorPos;
        float _ropeLength;
        float _angle;        // radians from vertical (+ = right)
        float _angularVel;   // radians/second
        const float Gravity = 9.8f;

        // Flying state
        Vector2 _playerPos;
        Vector2 _flyVelocity;

        // Rope line renderer
        LineRenderer _ropeLine;

        // Current anchor index
        int _currentAnchorIndex = -1;

        // Platform tracking
        int _currentPlatformIndex = 0;  // player is on this platform

        bool _stageSetup;
        int _stage;
        bool _variableRopeLength;

        // Coroutines
        Coroutine _landingEffectCo;

        Material _ropeMaterial;

        void Awake()
        {
            _ropeLine = gameObject.AddComponent<LineRenderer>();
            _ropeLine.startWidth = 0.05f;
            _ropeLine.endWidth = 0.05f;
            _ropeLine.positionCount = 2;
            _ropeLine.useWorldSpace = true;
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _ropeMaterial = new Material(shader);
                _ropeLine.material = _ropeMaterial;
            }
            _ropeLine.startColor = new Color(0.6f, 0.4f, 0.2f);
            _ropeLine.endColor = new Color(0.6f, 0.4f, 0.2f);
            _ropeLine.enabled = false;
        }

        void OnDestroy()
        {
            if (_ropeMaterial != null) Destroy(_ropeMaterial);
        }

        public void SetupStage(StageManager.StageConfig config, int stage)
        {
            _stage = stage;
            _variableRopeLength = stage >= 2;
            _isGripping = false;
            _isFlying = false;
            _isOnPlatform = true;
            _currentAnchorIndex = -1;
            _currentPlatformIndex = 0;
            _ropeLine.enabled = false;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active)
            {
                _isGripping = false;
                _isFlying = false;
                _ropeLine.enabled = false;
            }
        }

        void Update()
        {
            if (!_isActive) return;

            UpdatePlayerVisual();
            HandleInput();

            if (_isGripping) UpdatePendulum();
            else if (_isFlying) UpdateFlying();

            DrawRope();
        }

        void HandleInput()
        {
            bool pressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool released = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;

            if (pressed && (_isOnPlatform || _isFlying) && !_isGripping)
            {
                TryGrab();
            }
            else if (released && _isGripping)
            {
                Release();
            }
        }

        void TryGrab()
        {
            var anchors = _platformManager.Anchors;
            if (anchors == null || anchors.Count == 0) return;

            // Find nearest reachable anchor ahead of player
            int best = -1;
            float bestDist = float.MaxValue;
            for (int i = 0; i < anchors.Count; i++)
            {
                float dx = anchors[i].position.x - _playerPos.x;
                if (dx < -0.5f) continue; // behind player
                float dist = Vector2.Distance(_playerPos, anchors[i].position);
                if (dist < anchors[i].ropeLength + 1.5f && dist < bestDist)
                {
                    bestDist = dist;
                    best = i;
                }
            }

            if (best < 0 && anchors.Count > 0)
            {
                // grab nearest anchor
                for (int i = 0; i < anchors.Count; i++)
                {
                    float dist = Vector2.Distance(_playerPos, anchors[i].position);
                    if (dist < bestDist) { bestDist = dist; best = i; }
                }
            }

            if (best < 0) return;

            _currentAnchorIndex = best;
            _anchorPos = anchors[best].position;
            _ropeLength = Mathf.Max(0.1f, anchors[best].ropeLength);

            // Compute initial angle from anchor to player
            Vector2 toPlayer = _playerPos - _anchorPos;
            _angle = Mathf.Atan2(toPlayer.x, -toPlayer.y);
            // Inherit velocity from flying or give slight push
            float initAngularVel = _isFlying
                ? (_flyVelocity.x / _ropeLength) * 0.6f
                : 0.1f;
            _angularVel = initAngularVel;

            _isGripping = true;
            _isFlying = false;
            _isOnPlatform = false;
        }

        void Release()
        {
            // Convert pendulum velocity to linear fly velocity
            float vx = _ropeLength * _angularVel * Mathf.Cos(_angle);
            float vy = _ropeLength * _angularVel * (-Mathf.Sin(_angle));
            _flyVelocity = new Vector2(vx, vy);

            _isGripping = false;
            _isFlying = true;
            _ropeLine.enabled = false;
        }

        void UpdatePendulum()
        {
            // Simple pendulum: d2θ/dt2 = -(g/L)*sin(θ) + wind damping
            float dt = Time.deltaTime;
            float windAcc = _platformManager.WindStrength / _ropeLength;
            float angularAcc = -(Gravity / _ropeLength) * Mathf.Sin(_angle) + windAcc * dt;
            _angularVel += angularAcc * dt;
            _angularVel *= 0.998f; // slight damping
            _angle += _angularVel * dt;

            // Clamp angle to prevent full rotation
            _angle = Mathf.Clamp(_angle, -Mathf.PI * 0.6f, Mathf.PI * 0.6f);

            _playerPos = new Vector2(
                _anchorPos.x + _ropeLength * Mathf.Sin(_angle),
                _anchorPos.y - _ropeLength * Mathf.Cos(_angle)
            );

            CheckFallOffScreen();
        }

        void UpdateFlying()
        {
            float dt = Time.deltaTime;
            _flyVelocity.y -= Gravity * dt;
            _flyVelocity.x += _platformManager.WindStrength * dt * 0.3f;
            _playerPos += _flyVelocity * dt;

            // Check landing on platforms
            CheckLanding();
            CheckFallOffScreen();
        }

        void CheckLanding()
        {
            var platforms = _platformManager.Platforms;
            if (platforms == null) return;

            for (int i = 0; i < platforms.Count; i++)
            {
                var pd = platforms[i];
                if (pd.collapsed || !pd.go.activeSelf) continue;

                // Get current platform center (may be moving)
                float px = pd.go.transform.position.x;
                float halfW = pd.width * 0.5f;
                float topY = pd.topY;

                bool inX = _playerPos.x > px - halfW && _playerPos.x < px + halfW;
                bool atY = _flyVelocity.y < 0 && _playerPos.y <= topY + 0.1f && _playerPos.y >= topY - 0.4f;

                if (!inX || !atY) continue;

                // Land!
                _playerPos.y = topY;
                _flyVelocity = Vector2.zero;
                _isFlying = false;
                _isOnPlatform = true;
                _currentPlatformIndex = i;

                float accuracy = Mathf.Abs(_playerPos.x - px) / halfW; // 0=center, 1=edge

                if (pd.isGoal)
                {
                    _gameManager.OnGoalReached();
                }
                else
                {
                    _gameManager.OnLanding(accuracy);
                    if (pd.isCollapsing) _platformManager.TriggerCollapse(pd);
                }
                return;
            }
        }

        void CheckFallOffScreen()
        {
            var cam = Camera.main;
            if (cam == null) return;
            if (_playerPos.y < -cam.orthographicSize - 1f)
                _gameManager.OnPlayerFall();
        }

        void UpdatePlayerVisual()
        {
            if (_playerSr != null)
                _playerSr.transform.position = _playerPos;
        }

        void DrawRope()
        {
            if (!_isGripping) { _ropeLine.enabled = false; return; }
            _ropeLine.enabled = true;
            _ropeLine.SetPosition(0, _anchorPos);
            _ropeLine.SetPosition(1, _playerPos);
        }

        public void PlayLandingEffect()
        {
            if (_landingEffectCo != null) StopCoroutine(_landingEffectCo);
            _landingEffectCo = StartCoroutine(LandingPulse());
        }

        IEnumerator LandingPulse()
        {
            if (_playerSr == null) yield break;
            Transform t = _playerSr.transform;
            Vector3 origScale = t.localScale;
            float elapsed = 0f;
            float dur = 0.2f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float s = 1f + 0.3f * Mathf.Sin(elapsed / dur * Mathf.PI);
                t.localScale = origScale * s;
                yield return null;
            }
            t.localScale = origScale;
        }

        // Called by SceneSetup to place player on first platform
        public void PlaceOnFirstPlatform()
        {
            var platforms = _platformManager.Platforms;
            if (platforms == null || platforms.Count == 0) return;
            var first = platforms[0];
            _playerPos = new Vector2(first.go.transform.position.x, first.topY);
            _currentPlatformIndex = 0;
            _isOnPlatform = true;
            UpdatePlayerVisual();
        }

        public Vector2 PlayerPosition => _playerPos;
        public void SetInitialPosition(Vector2 pos) { _playerPos = pos; }
    }
}
