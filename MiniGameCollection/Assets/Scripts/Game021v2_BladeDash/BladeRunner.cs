using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game021v2_BladeDash
{
    public enum BladeType { Normal, Low, High, Moving }

    public class BladeRunner : MonoBehaviour
    {
        [SerializeField] BladeDashGameManager _gameManager;
        [SerializeField] SpriteRenderer _playerRenderer;
        [SerializeField] SpriteRenderer _playerSlideRenderer;
        [SerializeField] Sprite _bladeNormalSprite;
        [SerializeField] Sprite _bladeLowSprite;
        [SerializeField] Sprite _bladeHighSprite;
        [SerializeField] Sprite _coinSprite;

        // Lane config (set in SetupStage)
        float[] _laneX = new float[3];
        float _playerY;
        float _spawnY;
        float _despawnY;

        // Player state
        int _currentLane = 1;
        bool _isJumping;
        bool _isSliding;
        bool _isActive;

        // Scroll
        float _scrollSpeed = 4f;
        float _baseScrollSpeed = 4f;

        // Spawn timing
        float _spawnInterval = 2.0f;
        float _spawnTimer;
        float _coinSpawnInterval = 1.2f;
        float _coinSpawnTimer;

        // Complexity
        int _complexityFactor;

        // Object pools
        readonly List<GameObject> _blades = new();
        readonly List<GameObject> _coins = new();

        // Near miss tracking
        const float NearMissDistance = 0.35f;
        const float HitDistance = 0.22f;
        readonly HashSet<GameObject> _triggeredNearMiss = new();

        // Swipe input
        Vector2 _pointerDownPos;
        bool _pointerDown;
        const float SwipeThreshold = 30f;

        // Player jump params
        float _jumpHeight = 0.5f;
        float _jumpDuration = 0.5f;
        float _playerBaseY;

        // Camera
        float _camHeight;
        float _camWidth;

        // Blade sizes (world units)
        Vector2 _bladeSizeNormal = new Vector2(0.9f, 0.28f);
        Vector2 _bladeSizeLow    = new Vector2(0.9f, 0.18f);
        Vector2 _bladeSizeHigh   = new Vector2(0.9f, 0.48f);
        Vector2 _coinSize        = new Vector2(0.35f, 0.35f);

        void Awake()
        {
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[BladeRunner] Main Camera not found."); return; }
            _camHeight = cam.orthographicSize;
            _camWidth = _camHeight * cam.aspect;

            float laneSpacing = _camWidth * 0.38f;
            _laneX[0] = -laneSpacing;
            _laneX[1] = 0f;
            _laneX[2] = laneSpacing;

            _playerY = -_camHeight + 2.8f;
            _playerBaseY = _playerY;
            _spawnY = _camHeight + 1.2f;
            _despawnY = -_camHeight - 1.5f;
        }

        public void SetupStage(StageManager.StageConfig config, int targetScore)
        {
            StopAllCoroutines();
            if (_playerRenderer) _playerRenderer.gameObject.SetActive(true);
            if (_playerSlideRenderer) _playerSlideRenderer.gameObject.SetActive(false);
            ClearAllObjects();
            _scrollSpeed = _baseScrollSpeed * config.speedMultiplier;
            _complexityFactor = (int)config.complexityFactor;
            _spawnInterval = Mathf.Max(0.7f, 2.2f / config.speedMultiplier);
            _coinSpawnInterval = Mathf.Max(0.5f, 1.2f / config.speedMultiplier);
            _spawnTimer = _spawnInterval;
            _coinSpawnTimer = _coinSpawnInterval * 0.5f;
            _currentLane = 1;
            _isJumping = false;
            _isSliding = false;
            _playerBaseY = _playerY;
            ApplyPlayerTransform();
            _isActive = true;
        }

        public void StopGame()
        {
            _isActive = false;
            StopAllCoroutines();
            _isJumping = false;
            _isSliding = false;
            if (_playerRenderer) _playerRenderer.gameObject.SetActive(true);
            if (_playerSlideRenderer) _playerSlideRenderer.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!_isActive) return;
            HandleInput();
            ScrollObjects();
            SpawnBlades();
            SpawnCoins();
            CheckCollisions();
        }

        void HandleInput()
        {
            // Mouse/touch input
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _pointerDown = true;
                    _pointerDownPos = Mouse.current.position.ReadValue();
                }
                if (Mouse.current.leftButton.wasReleasedThisFrame && _pointerDown)
                {
                    _pointerDown = false;
                    Vector2 delta = Mouse.current.position.ReadValue() - _pointerDownPos;
                    ProcessSwipe(delta);
                }
            }

            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    _pointerDown = true;
                    _pointerDownPos = touch.position.ReadValue();
                }
                if (touch.press.wasReleasedThisFrame && _pointerDown)
                {
                    _pointerDown = false;
                    Vector2 delta = touch.position.ReadValue() - _pointerDownPos;
                    ProcessSwipe(delta);
                }
            }
        }

        void ProcessSwipe(Vector2 delta)
        {
            if (delta.magnitude < SwipeThreshold) return;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x < 0) MoveLeft();
                else MoveRight();
            }
            else
            {
                if (delta.y > 0) Jump();
                else Slide();
            }
        }

        void MoveLeft()
        {
            if (_currentLane > 0) _currentLane--;
            ApplyPlayerTransform();
        }

        void MoveRight()
        {
            if (_currentLane < 2) _currentLane++;
            ApplyPlayerTransform();
        }

        void Jump()
        {
            if (_isJumping || _isSliding) return;
            StartCoroutine(DoJump());
        }

        void Slide()
        {
            if (_isJumping || _isSliding) return;
            StartCoroutine(DoSlide());
        }

        IEnumerator DoJump()
        {
            _isJumping = true;
            if (_playerRenderer) _playerRenderer.color = new Color(1f, 0.9f, 0.5f, 1f);
            float elapsed = 0f;
            while (elapsed < _jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _jumpDuration;
                float yOffset = _jumpHeight * 4f * t * (1f - t);
                if (_playerRenderer)
                {
                    var pos = _playerRenderer.transform.localPosition;
                    pos.y = _playerBaseY + yOffset;
                    _playerRenderer.transform.localPosition = pos;
                }
                yield return null;
            }
            if (_playerRenderer)
            {
                _playerRenderer.color = Color.white;
                var pos = _playerRenderer.transform.localPosition;
                pos.y = _playerBaseY;
                _playerRenderer.transform.localPosition = pos;
            }
            _isJumping = false;
        }

        IEnumerator DoSlide()
        {
            _isSliding = true;
            if (_playerRenderer) _playerRenderer.gameObject.SetActive(false);
            if (_playerSlideRenderer) _playerSlideRenderer.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            if (_playerRenderer) _playerRenderer.gameObject.SetActive(true);
            if (_playerSlideRenderer) _playerSlideRenderer.gameObject.SetActive(false);
            _isSliding = false;
        }

        void ApplyPlayerTransform()
        {
            float x = _laneX[_currentLane];
            if (_playerRenderer)
            {
                var pos = _playerRenderer.transform.localPosition;
                pos.x = x;
                _playerRenderer.transform.localPosition = pos;
            }
            if (_playerSlideRenderer)
            {
                var pos = _playerSlideRenderer.transform.localPosition;
                pos.x = x;
                _playerSlideRenderer.transform.localPosition = pos;
            }
        }

        void ScrollObjects()
        {
            float dy = _scrollSpeed * Time.deltaTime;
            foreach (var blade in _blades)
            {
                if (blade == null) continue;
                var p = blade.transform.localPosition;
                // Moving blade horizontal movement
                var bdata = blade.GetComponent<BladeData>();
                if (bdata != null && bdata.bladeType == BladeType.Moving)
                {
                    bdata.moveTimer += Time.deltaTime * bdata.moveSpeed;
                    p.x = bdata.originX + Mathf.Sin(bdata.moveTimer) * _camWidth * 0.3f;
                }
                p.y -= dy;
                blade.transform.localPosition = p;
            }
            foreach (var coin in _coins)
            {
                if (coin == null) continue;
                var p = coin.transform.localPosition;
                p.y -= dy;
                coin.transform.localPosition = p;
            }
            // Clean up triggered near miss set for despawned blades
            _blades.FindAll(b => b == null || b.transform.localPosition.y < _despawnY)
                   .ForEach(b => _triggeredNearMiss.Remove(b));
            _blades.RemoveAll(b => b == null || b.transform.localPosition.y < _despawnY);
            _coins.RemoveAll(c => c == null || c.transform.localPosition.y < _despawnY);
        }

        void SpawnBlades()
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0) return;
            _spawnTimer = _spawnInterval + Random.Range(-0.2f, 0.2f);
            SpawnBlade();
        }

        void SpawnBlade()
        {
            // Pick lane(s) - possibly block multiple lanes for higher complexity
            int numBlades = _complexityFactor >= 3 ? (Random.value < 0.3f ? 2 : 1) : 1;
            List<int> usedLanes = new();
            for (int i = 0; i < numBlades; i++)
            {
                int lane;
                int attempts = 0;
                do { lane = Random.Range(0, 3); attempts++; }
                while (usedLanes.Contains(lane) && attempts < 10);
                usedLanes.Add(lane);

                BladeType btype = PickBladeType();
                CreateBladeAt(lane, btype);
            }
        }

        BladeType PickBladeType()
        {
            float r = Random.value;
            if (_complexityFactor >= 4)
            {
                // All types
                if (r < 0.25f) return BladeType.Low;
                if (r < 0.50f) return BladeType.High;
                if (r < 0.80f) return BladeType.Moving;
                return BladeType.Normal;
            }
            if (_complexityFactor >= 3)
            {
                if (r < 0.2f) return BladeType.Low;
                if (r < 0.4f) return BladeType.High;
                if (r < 0.7f) return BladeType.Moving;
                return BladeType.Normal;
            }
            if (_complexityFactor >= 2)
            {
                if (r < 0.2f) return BladeType.Low;
                if (r < 0.4f) return BladeType.High;
                return BladeType.Normal;
            }
            if (_complexityFactor >= 1)
            {
                if (r < 0.2f) return BladeType.Low;
                return BladeType.Normal;
            }
            return BladeType.Normal;
        }

        void CreateBladeAt(int lane, BladeType btype)
        {
            var obj = new GameObject($"Blade_{btype}");
            obj.transform.SetParent(transform);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;

            Vector2 size;
            switch (btype)
            {
                case BladeType.Low:
                    sr.sprite = _bladeLowSprite != null ? _bladeLowSprite : _bladeNormalSprite;
                    size = _bladeSizeLow;
                    break;
                case BladeType.High:
                    sr.sprite = _bladeHighSprite != null ? _bladeHighSprite : _bladeNormalSprite;
                    size = _bladeSizeHigh;
                    break;
                default:
                    sr.sprite = _bladeNormalSprite;
                    size = _bladeSizeNormal;
                    break;
            }
            if (sr.sprite != null)
            {
                float scaleX = size.x / (sr.sprite.rect.width / sr.sprite.pixelsPerUnit);
                float scaleY = size.y / (sr.sprite.rect.height / sr.sprite.pixelsPerUnit);
                obj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            var bdata = obj.AddComponent<BladeData>();
            bdata.bladeType = btype;
            bdata.originX = _laneX[lane];
            bdata.moveSpeed = 1.5f + Random.Range(0f, 1f);

            obj.transform.localPosition = new Vector3(_laneX[lane], _spawnY, 0f);
            _blades.Add(obj);
        }

        void SpawnCoins()
        {
            _coinSpawnTimer -= Time.deltaTime;
            if (_coinSpawnTimer > 0) return;
            _coinSpawnTimer = _coinSpawnInterval + Random.Range(-0.3f, 0.3f);

            int lane = Random.Range(0, 3);
            var obj = new GameObject("Coin");
            obj.transform.SetParent(transform);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _coinSprite;
            sr.sortingOrder = 3;
            if (sr.sprite != null)
            {
                float scaleX = _coinSize.x / (sr.sprite.rect.width / sr.sprite.pixelsPerUnit);
                float scaleY = _coinSize.y / (sr.sprite.rect.height / sr.sprite.pixelsPerUnit);
                obj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
            obj.transform.localPosition = new Vector3(_laneX[lane], _spawnY + 0.5f, 0f);
            _coins.Add(obj);
        }

        void CheckCollisions()
        {
            // Get player world position
            Vector3 playerPos = _playerRenderer != null
                ? _playerRenderer.transform.position
                : new Vector3(_laneX[_currentLane], _playerY, 0f);

            // Player hit box based on state
            Vector2 playerSize = _isSliding ? new Vector2(0.55f, 0.28f) : new Vector2(0.38f, 0.55f);
            float playerHeadY = playerPos.y + playerSize.y * 0.5f;
            float playerFeetY = playerPos.y - playerSize.y * 0.5f;

            // Blade collision
            for (int i = _blades.Count - 1; i >= 0; i--)
            {
                var blade = _blades[i];
                if (blade == null) continue;
                var bdata = blade.GetComponent<BladeData>();
                if (bdata == null) continue;

                Vector3 bp = blade.transform.position;
                Vector2 bSize = GetBladeHitSize(bdata.bladeType);

                // Check X alignment (same lane roughly)
                if (Mathf.Abs(bp.x - playerPos.x) > bSize.x * 0.5f + playerSize.x * 0.5f) continue;

                // Vertical check depending on blade type
                bool hit = false;
                if (bdata.bladeType == BladeType.Low)
                {
                    // Low blade: jump to avoid
                    float bladeTopY = bp.y + bSize.y * 0.5f;
                    if (!_isJumping && playerFeetY < bladeTopY && playerPos.y < bp.y + bSize.y)
                        hit = true;
                }
                else if (bdata.bladeType == BladeType.High)
                {
                    // High blade: slide to avoid
                    float bladeBottomY = bp.y - bSize.y * 0.5f;
                    if (!_isSliding && playerHeadY > bladeBottomY && playerPos.y > bp.y - bSize.y)
                        hit = true;
                }
                else
                {
                    // Normal/Moving: must be in different lane
                    float dy = Mathf.Abs(bp.y - playerPos.y);
                    float hitRadius = (bSize.y + playerSize.y) * 0.4f;
                    if (dy < hitRadius) hit = true;
                }

                if (hit)
                {
                    _gameManager.OnGameOver();
                    return;
                }

                // Near miss check
                if (!_triggeredNearMiss.Contains(blade))
                {
                    float dist = Vector2.Distance(new Vector2(bp.x, bp.y), new Vector2(playerPos.x, playerPos.y));
                    if (dist < NearMissDistance && dist > HitDistance)
                    {
                        _triggeredNearMiss.Add(blade);
                        _gameManager.OnNearMiss();
                    }
                }
            }

            // Coin collection
            for (int i = _coins.Count - 1; i >= 0; i--)
            {
                var coin = _coins[i];
                if (coin == null) continue;
                float dist = Vector2.Distance(new Vector2(coin.transform.position.x, coin.transform.position.y),
                                              new Vector2(playerPos.x, playerPos.y));
                if (dist < 0.4f)
                {
                    StartCoroutine(CoinCollectAnim(coin));
                    _coins.RemoveAt(i);
                    _gameManager.OnCoinCollected();
                }
            }
        }

        Vector2 GetBladeHitSize(BladeType t)
        {
            return t switch
            {
                BladeType.Low => _bladeSizeLow,
                BladeType.High => _bladeSizeHigh,
                _ => _bladeSizeNormal
            };
        }

        IEnumerator CoinCollectAnim(GameObject coin)
        {
            if (coin == null) yield break;
            float elapsed = 0f;
            float dur = 0.2f;
            Vector3 startScale = coin.transform.localScale;
            var sr = coin.GetComponent<SpriteRenderer>();
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float scale = Mathf.Lerp(1f, 0f, t);
                if (coin != null) coin.transform.localScale = startScale * (1f + t * 0.5f) * (1f - t);
                if (sr != null) sr.color = new Color(1f, 1f, 0.3f, 1f - t);
                yield return null;
            }
            if (coin != null) Destroy(coin);
        }

        void ClearAllObjects()
        {
            foreach (var b in _blades) if (b != null) Destroy(b);
            _blades.Clear();
            foreach (var c in _coins) if (c != null) Destroy(c);
            _coins.Clear();
            _triggeredNearMiss.Clear();
        }

        void OnDestroy()
        {
            ClearAllObjects();
        }
    }

    public class BladeData : MonoBehaviour
    {
        public BladeType bladeType;
        public float originX;
        public float moveTimer;
        public float moveSpeed = 1.5f;
    }
}
