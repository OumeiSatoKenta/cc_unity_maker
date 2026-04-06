using UnityEngine;
using System.Collections.Generic;

namespace Game056v2_InflateFloat
{
    public class CourseManager : MonoBehaviour
    {
        [SerializeField] InflateFloatGameManager _gm;
        [SerializeField] BalloonController _balloon;
        [SerializeField] Sprite _obstacleSprite;
        [SerializeField] Sprite _coinSprite;
        [SerializeField] Sprite _goalSprite;
        [SerializeField] Sprite _spikeSprite;

        float _scrollSpeed = 2f;
        float _gapSize = 3.0f;
        int _totalObstacles = 8;
        bool _movingObstacles = false;
        bool _narrowSection = false;
        bool _hasWind = false;
        bool _hasSpike = false;
        int _stageNum = 1;

        float _camSize;
        float _camHalfWidth;
        float _spawnX;
        float _despawnX;

        List<GameObject> _obstacles = new List<GameObject>();
        List<GameObject> _coins = new List<GameObject>();
        GameObject _goal;

        float _nextSpawnDist;
        float _spawnInterval = 4f;
        float _traveledDist;
        int _spawnedObstacles = 0;
        bool _goalSpawned = false;
        int _passedObstacles = 0;

        public int TotalCoins { get; private set; }
        int _collectedCoins = 0;

        public void SetupStage(StageManager.StageConfig config, int stageNum)
        {
            _stageNum = stageNum;
            _scrollSpeed = 2f + config.speedMultiplier * 0.5f;
            _totalObstacles = 5 + Mathf.RoundToInt(config.countMultiplier * 3f);
            _gapSize = stageNum == 1 ? 3.5f : stageNum == 2 ? 3.0f : stageNum == 3 ? 2.2f : stageNum == 4 ? 2.8f : 2.0f;
            _movingObstacles = stageNum >= 2;
            _narrowSection = stageNum >= 3;
            _hasWind = stageNum >= 4;
            _hasSpike = stageNum >= 5;
            _spawnInterval = Mathf.Max(1.0f, 4f - config.speedMultiplier * 0.3f);

            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[CourseManager] Camera.main not found"); return; }
            _camSize = cam.orthographicSize;
            _camHalfWidth = _camSize * cam.aspect;
            _spawnX = _camHalfWidth + 3f;
            _despawnX = -_camHalfWidth - 3f;

            ClearAll();
            _spawnedObstacles = 0;
            _goalSpawned = false;
            _passedObstacles = 0;
            _collectedCoins = 0;
            TotalCoins = _totalObstacles * 2;
            _traveledDist = 0f;
            _nextSpawnDist = 0f;
        }

        void ClearAll()
        {
            foreach (var o in _obstacles) if (o != null) Destroy(o);
            foreach (var c in _coins) if (c != null) Destroy(c);
            if (_goal != null) Destroy(_goal);
            _obstacles.Clear();
            _coins.Clear();
            _goal = null;
        }

        void Update()
        {
            if (!_gm.IsPlaying()) return;

            // Move all objects left
            float move = _scrollSpeed * Time.deltaTime;
            foreach (var o in _obstacles) if (o != null) o.transform.position += Vector3.left * move;
            foreach (var c in _coins) if (c != null) c.transform.position += Vector3.left * move;
            if (_goal != null) _goal.transform.position += Vector3.left * move;

            // Animate moving obstacles
            if (_movingObstacles)
            {
                float t = Time.time;
                for (int i = 0; i < _obstacles.Count; i++)
                {
                    var obs = _obstacles[i];
                    if (obs == null) continue;
                    var mover = obs.GetComponent<ObstacleMover>();
                    if (mover != null) mover.UpdatePos(t);
                }
            }

            // Despawn + check pass
            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                var o = _obstacles[i];
                if (o == null) { _obstacles.RemoveAt(i); continue; }
                if (o.transform.position.x < _despawnX)
                {
                    var info = o.GetComponent<ObstacleInfo>();
                    if (info != null && info.isTopPipe) { _passedObstacles++; _gm.OnObstaclePassed(); }
                    Destroy(o);
                    _obstacles.RemoveAt(i);
                }
            }

            // Coin collect
            if (_balloon != null)
            {
                for (int i = _coins.Count - 1; i >= 0; i--)
                {
                    var c = _coins[i];
                    if (c == null) { _coins.RemoveAt(i); continue; }
                    if (c.transform.position.x < _despawnX)
                    {
                        _gm.OnMissCombo();
                        Destroy(c);
                        _coins.RemoveAt(i);
                        continue;
                    }
                    float dist = Vector2.Distance(_balloon.transform.position, c.transform.position);
                    if (dist < 0.5f)
                    {
                        _balloon.PlayCoinEffect();
                        _gm.OnCoinCollected();
                        _collectedCoins++;
                        Destroy(c);
                        _coins.RemoveAt(i);
                    }
                }
            }

            // Goal check
            if (_goal != null && _balloon != null)
            {
                float dist = Vector2.Distance(_balloon.transform.position, _goal.transform.position);
                if (dist < 1.0f)
                {
                    Destroy(_goal);
                    _goal = null;
                    _gm.OnGoalReached();
                }
                if (_goal != null && _goal.transform.position.x < _despawnX)
                {
                    Destroy(_goal);
                    _goal = null;
                }
            }

            // Obstacle spawn based on total distance traveled
            _traveledDist += _scrollSpeed * Time.deltaTime;
            if (_spawnedObstacles < _totalObstacles)
            {
                if (_traveledDist >= _nextSpawnDist)
                {
                    SpawnObstaclePair(_spawnX);
                    SpawnCoinsNearObstacle(_spawnX);
                    _spawnedObstacles++;
                    _nextSpawnDist = _traveledDist + _spawnInterval;
                }
            }
            else if (!_goalSpawned && _obstacles.Count == 0)
            {
                SpawnGoal(_spawnX);
                _goalSpawned = true;
            }

            // Update distance
            float progress = _spawnedObstacles > 0
                ? Mathf.Clamp01((float)_passedObstacles / _totalObstacles)
                : 0f;
            _gm.OnDistanceChanged(progress);
        }

        void SpawnObstaclePair(float x)
        {
            float gap = _gapSize;
            // Narrow section in stage 3
            if (_narrowSection && _spawnedObstacles >= _totalObstacles / 2)
                gap = 2.0f;
            if (_hasSpike && _spawnedObstacles % 3 == 2)
                SpawnSpikeRow(x);

            float gapCenterY = Random.Range(-1.5f, 1.5f);
            float topY = gapCenterY + gap * 0.5f;
            float botY = gapCenterY - gap * 0.5f;

            float pipeH = (_camSize - Mathf.Abs(topY)) * 2f;
            if (pipeH < 0.5f) pipeH = 0.5f;

            // Top pipe
            var top = SpawnObstacleBar(x, topY + pipeH * 0.5f + 0.5f, pipeH, gapCenterY, _movingObstacles, true);
            // Bottom pipe
            float botH = (Mathf.Abs(botY) + _camSize) * 2f;
            if (botH < 0.5f) botH = 0.5f;
            var bot = SpawnObstacleBar(x, botY - botH * 0.5f - 0.5f, botH, gapCenterY, _movingObstacles, false);

            _obstacles.Add(top);
            _obstacles.Add(bot);
        }

        GameObject SpawnObstacleBar(float x, float y, float height, float gapCenter, bool moving, bool isTop)
        {
            var obj = new GameObject("Obstacle");
            var info = obj.AddComponent<ObstacleInfo>();
            info.isTopPipe = isTop;
            obj.transform.position = new Vector3(x, y, 0f);
            obj.transform.localScale = new Vector3(1f, height, 1f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _obstacleSprite;
            sr.sortingOrder = 2;
            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            if (moving)
            {
                var mover = obj.AddComponent<ObstacleMover>();
                mover.Init(y, gapCenter, 0.8f, Random.Range(0f, Mathf.PI));
            }

            // Trigger detection
            var trigger = obj.AddComponent<ObstacleTrigger>();
            trigger.Init(_gm);
            return obj;
        }

        void SpawnSpikeRow(float x)
        {
            for (int i = -2; i <= 2; i++)
            {
                if (i == 0) continue;
                var spike = new GameObject("Spike");
                spike.transform.position = new Vector3(x, i * 1.5f, 0f);
                spike.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                var sr = spike.AddComponent<SpriteRenderer>();
                sr.sprite = _spikeSprite;
                sr.sortingOrder = 3;
                var col = spike.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.4f;
                var trigger = spike.AddComponent<ObstacleTrigger>();
                trigger.Init(_gm);
                _obstacles.Add(spike);
            }
        }

        void SpawnCoinsNearObstacle(float x)
        {
            float gapCenter = Random.Range(-1.5f, 1.5f);
            // Two coins near the gap
            for (int i = -1; i <= 1; i += 2)
            {
                float y = gapCenter + i * 0.6f;
                SpawnCoin(x + Random.Range(-0.3f, 0.3f), y);
            }
        }

        void SpawnCoin(float x, float y)
        {
            var coin = new GameObject("Coin");
            coin.transform.position = new Vector3(x, y, 0f);
            coin.transform.localScale = Vector3.one * 0.5f;
            var sr = coin.AddComponent<SpriteRenderer>();
            sr.sprite = _coinSprite;
            sr.sortingOrder = 4;
            _coins.Add(coin);
        }

        void SpawnGoal(float x)
        {
            _goal = new GameObject("Goal");
            _goal.transform.position = new Vector3(x, 0f, 0f);
            _goal.transform.localScale = Vector3.one;
            var sr = _goal.AddComponent<SpriteRenderer>();
            sr.sprite = _goalSprite;
            sr.sortingOrder = 5;
        }
    }

    public class ObstacleInfo : MonoBehaviour
    {
        public bool isTopPipe;
    }

    public class ObstacleMover : MonoBehaviour
    {
        float _baseY;
        float _gapCenter;
        float _amplitude;
        float _phase;
        float _frequency = 1.5f;

        public void Init(float baseY, float gapCenter, float amplitude, float phase)
        {
            _baseY = baseY;
            _gapCenter = gapCenter;
            _amplitude = amplitude;
            _phase = phase;
        }

        public void UpdatePos(float t)
        {
            float offset = Mathf.Sin(t * _frequency + _phase) * _amplitude;
            Vector3 pos = transform.position;
            pos.y = _baseY + offset;
            transform.position = pos;
        }
    }

    public class ObstacleTrigger : MonoBehaviour
    {
        InflateFloatGameManager _gm;

        public void Init(InflateFloatGameManager gm) { _gm = gm; }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.name == "Balloon")
            {
                var balloon = other.GetComponent<BalloonController>();
                if (balloon != null) balloon.PopFromCollision();
            }
        }
    }
}
