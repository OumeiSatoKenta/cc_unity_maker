using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game023v2_ChainSlash
{
    public enum EnemyType { Normal, Shield, Bomb }

    public class EnemyData
    {
        public EnemyType type;
        public int colorIndex; // 0=Red, 1=Blue
        public bool isMoving;
        public bool shieldActive;
        public float moveAngle;
        public float moveSpeed;
        public bool isChained;
        public bool isDestroyed; // set true when SlashAnimation begins
        public int shieldHits; // for shield enemy: hits needed (start at 2)
        public GameObject go;
        public SpriteRenderer sr;
    }

    public class ChainSlashController : MonoBehaviour
    {
        [SerializeField] ChainSlashGameManager _gameManager;
        [SerializeField] ChainSlashUI _ui;
        [SerializeField] Sprite _spriteEnemyRed;
        [SerializeField] Sprite _spriteEnemyBlue;
        [SerializeField] Sprite _spriteEnemyShield;
        [SerializeField] Sprite _spriteEnemyBomb;
        [SerializeField] Sprite _spriteChainLink;

        bool _isActive;
        List<EnemyData> _enemies = new List<EnemyData>();
        List<EnemyData> _chain = new List<EnemyData>();
        List<LineRenderer> _chainLines = new List<LineRenderer>();

        bool _isDragging;
        float _stageTime;
        float _timeRemaining;

        // Stage parameters
        int _baseEnemyCount = 8;
        float _moveSpeedBase = 0f;
        float _bombRatio = 0f;
        float _shieldRatio = 0f;
        float _movingRatio = 0f;
        bool _twoColors = false;

        // Camera bounds (computed per stage)
        float _minX, _maxX, _minY, _maxY;
        const float EnemyRadius = 0.45f;
        const float EnemySize = 0.9f;

        // Combo flash
        GameObject _comboFlashObj;
        float _comboFlashTimer;

        // Camera shake
        Vector3 _camOrigPos;
        float _shakeTimer;
        float _shakeAmplitude;

        void Awake()
        {
            ComputeBounds();
        }

        void ComputeBounds()
        {
            float camSize = Camera.main != null ? Camera.main.orthographicSize : 5f;
            float camWidth = camSize * (Camera.main != null ? Camera.main.aspect : 9f / 16f);
            float topMargin = 1.4f;
            float bottomMargin = 3.0f;
            _minY = -camSize + bottomMargin;
            _maxY = camSize - topMargin;
            _minX = -camWidth + 0.8f;
            _maxX = camWidth - 0.8f;
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            StopGame();
            _baseEnemyCount = Mathf.RoundToInt(8 * config.countMultiplier);
            _moveSpeedBase = config.speedMultiplier * 0.8f;
            float complexity = config.complexityFactor;

            // Stage-specific new rules (stageNumber is 1-based)
            _twoColors = stageNumber >= 2;
            _movingRatio = stageNumber >= 3 ? 0.3f : 0f;
            _shieldRatio = stageNumber >= 4 ? 0.2f * complexity : 0f;
            _bombRatio = stageNumber >= 5 ? 0.15f * complexity : 0f;
            _stageTime = stageNumber switch { 1 => 60f, 2 => 55f, 3 => 50f, 4 => 45f, _ => 40f };
            _timeRemaining = _stageTime;

            _isActive = true;
            SpawnEnemies();
        }

        public void StopGame()
        {
            _isActive = false;
            CancelChain();
            ClearAllEnemies();
        }

        void SpawnEnemies()
        {
            ClearAllEnemies();
            int count = _baseEnemyCount;
            var usedPositions = new List<Vector2>();

            for (int i = 0; i < count; i++)
            {
                EnemyType type = DetermineType();
                int colorIdx = _twoColors ? Random.Range(0, 2) : 0;
                bool isMoving = _moveSpeedBase > 0f && Random.value < _movingRatio;

                Vector2 pos = FindOpenPosition(usedPositions);
                usedPositions.Add(pos);

                var data = new EnemyData
                {
                    type = type,
                    colorIndex = colorIdx,
                    isMoving = isMoving,
                    shieldActive = type == EnemyType.Shield,
                    shieldHits = type == EnemyType.Shield ? 2 : 0,
                    moveAngle = Random.Range(0f, 360f),
                    moveSpeed = _moveSpeedBase * Random.Range(0.8f, 1.2f)
                };

                var go = new GameObject($"Enemy_{i}");
                go.transform.position = new Vector3(pos.x, pos.y, 0f);
                go.transform.localScale = Vector3.one * EnemySize;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = GetSprite(data);
                sr.sortingOrder = 5;
                data.go = go;
                data.sr = sr;

                // Collider for raycast
                var col = go.AddComponent<CircleCollider2D>();
                col.radius = EnemyRadius;

                _enemies.Add(data);
            }
        }

        EnemyType DetermineType()
        {
            float r = Random.value;
            if (r < _bombRatio) return EnemyType.Bomb;
            if (r < _bombRatio + _shieldRatio) return EnemyType.Shield;
            return EnemyType.Normal;
        }

        Sprite GetSprite(EnemyData d)
        {
            return d.type switch
            {
                EnemyType.Shield => _spriteEnemyShield,
                EnemyType.Bomb => _spriteEnemyBomb,
                _ => d.colorIndex == 0 ? _spriteEnemyRed : _spriteEnemyBlue
            };
        }

        Vector2 FindOpenPosition(List<Vector2> used)
        {
            for (int attempts = 0; attempts < 30; attempts++)
            {
                float x = Random.Range(_minX, _maxX);
                float y = Random.Range(_minY, _maxY);
                var candidate = new Vector2(x, y);
                bool ok = true;
                foreach (var u in used)
                {
                    if (Vector2.Distance(candidate, u) < EnemySize * 1.2f)
                    { ok = false; break; }
                }
                if (ok) return candidate;
            }
            return new Vector2(Random.Range(_minX, _maxX), Random.Range(_minY, _maxY));
        }

        void Update()
        {
            if (!_isActive) return;

            // Timer
            _timeRemaining -= Time.deltaTime;
            _ui?.UpdateTimer(_timeRemaining, _stageTime);
            if (_timeRemaining <= 0f)
            {
                _isActive = false;
                CancelChain();
                _gameManager.OnTimeUp();
                return;
            }

            // Move enemies
            MoveEnemies();

            // Input
            HandleInput();

            // Camera shake
            UpdateCameraShake();
        }

        void MoveEnemies()
        {
            float camSize = Camera.main != null ? Camera.main.orthographicSize : 5f;
            float camW = camSize * (Camera.main != null ? Camera.main.aspect : 0.5625f);
            foreach (var e in _enemies)
            {
                if (!e.isMoving || e.isChained || e.isDestroyed || e.go == null) continue;
                Vector3 pos = e.go.transform.position;
                float rad = e.moveAngle * Mathf.Deg2Rad;
                pos.x += Mathf.Cos(rad) * e.moveSpeed * Time.deltaTime;
                pos.y += Mathf.Sin(rad) * e.moveSpeed * Time.deltaTime;
                // Bounce
                if (pos.x < _minX || pos.x > _maxX) { e.moveAngle = 180f - e.moveAngle; pos.x = Mathf.Clamp(pos.x, _minX, _maxX); }
                if (pos.y < _minY || pos.y > _maxY) { e.moveAngle = -e.moveAngle; pos.y = Mathf.Clamp(pos.y, _minY, _maxY); }
                e.go.transform.position = pos;
            }
        }

        void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _chain.Clear();
            }

            if (_isDragging)
            {
                Vector2 screenPos = mouse.position.ReadValue();
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var data = FindEnemyByGO(hit.gameObject);
                    if (data != null && !data.isChained)
                        TryAddToChain(data);
                }
                UpdateChainLines();
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
                if (_chain.Count >= 1)
                    SlashChain();
                else
                    CancelChain();
            }
        }

        void TryAddToChain(EnemyData data)
        {
            if (data.type == EnemyType.Bomb)
            {
                // Bomb hit → cancel with shake
                CancelChainWithPenalty();
                return;
            }
            if (data.type == EnemyType.Shield && data.shieldHits > 1)
            {
                // First touch: reduce shield
                data.shieldHits--;
                StartCoroutine(ShieldHitEffect(data));
                return;
            }
            data.isChained = true;
            _chain.Add(data);
            data.sr.color = new Color(1f, 1f, 0.3f);
            StartCoroutine(ChainPulse(data));
        }

        EnemyData FindEnemyByGO(GameObject go)
        {
            foreach (var e in _enemies)
                if (e.go == go) return e;
            return null;
        }

        void SlashChain()
        {
            if (_chain.Count == 0) return;
            bool allSameColor = true;
            int firstColor = _chain[0].colorIndex;
            foreach (var e in _chain)
                if (e.colorIndex != firstColor || e.type != EnemyType.Normal)
                    allSameColor = false;

            int count = _chain.Count;
            _gameManager.OnSlashExecuted(count, allSameColor);

            // Destroy chained enemies with animation
            StartCoroutine(SlashAnimation(_chain.ToArray()));
            foreach (var e in _chain)
                _enemies.Remove(e);
            _chain.Clear();
            ClearChainLines();

            // Respawn if too few
            if (_enemies.Count < 3)
                SpawnEnemies();
        }

        IEnumerator SlashAnimation(EnemyData[] sliced)
        {
            // Mark as destroyed immediately to prevent double-Destroy from ClearAllEnemies
            foreach (var e in sliced)
                e.isDestroyed = true;

            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.25f;
                foreach (var e in sliced)
                    if (e.go != null)
                        e.go.transform.localScale = Vector3.one * EnemySize * (1f - ratio);
                yield return null;
            }
            foreach (var e in sliced)
                if (e.go != null)
                {
                    Destroy(e.go);
                    e.go = null;
                }
        }

        void CancelChain()
        {
            foreach (var e in _chain)
            {
                e.isChained = false;
                if (e.go != null) e.sr.color = Color.white;
            }
            _chain.Clear();
            ClearChainLines();
        }

        void CancelChainWithPenalty()
        {
            CancelChain();
            StartCoroutine(BombEffect());
        }

        IEnumerator BombEffect()
        {
            // Red flash overlay (camera shake)
            TriggerCameraShake(0.3f, 0.18f);
            // Flash all enemies red briefly
            foreach (var e in _enemies)
                if (e.go != null) e.sr.color = new Color(1f, 0.3f, 0.3f);
            yield return new WaitForSeconds(0.2f);
            foreach (var e in _enemies)
                if (e.go != null && !e.isChained) e.sr.color = Color.white;
        }

        IEnumerator ChainPulse(EnemyData data)
        {
            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.15f;
                float scale = EnemySize * (1f + 0.2f * Mathf.Sin(ratio * Mathf.PI));
                if (data.go != null)
                    data.go.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            if (data.go != null)
                data.go.transform.localScale = Vector3.one * EnemySize;
        }

        IEnumerator ShieldHitEffect(EnemyData data)
        {
            if (data.go == null) yield break;
            data.sr.color = new Color(0.8f, 0.8f, 1f);
            yield return new WaitForSeconds(0.15f);
            if (data.go != null) data.sr.color = Color.white;
        }

        void UpdateChainLines()
        {
            ClearChainLines();
            for (int i = 0; i < _chain.Count - 1; i++)
            {
                var lr = new GameObject($"ChainLine_{i}").AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPosition(0, _chain[i].go.transform.position);
                lr.SetPosition(1, _chain[i + 1].go.transform.position);
                lr.startWidth = 0.08f;
                lr.endWidth = 0.08f;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = new Color(1f, 0.9f, 0.2f, 0.9f);
                lr.endColor = new Color(1f, 0.7f, 0.1f, 0.9f);
                lr.sortingOrder = 8;
                _chainLines.Add(lr);
            }
        }

        void ClearChainLines()
        {
            foreach (var lr in _chainLines)
                if (lr != null) Destroy(lr.gameObject);
            _chainLines.Clear();
        }

        void ClearAllEnemies()
        {
            foreach (var e in _enemies)
                if (e.go != null && !e.isDestroyed) Destroy(e.go);
            _enemies.Clear();
            _chain.Clear();
            ClearChainLines();
        }

        void TriggerCameraShake(float duration, float amplitude)
        {
            _camOrigPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            _shakeTimer = duration;
            _shakeAmplitude = amplitude;
        }

        void UpdateCameraShake()
        {
            if (_shakeTimer <= 0f) return;
            _shakeTimer -= Time.deltaTime;
            if (Camera.main == null) return;
            if (_shakeTimer > 0f)
            {
                float x = Random.Range(-_shakeAmplitude, _shakeAmplitude);
                float y = Random.Range(-_shakeAmplitude, _shakeAmplitude);
                Camera.main.transform.position = _camOrigPos + new Vector3(x, y, 0f);
            }
            else
            {
                Camera.main.transform.position = _camOrigPos;
            }
        }

        public int GetCurrentChainCount() => _chain.Count;
        public float GetTimeRemaining() => _timeRemaining;

        void OnDestroy()
        {
            ClearAllEnemies();
        }
    }

}
