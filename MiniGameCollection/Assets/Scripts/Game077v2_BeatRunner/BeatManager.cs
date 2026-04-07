using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game077v2_BeatRunner
{
    public class BeatManager : MonoBehaviour
    {
        [SerializeField] BeatRunnerGameManager _gameManager;
        [SerializeField] SpriteRenderer _runnerRenderer;
        [SerializeField] Transform _obstaclePool;
        [SerializeField] Transform _groundContainer;
        [SerializeField] Sprite _runnerIdleSprite;
        [SerializeField] Sprite _runnerJumpSprite;
        [SerializeField] Sprite _runnerSlideSprite;
        [SerializeField] Sprite _obstacleJumpSprite;
        [SerializeField] Sprite _obstacleSlideSprite;
        [SerializeField] Sprite _coinSprite;
        [SerializeField] Sprite _groundTileSprite;

        // Judgment windows (seconds)
        const float PERFECT_WINDOW = 0.08f;
        const float GREAT_WINDOW   = 0.15f;
        const float GOOD_WINDOW    = 0.25f;
        const int   MAX_LIFE       = 3;
        const int   BEATS_PER_STAGE = 20;

        // Score multiplier caps
        const float PERFECT_COMBO_MULT_MAX = 3.0f;
        const float GREAT_COMBO_MULT_MAX   = 2.0f;

        // Stage data
        struct StageData
        {
            public float beatInterval;
            public float baseSpeed;
            public bool hasSlide;
            public bool hasCoin;
            public bool hasAerial;
            public bool hasBossZone;
        }

        readonly StageData[] _stageData = new StageData[]
        {
            new StageData { beatInterval=0.60f, baseSpeed=3.0f, hasSlide=false, hasCoin=false, hasAerial=false, hasBossZone=false },
            new StageData { beatInterval=0.50f, baseSpeed=4.0f, hasSlide=true,  hasCoin=false, hasAerial=false, hasBossZone=false },
            new StageData { beatInterval=0.43f, baseSpeed=5.0f, hasSlide=true,  hasCoin=true,  hasAerial=false, hasBossZone=false },
            new StageData { beatInterval=0.375f,baseSpeed=6.0f, hasSlide=true,  hasCoin=true,  hasAerial=true,  hasBossZone=false },
            new StageData { beatInterval=0.33f, baseSpeed=7.0f, hasSlide=true,  hasCoin=true,  hasAerial=true,  hasBossZone=true  },
        };

        bool _isActive;
        bool _isPlaying;
        int _life;
        int _combo;
        int _totalScore;
        int _stageIndex;
        int _beatsCompleted;
        int _beatsSpawned;
        float _currentSpeed;
        float _beatInterval;
        float _nextBeatTime;
        float _speedBoost; // from combo

        // Camera layout
        float _camSize;
        float _camWidth;
        float _runnerX;
        float _groundY;
        float _obstacleSpawnX;
        float _obstacleDestroyX;
        float _jumpTargetY;
        float _slideTargetY;
        float _runnerBaseY;
        const float BOTTOM_MARGIN = 3.0f;

        // Runner state
        enum RunnerState { Idle, Jump, Slide }
        RunnerState _runnerState = RunnerState.Idle;
        float _jumpTimer;
        float _slideTimer;
        const float JUMP_DURATION = 0.5f;
        const float SLIDE_DURATION = 0.4f;
        Vector3 _runnerPos;

        // Pending beat
        struct PendingBeat
        {
            public float beatTime;
            public ObstacleType type;
            public GameObject obstacle;
        }
        enum ObstacleType { Jump, Slide, Coin }
        List<PendingBeat> _pendingBeats = new List<PendingBeat>();
        List<GameObject> _activeObstacles = new List<GameObject>();
        List<GameObject> _groundTiles = new List<GameObject>();

        // Ground scroll
        float _groundTileWidth = 2f;
        int _groundTileCount = 8;

        // Input swipe
        Vector2 _touchStartPos;
        bool _touchStarted;

        public int TotalScore => _totalScore;

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            var sd = _stageData[Mathf.Clamp(stageIndex, 0, _stageData.Length - 1)];
            _beatInterval = sd.beatInterval / config.speedMultiplier;
            _currentSpeed = sd.baseSpeed * config.speedMultiplier;
            _speedBoost = 1.0f;
            _beatsCompleted = 0;
            _beatsSpawned = 0;
            _combo = 0;
            _life = MAX_LIFE;
            ClearObstacles();
            _nextBeatTime = Time.time + _beatInterval * 2f; // small warmup
            _isActive = true;
            _isPlaying = true;
            _gameManager.UpdateLifeDisplay(_life);
            _gameManager.UpdateComboDisplay(0);
            _gameManager.UpdateProgressDisplay(0, BEATS_PER_STAGE);
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            _isPlaying = active;
            if (!active)
            {
                StopAllCoroutines();
                if (Camera.main != null)
                    Camera.main.transform.position = new Vector3(0f, 0f, -10f);
            }
        }

        void Awake()
        {
            _camSize = Camera.main.orthographicSize;
            _camWidth = _camSize * Camera.main.aspect;
            _runnerX = -_camWidth * 0.35f;
            _groundY = -_camSize + BOTTOM_MARGIN;
            _runnerBaseY = _groundY + 0.6f;
            _jumpTargetY = _runnerBaseY + _camSize * 0.35f;
            _slideTargetY = _runnerBaseY - 0.3f;
            _obstacleSpawnX = _camWidth + 1.5f;
            _obstacleDestroyX = -_camWidth - 2f;
            SetupGround();
            SetupRunnerPosition();
        }

        void SetupRunnerPosition()
        {
            if (_runnerRenderer == null) return;
            _runnerPos = new Vector3(_runnerX, _runnerBaseY, 0f);
            _runnerRenderer.transform.position = _runnerPos;
        }

        void SetupGround()
        {
            if (_groundTileSprite == null || _groundContainer == null) return;
            _groundTileWidth = _groundTileSprite.bounds.size.x > 0 ? _groundTileSprite.bounds.size.x : 2f;
            for (int i = 0; i < _groundTileCount; i++)
            {
                var go = new GameObject("Ground_" + i);
                go.transform.SetParent(_groundContainer);
                go.transform.position = new Vector3(-_camWidth + i * _groundTileWidth, _groundY - 0.2f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _groundTileSprite;
                sr.sortingOrder = -1;
                _groundTiles.Add(go);
            }
        }

        void Update()
        {
            if (!_isActive || !_isPlaying) return;
            HandleInput();
            UpdateRunnerAnimation();
            ScrollGround();
            MoveObstacles();
            CheckBeatWindow();
            SpawnNextBeat();
        }

        void HandleInput()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _touchStartPos = Mouse.current.position.ReadValue();
                _touchStarted = true;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame && _touchStarted)
            {
                _touchStarted = false;
                var endPos = Mouse.current.position.ReadValue();
                var delta = endPos - _touchStartPos;
                bool isSwipe = delta.magnitude > 40f;

                if (isSwipe)
                {
                    if (delta.y > 0) TryAction(RunnerState.Jump);
                    else TryAction(RunnerState.Slide);
                }
                else
                {
                    // tap: upper half = jump, lower half = slide
                    float screenY = _touchStartPos.y / Screen.height;
                    if (screenY > 0.5f) TryAction(RunnerState.Jump);
                    else TryAction(RunnerState.Slide);
                }
            }
        }

        void TryAction(RunnerState action)
        {
            if (_runnerState != RunnerState.Idle) return; // prevent double input
            // Find nearest pending beat
            float now = Time.time;
            float bestDiff = float.MaxValue;
            int bestIdx = -1;
            for (int i = 0; i < _pendingBeats.Count; i++)
            {
                float diff = Mathf.Abs(_pendingBeats[i].beatTime - now);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestIdx = i;
                }
            }

            if (bestIdx >= 0 && bestDiff <= GOOD_WINDOW)
            {
                var beat = _pendingBeats[bestIdx];
                bool correct = (action == RunnerState.Jump && beat.type == ObstacleType.Jump)
                            || (action == RunnerState.Slide && beat.type == ObstacleType.Slide)
                            || (beat.type == ObstacleType.Coin); // coin accepts any action

                if (correct)
                {
                    EvaluateJudgement(bestDiff, beat);
                    _pendingBeats.RemoveAt(bestIdx);
                    if (beat.obstacle != null)
                        StartCoroutine(FadeObstacle(beat.obstacle));
                }
                else
                {
                    RegisterMiss("WRONG");
                }
            }
            else
            {
                // No nearby beat: free action (no score effect)
            }

            // Apply runner visual state
            if (action == RunnerState.Jump && _runnerState == RunnerState.Idle)
                StartCoroutine(DoJump());
            else if (action == RunnerState.Slide && _runnerState == RunnerState.Idle)
                StartCoroutine(DoSlide());
        }

        void EvaluateJudgement(float diff, PendingBeat beat)
        {
            _combo++;
            string judgeText;
            Color judgeColor;
            int baseScore;
            float speedMod;

            if (diff <= PERFECT_WINDOW)
            {
                baseScore = 100;
                float mult = Mathf.Min(1.0f + _combo * 0.1f, PERFECT_COMBO_MULT_MAX);
                _totalScore += Mathf.RoundToInt(baseScore * mult);
                speedMod = 0.10f;
                judgeText = "PERFECT!";
                judgeColor = new Color(1f, 0.9f, 0.1f);
            }
            else if (diff <= GREAT_WINDOW)
            {
                baseScore = 60;
                float mult = Mathf.Min(1.0f + _combo * 0.05f, GREAT_COMBO_MULT_MAX);
                _totalScore += Mathf.RoundToInt(baseScore * mult);
                speedMod = 0.05f;
                judgeText = "GREAT!";
                judgeColor = new Color(0.2f, 0.9f, 1f);
            }
            else // GOOD
            {
                _totalScore += 20;
                speedMod = 0f;
                judgeText = "GOOD";
                judgeColor = new Color(0.5f, 1f, 0.5f);
            }

            _speedBoost = Mathf.Min(_speedBoost + speedMod, 2.0f);
            _gameManager.UpdateScoreDisplay(_totalScore);
            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.ShowJudgement(judgeText, judgeColor);

            _beatsCompleted++;
            _gameManager.UpdateProgressDisplay(_beatsCompleted, BEATS_PER_STAGE);
            CheckStageClear();
        }

        void RegisterMiss(string reason = "MISS")
        {
            _combo = 0;
            _speedBoost = Mathf.Max(_speedBoost - 0.15f, 0.5f);
            _life--;
            _gameManager.UpdateComboDisplay(0);
            _gameManager.ShowJudgement("MISS", new Color(1f, 0.3f, 0.3f));
            _gameManager.UpdateLifeDisplay(_life);
            StartCoroutine(CameraShake(0.2f, 0.25f));

            if (_life <= 0)
            {
                _isPlaying = false;
                _gameManager.OnGameOver();
            }
        }

        void CheckBeatWindow()
        {
            // Any beat that passed GOOD_WINDOW without action → miss
            float now = Time.time;
            for (int i = _pendingBeats.Count - 1; i >= 0; i--)
            {
                if (!_isPlaying) break; // stop processing after game over
                if (now - _pendingBeats[i].beatTime > GOOD_WINDOW + 0.05f)
                {
                    if (_pendingBeats[i].obstacle != null)
                        StartCoroutine(CollideObstacle(_pendingBeats[i].obstacle));
                    _pendingBeats.RemoveAt(i);
                    RegisterMiss();
                }
            }
        }

        void SpawnNextBeat()
        {
            if (!_isPlaying) return;
            if (Time.time < _nextBeatTime) return;
            if (_beatsSpawned >= BEATS_PER_STAGE) return;

            _nextBeatTime = Time.time + _beatInterval;
            var sd = _stageData[Mathf.Clamp(_stageIndex, 0, _stageData.Length - 1)];

            ObstacleType type = ObstacleType.Jump;
            if (sd.hasSlide && _beatsCompleted % 2 == 1) type = ObstacleType.Slide;
            if (sd.hasCoin && _beatsCompleted % 5 == 4) type = ObstacleType.Coin;
            if (sd.hasAerial && _beatsCompleted % 7 == 6) type = ObstacleType.Slide; // aerial = slide needed

            // Boss zone: last 5 beats alternate rapidly
            if (sd.hasBossZone && _beatsCompleted >= BEATS_PER_STAGE - 6)
                type = (_beatsCompleted % 2 == 0) ? ObstacleType.Jump : ObstacleType.Slide;

            SpawnObstacle(type);
        }

        void SpawnObstacle(ObstacleType type)
        {
            var go = new GameObject("Obstacle_" + type);
            go.transform.SetParent(_obstaclePool);
            var sr = go.AddComponent<SpriteRenderer>();

            float yPos = _runnerBaseY;
            if (type == ObstacleType.Jump)
            {
                sr.sprite = _obstacleJumpSprite;
                yPos = _groundY + 0.8f;
                sr.sortingOrder = 2;
            }
            else if (type == ObstacleType.Slide)
            {
                sr.sprite = _obstacleSlideSprite;
                yPos = _runnerBaseY + 0.3f; // aerial: above runner stand height
                sr.sortingOrder = 2;
            }
            else // Coin
            {
                sr.sprite = _coinSprite;
                yPos = _runnerBaseY + 0.5f;
                sr.sortingOrder = 2;
            }

            go.transform.position = new Vector3(_obstacleSpawnX, yPos, 0f);
            _activeObstacles.Add(go);

            // Beat time: when obstacle reaches runner X
            float travelDist = _obstacleSpawnX - _runnerX;
            float actualSpeed = _currentSpeed * _speedBoost;
            float travelTime = travelDist / actualSpeed;
            _pendingBeats.Add(new PendingBeat
            {
                beatTime = Time.time + travelTime,
                type = type,
                obstacle = go
            });
            _beatsSpawned++;
        }

        void MoveObstacles()
        {
            float actualSpeed = _currentSpeed * _speedBoost;
            for (int i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                if (_activeObstacles[i] == null) { _activeObstacles.RemoveAt(i); continue; }
                _activeObstacles[i].transform.position += Vector3.left * actualSpeed * Time.deltaTime;
                if (_activeObstacles[i].transform.position.x < _obstacleDestroyX)
                {
                    Destroy(_activeObstacles[i]);
                    _activeObstacles.RemoveAt(i);
                }
            }
        }

        void ScrollGround()
        {
            float actualSpeed = _currentSpeed * _speedBoost;
            float totalWidth = _groundTileWidth * _groundTileCount;
            foreach (var tile in _groundTiles)
            {
                if (tile == null) continue;
                tile.transform.position += Vector3.left * actualSpeed * Time.deltaTime;
                if (tile.transform.position.x < -_camWidth - _groundTileWidth)
                    tile.transform.position += Vector3.right * totalWidth;
            }
        }

        void UpdateRunnerAnimation()
        {
            if (_runnerRenderer == null) return;
            switch (_runnerState)
            {
                case RunnerState.Idle:  _runnerRenderer.sprite = _runnerIdleSprite; break;
                case RunnerState.Jump:  _runnerRenderer.sprite = _runnerJumpSprite; break;
                case RunnerState.Slide: _runnerRenderer.sprite = _runnerSlideSprite; break;
            }
        }

        IEnumerator DoJump()
        {
            _runnerState = RunnerState.Jump;
            float elapsed = 0f;
            Vector3 start = new Vector3(_runnerX, _runnerBaseY, 0f);
            Vector3 peak  = new Vector3(_runnerX, _jumpTargetY, 0f);
            while (elapsed < JUMP_DURATION)
            {
                float t = elapsed / JUMP_DURATION;
                float y = Mathf.Lerp(start.y, peak.y, Mathf.Sin(t * Mathf.PI));
                _runnerRenderer.transform.position = new Vector3(_runnerX, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _runnerRenderer.transform.position = start;
            _runnerState = RunnerState.Idle;
        }

        IEnumerator DoSlide()
        {
            _runnerState = RunnerState.Slide;
            yield return new WaitForSeconds(SLIDE_DURATION);
            _runnerState = RunnerState.Idle;
        }

        IEnumerator FadeObstacle(GameObject go)
        {
            if (go == null) yield break;
            var sr = go.GetComponent<SpriteRenderer>();
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                if (go == null || sr == null) yield break;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (go != null) Destroy(go);
            _activeObstacles.Remove(go);
        }

        IEnumerator CollideObstacle(GameObject go)
        {
            if (go == null) yield break;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color orig = sr.color;
                sr.color = new Color(1f, 0.2f, 0.2f, 1f);
                yield return new WaitForSeconds(0.15f);
                if (sr != null) sr.color = orig;
            }
            yield return new WaitForSeconds(0.1f);
            if (go != null) Destroy(go);
            _activeObstacles.Remove(go);
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            Vector3 orig = cam.transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                cam.transform.position = orig + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cam.transform.position = orig;
        }

        void CheckStageClear()
        {
            if (_beatsCompleted >= BEATS_PER_STAGE)
            {
                _isPlaying = false;
                _gameManager.OnStageClear();
            }
        }

        void ClearObstacles()
        {
            foreach (var go in _activeObstacles)
                if (go != null) Destroy(go);
            _activeObstacles.Clear();
            _pendingBeats.Clear();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            ClearObstacles();
        }
    }
}
