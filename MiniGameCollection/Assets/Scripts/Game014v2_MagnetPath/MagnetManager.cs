using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game014v2_MagnetPath
{
    public class MagnetManager : MonoBehaviour
    {
        [System.Serializable]
        public class MagnetData
        {
            public Vector2Int gridPos;
            public bool isNorth;      // true=N極(引力), false=S極(斥力)
            public bool isLarge;      // ステージ3+の強化磁石
            public bool isSwitch;     // ステージ4のスイッチ磁石
            public bool switchFired;  // スイッチ磁石の発火済みフラグ（毎フレーム反転防止）
        }

        [System.Serializable]
        public class WallData
        {
            public Vector2Int gridPos;
        }

        [System.Serializable]
        public class StageLayoutData
        {
            public MagnetData[] magnets;
            public WallData[] walls;
            public Vector2Int startPos;
            public Vector2Int goalPos;
            public Vector2Int? startPos2;   // ステージ5用
            public Vector2Int? goalPos2;    // ステージ5用
            public int maxSwitches;
            public int gridSize;
        }

        [SerializeField] MagnetPathGameManager _gameManager;
        [SerializeField] Sprite _magnetNSprite;
        [SerializeField] Sprite _magnetSSprite;
        [SerializeField] Sprite _magnetNLargeSprite;
        [SerializeField] Sprite _magnetSLargeSprite;
        [SerializeField] Sprite _magnetSwitchSprite;
        [SerializeField] Sprite _ballSprite;
        [SerializeField] Sprite _ball2Sprite;
        [SerializeField] Sprite _goalSprite;
        [SerializeField] Sprite _goal2Sprite;
        [SerializeField] Sprite _wallSprite;

        static readonly StageLayoutData[] StageLayouts = new StageLayoutData[]
        {
            // Stage 1: 2磁石直線配置
            new StageLayoutData
            {
                gridSize = 5,
                maxSwitches = 5,
                startPos = new Vector2Int(2, 0),
                goalPos = new Vector2Int(2, 4),
                magnets = new MagnetData[]
                {
                    new MagnetData { gridPos = new Vector2Int(2, 1), isNorth = true },
                    new MagnetData { gridPos = new Vector2Int(2, 3), isNorth = false },
                },
                walls = new WallData[0],
            },
            // Stage 2: 4磁石L字 + 壁
            new StageLayoutData
            {
                gridSize = 5,
                maxSwitches = 6,
                startPos = new Vector2Int(0, 0),
                goalPos = new Vector2Int(4, 4),
                magnets = new MagnetData[]
                {
                    new MagnetData { gridPos = new Vector2Int(1, 1), isNorth = true },
                    new MagnetData { gridPos = new Vector2Int(3, 1), isNorth = false },
                    new MagnetData { gridPos = new Vector2Int(1, 3), isNorth = false },
                    new MagnetData { gridPos = new Vector2Int(3, 3), isNorth = true },
                },
                walls = new WallData[]
                {
                    new WallData { gridPos = new Vector2Int(2, 2) },
                    new WallData { gridPos = new Vector2Int(2, 0) },
                },
            },
            // Stage 3: 6磁石 + 強弱磁石
            new StageLayoutData
            {
                gridSize = 6,
                maxSwitches = 8,
                startPos = new Vector2Int(0, 0),
                goalPos = new Vector2Int(5, 5),
                magnets = new MagnetData[]
                {
                    new MagnetData { gridPos = new Vector2Int(1, 1), isNorth = true, isLarge = true },
                    new MagnetData { gridPos = new Vector2Int(4, 1), isNorth = false },
                    new MagnetData { gridPos = new Vector2Int(2, 2), isNorth = false },
                    new MagnetData { gridPos = new Vector2Int(3, 3), isNorth = true, isLarge = true },
                    new MagnetData { gridPos = new Vector2Int(1, 4), isNorth = true },
                    new MagnetData { gridPos = new Vector2Int(4, 4), isNorth = false },
                },
                walls = new WallData[]
                {
                    new WallData { gridPos = new Vector2Int(0, 3) },
                    new WallData { gridPos = new Vector2Int(5, 2) },
                },
            },
            // Stage 4: 8磁石 + スイッチ磁石 + 移動障害物
            new StageLayoutData
            {
                gridSize = 6,
                maxSwitches = 10,
                startPos = new Vector2Int(0, 0),
                goalPos = new Vector2Int(5, 5),
                magnets = new MagnetData[]
                {
                    new MagnetData { gridPos = new Vector2Int(1, 0), isNorth = true },
                    new MagnetData { gridPos = new Vector2Int(4, 0), isNorth = false },
                    new MagnetData { gridPos = new Vector2Int(2, 1), isNorth = false, isSwitch = true },
                    new MagnetData { gridPos = new Vector2Int(0, 3), isNorth = true },
                    new MagnetData { gridPos = new Vector2Int(3, 2), isNorth = true },
                    new MagnetData { gridPos = new Vector2Int(5, 2), isNorth = false },
                    new MagnetData { gridPos = new Vector2Int(2, 4), isNorth = false, isSwitch = true },
                    new MagnetData { gridPos = new Vector2Int(4, 4), isNorth = true },
                },
                walls = new WallData[]
                {
                    new WallData { gridPos = new Vector2Int(1, 2) },
                    new WallData { gridPos = new Vector2Int(3, 3) },
                    new WallData { gridPos = new Vector2Int(5, 1) },
                },
            },
            // Stage 5: 10磁石 + 2球同時
            new StageLayoutData
            {
                gridSize = 7,
                maxSwitches = 12,
                startPos = new Vector2Int(0, 0),
                goalPos = new Vector2Int(6, 6),
                startPos2 = new Vector2Int(6, 0),
                goalPos2 = new Vector2Int(0, 6),
                magnets = new MagnetData[]
                {
                    new MagnetData { gridPos = new Vector2Int(1, 1), isNorth = true, isLarge = true },
                    new MagnetData { gridPos = new Vector2Int(5, 1), isNorth = false, isLarge = true },
                    new MagnetData { gridPos = new Vector2Int(3, 1), isNorth = false },
                    new MagnetData { gridPos = new Vector2Int(2, 3), isNorth = true },
                    new MagnetData { gridPos = new Vector2Int(4, 3), isNorth = false },
                    new MagnetData { gridPos = new Vector2Int(3, 3), isNorth = true, isSwitch = true },
                    new MagnetData { gridPos = new Vector2Int(0, 4), isNorth = false },
                    new MagnetData { gridPos = new Vector2Int(6, 4), isNorth = true },
                    new MagnetData { gridPos = new Vector2Int(1, 5), isNorth = false, isLarge = true },
                    new MagnetData { gridPos = new Vector2Int(5, 5), isNorth = true, isLarge = true },
                },
                walls = new WallData[]
                {
                    new WallData { gridPos = new Vector2Int(3, 0) },
                    new WallData { gridPos = new Vector2Int(0, 2) },
                    new WallData { gridPos = new Vector2Int(6, 2) },
                    new WallData { gridPos = new Vector2Int(3, 6) },
                },
            },
        };

        // Runtime state
        Camera _camera;
        StageLayoutData _currentLayout;
        List<GameObject> _magnetObjects = new List<GameObject>();
        List<MagnetData> _currentMagnets = new List<MagnetData>();
        List<GameObject> _wallObjects = new List<GameObject>();
        GameObject _ballObject;
        GameObject _ball2Object;
        GameObject _goalObject;
        GameObject _goal2Object;

        Vector2 _ballPos;
        Vector2 _ballVelocity;
        Vector2 _ball2Pos;
        Vector2 _ball2Velocity;

        bool _isMoving;
        bool _ball1Reached;
        bool _ball2Reached;
        bool _hasTwoBalls;

        int _switchCount;
        int _maxSwitches;
        int _stageIndex;
        float _speedMultiplier;

        float _cellSize;
        Vector2 _gridOrigin;

        const float BASE_MAGNET_STRENGTH = 2.5f;
        const float MAGNET_INFLUENCE_RADIUS = 2.0f;
        const float BALL_MAX_SPEED = 4.0f;
        const float BALL_DAMPING = 0.95f;
        const float GOAL_RADIUS = 0.3f;
        const float SWITCH_MAGNET_TRIGGER_RADIUS = 0.5f;

        public int SwitchCount => _switchCount;
        public int MaxSwitches => _maxSwitches;
        public bool IsMoving => _isMoving;

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _camera = Camera.main;
            _stageIndex = stageIndex;
            _speedMultiplier = config.speedMultiplier;

            ClearAll();

            int layoutIdx = Mathf.Clamp(stageIndex - 1, 0, StageLayouts.Length - 1);
            _currentLayout = StageLayouts[layoutIdx];
            _maxSwitches = _currentLayout.maxSwitches;
            _switchCount = _maxSwitches;
            _hasTwoBalls = _currentLayout.startPos2.HasValue;

            CalculateLayout();
            SpawnElements();
        }

        public void ResetStage()
        {
            if (_currentLayout == null) return;
            _isMoving = false;
            _ball1Reached = false;
            _ball2Reached = false;
            _switchCount = _maxSwitches;

            // Restore magnets to initial state
            for (int i = 0; i < _currentMagnets.Count; i++)
            {
                _currentMagnets[i] = new MagnetData
                {
                    gridPos = _currentLayout.magnets[i].gridPos,
                    isNorth = _currentLayout.magnets[i].isNorth,
                    isLarge = _currentLayout.magnets[i].isLarge,
                    isSwitch = _currentLayout.magnets[i].isSwitch,
                };
                UpdateMagnetSprite(i);
            }

            ResetBallPosition();
            _gameManager.OnBallReset();
        }

        void CalculateLayout()
        {
            float camSize = _camera.orthographicSize;
            float camWidth = camSize * _camera.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 3.0f;
            float availableH = camSize * 2f - topMargin - bottomMargin;
            float availableW = camWidth * 2f - 0.4f;

            int gs = _currentLayout.gridSize;
            _cellSize = Mathf.Min(availableH / gs, availableW / gs, 1.2f);

            float totalW = _cellSize * gs;
            float totalH = _cellSize * gs;
            _gridOrigin = new Vector2(-totalW * 0.5f, -(camSize) + bottomMargin);
        }

        void SpawnElements()
        {
            // Walls
            foreach (var w in _currentLayout.walls)
            {
                var go = new GameObject("Wall");
                go.transform.SetParent(transform);
                go.transform.position = GridToWorld(w.gridPos);
                go.transform.localScale = Vector3.one * _cellSize * 0.9f;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _wallSprite;
                sr.sortingOrder = 1;
                _wallObjects.Add(go);
            }

            // Magnets
            _currentMagnets.Clear();
            for (int i = 0; i < _currentLayout.magnets.Length; i++)
            {
                var md = new MagnetData
                {
                    gridPos = _currentLayout.magnets[i].gridPos,
                    isNorth = _currentLayout.magnets[i].isNorth,
                    isLarge = _currentLayout.magnets[i].isLarge,
                    isSwitch = _currentLayout.magnets[i].isSwitch,
                };
                _currentMagnets.Add(md);

                var go = new GameObject($"Magnet_{i}");
                go.transform.SetParent(transform);
                go.transform.position = GridToWorld(md.gridPos);
                float scale = md.isLarge ? _cellSize * 0.95f : _cellSize * 0.85f;
                go.transform.localScale = Vector3.one * scale;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = GetMagnetSprite(md);
                sr.sortingOrder = 2;

                var col = go.AddComponent<CircleCollider2D>();
                col.radius = 0.5f;
                col.isTrigger = true;

                _magnetObjects.Add(go);
            }

            // Goal
            _goalObject = new GameObject("Goal");
            _goalObject.transform.SetParent(transform);
            _goalObject.transform.position = GridToWorld(_currentLayout.goalPos);
            _goalObject.transform.localScale = Vector3.one * _cellSize * 0.8f;
            var goalSr = _goalObject.AddComponent<SpriteRenderer>();
            goalSr.sprite = _goalSprite;
            goalSr.sortingOrder = 1;

            if (_hasTwoBalls)
            {
                _goal2Object = new GameObject("Goal2");
                _goal2Object.transform.SetParent(transform);
                _goal2Object.transform.position = GridToWorld(_currentLayout.goalPos2.Value);
                _goal2Object.transform.localScale = Vector3.one * _cellSize * 0.8f;
                var g2Sr = _goal2Object.AddComponent<SpriteRenderer>();
                g2Sr.sprite = _goal2Sprite;
                g2Sr.sortingOrder = 1;
            }

            // Balls
            _ballObject = new GameObject("Ball");
            _ballObject.transform.SetParent(transform);
            var ballSr = _ballObject.AddComponent<SpriteRenderer>();
            ballSr.sprite = _ballSprite;
            ballSr.sortingOrder = 3;
            _ballObject.transform.localScale = Vector3.one * _cellSize * 0.7f;

            if (_hasTwoBalls)
            {
                _ball2Object = new GameObject("Ball2");
                _ball2Object.transform.SetParent(transform);
                var b2Sr = _ball2Object.AddComponent<SpriteRenderer>();
                b2Sr.sprite = _ball2Sprite;
                b2Sr.sortingOrder = 3;
                _ball2Object.transform.localScale = Vector3.one * _cellSize * 0.7f;
            }

            ResetBallPosition();
        }

        void ResetBallPosition()
        {
            _ballPos = GridToWorld(_currentLayout.startPos);
            _ballVelocity = Vector2.zero;
            _ballObject.transform.position = _ballPos;

            if (_hasTwoBalls && _ball2Object != null)
            {
                _ball2Pos = GridToWorld(_currentLayout.startPos2.Value);
                _ball2Velocity = Vector2.zero;
                _ball2Object.transform.position = _ball2Pos;
            }
        }

        Vector2 GridToWorld(Vector2Int gp)
        {
            return _gridOrigin + new Vector2(gp.x + 0.5f, gp.y + 0.5f) * _cellSize;
        }

        Sprite GetMagnetSprite(MagnetData md)
        {
            if (md.isSwitch) return _magnetSwitchSprite;
            if (md.isLarge) return md.isNorth ? _magnetNLargeSprite : _magnetSLargeSprite;
            return md.isNorth ? _magnetNSprite : _magnetSSprite;
        }

        void UpdateMagnetSprite(int idx)
        {
            if (idx < 0 || idx >= _magnetObjects.Count) return;
            var sr = _magnetObjects[idx].GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = GetMagnetSprite(_currentMagnets[idx]);
        }

        void Update()
        {
            if (_gameManager.State == MagnetPathGameManager.GameState.Playing)
            {
                HandleInput();
            }
            else if (_gameManager.State == MagnetPathGameManager.GameState.BallMoving)
            {
                SimulateBall(Time.deltaTime);
            }
        }

        void HandleInput()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));

            // Check magnet tap
            for (int i = 0; i < _magnetObjects.Count; i++)
            {
                float dist = Vector2.Distance(worldPos, (Vector2)_magnetObjects[i].transform.position);
                if (dist < _cellSize * 0.5f)
                {
                    if (!_currentMagnets[i].isSwitch)
                    {
                        SwitchMagnet(i);
                    }
                    return;
                }
            }
        }

        void SwitchMagnet(int idx)
        {
            if (_switchCount <= 0)
            {
                _gameManager.OnSwitchLimitExceeded();
                return;
            }

            _switchCount--;
            _currentMagnets[idx].isNorth = !_currentMagnets[idx].isNorth;
            UpdateMagnetSprite(idx);

            // Visual feedback: scale pulse
            StartCoroutine(ScalePulse(_magnetObjects[idx].transform, 1.3f, 0.15f));
        }

        IEnumerator ScalePulse(Transform t, float peakScale, float duration)
        {
            if (t == null) yield break;
            float baseScale = t.localScale.x;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                float ratio = elapsed / duration;
                float s = ratio < 0.5f
                    ? Mathf.Lerp(baseScale, baseScale * peakScale, ratio * 2f)
                    : Mathf.Lerp(baseScale * peakScale, baseScale, (ratio - 0.5f) * 2f);
                t.localScale = Vector3.one * s;
                yield return null;
            }
            t.localScale = Vector3.one * baseScale;
        }

        public void LaunchBall()
        {
            if (_gameManager.State != MagnetPathGameManager.GameState.Playing) return;
            _isMoving = true;
            _ball1Reached = false;
            _ball2Reached = false;
            _ballVelocity = Vector2.zero;
            if (_hasTwoBalls) _ball2Velocity = Vector2.zero;
            _gameManager.OnBallLaunched();
        }

        void SimulateBall(float dt)
        {
            bool allDone = true;

            if (!_ball1Reached)
            {
                MoveBall(ref _ballPos, ref _ballVelocity, dt, false);
                _ballObject.transform.position = _ballPos;

                // Check goal
                float distGoal = Vector2.Distance(_ballPos, (Vector2)_goalObject.transform.position);
                if (distGoal < GOAL_RADIUS * _cellSize)
                {
                    _ball1Reached = true;
                    StartCoroutine(GoalEffect(_ballObject.transform));
                }
                else if (IsOutOfBounds(_ballPos))
                {
                    _gameManager.OnBallOutOfBounds();
                    return;
                }
                else
                {
                    allDone = false;
                }
            }

            if (_hasTwoBalls && !_ball2Reached && _ball2Object != null)
            {
                MoveBall(ref _ball2Pos, ref _ball2Velocity, dt, true);
                _ball2Object.transform.position = _ball2Pos;

                float distGoal2 = Vector2.Distance(_ball2Pos, (Vector2)_goal2Object.transform.position);
                if (distGoal2 < GOAL_RADIUS * _cellSize)
                {
                    _ball2Reached = true;
                    StartCoroutine(GoalEffect(_ball2Object.transform));
                }
                else if (IsOutOfBounds(_ball2Pos))
                {
                    _gameManager.OnBallOutOfBounds();
                    return;
                }
                else
                {
                    allDone = false;
                }
            }
            if (_ball1Reached && (!_hasTwoBalls || _ball2Reached))
                allDone = true;

            if (allDone)
            {
                _isMoving = false;
                _gameManager.OnGoalReached(_switchCount, _maxSwitches);
            }
        }

        void MoveBall(ref Vector2 pos, ref Vector2 vel, float dt, bool isSecond)
        {
            Vector2 force = Vector2.zero;

            for (int i = 0; i < _currentMagnets.Count; i++)
            {
                Vector2 mPos = (Vector2)_magnetObjects[i].transform.position;
                Vector2 dir = mPos - pos;
                float dist = dir.magnitude;

                float baseRadius = MAGNET_INFLUENCE_RADIUS * _cellSize;
                float strength = BASE_MAGNET_STRENGTH;

                if (_currentMagnets[i].isLarge)
                {
                    baseRadius *= 1.5f;
                    strength *= 1.5f;
                }

                if (dist < baseRadius && dist > 0.01f)
                {
                    float falloff = 1f - (dist / baseRadius);
                    float magnitude = strength * falloff * falloff * _speedMultiplier;
                    if (_currentMagnets[i].isNorth)
                        force += dir.normalized * magnitude;   // 引力
                    else
                        force -= dir.normalized * magnitude;   // 斥力

                    // Switch magnet trigger (one-shot per launch)
                    if (_currentMagnets[i].isSwitch && !_currentMagnets[i].switchFired && dist < SWITCH_MAGNET_TRIGGER_RADIUS * _cellSize)
                    {
                        _currentMagnets[i].isNorth = !_currentMagnets[i].isNorth;
                        _currentMagnets[i].switchFired = true;
                        UpdateMagnetSprite(i);
                        StartCoroutine(ScalePulse(_magnetObjects[i].transform, 1.4f, 0.2f));
                    }
                }
            }

            vel += force * dt;
            float speed = vel.magnitude;
            if (speed > BALL_MAX_SPEED * _speedMultiplier)
                vel = vel.normalized * BALL_MAX_SPEED * _speedMultiplier;
            vel *= BALL_DAMPING;

            pos += vel * dt;
        }

        bool IsOutOfBounds(Vector2 pos)
        {
            float camSize = _camera.orthographicSize;
            float camWidth = camSize * _camera.aspect;
            return Mathf.Abs(pos.x) > camWidth + 0.5f || Mathf.Abs(pos.y) > camSize + 0.5f;
        }

        IEnumerator GoalEffect(Transform t)
        {
            // Scale pop + white flash
            if (t == null) yield break;
            var sr = t.GetComponent<SpriteRenderer>();
            float elapsed = 0f;
            float dur = 0.3f;
            Vector3 baseScale = t.localScale;
            while (elapsed < dur)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                float r = elapsed / dur;
                float s = r < 0.5f ? Mathf.Lerp(1f, 1.5f, r * 2f) : Mathf.Lerp(1.5f, 0.8f, (r - 0.5f) * 2f);
                t.localScale = baseScale * s;
                if (sr != null) sr.color = Color.Lerp(Color.white, new Color(0.5f, 1f, 0.5f), r);
                yield return null;
            }
            t.localScale = baseScale;
            if (sr != null) sr.color = Color.white;
        }

        void ClearAll()
        {
            foreach (var go in _magnetObjects) if (go != null) Destroy(go);
            _magnetObjects.Clear();
            _currentMagnets.Clear();
            foreach (var go in _wallObjects) if (go != null) Destroy(go);
            _wallObjects.Clear();
            if (_ballObject != null) { Destroy(_ballObject); _ballObject = null; }
            if (_ball2Object != null) { Destroy(_ball2Object); _ball2Object = null; }
            if (_goalObject != null) { Destroy(_goalObject); _goalObject = null; }
            if (_goal2Object != null) { Destroy(_goal2Object); _goal2Object = null; }
            _isMoving = false;
        }

        void OnDestroy()
        {
            ClearAll();
        }
    }
}
