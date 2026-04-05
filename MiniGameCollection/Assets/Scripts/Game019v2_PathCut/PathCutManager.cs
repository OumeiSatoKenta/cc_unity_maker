using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game019v2_PathCut
{
    public class PathCutManager : MonoBehaviour
    {
        [SerializeField] PathCutGameManager _gameManager;
        [SerializeField] PathCutUI _ui;

        [SerializeField] Sprite _ballSprite;
        [SerializeField] Sprite _starSprite;
        [SerializeField] Sprite _ropeSprite;
        [SerializeField] Sprite _anchorSprite;
        [SerializeField] Sprite _bumperSprite;
        [SerializeField] Sprite _airCushionSprite;

        // Stage data
        int _stageNum;
        int _maxCuts;
        int _cutsUsed;
        int _starsTotal;
        int _starsCollected;
        int _complexityFactor;
        bool _isActive;

        // Runtime objects
        readonly List<GameObject> _ropes = new List<GameObject>();
        readonly List<GameObject> _balls = new List<GameObject>();
        readonly List<GameObject> _stars = new List<GameObject>();
        readonly List<GameObject> _bumpers = new List<GameObject>();
        readonly List<GameObject> _airCushions = new List<GameObject>();
        readonly List<TimedRope> _timedRopes = new List<TimedRope>();

        // Swipe detection
        bool _swipeStarted;
        Vector2 _swipeStart;
        Vector2 _swipeEnd;

        // Stage layout params
        readonly int[] _ropeCountByStage = { 1, 2, 3, 2, 3 };
        readonly int[] _ballCountByStage = { 1, 1, 1, 2, 2 };
        readonly int[] _starCountByStage = { 1, 2, 3, 4, 5 };
        readonly int[] _maxCutsByStage   = { 3, 4, 5, 5, 6 };

        // Cached camera reference
        Camera _mainCamera;

        // Rope segment data
        struct RopeData
        {
            public Vector2 anchorPos;
            public Vector2 ballAttachPos;
            public int ballIndex;
            public bool isCut;
            public bool isTimed;
            public float timeLeft;
            public List<GameObject> segments;
            public LineRenderer lineRenderer;
            public Rigidbody2D anchorBody;
        }

        readonly List<RopeData> _ropeDataList = new List<RopeData>();

        void Awake()
        {
            if (_gameManager == null) _gameManager = GetComponentInParent<PathCutGameManager>();
            if (_ui == null) _ui = GetComponentInParent<PathCutUI>();
            _mainCamera = Camera.main;
        }

        public void SetupStage(StageManager.StageConfig config, int stageNum)
        {
            _isActive = false;
            StopAllCoroutines();
            ClearStage();
            if (_mainCamera == null) _mainCamera = Camera.main;

            _stageNum = stageNum;
            int idx = Mathf.Clamp(stageNum - 1, 0, 4);
            _maxCuts = _maxCutsByStage[idx];
            _cutsUsed = 0;
            _starsTotal = _starCountByStage[idx];
            _starsCollected = 0;
            _complexityFactor = Mathf.RoundToInt(config.complexityFactor);

            int ropeCount = _ropeCountByStage[idx];
            int ballCount = _ballCountByStage[idx];

            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float bottomMargin = 2.8f;
            float topAnchorY = camSize - 1.0f;
            float gameAreaBottom = -camSize + bottomMargin;

            // Create balls
            for (int i = 0; i < ballCount; i++)
            {
                float xPos = ballCount == 1 ? 0f : (i == 0 ? -camWidth * 0.4f : camWidth * 0.4f);
                float anchorY = topAnchorY - (i * 0.5f);
                CreateBall(i, new Vector2(xPos, anchorY - 2.5f));
            }

            // Create ropes
            _ropeDataList.Clear();
            for (int i = 0; i < ropeCount; i++)
            {
                float xAnchor;
                if (ropeCount == 1)
                    xAnchor = 0f;
                else if (ropeCount == 2)
                    xAnchor = (i == 0) ? -camWidth * 0.35f : camWidth * 0.35f;
                else
                    xAnchor = (i - 1) * camWidth * 0.35f;

                int assignedBall = (ballCount == 1) ? 0 : Mathf.Min(i, ballCount - 1);
                float anchorY = topAnchorY - (assignedBall * 0.5f);
                Vector2 anchorPos = new Vector2(xAnchor, anchorY);

                bool isTimed = (_complexityFactor >= 4) && (i == ropeCount - 1);
                CreateRope(i, anchorPos, assignedBall, isTimed);
            }

            // Create stars
            for (int i = 0; i < _starsTotal; i++)
            {
                float starX;
                float starY;
                if (_starsTotal == 1)
                {
                    starX = 0f;
                    starY = gameAreaBottom + 1.2f;
                }
                else
                {
                    float spread = Mathf.Min(camWidth * 0.7f, 2.5f);
                    starX = Mathf.Lerp(-spread, spread, (float)i / (_starsTotal - 1));
                    starY = gameAreaBottom + 0.8f + (i % 2) * 0.8f;
                }
                CreateStar(i, new Vector2(starX, starY));
            }

            // Stage 2+: Add bumpers
            if (_complexityFactor >= 1)
            {
                float bumperY = gameAreaBottom + 2.5f;
                CreateBumper(new Vector2(camWidth * 0.3f, bumperY), new Vector2(1.5f, 0.25f));
            }

            // Stage 3+: Add air cushion
            if (_complexityFactor >= 2)
            {
                float cushionY = gameAreaBottom + 3.5f;
                CreateAirCushion(new Vector2(-camWidth * 0.25f, cushionY), new Vector2(1.8f, 0.5f));
            }

            // Stage 4+: Link ropes between balls (visual only)
            if (_complexityFactor >= 3 && ballCount >= 2)
            {
                CreateBallLink();
            }

            _ui.UpdateCutCount(_maxCuts - _cutsUsed, _maxCuts);
            _ui.UpdateStarCount(_starsCollected, _starsTotal);

            _isActive = true;
        }

        void ClearStage()
        {
            foreach (var r in _ropes) if (r != null) Destroy(r);
            foreach (var b in _balls) if (b != null) Destroy(b);
            foreach (var s in _stars) if (s != null) Destroy(s);
            foreach (var bm in _bumpers) if (bm != null) Destroy(bm);
            foreach (var ac in _airCushions) if (ac != null) Destroy(ac);
            _ropes.Clear();
            _balls.Clear();
            _stars.Clear();
            _bumpers.Clear();
            _airCushions.Clear();
            _timedRopes.Clear();
            _ropeDataList.Clear();
            _swipeStarted = false;
        }

        void CreateBall(int index, Vector2 pos)
        {
            var go = new GameObject($"Ball_{index}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _ballSprite;
            sr.sortingOrder = 5;
            float ballScale = 0.45f;
            go.transform.localScale = Vector3.one * ballScale;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // initially suspended by rope
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            var detector = go.AddComponent<BallStarDetector>();
            detector.Init(this, index);

            _balls.Add(go);
        }

        void CreateRope(int ropeIndex, Vector2 anchorPos, int ballIndex, bool isTimed)
        {
            // Create anchor visual
            var anchorGo = new GameObject($"Anchor_{ropeIndex}");
            anchorGo.transform.position = anchorPos;
            if (_anchorSprite != null)
            {
                var sr = anchorGo.AddComponent<SpriteRenderer>();
                sr.sprite = _anchorSprite;
                sr.sortingOrder = 3;
                anchorGo.transform.localScale = Vector3.one * 0.3f;
            }
            _ropes.Add(anchorGo);

            // Create rope as LineRenderer between anchor and ball
            var ropeGo = new GameObject($"Rope_{ropeIndex}");
            var lr = ropeGo.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = 0.12f;
            lr.endWidth = 0.08f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = isTimed ? new Color(1f, 0.5f, 0f) : new Color(0.55f, 0.43f, 0.31f);
            lr.endColor = isTimed ? new Color(1f, 0.3f, 0f) : new Color(0.36f, 0.27f, 0.18f);
            lr.sortingOrder = 4;
            _ropes.Add(ropeGo);

            Rigidbody2D storedAnchorBody = null;
            if (ballIndex < _balls.Count)
            {
                var ballPos = _balls[ballIndex].transform.position;
                lr.SetPosition(0, anchorPos);
                lr.SetPosition(1, ballPos);

                // Make ball hang from anchor using HingeJoint2D
                var rb = _balls[ballIndex].GetComponent<Rigidbody2D>();

                // Create anchor body
                var anchorBody = anchorGo.AddComponent<Rigidbody2D>();
                anchorBody.bodyType = RigidbodyType2D.Static;
                storedAnchorBody = anchorBody;

                // Create HingeJoint on ball
                var hinge = _balls[ballIndex].AddComponent<HingeJoint2D>();
                hinge.connectedBody = anchorBody;
                hinge.anchor = Vector2.zero;
                hinge.connectedAnchor = Vector2.zero;

                rb.gravityScale = 1f; // ball falls under gravity, held by hinge
            }

            var rd = new RopeData
            {
                anchorPos = anchorPos,
                ballIndex = ballIndex,
                isCut = false,
                isTimed = isTimed,
                timeLeft = isTimed ? 5f : 0f,
                lineRenderer = lr,
                segments = new List<GameObject> { anchorGo, ropeGo },
                anchorBody = storedAnchorBody
            };
            _ropeDataList.Add(rd);

            if (isTimed)
            {
                _timedRopes.Add(new TimedRope { ropeIndex = ropeIndex, timeLeft = 5f });
            }
        }

        void CreateStar(int index, Vector2 pos)
        {
            var go = new GameObject($"Star_{index}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _starSprite;
            sr.sortingOrder = 5;
            go.transform.localScale = Vector3.one * 0.35f;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            _stars.Add(go);
        }

        void CreateBumper(Vector2 pos, Vector2 size)
        {
            var go = new GameObject("Bumper");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _bumperSprite;
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            var rb2d = go.AddComponent<Rigidbody2D>();
            rb2d.bodyType = RigidbodyType2D.Static;

            var pm = go.AddComponent<PhysicsMaterial2DHolder>();
            var pmat = new PhysicsMaterial2D("BumperMat");
            pmat.bounciness = 0.8f;
            pmat.friction = 0.0f;
            col.sharedMaterial = pmat;

            _bumpers.Add(go);
        }

        void CreateAirCushion(Vector2 pos, Vector2 size)
        {
            var go = new GameObject("AirCushion");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _airCushionSprite;
            sr.color = new Color(1f, 1f, 1f, 0.7f);
            sr.sortingOrder = 2;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;

            var zone = go.AddComponent<AirCushionZone>();

            _airCushions.Add(go);
        }

        void CreateBallLink()
        {
            if (_balls.Count < 2) return;
            var go = new GameObject("BallLink");
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = 0.08f;
            lr.endWidth = 0.08f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(0.8f, 0.4f, 0f);
            lr.endColor = new Color(0.8f, 0.4f, 0f);
            lr.sortingOrder = 4;

            var link = go.AddComponent<BallLinkRenderer>();
            link.Init(_balls[0].transform, _balls[1].transform, lr);

            // SpringJoint2D to connect balls
            var spring = _balls[0].AddComponent<SpringJoint2D>();
            spring.connectedBody = _balls[1].GetComponent<Rigidbody2D>();
            spring.distance = Vector2.Distance(_balls[0].transform.position, _balls[1].transform.position);
            spring.frequency = 1.5f;
            spring.dampingRatio = 0.3f;

            _ropes.Add(go);
        }

        void Update()
        {
            if (!_isActive) return;
            if (_gameManager.State != PathCutGameManager.GameState.Playing) return;

            HandleSwipeInput();
            UpdateRopeVisuals();
            UpdateTimedRopes();
            CheckBallsOutOfBounds();
        }

        void HandleSwipeInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _swipeStarted = true;
                _swipeStart = mouse.position.ReadValue();
            }
            else if (mouse.leftButton.wasReleasedThisFrame && _swipeStarted)
            {
                _swipeEnd = mouse.position.ReadValue();
                _swipeStarted = false;

                if (Vector2.Distance(_swipeStart, _swipeEnd) > 20f)
                {
                    TrySliceRope(_swipeStart, _swipeEnd);
                }
            }
        }

        void TrySliceRope(Vector2 screenStart, Vector2 screenEnd)
        {
            if (_mainCamera == null) return;
            var camStart = _mainCamera.ScreenToWorldPoint(new Vector3(screenStart.x, screenStart.y, 10f));
            var camEnd = _mainCamera.ScreenToWorldPoint(new Vector3(screenEnd.x, screenEnd.y, 10f));

            int cutCount = 0;
            for (int i = 0; i < _ropeDataList.Count; i++)
            {
                var rd = _ropeDataList[i];
                if (rd.isCut) continue;
                if (rd.ballIndex >= _balls.Count) continue;

                Vector2 ropeStart = rd.anchorPos;
                Vector2 ropeEnd = _balls[rd.ballIndex].transform.position;

                if (SegmentsIntersect(camStart, camEnd, ropeStart, ropeEnd))
                {
                    CutRope(i);
                    cutCount++;
                }
            }

            if (cutCount > 0)
            {
                _cutsUsed += cutCount;
                _ui.UpdateCutCount(_maxCuts - _cutsUsed, _maxCuts);

                if (_cutsUsed >= _maxCuts && _starsCollected < _starsTotal)
                {
                    _isActive = false;
                    StartCoroutine(DelayedGameOver());
                }
            }
        }

        void CutRope(int ropeIndex)
        {
            var rd = _ropeDataList[ropeIndex];
            rd.isCut = true;
            _ropeDataList[ropeIndex] = rd;

            // Hide rope line
            if (rd.lineRenderer != null)
                rd.lineRenderer.enabled = false;

            // Remove HingeJoint from ball so it falls freely
            if (rd.ballIndex < _balls.Count && rd.anchorBody != null)
            {
                var ball = _balls[rd.ballIndex];
                var hinges = ball.GetComponents<HingeJoint2D>();
                foreach (var h in hinges)
                {
                    if (h.connectedBody == rd.anchorBody)
                    {
                        Destroy(rd.anchorBody.gameObject); // destroy anchor body
                        Destroy(h);
                        break;
                    }
                }

                // Flash effect
                StartCoroutine(CutFlash(ball));
            }
        }

        IEnumerator CutFlash(GameObject ball)
        {
            var sr = ball.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            var orig = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sr.color = orig;
        }

        void UpdateRopeVisuals()
        {
            for (int i = 0; i < _ropeDataList.Count; i++)
            {
                var rd = _ropeDataList[i];
                if (rd.isCut || rd.lineRenderer == null) continue;
                if (rd.ballIndex >= _balls.Count) continue;
                rd.lineRenderer.SetPosition(0, rd.anchorPos);
                rd.lineRenderer.SetPosition(1, _balls[rd.ballIndex].transform.position);
            }
        }

        void UpdateTimedRopes()
        {
            for (int i = _timedRopes.Count - 1; i >= 0; i--)
            {
                var tr = _timedRopes[i];
                tr.timeLeft -= Time.deltaTime;
                _timedRopes[i] = tr;

                if (tr.ropeIndex < _ropeDataList.Count)
                {
                    var rd = _ropeDataList[tr.ropeIndex];
                    if (!rd.isCut && rd.lineRenderer != null)
                    {
                        // Flicker as time runs out
                        float alpha = tr.timeLeft < 2f ? (Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f) : 1f;
                        var c = rd.lineRenderer.startColor;
                        c.a = alpha;
                        rd.lineRenderer.startColor = c;
                        rd.lineRenderer.endColor = c;
                    }
                }

                if (tr.timeLeft <= 0f)
                {
                    if (tr.ropeIndex < _ropeDataList.Count && !_ropeDataList[tr.ropeIndex].isCut)
                    {
                        CutRope(tr.ropeIndex);
                        // Timed rope auto-cut doesn't count against player's cut limit
                    }
                    _timedRopes.RemoveAt(i);
                }
            }
        }

        void CheckBallsOutOfBounds()
        {
            float camSize = Camera.main.orthographicSize;
            float minY = -camSize - 2f;

            foreach (var ball in _balls)
            {
                if (ball == null) continue;
                if (ball.transform.position.y < minY)
                {
                    if (_starsCollected < _starsTotal)
                    {
                        _isActive = false;
                        _gameManager.OnGameOver();
                        return;
                    }
                }
            }
        }

        public void OnBallHitStar(int ballIndex, GameObject starGo)
        {
            if (!_isActive) return;
            if (!starGo.activeSelf) return;

            _starsCollected++;
            _ui.UpdateStarCount(_starsCollected, _starsTotal);

            StartCoroutine(StarCollectEffect(starGo));

            if (_starsCollected >= _starsTotal)
            {
                _isActive = false;
                StartCoroutine(DelayedStageClear());
            }
        }

        IEnumerator StarCollectEffect(GameObject starGo)
        {
            starGo.SetActive(false);
            yield return null;
        }

        IEnumerator DelayedStageClear()
        {
            yield return new WaitForSeconds(0.5f);
            _gameManager.OnStageClear(_cutsUsed, _maxCuts, _starsCollected);
        }

        IEnumerator DelayedGameOver()
        {
            yield return new WaitForSeconds(0.3f);
            if (_gameManager.State == PathCutGameManager.GameState.Playing)
            {
                _isActive = false;
                _gameManager.OnGameOver();
            }
        }

        bool SegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float d1x = p2.x - p1.x, d1y = p2.y - p1.y;
            float d2x = p4.x - p3.x, d2y = p4.y - p3.y;
            float cross = d1x * d2y - d1y * d2x;
            if (Mathf.Abs(cross) < 0.0001f) return false;
            float t = ((p3.x - p1.x) * d2y - (p3.y - p1.y) * d2x) / cross;
            float u = ((p3.x - p1.x) * d1y - (p3.y - p1.y) * d1x) / cross;
            return t >= 0f && t <= 1f && u >= 0f && u <= 1f;
        }

        void OnDestroy()
        {
            ClearStage();
        }

        struct TimedRope
        {
            public int ropeIndex;
            public float timeLeft;
        }
    }

    // Helper: detect ball-star collision
    public class BallStarDetector : MonoBehaviour
    {
        PathCutManager _manager;
        int _ballIndex;

        public void Init(PathCutManager manager, int index)
        {
            _manager = manager;
            _ballIndex = index;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_manager == null) return;
            if (other.gameObject.name.StartsWith("Star_"))
            {
                _manager.OnBallHitStar(_ballIndex, other.gameObject);
            }
        }
    }

    // Helper: air cushion upward force
    public class AirCushionZone : MonoBehaviour
    {
        void OnTriggerStay2D(Collider2D other)
        {
            var rb = other.GetComponent<Rigidbody2D>();
            if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            {
                rb.AddForce(Vector2.up * 18f * Time.fixedDeltaTime, ForceMode2D.Force);
            }
        }
    }

    // Helper: renders line between two transforms
    public class BallLinkRenderer : MonoBehaviour
    {
        Transform _a, _b;
        LineRenderer _lr;

        public void Init(Transform a, Transform b, LineRenderer lr)
        {
            _a = a; _b = b; _lr = lr;
        }

        void Update()
        {
            if (_a != null && _b != null && _lr != null)
            {
                _lr.SetPosition(0, _a.position);
                _lr.SetPosition(1, _b.position);
            }
        }
    }

    // Helper: holds PhysicsMaterial2D reference to avoid GC
    public class PhysicsMaterial2DHolder : MonoBehaviour { }
}
