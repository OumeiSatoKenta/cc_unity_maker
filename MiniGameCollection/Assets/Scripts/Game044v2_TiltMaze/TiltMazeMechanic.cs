using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game044v2_TiltMaze
{
    public class TiltMazeMechanic : MonoBehaviour
    {
        [SerializeField] TiltMazeGameManager _gameManager;
        [SerializeField] TiltMazeUI _ui;

        // Sprites
        [SerializeField] Sprite _spriteBall;
        [SerializeField] Sprite _spriteWall;
        [SerializeField] Sprite _spriteHole;
        [SerializeField] Sprite _spriteGoal;
        [SerializeField] Sprite _spriteCoin;
        [SerializeField] Sprite _spriteIceFloor;
        [SerializeField] Sprite _spriteWarpIn;
        [SerializeField] Sprite _spriteWarpOut;

        bool _isActive;
        int _currentStage;
        float _timeLimit;
        float _remainingTime;
        float _maxAngle;
        float _ballFriction;

        // Drag
        bool _isDragging;
        Vector2 _dragStartScreen;
        float _mazeStartAngle;
        float _holdTimer;
        float _brakeGauge = 1f;
        bool _braking;

        // Objects
        GameObject _mazeRoot;
        GameObject _ball;
        Rigidbody2D _ballRb;
        SpriteRenderer _ballSr;
        Vector2 _ballStartLocalPos;
        List<GameObject> _holes = new List<GameObject>();
        List<GameObject> _coins = new List<GameObject>();
        List<MovingWallController> _movingWalls = new List<MovingWallController>();
        GameObject _warpIn;
        GameObject _warpOut;
        GameObject _iceFloor;
        bool _ballInHoleCooldown;

        // Stage config arrays
        static readonly float[] MaxAngles = { 30f, 35f, 40f, 40f, 45f };
        static readonly float[] BallFrictions = { 0.4f, 0.4f, 0.35f, 0.35f, 0.3f };

        // --- Maze layout data (local positions relative to MazeRoot center) ---
        // Each stage defines: walls(pos, rot, scale), holes(pos), coins(pos), goal(pos)
        // Ball start pos

        public void SetActive(bool active)
        {
            _isActive = active;
            if (_ballRb != null) _ballRb.simulated = active;
        }

        public void SetupStage(int stage, float timeLimit)
        {
            _currentStage = stage;
            _timeLimit = timeLimit;
            _remainingTime = timeLimit;
            _maxAngle = MaxAngles[Mathf.Clamp(stage, 0, MaxAngles.Length - 1)];
            _ballFriction = BallFrictions[Mathf.Clamp(stage, 0, BallFrictions.Length - 1)];
            _brakeGauge = 1f;
            _braking = false;
            _isDragging = false;
            _holdTimer = 0f;
            _ballInHoleCooldown = false;

            if (_mazeRoot != null) Destroy(_mazeRoot);
            _holes.Clear();
            _coins.Clear();
            _movingWalls.Clear();
            _warpIn = null;
            _warpOut = null;
            _iceFloor = null;

            BuildMaze(stage);
            _isActive = true;
            if (_ballRb != null) _ballRb.simulated = true;
        }

        void BuildMaze(int stage)
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 2.8f;
            float mazeH = (camSize * 2f) - topMargin - bottomMargin; // ~6.0
            float mazeW = Mathf.Min(camWidth * 2f - 0.4f, mazeH);   // square or width-bound
            float halfH = mazeH * 0.5f;
            float halfW = mazeW * 0.5f;
            float mazeY = camSize - topMargin - halfH; // center of game area

            _mazeRoot = new GameObject("MazeRoot");
            _mazeRoot.transform.position = new Vector3(0f, mazeY, 0f);

            float wallThick = 0.18f;
            float wallLen = mazeW;

            // Border walls
            CreateWall(_mazeRoot.transform, new Vector2(0, halfH), 0f, new Vector2(wallLen, wallThick), true);
            CreateWall(_mazeRoot.transform, new Vector2(0, -halfH), 0f, new Vector2(wallLen, wallThick), true);
            CreateWall(_mazeRoot.transform, new Vector2(-halfW, 0), 90f, new Vector2(mazeH, wallThick), true);
            CreateWall(_mazeRoot.transform, new Vector2(halfW, 0), 90f, new Vector2(mazeH, wallThick), true);

            float s = mazeW; // scale reference

            // Stage-specific inner layout
            if (stage == 0)
            {
                // Simple: 3 inner walls, 2 holes, 1 goal
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.3f, halfH*0.2f), 0f, new Vector2(s*0.55f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.3f, -halfH*0.2f), 0f, new Vector2(s*0.55f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(0f, 0f), 90f, new Vector2(mazeH*0.3f, wallThick), true);

                CreateHole(new Vector2(-halfW*0.6f, -halfH*0.5f));
                CreateHole(new Vector2(halfW*0.6f, halfH*0.5f));
                CreateGoal(new Vector2(halfW*0.65f, -halfH*0.65f));
                CreateBall(new Vector2(-halfW*0.65f, halfH*0.65f));
            }
            else if (stage == 1)
            {
                // Coins added
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.25f, halfH*0.25f), 0f, new Vector2(s*0.5f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.25f, -halfH*0.1f), 0f, new Vector2(s*0.5f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.1f, halfH*0.4f), 90f, new Vector2(mazeH*0.35f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.4f, -halfH*0.35f), 90f, new Vector2(mazeH*0.3f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(0f, 0f), 0f, new Vector2(s*0.2f, wallThick), true);

                CreateHole(new Vector2(-halfW*0.55f, halfH*0.5f));
                CreateHole(new Vector2(halfW*0.5f, halfH*0.4f));
                CreateHole(new Vector2(0f, -halfH*0.6f));
                CreateCoin(new Vector2(-halfW*0.2f, -halfH*0.35f));
                CreateCoin(new Vector2(halfW*0.45f, -halfH*0.45f));
                CreateCoin(new Vector2(-halfW*0.5f, halfH*0.0f));
                CreateGoal(new Vector2(halfW*0.65f, -halfH*0.7f));
                CreateBall(new Vector2(-halfW*0.65f, halfH*0.65f));
            }
            else if (stage == 2)
            {
                // Moving wall
                CreateWall(_mazeRoot.transform, new Vector2(0f, halfH*0.3f), 0f, new Vector2(s*0.45f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.2f, -halfH*0.2f), 0f, new Vector2(s*0.45f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.2f, halfH*0.0f), 90f, new Vector2(mazeH*0.35f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.5f, halfH*0.1f), 90f, new Vector2(mazeH*0.25f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.1f, -halfH*0.45f), 0f, new Vector2(s*0.3f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.1f, halfH*0.6f), 0f, new Vector2(s*0.25f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.5f, halfH*0.5f), 90f, new Vector2(mazeH*0.2f, wallThick), true);

                // Moving wall
                var mw = CreateWall(_mazeRoot.transform, new Vector2(halfW*0.35f, -halfH*0.2f), 90f, new Vector2(mazeH*0.28f, wallThick), true);
                var mwc = mw.AddComponent<MovingWallController>();
                mwc.Init(new Vector2(halfW*0.35f, -halfH*0.2f), new Vector2(halfW*0.35f, halfH*0.2f), 1.5f);
                _movingWalls.Add(mwc);

                CreateHole(new Vector2(-halfW*0.6f, -halfH*0.55f));
                CreateHole(new Vector2(halfW*0.6f, halfH*0.55f));
                CreateHole(new Vector2(-halfW*0.1f, halfH*0.0f));
                CreateHole(new Vector2(halfW*0.0f, -halfH*0.6f));
                CreateCoin(new Vector2(-halfW*0.4f, halfH*0.5f));
                CreateCoin(new Vector2(halfW*0.6f, -halfH*0.35f));
                CreateCoin(new Vector2(-halfW*0.6f, halfH*0.0f));
                CreateGoal(new Vector2(halfW*0.7f, -halfH*0.7f));
                CreateBall(new Vector2(-halfW*0.7f, halfH*0.7f));
            }
            else if (stage == 3)
            {
                // Warp holes
                CreateWall(_mazeRoot.transform, new Vector2(0f, halfH*0.25f), 0f, new Vector2(s*0.5f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.3f, -halfH*0.15f), 0f, new Vector2(s*0.4f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.3f, halfH*0.05f), 90f, new Vector2(mazeH*0.3f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.5f, halfH*0.15f), 90f, new Vector2(mazeH*0.25f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.1f, -halfH*0.5f), 0f, new Vector2(s*0.35f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.1f, halfH*0.6f), 0f, new Vector2(s*0.3f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.55f, halfH*0.45f), 90f, new Vector2(mazeH*0.2f, wallThick), true);

                // Warp
                _warpIn = CreateSpecialObject(new Vector2(-halfW*0.6f, -halfH*0.0f), 0.45f, _spriteWarpIn, "WarpIn", false);
                if (_warpIn != null) { var c = _warpIn.AddComponent<CircleCollider2D>(); c.isTrigger = true; c.radius = 0.5f; }
                _warpOut = CreateSpecialObject(new Vector2(halfW*0.6f, halfH*0.15f), 0.45f, _spriteWarpOut, "WarpOut", false);

                CreateHole(new Vector2(-halfW*0.0f, halfH*0.0f));
                CreateHole(new Vector2(halfW*0.5f, -halfH*0.5f));
                CreateHole(new Vector2(-halfW*0.5f, -halfH*0.5f));
                CreateHole(new Vector2(halfW*0.0f, halfH*0.5f));
                CreateCoin(new Vector2(-halfW*0.3f, halfH*0.55f));
                CreateCoin(new Vector2(halfW*0.3f, -halfH*0.55f));
                CreateCoin(new Vector2(halfW*0.55f, halfH*0.7f));
                CreateGoal(new Vector2(halfW*0.7f, -halfH*0.75f));
                CreateBall(new Vector2(-halfW*0.7f, halfH*0.7f));
            }
            else // stage 4
            {
                // Ice floor + all elements
                CreateWall(_mazeRoot.transform, new Vector2(0f, halfH*0.3f), 0f, new Vector2(s*0.45f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.25f, -halfH*0.1f), 0f, new Vector2(s*0.4f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.15f, halfH*0.05f), 90f, new Vector2(mazeH*0.3f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.5f, halfH*0.2f), 90f, new Vector2(mazeH*0.25f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.05f, -halfH*0.45f), 0f, new Vector2(s*0.35f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.05f, halfH*0.6f), 0f, new Vector2(s*0.3f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(halfW*0.6f, halfH*0.5f), 90f, new Vector2(mazeH*0.2f, wallThick), true);
                CreateWall(_mazeRoot.transform, new Vector2(-halfW*0.2f, -halfH*0.6f), 0f, new Vector2(s*0.3f, wallThick), true);

                // Moving wall
                var mw = CreateWall(_mazeRoot.transform, new Vector2(halfW*0.4f, -halfH*0.25f), 90f, new Vector2(mazeH*0.25f, wallThick), true);
                var mwc = mw.AddComponent<MovingWallController>();
                mwc.Init(new Vector2(halfW*0.4f, -halfH*0.4f), new Vector2(halfW*0.4f, halfH*0.0f), 1.5f);
                _movingWalls.Add(mwc);

                // Warp
                _warpIn = CreateSpecialObject(new Vector2(-halfW*0.55f, halfH*0.55f), 0.45f, _spriteWarpIn, "WarpIn", false);
                if (_warpIn != null) { var c = _warpIn.AddComponent<CircleCollider2D>(); c.isTrigger = true; c.radius = 0.5f; }
                _warpOut = CreateSpecialObject(new Vector2(halfW*0.55f, -halfH*0.0f), 0.45f, _spriteWarpOut, "WarpOut", false);

                // Ice floor
                _iceFloor = CreateSpecialObject(new Vector2(0f, halfH*0.0f), 1.5f, _spriteIceFloor, "IceFloor", false);
                if (_iceFloor != null)
                {
                    var col = _iceFloor.AddComponent<BoxCollider2D>();
                    col.isTrigger = true;
                    col.size = new Vector2(1.5f, 0.6f);
                }

                CreateHole(new Vector2(halfW*0.0f, halfH*0.15f));
                CreateHole(new Vector2(-halfW*0.0f, -halfH*0.5f));
                CreateHole(new Vector2(halfW*0.5f, halfH*0.5f));
                CreateHole(new Vector2(-halfW*0.5f, -halfH*0.5f));
                CreateHole(new Vector2(halfW*0.25f, -halfH*0.7f));
                CreateCoin(new Vector2(-halfW*0.4f, halfH*0.5f));
                CreateCoin(new Vector2(halfW*0.5f, halfH*0.0f));
                CreateCoin(new Vector2(halfW*0.65f, -halfH*0.5f));
                CreateGoal(new Vector2(halfW*0.7f, -halfH*0.75f));
                CreateBall(new Vector2(-halfW*0.7f, halfH*0.7f));
            }

            _gameManager.SetTotalCoins(_coins.Count);
        }

        GameObject CreateWall(Transform parent, Vector2 localPos, float rotZ, Vector2 scale, bool isStatic)
        {
            var go = new GameObject("Wall");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(0, 0, rotZ);
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _spriteWall;
            sr.color = new Color(0.6f, 0.4f, 0.2f, 1f);
            sr.sortingOrder = 1;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            var rb2d = go.AddComponent<Rigidbody2D>();
            rb2d.bodyType = RigidbodyType2D.Static;

            return go;
        }

        void CreateHole(Vector2 localPos)
        {
            var go = new GameObject("Hole");
            go.transform.SetParent(_mazeRoot.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * 0.4f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _spriteHole;
            sr.sortingOrder = 0;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            go.tag = "Hole";
            _holes.Add(go);
        }

        void CreateGoal(Vector2 localPos)
        {
            var go = new GameObject("Goal");
            go.transform.SetParent(_mazeRoot.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * 0.5f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _spriteGoal;
            sr.sortingOrder = 0;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            go.tag = "Goal";
        }

        void CreateCoin(Vector2 localPos)
        {
            var go = new GameObject("Coin");
            go.transform.SetParent(_mazeRoot.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * 0.35f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _spriteCoin;
            sr.sortingOrder = 2;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            go.tag = "Coin";
            _coins.Add(go);
        }

        void CreateBall(Vector2 localPos)
        {
            _ball = new GameObject("Ball");
            _ball.transform.SetParent(_mazeRoot.transform, false);
            _ball.transform.localPosition = localPos;
            _ball.transform.localScale = Vector3.one * 0.35f;
            _ballStartLocalPos = localPos;

            _ballSr = _ball.AddComponent<SpriteRenderer>();
            _ballSr.sprite = _spriteBall;
            _ballSr.sortingOrder = 5;

            var col = _ball.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            var mat = new PhysicsMaterial2D { friction = _ballFriction, bounciness = 0.1f };
            col.sharedMaterial = mat;

            _ballRb = _ball.AddComponent<Rigidbody2D>();
            _ballRb.gravityScale = 0f; // gravity driven by tilt
            _ballRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _ballRb.linearDamping = 0.5f;

            var trigger = _ball.AddComponent<BallTriggerListener>();
            trigger.Init(this);
        }

        GameObject CreateSpecialObject(Vector2 localPos, float scale, Sprite sprite, string name, bool isTrigger)
        {
            if (sprite == null) return null;
            var go = new GameObject(name);
            go.transform.SetParent(_mazeRoot.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 0;

            return go;
        }

        void Update()
        {
            if (!_isActive) return;

            HandleInput();
            HandleTimer();
            HandleBrakeRecharge();
            ApplyTiltGravity();
        }

        void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _dragStartScreen = mouse.position.ReadValue();
                _mazeStartAngle = _mazeRoot != null ? _mazeRoot.transform.eulerAngles.z : 0f;
                _holdTimer = 0f;
            }

            if (mouse.leftButton.isPressed)
            {
                _holdTimer += Time.deltaTime;
                if (_holdTimer >= 0.5f && _brakeGauge > 0f && !_braking)
                {
                    _braking = true;
                }

                if (_isDragging && _mazeRoot != null)
                {
                    Vector2 currentScreen = mouse.position.ReadValue();
                    float deltaX = currentScreen.x - _dragStartScreen.x;
                    float rotSensitivity = 0.15f;
                    float newAngle = _mazeStartAngle - deltaX * rotSensitivity;
                    // Normalize to -180..180
                    while (newAngle > 180f) newAngle -= 360f;
                    while (newAngle < -180f) newAngle += 360f;
                    newAngle = Mathf.Clamp(newAngle, -_maxAngle, _maxAngle);
                    _mazeRoot.transform.rotation = Quaternion.Euler(0, 0, newAngle);
                }
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
                _braking = false;
                _holdTimer = 0f;
            }
        }

        void ApplyTiltGravity()
        {
            if (_ballRb == null || _mazeRoot == null) return;

            float angle = _mazeRoot.transform.eulerAngles.z;
            if (angle > 180f) angle -= 360f;
            float rad = angle * Mathf.Deg2Rad;
            float gravStrength = _braking ? 2f : 9.8f;

            // Gravity in world space, projected considering maze rotation
            Vector2 gravDir = new Vector2(-Mathf.Sin(rad), -Mathf.Cos(rad));
            _ballRb.AddForce(gravDir * gravStrength, ForceMode2D.Force);

            // Apply brake
            if (_braking)
            {
                _ballRb.linearVelocity = Vector2.Lerp(_ballRb.linearVelocity, Vector2.zero, Time.deltaTime * 5f);
                _brakeGauge = Mathf.Max(0f, _brakeGauge - Time.deltaTime * 0.2f);
                _gameManager.OnBrakeGaugeUpdated(_brakeGauge);
                _ballSr.color = Color.Lerp(_ballSr.color, new Color(0.5f, 0.7f, 1f, 1f), Time.deltaTime * 5f);
            }
            else
            {
                _ballSr.color = Color.Lerp(_ballSr.color, Color.white, Time.deltaTime * 3f);
            }
        }

        void HandleTimer()
        {
            _remainingTime -= Time.deltaTime;
            if (_remainingTime < 0f) _remainingTime = 0f;
            _gameManager.OnTimerUpdated(_remainingTime);
            if (_remainingTime <= 0f)
            {
                _gameManager.OnTimeUp();
            }
        }

        void HandleBrakeRecharge()
        {
            if (!_braking && _brakeGauge < 1f)
            {
                _brakeGauge = Mathf.Min(1f, _brakeGauge + Time.deltaTime * (1f / 30f));
                _gameManager.OnBrakeGaugeUpdated(_brakeGauge);
            }
        }

        public void OnBallTriggerEnter(Collider2D other)
        {
            if (!_isActive || _ballInHoleCooldown) return;

            if (other.CompareTag("Hole"))
            {
                StartCoroutine(BallFellRoutine());
            }
            else if (other.CompareTag("Goal"))
            {
                _isActive = false;
                StartCoroutine(GoalReachedRoutine());
            }
            else if (other.CompareTag("Coin") && other.gameObject.activeSelf)
            {
                other.gameObject.SetActive(false);
                _coins.Remove(other.gameObject);
                _gameManager.OnCoinCollected();
                StartCoroutine(CoinPopRoutine(other.gameObject));
            }
            else if (other.gameObject.name == "WarpIn" && _warpOut != null)
            {
                // Teleport ball to warp out position
                _ball.transform.position = _warpOut.transform.position;
                if (_ballRb != null) _ballRb.linearVelocity = Vector2.zero;
            }
            else if (other.gameObject.name == "IceFloor" && _ballRb != null)
            {
                // Handled in physics material - just reduce damping
                _ballRb.linearDamping = 0.05f;
            }
        }

        public void OnBallTriggerExit(Collider2D other)
        {
            if (other.gameObject.name == "IceFloor" && _ballRb != null)
            {
                _ballRb.linearDamping = 0.5f;
            }
        }

        IEnumerator BallFellRoutine()
        {
            _ballInHoleCooldown = true;
            // Red flash
            if (_ballSr != null)
            {
                _ballSr.color = Color.red;
                yield return new WaitForSeconds(0.3f);
                if (!_isActive) { _ballInHoleCooldown = false; yield break; }
                _ballSr.color = Color.white;
            }
            // Respawn
            if (_ball != null && _ballRb != null)
            {
                _ballRb.linearVelocity = Vector2.zero;
                _ball.transform.localPosition = _ballStartLocalPos;
            }
            _gameManager.OnBallFell();
            yield return new WaitForSeconds(0.5f);
            _ballInHoleCooldown = false;
        }

        IEnumerator GoalReachedRoutine()
        {
            // Goal green flash
            var goalObj = GameObject.FindWithTag("Goal");
            if (goalObj != null)
            {
                var sr = goalObj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.green;
            }
            // Ball scale up
            if (_ball != null)
            {
                float t = 0f;
                Vector3 baseScale = _ball.transform.localScale;
                while (t < 0.3f)
                {
                    t += Time.deltaTime;
                    _ball.transform.localScale = baseScale * (1f + t * 2f);
                    yield return null;
                }
            }
            yield return new WaitForSeconds(0.2f);
            _gameManager.OnGoalReached(_remainingTime);
        }

        IEnumerator CoinPopRoutine(GameObject coin)
        {
            coin.SetActive(true);
            float t = 0f;
            Vector3 baseScale = coin.transform.localScale;
            while (t < 0.15f)
            {
                if (coin == null) yield break;
                t += Time.deltaTime;
                float ratio = t / 0.15f;
                coin.transform.localScale = baseScale * (1f + ratio * 0.5f);
                var sr = coin.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 1f, 0.5f, 1f - ratio);
                yield return null;
            }
            if (coin != null) Destroy(coin);
        }
    }

    public class MovingWallController : MonoBehaviour
    {
        Vector2 _posA;
        Vector2 _posB;
        float _period;
        float _timer;

        public void Init(Vector2 posA, Vector2 posB, float period)
        {
            _posA = posA;
            _posB = posB;
            _period = period;
        }

        void Update()
        {
            _timer += Time.deltaTime;
            float t = Mathf.PingPong(_timer / _period, 1f);
            transform.localPosition = Vector2.Lerp(_posA, _posB, t);
        }
    }

    public class BallTriggerListener : MonoBehaviour
    {
        TiltMazeMechanic _mechanic;

        public void Init(TiltMazeMechanic mechanic)
        {
            _mechanic = mechanic;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            _mechanic?.OnBallTriggerEnter(other);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            _mechanic?.OnBallTriggerExit(other);
        }
    }
}
