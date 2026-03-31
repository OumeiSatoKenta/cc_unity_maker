using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game019_PathCut
{
    public class PathCutManager : MonoBehaviour
    {
        [SerializeField] private GameObject _ropePrefab;
        [SerializeField] private GameObject _ballPrefab;
        [SerializeField] private GameObject _starPrefab;
        [SerializeField] private GameObject _platformPrefab;

        private readonly List<RopeSegment> _ropes = new List<RopeSegment>();
        private readonly List<GameObject> _stageObjects = new List<GameObject>();
        private GameObject _ballObj;
        private Vector2 _ballPos;
        private Vector2 _starPos;
        private bool _ballFalling;
        private float _ballVelocity;

        private PathCutGameManager _gameManager;
        private Camera _mainCamera;

        public static int StageCount => 3;

        private struct RopeData
        {
            public Vector2 position;
            public bool isHorizontal;
            public int holdsBallIndex; // -1 = doesn't hold ball
            public RopeData(float x, float y, bool h, int holds) { position = new Vector2(x, y); isHorizontal = h; holdsBallIndex = holds; }
        }

        private struct PlatformData
        {
            public Vector2 position;
            public PlatformData(float x, float y) { position = new Vector2(x, y); }
        }

        private int _ballHeldByRope = -1;

        private void Awake()
        {
            _gameManager = GetComponentInParent<PathCutGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleInput();
            if (_ballFalling)
                SimulateBall();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var rope = hit.GetComponent<RopeSegment>();
                    if (rope != null && !rope.IsCut)
                    {
                        rope.Cut();
                        if (_gameManager != null) _gameManager.OnRopeCut();

                        // Check if this rope was holding the ball
                        if (rope.RopeIndex == _ballHeldByRope)
                        {
                            _ballFalling = true;
                            _ballVelocity = 0f;
                        }
                    }
                }
            }
        }

        private void SimulateBall()
        {
            _ballVelocity -= 9.8f * Time.deltaTime;
            _ballPos.y += _ballVelocity * Time.deltaTime;

            if (_ballObj != null)
                _ballObj.transform.position = _ballPos;

            // Check star collision
            if (Vector2.Distance(_ballPos, _starPos) < 0.5f)
            {
                _ballFalling = false;
                if (_gameManager != null) _gameManager.OnPuzzleSolved();
                return;
            }

            // Check if ball fell off screen
            if (_ballPos.y < -6f)
            {
                _ballFalling = false;
                if (_gameManager != null) _gameManager.OnBallLost();
            }
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            _ballFalling = false;
            _ballVelocity = 0f;
            var data = GetStageData(stageIndex);
            BuildStage(data);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
            _ropes.Clear();
            _ballObj = null;
        }

        private void BuildStage(StageData data)
        {
            // Ropes
            for (int i = 0; i < data.ropes.Count; i++)
            {
                var rd = data.ropes[i];
                if (_ropePrefab == null) continue;
                var obj = Instantiate(_ropePrefab, transform);
                obj.transform.position = rd.position;
                if (!rd.isHorizontal)
                    obj.transform.rotation = Quaternion.Euler(0, 0, 90);
                obj.transform.localScale = new Vector3(2f, 1f, 1f);
                obj.name = $"Rope_{i}";

                var seg = obj.GetComponent<RopeSegment>();
                if (seg != null) seg.Initialize(i);
                _ropes.Add(seg);
                _stageObjects.Add(obj);

                if (rd.holdsBallIndex >= 0)
                    _ballHeldByRope = i;
            }

            // Platforms
            foreach (var pd in data.platforms)
            {
                if (_platformPrefab == null) continue;
                var obj = Instantiate(_platformPrefab, transform);
                obj.transform.position = pd.position;
                obj.transform.localScale = new Vector3(2f, 1f, 1f);
                _stageObjects.Add(obj);
            }

            // Ball
            _ballPos = data.ballPos;
            if (_ballPrefab != null)
            {
                _ballObj = Instantiate(_ballPrefab, transform);
                _ballObj.transform.position = _ballPos;
                _stageObjects.Add(_ballObj);
            }

            // Star
            _starPos = data.starPos;
            if (_starPrefab != null)
            {
                var obj = Instantiate(_starPrefab, transform);
                obj.transform.position = _starPos;
                _stageObjects.Add(obj);
            }
        }

        #region Stage Data

        private struct StageData
        {
            public Vector2 ballPos, starPos;
            public List<RopeData> ropes;
            public List<PlatformData> platforms;
        }

        private StageData GetStageData(int index)
        {
            switch (index % StageCount)
            {
                case 0: return GetStage1();
                case 1: return GetStage2();
                case 2: return GetStage3();
                default: return GetStage1();
            }
        }

        private StageData GetStage1()
        {
            return new StageData
            {
                ballPos = new Vector2(0, 2),
                starPos = new Vector2(0, -2),
                ropes = new List<RopeData> {
                    new RopeData(0, 1.2f, true, 0),
                },
                platforms = new List<PlatformData>()
            };
        }

        private StageData GetStage2()
        {
            return new StageData
            {
                ballPos = new Vector2(-1, 3),
                starPos = new Vector2(1, -2),
                ropes = new List<RopeData> {
                    new RopeData(-1, 2.2f, true, 0),
                    new RopeData(1, 0f, true, -1),
                },
                platforms = new List<PlatformData> {
                    new PlatformData(0, 0.5f),
                }
            };
        }

        private StageData GetStage3()
        {
            return new StageData
            {
                ballPos = new Vector2(-2, 3),
                starPos = new Vector2(2, -3),
                ropes = new List<RopeData> {
                    new RopeData(-2, 2.2f, true, 0),
                    new RopeData(0, 0f, true, -1),
                    new RopeData(2, -1f, true, -1),
                },
                platforms = new List<PlatformData> {
                    new PlatformData(-1, 1f),
                    new PlatformData(1, -0.5f),
                }
            };
        }

        #endregion
    }
}
