using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game032v2_SpinCutter
{
    public class SpinCutterMechanic : MonoBehaviour
    {
        [SerializeField] SpinCutterGameManager _gameManager;
        [SerializeField] SpinCutterUI _ui;

        // Configurable from SceneSetup
        [SerializeField] Sprite _bladeSprite;
        [SerializeField] Sprite _enemySprite;
        [SerializeField] Sprite _movingEnemySprite;
        [SerializeField] Sprite _obstacleSprite;

        // Runtime state
        float _radiusNormalized = 0.5f; // 0..1 -> maps to actual radius
        float _speedNormalized = 0.5f;  // 0..1 -> maps to actual angular speed

        float _minRadius = 0.8f;
        float _maxRadius = 3.5f;
        float _minAngularSpeed = 80f;
        float _maxAngularSpeed = 400f;
        float _bladeDuration = 4f;

        int _remainingLaunches;
        int _totalLaunches;

        List<EnemyController> _enemies = new List<EnemyController>();
        List<GameObject> _obstacles = new List<GameObject>();
        List<BladeController> _activeBlades = new List<BladeController>();

        bool _hasObstacles;
        bool _hasMovingEnemies;
        float _speedMult;
        int _launchesUsed;
        int _totalEnemiesKilledThisStage;

        public int RemainingLaunches => _remainingLaunches;
        public int RemainingEnemies => CountAliveEnemies();

        public void SetRadiusNormalized(float v)
        {
            _radiusNormalized = Mathf.Clamp01(v);
            UpdatePreview();
        }

        public void SetSpeedNormalized(float v)
        {
            _speedNormalized = Mathf.Clamp01(v);
        }

        float GetRadius() => Mathf.Lerp(_minRadius, _maxRadius, _radiusNormalized);
        float GetAngularSpeed() => Mathf.Lerp(_minAngularSpeed, _maxAngularSpeed, _speedNormalized) * _speedMult;

        void UpdatePreview()
        {
            // Notify UI to update line renderer
            if (_ui != null)
                _ui.UpdateOrbitPreview(Vector3.zero, GetRadius());
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            ClearStage();

            _speedMult = config.speedMultiplier;
            int enemyCount = config.countMultiplier; // directly as int
            _hasObstacles = config.complexityFactor >= 0.4f;
            _hasMovingEnemies = config.complexityFactor >= 0.6f;

            // Blade duration shortens from stage 4
            _bladeDuration = config.complexityFactor >= 0.5f ? 3.0f : 4.5f;

            // Launch limit
            _totalLaunches = Mathf.RoundToInt(2 + config.complexityFactor * 5);
            _remainingLaunches = _totalLaunches;
            _launchesUsed = 0;
            _totalEnemiesKilledThisStage = 0;

            // Build game area bounds
            float camSize = Camera.main != null ? Camera.main.orthographicSize : 5f;
            float camW = camSize * (Camera.main != null ? Camera.main.aspect : 0.5625f);
            float bottomMargin = 3.5f;
            float topMargin = 1.2f;
            float gameBottom = -camSize + bottomMargin;
            float gameTop = camSize - topMargin;
            float gameLeft = -camW + 0.8f;
            float gameRight = camW - 0.8f;

            // Spawn enemies
            SpawnEnemies(enemyCount, stageIndex, gameLeft, gameRight, gameBottom, gameTop);

            // Spawn obstacles (stage 3+)
            if (_hasObstacles)
                SpawnObstacles(stageIndex, gameLeft, gameRight, gameBottom, gameTop);

            if (_ui != null)
            {
                _ui.UpdateLaunches(_remainingLaunches);
                _ui.UpdateEnemies(RemainingEnemies);
                _ui.UpdateOrbitPreview(Vector3.zero, GetRadius());
            }
        }

        void SpawnEnemies(int count, int stageIndex, float left, float right, float bottom, float top)
        {
            if (count <= 0) return;
            // Stage 1: circular arrangement
            // Stage 2+: scattered
            for (int i = 0; i < count; i++)
            {
                Vector3 pos;
                if (stageIndex == 0)
                {
                    float angle = i * (360f / count) * Mathf.Deg2Rad;
                    float r = 2.0f;
                    pos = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r + (bottom + top) * 0.5f, 0f);
                }
                else
                {
                    pos = RandomPositionInArea(left, right, bottom, top);
                }

                var go = new GameObject("Enemy_" + i);
                go.tag = "Enemy";
                go.transform.position = pos;

                var sr = go.AddComponent<SpriteRenderer>();
                bool isMoving = _hasMovingEnemies && i % 3 == 0;
                sr.sprite = (isMoving && _movingEnemySprite != null) ? _movingEnemySprite : _enemySprite;
                sr.sortingOrder = 5;
                if (sr.sprite != null)
                {
                    float targetSize = 0.55f;
                    float s = targetSize / (sr.sprite.rect.width / sr.sprite.pixelsPerUnit);
                    go.transform.localScale = Vector3.one * s;
                }

                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;

                var ec = go.AddComponent<EnemyController>();
                ec.Initialize(isMoving, 1.2f * _speedMult, 0.8f);

                _enemies.Add(ec);
            }
        }

        void SpawnObstacles(int stageIndex, float left, float right, float bottom, float top)
        {
            int count = stageIndex == 2 ? 2 : (stageIndex == 3 ? 3 : 4);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Obstacle_" + i);
                go.tag = "Obstacle";
                go.transform.position = RandomPositionInArea(left, right, bottom, top);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _obstacleSprite;
                sr.sortingOrder = 4;
                if (_obstacleSprite != null)
                {
                    float targetSize = 0.65f;
                    float s = targetSize / (_obstacleSprite.rect.width / _obstacleSprite.pixelsPerUnit);
                    go.transform.localScale = Vector3.one * s;
                }

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = Vector2.one;

                _obstacles.Add(go);
            }
        }

        Vector3 RandomPositionInArea(float left, float right, float bottom, float top)
        {
            float x = Random.Range(left, right);
            float y = Random.Range(bottom + 0.3f, top - 0.3f);
            return new Vector3(x, y, 0f);
        }

        public bool HasRemainingLaunches() => _remainingLaunches > 0;

        public void LaunchBlade()
        {
            if (!HasRemainingLaunches()) return;
            _remainingLaunches--;
            _launchesUsed++;

            float radius = GetRadius();
            float angularSpeed = GetAngularSpeed();

            var go = new GameObject("Blade");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _bladeSprite;
            sr.sortingOrder = 10;
            if (_bladeSprite != null)
            {
                float targetSize = 0.5f;
                float s = targetSize / (_bladeSprite.rect.width / _bladeSprite.pixelsPerUnit);
                go.transform.localScale = Vector3.one * s;
            }

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            var blade = go.AddComponent<BladeController>();
            blade.Initialize(Vector3.zero, radius, angularSpeed, _bladeDuration, this);
            _activeBlades.Add(blade);

            if (_ui != null)
                _ui.UpdateLaunches(_remainingLaunches);
        }

        public void OnBladeHitObstacle()
        {
            StartCoroutine(ObstacleShake());
        }

        IEnumerator ObstacleShake()
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            Vector3 orig = cam.transform.position;
            float elapsed = 0f;
            float duration = 0.3f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float s = 0.2f * (1f - elapsed / duration);
                cam.transform.position = orig + (Vector3)Random.insideUnitCircle * s;
                yield return null;
            }
            cam.transform.position = orig;
        }

        public void OnBladeDied(BladeController blade)
        {
            _activeBlades.Remove(blade);
            int killed = blade.EnemiesKilled;
            _totalEnemiesKilledThisStage += killed;

            int remaining = CountAliveEnemies();
            if (_ui != null)
                _ui.UpdateEnemies(remaining);

            if (remaining <= 0 && _gameManager != null)
            {
                // Determine combo multiplier
                float comboMult = _totalEnemiesKilledThisStage >= 10 ? 3.0f :
                                  (_totalEnemiesKilledThisStage >= 5 ? 2.0f : 1.5f);
                _gameManager.OnAllEnemiesDefeated(_totalEnemiesKilledThisStage, _remainingLaunches, comboMult);
            }
            else if (_activeBlades.Count == 0 && _gameManager != null)
            {
                _gameManager.OnBladeLanded();
            }
        }

        int CountAliveEnemies()
        {
            int count = 0;
            foreach (var e in _enemies)
            {
                if (e != null && !e.IsDead) count++;
            }
            return count;
        }

        void ClearStage()
        {
            foreach (var b in _activeBlades)
                if (b != null) Destroy(b.gameObject);
            _activeBlades.Clear();

            foreach (var e in _enemies)
                if (e != null) Destroy(e.gameObject);
            _enemies.Clear();

            foreach (var o in _obstacles)
                if (o != null) Destroy(o);
            _obstacles.Clear();
        }

        void OnDestroy()
        {
            ClearStage();
        }
    }
}
