using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game025v2_TowerDefend
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] Sprite _spriteNormal;
        [SerializeField] Sprite _spriteFast;
        [SerializeField] Sprite _spriteFlying;
        [SerializeField] Sprite _spriteBreaker;

        TowerDefendGameManager _gameManager;
        WallManager _wallManager;

        // Stage config
        int _stage;
        float _speedMultiplier;
        float _countMultiplier;
        float _complexityFactor;

        // Wave state
        int _currentWave = -1;
        public int TotalWaves { get; private set; }
        public bool HasMoreWaves => _currentWave + 1 < TotalWaves;
        public bool WasWavePerfect { get; private set; }

        // Active enemies in current wave
        HashSet<Enemy> _activeEnemies = new();
        int _spawnedCount;
        int _totalInWave;
        bool _waveRunning;

        // Paths
        List<Vector3> _startPositions = new();
        Vector3 _goalPosition;

        Coroutine _spawnCoroutine;

        public void Initialize(TowerDefendGameManager gm, WallManager wm)
        {
            _gameManager = gm;
            _wallManager = wm;
        }

        public void SetPaths(List<Vector3> startPositions, Vector3 goalPosition)
        {
            _startPositions = startPositions;
            _goalPosition = goalPosition;
        }

        public void SetupStage(StageManager.StageConfig config, int stage)
        {
            _stage = stage;
            _speedMultiplier = config.speedMultiplier;
            _countMultiplier = config.countMultiplier;
            _complexityFactor = config.complexityFactor;
            _currentWave = -1;
            TotalWaves = stage switch
            {
                1 => 2,
                2 => 3,
                3 => 4,
                4 => 4,
                5 => 5,
                _ => 2
            };
            _activeEnemies.Clear();
            _waveRunning = false;
        }

        public void StartNextWave()
        {
            _currentWave++;
            WasWavePerfect = true;
            _activeEnemies.Clear();
            _spawnedCount = 0;
            _totalInWave = GetWaveEnemyCount();
            _waveRunning = true;

            if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = StartCoroutine(SpawnWave());
        }

        int GetWaveEnemyCount()
        {
            int base_count = _stage switch
            {
                1 => 5,
                2 => 7,
                3 => 8,
                4 => 10,
                5 => 12,
                _ => 5
            };
            return Mathf.RoundToInt(base_count * _countMultiplier);
        }

        EnemyType GetEnemyType(int index)
        {
            float r = Random.value;
            return _stage switch
            {
                1 => EnemyType.Normal,
                2 => r < 0.20f ? EnemyType.Fast : EnemyType.Normal,
                3 => r < 0.15f ? EnemyType.Flying : r < 0.30f ? EnemyType.Fast : EnemyType.Normal,
                4 => r < 0.20f ? EnemyType.Breaker : r < 0.35f ? EnemyType.Fast : EnemyType.Normal,
                5 => r < 0.15f ? EnemyType.Flying : r < 0.30f ? EnemyType.Breaker : r < 0.45f ? EnemyType.Fast : EnemyType.Normal,
                _ => EnemyType.Normal
            };
        }

        IEnumerator SpawnWave()
        {
            float spawnInterval = _stage >= 4 ? 0.8f : _stage >= 2 ? 1.0f : 1.3f;
            spawnInterval /= _speedMultiplier;

            for (int i = 0; i < _totalInWave; i++)
            {
                if (!_waveRunning) yield break;
                SpawnEnemy(i);
                _spawnedCount++;
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        void SpawnEnemy(int index)
        {
            var type = GetEnemyType(index);
            // Stage 5: alternate spawn points
            if (_startPositions.Count == 0) return;
            int startIdx = _stage == 5 ? index % _startPositions.Count : 0;
            startIdx = Mathf.Clamp(startIdx, 0, _startPositions.Count - 1);
            Vector3 spawnPos = _startPositions[startIdx];

            var obj = new GameObject($"Enemy_{_currentWave}_{index}");
            obj.transform.position = spawnPos;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetSprite(type);
            sr.sortingOrder = 5;

            float size = type == EnemyType.Flying ? 0.6f : 0.5f;
            obj.transform.localScale = Vector3.one * size;

            if (type != EnemyType.Flying)
            {
                var col = obj.AddComponent<CircleCollider2D>();
                col.radius = 0.5f;
                col.isTrigger = true;
            }

            var enemy = obj.AddComponent<Enemy>();
            float speed = GetSpeed(type);

            List<Vector3> path;
            if (type == EnemyType.Flying)
                path = new List<Vector3> { _goalPosition };
            else if (_wallManager != null)
                path = _wallManager.CalculatePath(spawnPos, _goalPosition);
            else
                path = new List<Vector3> { _goalPosition };

            enemy.Initialize(type, path, speed, _gameManager, this, _wallManager, _goalPosition);
            _activeEnemies.Add(enemy);
        }

        float GetSpeed(EnemyType type)
        {
            float base_speed = type switch
            {
                EnemyType.Fast => 3.5f,
                EnemyType.Flying => 2.5f,
                EnemyType.Breaker => 1.5f,
                _ => 2.0f
            };
            return base_speed * _speedMultiplier;
        }

        Sprite GetSprite(EnemyType type) => type switch
        {
            EnemyType.Fast => _spriteFast != null ? _spriteFast : _spriteNormal,
            EnemyType.Flying => _spriteFlying != null ? _spriteFlying : _spriteNormal,
            EnemyType.Breaker => _spriteBreaker != null ? _spriteBreaker : _spriteNormal,
            _ => _spriteNormal
        };

        public void OnEnemyRemoved(Enemy enemy)
        {
            _activeEnemies.Remove(enemy);
            CheckWaveComplete();
        }

        void CheckWaveComplete()
        {
            if (!_waveRunning) return;
            if (_spawnedCount < _totalInWave) return;
            if (_activeEnemies.Count > 0) return;

            _waveRunning = false;
            _gameManager?.OnWaveCleared(_currentWave);
        }

        public void StopAll()
        {
            _waveRunning = false;
            if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
            foreach (var e in new List<Enemy>(_activeEnemies))
                if (e != null) e.StopMoving();
            _activeEnemies.Clear();
        }

        // Called by enemy when it reaches goal (marks wave as not perfect)
        public void OnEnemyBreached()
        {
            WasWavePerfect = false;
        }
    }
}
