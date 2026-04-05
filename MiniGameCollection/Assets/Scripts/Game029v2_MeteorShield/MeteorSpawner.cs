using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game029v2_MeteorShield
{
    public class MeteorSpawner : MonoBehaviour
    {
        [SerializeField] MeteorShieldGameManager _gameManager;
        [SerializeField] Sprite _spriteSmall;
        [SerializeField] Sprite _spriteLarge;
        [SerializeField] Sprite _spriteSplit;

        List<MeteorObject> _activeMeteors = new List<MeteorObject>();
        StageManager.StageConfig _currentConfig;
        int _currentStage;
        bool _isSpawning;
        Coroutine _spawnCo;

        // ステージ持続時間（秒）
        static readonly float[] StageDurations = { 30f, 30f, 40f, 40f, 50f };

        float _stageTimer;
        bool _stageActive;
        Coroutine _waitNotifyCo;

        void Update()
        {
            if (!_stageActive) return;
            if (_currentStage < 1 || _currentStage > StageDurations.Length) return;
            _stageTimer += Time.deltaTime;
            if (_stageTimer >= StageDurations[_currentStage - 1])
            {
                _stageActive = false;
                StopSpawning();
                if (_waitNotifyCo != null) StopCoroutine(_waitNotifyCo);
                _waitNotifyCo = StartCoroutine(WaitAndNotify());
            }
        }

        IEnumerator WaitAndNotify()
        {
            // 残隕石が消えるまで最大2秒待つ
            float wait = 0f;
            while (_activeMeteors.Count > 0 && wait < 2f)
            {
                wait += Time.deltaTime;
                yield return null;
            }
            _gameManager?.OnStageTimeUp();
        }

        public void SetupStage(StageManager.StageConfig config, int stage)
        {
            _currentConfig = config;
            _currentStage = stage;
            _stageTimer = 0f;
            _stageActive = true;

            // 既存隕石を全削除
            foreach (var m in _activeMeteors)
            {
                if (m != null) Destroy(m.gameObject);
            }
            _activeMeteors.Clear();

            if (_spawnCo != null) StopCoroutine(_spawnCo);
            _isSpawning = true;

            if (stage == 5)
                _spawnCo = StartCoroutine(SpawnGroupLoop());
            else
                _spawnCo = StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
        {
            _isSpawning = false;
            _stageActive = false;
            if (_spawnCo != null)
            {
                StopCoroutine(_spawnCo);
                _spawnCo = null;
            }
        }

        IEnumerator SpawnLoop()
        {
            float baseInterval = 2.0f / _currentConfig.countMultiplier;
            while (_isSpawning)
            {
                SpawnOneMeteor();
                float interval = baseInterval * Random.Range(0.8f, 1.2f);
                yield return new WaitForSeconds(interval);
            }
        }

        IEnumerator SpawnGroupLoop()
        {
            while (_isSpawning)
            {
                int count = Random.Range(5, 9);
                for (int i = 0; i < count; i++)
                {
                    SpawnOneMeteor();
                    yield return new WaitForSeconds(0.15f);
                }
                yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
            }
        }

        void SpawnOneMeteor()
        {
            MeteorType type = DecideMeteorType();
            Vector2 startPos = GetSpawnPosition(type);
            Vector2 direction = GetFallDirection(startPos, type);
            float speed = 3f * _currentConfig.speedMultiplier;

            GameObject go = new GameObject($"Meteor_{type}");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSprite(type);
            sr.sortingOrder = 5;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            float radius = type == MeteorType.Large ? 0.5f : (type == MeteorType.Split ? 0.4f : 0.3f);
            col.radius = radius;

            var meteor = go.AddComponent<MeteorObject>();
            meteor.Initialize(type, startPos, direction, speed, this);

            _activeMeteors.Add(meteor);
        }

        MeteorType DecideMeteorType()
        {
            float f = _currentConfig.complexityFactor;
            float rnd = Random.value;
            if (f >= 0.7f)
            {
                if (rnd < 0.20f) return MeteorType.Split;
                if (rnd < 0.35f) return MeteorType.Large;
                return MeteorType.Small;
            }
            if (f >= 0.5f)
            {
                if (rnd < 0.15f) return MeteorType.Large;
                return MeteorType.Small;
            }
            return MeteorType.Small;
        }

        Vector2 GetSpawnPosition(MeteorType type)
        {
            var cam = Camera.main;
            if (cam == null) return new Vector2(0f, 6f);

            float camH = cam.orthographicSize;
            float camW = camH * cam.aspect;

            float f = _currentConfig.complexityFactor;
            if (f >= 0.3f && Random.value < 0.3f)
            {
                // 斜め落下: 画面端上部から
                float side = Random.value < 0.5f ? -1f : 1f;
                float x = side * (camW + 0.5f);
                float y = camH + Random.Range(0.5f, 1.5f);
                return new Vector2(x, y);
            }
            else
            {
                // 真上から
                float x = Random.Range(-camW * 0.85f, camW * 0.85f);
                float y = camH + Random.Range(0.5f, 1.5f);
                return new Vector2(x, y);
            }
        }

        Vector2 GetFallDirection(Vector2 startPos, MeteorType type)
        {
            var cam = Camera.main;
            float starY = cam != null ? -(cam.orthographicSize - 1.5f) : -3.5f;
            // 星を少し狙った方向 + ランダム散らし
            float targetX = Random.Range(-1.5f, 1.5f);
            Vector2 dir = new Vector2(targetX - startPos.x, starY - startPos.y);
            return dir.normalized;
        }

        Sprite GetSprite(MeteorType type)
        {
            return type switch
            {
                MeteorType.Large => _spriteLarge,
                MeteorType.Split => _spriteSplit,
                _ => _spriteSmall,
            };
        }

        // Called by MeteorObject
        public void OnMeteorDestroyed(MeteorObject meteor, bool deflected)
        {
            _activeMeteors.Remove(meteor);
        }

        public void OnMeteorHitStar(MeteorObject meteor, float damage)
        {
            _activeMeteors.Remove(meteor);
            _gameManager?.OnStarHit(damage);
        }

        // 分裂処理
        public void SpawnSplitFragments(Vector2 origin)
        {
            for (int i = 0; i < 3; i++)
            {
                float angle = 60f + i * 120f; // 60, 180, 300度
                Vector2 dir = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));

                GameObject go = new GameObject("Meteor_SmallFragment");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _spriteSmall;
                sr.sortingOrder = 5;

                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.25f;

                var meteor = go.AddComponent<MeteorObject>();
                float fragSpeed = 2.5f * _currentConfig.speedMultiplier;
                // 下向き成分を確保
                Vector2 fragDir = (dir + Vector2.down * 0.5f).normalized;
                meteor.Initialize(MeteorType.Small, origin, fragDir, fragSpeed, this);
                _activeMeteors.Add(meteor);
            }
        }
    }
}
