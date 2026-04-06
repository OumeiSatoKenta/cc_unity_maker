using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game047v2_SpinBalance
{
    public enum CoinType { Normal, Heavy, Light, Bounce, Magnet }

    public class CoinData : MonoBehaviour
    {
        public CoinType CoinType;
        public Rigidbody2D Rb;
        public SpriteRenderer Sr;
        private bool _isDanger;
        private Coroutine _dangerCoroutine;

        public void SetDanger(bool danger)
        {
            if (_isDanger == danger) return;
            _isDanger = danger;
            if (_dangerCoroutine != null) StopCoroutine(_dangerCoroutine);
            if (danger)
                _dangerCoroutine = StartCoroutine(DangerFlash());
            else
            {
                Sr.color = Color.white;
                _dangerCoroutine = null;
            }
        }

        IEnumerator DangerFlash()
        {
            while (true)
            {
                Sr.color = new Color(1f, 0.3f, 0.3f, 1f);
                yield return new WaitForSeconds(0.2f);
                Sr.color = Color.white;
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    public class BalanceManager : MonoBehaviour
    {
        [SerializeField] SpinBalanceGameManager _gameManager;
        [SerializeField] Transform _platform;
        [SerializeField] Sprite _coinSprite;
        [SerializeField] Sprite _heavyCoinSprite;
        [SerializeField] Sprite _lightCoinSprite;
        [SerializeField] Sprite _bounceCoinSprite;
        [SerializeField] Sprite _magnetCoinSprite;

        private bool _isActive;
        private StageManager.StageConfig _config;
        private int _stageNumber;
        private List<CoinData> _coins = new List<CoinData>();
        private int _maxCoins;
        private float _spawnTimer;
        private float _spawnInterval;
        private float _stageTimer;
        private float _stageDuration;
        private bool _hasBounce;
        private bool _hasMagnet;
        private bool _hasShrink;
        private float _shrinkTimer;
        private float _platformOriginalScaleX;
        private float _rotSensitivity = 60f;
        private SpinBalanceUI _ui;

        // Brake
        private bool _brakeActive;
        private float _brakeCooldown;
        private const float BrakeDuration = 0.5f;
        private const float BrakeCooldownMax = 5f;
        private float _doubleClickTimer;
        private bool _waitingDoubleClick;

        // Magnet
        private const float MagnetForce = 3f;
        private const float MagnetRadius = 2f;

        public bool BrakeAvailable => _brakeCooldown <= 0f;
        public float BrakeCooldownRatio => Mathf.Clamp01(_brakeCooldown / BrakeCooldownMax);

        void Awake()
        {
            if (_platform != null)
                _platformOriginalScaleX = _platform.localScale.x;
            _ui = FindFirstObjectByType<SpinBalanceUI>();
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            _config = config;
            _stageNumber = stageNumber;
            _isActive = true;
            _brakeActive = false;
            _brakeCooldown = 0f;

            // Clear existing coins
            foreach (var c in _coins)
                if (c != null) Destroy(c.gameObject);
            _coins.Clear();

            // Restore platform scale
            if (_platform != null)
            {
                var s = _platform.localScale;
                _platform.localScale = new Vector3(_platformOriginalScaleX, s.y, s.z);
                _platform.rotation = Quaternion.identity;
            }

            // Stage parameters
            int stage = stageNumber;
            _stageDuration = GetStageDuration(stage);
            _maxCoins = Mathf.Max(1, GetMaxCoins(stage, config.countMultiplier));
            _spawnInterval = GetSpawnInterval(stage);
            _hasBounce = stage >= 3;
            _hasMagnet = stage >= 4;
            _hasShrink = stage >= 5;
            _stageTimer = 0f;
            _spawnTimer = 0f;
            _shrinkTimer = 0f;
            _rotSensitivity = 60f * config.speedMultiplier;

            // Spawn initial coin
            SpawnCoin();
        }

        float GetStageDuration(int s)
        {
            float[] d = { 20f, 30f, 40f, 50f, 60f };
            return s >= 1 && s <= 5 ? d[s - 1] : 20f;
        }

        int GetMaxCoins(int s, float mult)
        {
            int[] m = { 3, 5, 6, 7, 8 };
            int base_ = s >= 1 && s <= 5 ? m[s - 1] : 3;
            return Mathf.RoundToInt(base_ * mult);
        }

        float GetSpawnInterval(int s)
        {
            float[] i = { 5f, 4f, 4f, 3.5f, 3f };
            return s >= 1 && s <= 5 ? i[s - 1] : 5f;
        }

        public void StopGame()
        {
            _isActive = false;
        }

        void Update()
        {
            if (!_isActive) return;

            HandleInput();
            HandleBrake();
            HandleSpawn();
            HandleMagnet();
            HandleShrink();
            HandleFallDetection();
            HandleStageTimer();

            // Update brake UI
            if (_ui != null)
                _ui.UpdateBrake(BrakeAvailable, BrakeCooldownRatio);
        }

        void HandleInput()
        {
            if (Mouse.current == null || _platform == null) return;

            // Rotation
            if (Mouse.current.leftButton.isPressed)
            {
                float dx = Mouse.current.delta.ReadValue().x;
                _platform.Rotate(0f, 0f, -dx * _rotSensitivity * Time.deltaTime);
            }

            // Double click for brake
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (_waitingDoubleClick && _doubleClickTimer < 0.3f)
                {
                    TryActivateBrake();
                    _waitingDoubleClick = false;
                }
                else
                {
                    _waitingDoubleClick = true;
                    _doubleClickTimer = 0f;
                }
            }

            if (_waitingDoubleClick)
            {
                _doubleClickTimer += Time.deltaTime;
                if (_doubleClickTimer >= 0.4f)
                    _waitingDoubleClick = false;
            }
        }

        void TryActivateBrake()
        {
            if (_brakeCooldown > 0f) return;
            _gameManager.NotifyBrakeUsed();
            _brakeActive = true;
            _brakeCooldown = BrakeCooldownMax;
            StartCoroutine(BrakeEffect());
        }

        IEnumerator BrakeEffect()
        {
            // Freeze rigidbodies briefly
            var savedVels = new System.Collections.Generic.Dictionary<Rigidbody2D, Vector2>();
            var savedAngs = new System.Collections.Generic.Dictionary<Rigidbody2D, float>();
            foreach (var c in _coins)
            {
                if (c == null) continue;
                savedVels[c.Rb] = c.Rb.linearVelocity;
                savedAngs[c.Rb] = c.Rb.angularVelocity;
                c.Rb.linearVelocity = Vector2.zero;
                c.Rb.angularVelocity = 0f;
                c.Rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            // Camera shake
            StartCoroutine(CameraShake(0.3f, 0.15f));

            yield return new WaitForSeconds(BrakeDuration);

            foreach (var c in _coins)
            {
                if (c == null || c.Rb == null) continue;
                c.Rb.constraints = RigidbodyConstraints2D.None;
            }
            _brakeActive = false;
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            Vector3 orig = cam.transform.position;
            float t = 0f;
            while (t < duration)
            {
                cam.transform.position = orig + (Vector3)Random.insideUnitCircle * magnitude;
                t += Time.deltaTime;
                yield return null;
            }
            cam.transform.position = orig;
        }

        void HandleBrake()
        {
            if (_brakeCooldown > 0f)
                _brakeCooldown -= Time.deltaTime;
        }

        void HandleSpawn()
        {
            if (_coins.Count >= _maxCoins) return;
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer = 0f;
                SpawnCoin();
            }
        }

        void SpawnCoin()
        {
            if (_platform == null) return;

            CoinType type = PickCoinType();
            float spawnX = Random.Range(-1.5f, 1.5f);
            float spawnY = 2f;
            var pos = new Vector3(spawnX, spawnY, 0f);

            var coinObj = new GameObject($"Coin_{_coins.Count}_{type}");
            coinObj.tag = "Untagged";
            var cd = coinObj.AddComponent<CoinData>();
            cd.CoinType = type;

            var sr = coinObj.AddComponent<SpriteRenderer>();
            sr.sprite = GetCoinSprite(type);
            sr.sortingOrder = 5;
            cd.Sr = sr;

            var rb = coinObj.AddComponent<Rigidbody2D>();
            rb.mass = GetCoinMass(type);
            rb.linearDamping = 0.2f;

            var col = coinObj.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;

            if (type == CoinType.Bounce)
            {
                var mat = new PhysicsMaterial2D { bounciness = 0.9f, friction = 0.1f };
                col.sharedMaterial = mat;
            }

            cd.Rb = rb;
            coinObj.transform.position = pos;

            _coins.Add(cd);
            _gameManager.UpdateCoinCount(_coins.Count, _maxCoins);

            // Pop animation
            StartCoroutine(PopAnim(coinObj.transform));
        }

        IEnumerator PopAnim(Transform t)
        {
            if (t == null) yield break;
            t.localScale = Vector3.zero;
            float d = 0.3f, elapsed = 0f;
            while (elapsed < d)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                float p = elapsed / d;
                float s = p < 0.7f ? Mathf.Lerp(0f, 1.3f, p / 0.7f) : Mathf.Lerp(1.3f, 1f, (p - 0.7f) / 0.3f);
                t.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            if (t != null) t.localScale = Vector3.one;
        }

        CoinType PickCoinType()
        {
            int stage = _stageNumber;
            if (stage == 1) return CoinType.Normal;

            var pool = new List<CoinType> { CoinType.Normal, CoinType.Heavy, CoinType.Light };
            if (_hasBounce) pool.Add(CoinType.Bounce);
            if (_hasMagnet) pool.Add(CoinType.Magnet);
            return pool[Random.Range(0, pool.Count)];
        }

        Sprite GetCoinSprite(CoinType t)
        {
            return t switch
            {
                CoinType.Heavy  => _heavyCoinSprite  != null ? _heavyCoinSprite  : _coinSprite,
                CoinType.Light  => _lightCoinSprite  != null ? _lightCoinSprite  : _coinSprite,
                CoinType.Bounce => _bounceCoinSprite != null ? _bounceCoinSprite : _coinSprite,
                CoinType.Magnet => _magnetCoinSprite != null ? _magnetCoinSprite : _coinSprite,
                _               => _coinSprite,
            };
        }

        float GetCoinMass(CoinType t)
        {
            return t switch
            {
                CoinType.Heavy  => 2f,
                CoinType.Light  => 0.5f,
                _               => 1f,
            };
        }

        void HandleMagnet()
        {
            if (!_hasMagnet) return;
            foreach (var c in _coins)
            {
                if (c == null || c.CoinType != CoinType.Magnet) continue;
                foreach (var other in _coins)
                {
                    if (other == null || other == c) continue;
                    var diff = c.transform.position - other.transform.position;
                    float dist = diff.magnitude;
                    if (dist < MagnetRadius && dist > 0.01f)
                    {
                        float force = MagnetForce / (dist * dist);
                        other.Rb.AddForce(diff.normalized * force * Time.deltaTime, ForceMode2D.Force);
                    }
                }
            }
        }

        void HandleShrink()
        {
            if (!_hasShrink || _platform == null) return;
            _shrinkTimer += Time.deltaTime;
            if (_shrinkTimer >= 10f)
            {
                _shrinkTimer = 0f;
                var s = _platform.localScale;
                float newX = Mathf.Max(s.x * 0.9f, 1f);
                _platform.localScale = new Vector3(newX, s.y, s.z);
            }
        }

        void HandleFallDetection()
        {
            float camSize = Camera.main != null ? Camera.main.orthographicSize : 5f;
            float fallThreshold = -camSize - 1f;
            float dangerThreshold = camSize - 1f;

            for (int i = _coins.Count - 1; i >= 0; i--)
            {
                var c = _coins[i];
                if (c == null) { _coins.RemoveAt(i); continue; }

                float y = c.transform.position.y;
                if (y < fallThreshold)
                {
                    _coins.RemoveAt(i);
                    c.SetDanger(false);
                    Destroy(c.gameObject);
                    _gameManager.TriggerGameOver();
                    return;
                }

                // Danger detection: near edge of platform
                float px = _platform != null ? _platform.localScale.x * 3f : 3f;
                float relX = c.transform.position.x - (_platform != null ? _platform.position.x : 0f);
                bool danger = Mathf.Abs(relX) > px - 0.5f;
                c.SetDanger(danger);
            }
        }

        void HandleStageTimer()
        {
            if (!_isActive) return;
            _stageTimer += Time.deltaTime;
            float remaining = _stageDuration - _stageTimer;

            if (_ui != null) _ui.UpdateTimer(remaining);

            if (_stageTimer >= _stageDuration)
                _gameManager.TriggerStageClear();
        }
    }
}
