using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game039v2_BoomerangHero
{
    public class BoomerangMechanic : MonoBehaviour
    {
        [SerializeField] BoomerangHeroGameManager _gameManager;
        [SerializeField] LineRenderer _trajectoryLine;

        // Sprites
        [SerializeField] Sprite _spriteBoomerang;
        [SerializeField] Sprite _spriteEnemyNormal;
        [SerializeField] Sprite _spriteEnemyShielded;
        [SerializeField] Sprite _spriteEnemyMoving;
        [SerializeField] Sprite _spriteWall;
        [SerializeField] Sprite _spriteHitEffect;
        [SerializeField] Sprite _spriteShield;

        bool _isActive;
        bool _isAiming;
        bool _isBoomerangFlying;
        Vector2 _aimStartPos;
        Vector2 _playerWorldPos;
        Coroutine _boomerangCoroutine;

        int _ammoLeft;
        int _ammoMax;
        int _enemiesLeft;
        int _currentStage;
        int _bounceCount;  // wall bounce count for this throw

        List<GameObject> _enemies = new List<GameObject>();
        List<GameObject> _walls = new List<GameObject>();
        GameObject _boomerangObj;
        Rigidbody2D _boomerangRb;

        // Stage data: (enemyPositions relative, wallData, ammoCount, hasShield, hasMoving)
        static readonly StageData[] Stages = new StageData[]
        {
            // Stage 1: 2 enemies, no walls, 3 ammo
            new StageData(
                new Vector2[] { new Vector2(0.3f, 0.1f), new Vector2(0.3f, -0.2f) },
                new WallData[] { },
                3, false, false
            ),
            // Stage 2: 3 enemies, 1 L-wall, 3 ammo
            new StageData(
                new Vector2[] { new Vector2(0.4f, 0.3f), new Vector2(0.4f, -0.1f), new Vector2(0.2f, -0.4f) },
                new WallData[] { new WallData(new Vector2(0.1f, 0f), new Vector2(0.8f, 0.08f), 0f) },
                3, false, false
            ),
            // Stage 3: 4 enemies, 2 walls, 3 ammo
            new StageData(
                new Vector2[] { new Vector2(0.35f, 0.4f), new Vector2(0.45f, 0.1f), new Vector2(0.35f, -0.2f), new Vector2(0.2f, -0.4f) },
                new WallData[] {
                    new WallData(new Vector2(0.0f, 0.15f), new Vector2(0.7f, 0.08f), 0f),
                    new WallData(new Vector2(0.25f, 0.0f), new Vector2(0.08f, 0.6f), 0f)
                },
                3, false, false
            ),
            // Stage 4: 5 enemies (1 shielded), complex walls, 4 ammo
            new StageData(
                new Vector2[] { new Vector2(0.4f, 0.4f), new Vector2(0.4f, 0.15f), new Vector2(0.4f, -0.1f), new Vector2(0.3f, -0.35f), new Vector2(0.15f, -0.45f) },
                new WallData[] {
                    new WallData(new Vector2(0.05f, 0.1f), new Vector2(0.6f, 0.08f), 0f),
                    new WallData(new Vector2(0.2f, -0.15f), new Vector2(0.08f, 0.5f), 0f)
                },
                4, true, false
            ),
            // Stage 5: 6 enemies (1 shielded + 1 moving), complex walls, 4 ammo
            new StageData(
                new Vector2[] { new Vector2(0.42f, 0.42f), new Vector2(0.42f, 0.18f), new Vector2(0.42f, -0.06f), new Vector2(0.42f, -0.3f), new Vector2(0.25f, -0.42f), new Vector2(0.15f, 0.35f) },
                new WallData[] {
                    new WallData(new Vector2(0.05f, 0.15f), new Vector2(0.65f, 0.08f), 0f),
                    new WallData(new Vector2(0.22f, -0.1f), new Vector2(0.08f, 0.5f), 0f),
                    new WallData(new Vector2(-0.1f, -0.2f), new Vector2(0.4f, 0.08f), 20f)
                },
                4, true, true
            )
        };

        void Start()
        {
            _isActive = false;
        }

        void Update()
        {
            if (!_isActive || _isBoomerangFlying) return;
            if (_ammoLeft <= 0) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isAiming = true;
                _aimStartPos = mouse.position.ReadValue();
                ShowTrajectory(true);
            }
            else if (_isAiming && mouse.leftButton.isPressed)
            {
                UpdateAimPreview(mouse.position.ReadValue());
            }
            else if (_isAiming && mouse.leftButton.wasReleasedThisFrame)
            {
                _isAiming = false;
                ShowTrajectory(false);
                LaunchBoomerang(mouse.position.ReadValue());
            }
        }

        void UpdateAimPreview(Vector2 screenPos)
        {
            Vector2 dir = GetThrowDirection(screenPos);
            float force = GetThrowForce(screenPos);
            DrawTrajectoryPreview(dir, force);
        }

        Vector2 GetThrowDirection(Vector2 currentScreenPos)
        {
            Vector2 delta = currentScreenPos - _aimStartPos;
            if (delta.sqrMagnitude < 0.01f) return Vector2.right;
            return delta.normalized;
        }

        float GetThrowForce(Vector2 currentScreenPos)
        {
            float dist = Vector2.Distance(currentScreenPos, _aimStartPos);
            return Mathf.Clamp(dist / 100f, 0.5f, 3f) * 8f;
        }

        void DrawTrajectoryPreview(Vector2 dir, float force)
        {
            if (_trajectoryLine == null) return;
            List<Vector3> points = new List<Vector3>();
            Vector2 pos = _playerWorldPos;
            Vector2 vel = dir * force;
            int maxSteps = 30;
            int maxBounces = 3;
            int bounces = 0;

            points.Add(pos);
            for (int i = 0; i < maxSteps; i++)
            {
                Vector2 nextPos = pos + vel * 0.15f;
                // Check wall collision for preview
                RaycastHit2D hit = Physics2D.Linecast(pos, nextPos, LayerMask.GetMask("Wall"));
                if (hit.collider != null && bounces < maxBounces)
                {
                    vel = Vector2.Reflect(vel, hit.normal);
                    nextPos = hit.point + hit.normal * 0.05f;
                    bounces++;
                }
                pos = nextPos;
                points.Add(pos);
            }
            _trajectoryLine.positionCount = points.Count;
            _trajectoryLine.SetPositions(points.ToArray());
        }

        void ShowTrajectory(bool show)
        {
            if (_trajectoryLine != null)
                _trajectoryLine.enabled = show;
        }

        void LaunchBoomerang(Vector2 releaseScreenPos)
        {
            if (_ammoLeft <= 0) return;
            Vector2 dir = GetThrowDirection(releaseScreenPos);
            float force = GetThrowForce(releaseScreenPos);

            _ammoLeft--;
            _bounceCount = 0;
            _gameManager.OnAmmoUpdated(_ammoLeft, _ammoMax);

            if (_boomerangObj == null) CreateBoomerangObject();

            _boomerangObj.SetActive(true);
            _boomerangObj.transform.position = _playerWorldPos;
            _boomerangRb.linearVelocity = dir * force;
            _isBoomerangFlying = true;
            _boomerangCoroutine = StartCoroutine(BoomerangFlight());
        }

        IEnumerator BoomerangFlight()
        {
            float maxTime = 5f;
            float elapsed = 0f;
            while (elapsed < maxTime && _boomerangObj != null && _boomerangObj.activeSelf)
            {
                elapsed += Time.deltaTime;
                // Rotate boomerang sprite
                if (_boomerangObj != null)
                    _boomerangObj.transform.Rotate(0, 0, 720f * Time.deltaTime);

                yield return null;
            }

            // Boomerang expired
            if (_boomerangObj != null) _boomerangObj.SetActive(false);
            _isBoomerangFlying = false;

            CheckGameOver();
        }

        void CheckGameOver()
        {
            if (_gameManager.State != BoomerangHeroState.Playing) return;
            if (_enemiesLeft > 0 && _ammoLeft <= 0)
            {
                _gameManager.OnGameOver();
            }
        }

        void OnBoomerangCollision(GameObject other, Vector2 normal)
        {
            if (other.CompareTag("Wall"))
            {
                _bounceCount++;
                _boomerangRb.linearVelocity = Vector2.Reflect(_boomerangRb.linearVelocity, normal);
                // Flash effect on wall
                StartCoroutine(FlashObject(other, Color.white, 0.1f));
            }
            else if (other.CompareTag("Enemy"))
            {
                var enemy = other.GetComponent<EnemyController>();
                if (enemy != null && enemy.CanBeHit(_boomerangObj.transform.position))
                {
                    int basePoints = _bounceCount == 0 ? 10 : (_bounceCount == 1 ? 30 : 60);
                    enemy.Defeat();
                    _enemies.Remove(other);
                    _enemiesLeft--;
                    _gameManager.OnEnemyDefeated(basePoints);
                    _gameManager.OnEnemyCountUpdated(_enemiesLeft);

                    if (_enemiesLeft <= 0)
                    {
                        if (_boomerangCoroutine != null) { StopCoroutine(_boomerangCoroutine); _boomerangCoroutine = null; }
                        if (_boomerangObj != null) _boomerangObj.SetActive(false);
                        _isBoomerangFlying = false;
                        int bonus = _ammoLeft * 50;
                        _gameManager.OnStageClear(bonus);
                    }
                }
            }
        }

        IEnumerator FlashObject(GameObject obj, Color flashColor, float duration)
        {
            if (obj == null) yield break;
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            Color original = sr.color;
            sr.color = flashColor;
            yield return new WaitForSeconds(duration);
            if (sr != null) sr.color = original;
        }

        void CreateBoomerangObject()
        {
            _boomerangObj = new GameObject("Boomerang");
            var sr = _boomerangObj.AddComponent<SpriteRenderer>();
            sr.sprite = _spriteBoomerang;
            sr.sortingOrder = 10;
            _boomerangObj.transform.localScale = Vector3.one * 0.6f;

            _boomerangRb = _boomerangObj.AddComponent<Rigidbody2D>();
            _boomerangRb.gravityScale = 0f;
            _boomerangRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _boomerangRb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var col = _boomerangObj.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.isTrigger = true;

            var bc = _boomerangObj.AddComponent<BoomerangCollisionHandler>();
            bc.Init(this);
        }

        public void HandleBoomerangTrigger(Collider2D other, Vector2 contactNormal)
        {
            if (!_isBoomerangFlying) return;
            OnBoomerangCollision(other.gameObject, contactNormal);
        }

        public void SetupStage(int stageIndex)
        {
            if (_boomerangCoroutine != null) { StopCoroutine(_boomerangCoroutine); _boomerangCoroutine = null; }
            _isActive = true;
            _isBoomerangFlying = false;
            _isAiming = false;
            if (_boomerangObj != null) _boomerangObj.SetActive(false);

            // Clear old enemies and walls
            foreach (var e in _enemies) if (e != null) Destroy(e);
            foreach (var w in _walls) if (w != null) Destroy(w);
            _enemies.Clear();
            _walls.Clear();

            int idx = Mathf.Clamp(stageIndex, 0, Stages.Length - 1);
            var data = Stages[idx];
            _currentStage = idx;
            _ammoLeft = data.ammoCount;
            _ammoMax = data.ammoCount;
            _enemiesLeft = data.enemyPositions.Length;

            if (Camera.main == null) return;
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;

            _playerWorldPos = new Vector2(-camWidth * 0.55f, 0f);

            // Create walls
            foreach (var wd in data.wallDatas)
            {
                float wx = wd.relativePos.x * camWidth * 2f - camWidth + _playerWorldPos.x * 0.3f;
                float wy = wd.relativePos.y * camSize * 2f;
                CreateWall(new Vector2(wx, wy), new Vector2(wd.relativeScale.x * camWidth * 1.5f, wd.relativeScale.y * camSize * 2f), wd.rotation);
            }

            // Create enemies
            for (int i = 0; i < data.enemyPositions.Length; i++)
            {
                float ex = data.enemyPositions[i].x * camWidth * 2f - camWidth * 0.8f;
                float ey = data.enemyPositions[i].y * camSize * 2f;
                bool isShielded = data.hasShielded && i == 0;
                bool isMoving = data.hasMoving && i == data.enemyPositions.Length - 1;
                CreateEnemy(new Vector2(ex, ey), isShielded, isMoving);
            }

            _gameManager.OnAmmoUpdated(_ammoLeft, _ammoMax);
            _gameManager.OnEnemyCountUpdated(_enemiesLeft);
        }

        void CreateWall(Vector2 pos, Vector2 scale, float rotation)
        {
            var wall = new GameObject("Wall");
            wall.tag = "Wall";
            wall.layer = LayerMask.NameToLayer("Wall");
            wall.transform.position = pos;
            wall.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            wall.transform.rotation = Quaternion.Euler(0, 0, rotation);

            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = _spriteWall;
            sr.sortingOrder = 2;

            var col = wall.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            _walls.Add(wall);
        }

        void CreateEnemy(Vector2 pos, bool shielded, bool moving)
        {
            var enemy = new GameObject(shielded ? "EnemyShielded" : moving ? "EnemyMoving" : "Enemy");
            enemy.tag = "Enemy";
            enemy.transform.position = pos;
            enemy.transform.localScale = Vector3.one * 0.6f;

            var sr = enemy.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            if (shielded) sr.sprite = _spriteEnemyShielded;
            else if (moving) sr.sprite = _spriteEnemyMoving;
            else sr.sprite = _spriteEnemyNormal;

            var col = enemy.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;

            var ec = enemy.AddComponent<EnemyController>();
            ec.Init(shielded, moving, pos, _spriteHitEffect, _spriteShield);

            _enemies.Add(enemy);
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active)
            {
                _isAiming = false;
                ShowTrajectory(false);
                if (_boomerangObj != null) _boomerangObj.SetActive(false);
                _isBoomerangFlying = false;
            }
        }

        // Stage data structures
        struct StageData
        {
            public Vector2[] enemyPositions;
            public WallData[] wallDatas;
            public int ammoCount;
            public bool hasShielded;
            public bool hasMoving;

            public StageData(Vector2[] ep, WallData[] wd, int ammo, bool shielded, bool moving)
            {
                enemyPositions = ep; wallDatas = wd; ammoCount = ammo;
                hasShielded = shielded; hasMoving = moving;
            }
        }

        struct WallData
        {
            public Vector2 relativePos;
            public Vector2 relativeScale;
            public float rotation;
            public WallData(Vector2 p, Vector2 s, float r) { relativePos = p; relativeScale = s; rotation = r; }
        }
    }

    // Helper: boomerang collision handler
    public class BoomerangCollisionHandler : MonoBehaviour
    {
        BoomerangMechanic _mechanic;
        public void Init(BoomerangMechanic m) { _mechanic = m; }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_mechanic == null) return;
            Vector2 normal = ((Vector2)transform.position - (Vector2)other.bounds.center).normalized;
            _mechanic.HandleBoomerangTrigger(other, normal);
        }
    }

    // Helper: enemy controller
    public class EnemyController : MonoBehaviour
    {
        bool _isShielded;
        bool _isMoving;
        Vector2 _startPos;
        float _moveTimer;
        float _moveRange = 1.2f;
        float _moveSpeed = 1.5f;
        GameObject _shieldVisual;
        Sprite _hitEffectSprite;

        public void Init(bool shielded, bool moving, Vector2 startPos, Sprite hitSprite, Sprite shieldSprite)
        {
            _isShielded = shielded;
            _isMoving = moving;
            _startPos = startPos;
            _hitEffectSprite = hitSprite;

            if (shielded && shieldSprite != null)
            {
                _shieldVisual = new GameObject("Shield");
                _shieldVisual.transform.SetParent(transform);
                _shieldVisual.transform.localPosition = new Vector3(-0.5f, 0f, 0f);
                _shieldVisual.transform.localScale = Vector3.one * 1.2f;
                var sr = _shieldVisual.AddComponent<SpriteRenderer>();
                sr.sprite = shieldSprite;
                sr.sortingOrder = 4;
            }
        }

        void Update()
        {
            if (!_isMoving) return;
            _moveTimer += Time.deltaTime * _moveSpeed;
            float offset = Mathf.Sin(_moveTimer) * _moveRange;
            transform.position = new Vector2(_startPos.x, _startPos.y + offset);
        }

        public bool CanBeHit(Vector3 boomerangPos)
        {
            if (!_isShielded) return true;
            // Shielded enemy: can only be hit from behind (right side in this case)
            return boomerangPos.x > transform.position.x;
        }

        public void Defeat()
        {
            StartCoroutine(DefeatAnimation());
        }

        System.Collections.IEnumerator DefeatAnimation()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Pop scale up then down
                float t = 0f;
                while (t < 0.3f)
                {
                    if (sr == null || this == null) yield break;
                    t += Time.deltaTime;
                    float ratio = t / 0.3f;
                    float scale = ratio < 0.5f ? 1f + ratio * 1.0f : 1.5f - (ratio - 0.5f) * 3f;
                    transform.localScale = Vector3.one * 0.6f * Mathf.Max(scale, 0f);
                    sr.color = new Color(1f, 1f, 0.5f, 1f - ratio);
                    yield return null;
                }
            }

            // Hit effect
            if (_hitEffectSprite != null)
            {
                var fx = new GameObject("HitFX");
                fx.transform.position = transform.position;
                var fxSr = fx.AddComponent<SpriteRenderer>();
                fxSr.sprite = _hitEffectSprite;
                fxSr.sortingOrder = 15;
                fxSr.transform.localScale = Vector3.one * 0.8f;
                Destroy(fx, 0.3f);
            }

            Destroy(gameObject);
        }
    }
}
