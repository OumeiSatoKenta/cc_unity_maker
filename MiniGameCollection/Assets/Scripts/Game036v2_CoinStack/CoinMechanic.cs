using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game036v2_CoinStack
{
    public enum CoinType { Normal, Heavy, Light }

    public class CoinMechanic : MonoBehaviour
    {
        [SerializeField] CoinStackGameManager _gameManager;
        [SerializeField] SpriteRenderer _sliderRenderer;
        [SerializeField] Transform _coinStackRoot;
        [SerializeField] GameObject _targetLineObj;
        [SerializeField] Sprite _normalCoinSprite;
        [SerializeField] Sprite _heavyCoinSprite;
        [SerializeField] Sprite _lightCoinSprite;

        // Stage params
        float _slideSpeed = 2.0f;
        int _goalCoins = 5;
        int _stageIndex = 0;
        bool _isActive = false;

        // Slider state
        float _sliderX;
        float _sliderDir = 1f;
        float _camWidth;
        float _sliderBound;
        CoinType _currentCoinType;
        float _currentCoinScale = 1f;
        bool _isDropping;

        // Stack state
        List<GameObject> _stackedCoins = new List<GameObject>();
        int _placedCoins = 0;
        float _stackTopY;
        float _baseY;
        float _coinHeight = 0.5f;

        // Wind (stage 3+)
        bool _windEnabled;
        float _windDrift;

        // Quake (stage 5)
        bool _quakeEnabled;
        Coroutine _quakeCoroutine;

        void Awake()
        {
            float camSize = Camera.main.orthographicSize;
            _camWidth = camSize * Camera.main.aspect;
            _baseY = -camSize + 2.8f;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            _goalCoins = config.countMultiplier;
            _slideSpeed = 2.0f * config.speedMultiplier;
            _placedCoins = 0;

            _windEnabled = stageIndex >= 2;
            _quakeEnabled = stageIndex >= 4;

            // Clear previous stack
            foreach (var c in _stackedCoins)
                if (c != null) Destroy(c);
            _stackedCoins.Clear();
            _stackTopY = _baseY;

            // Stop quake
            if (_quakeCoroutine != null) StopCoroutine(_quakeCoroutine);

            // Update target line
            if (_targetLineObj != null)
            {
                float targetY = _baseY + _goalCoins * _coinHeight;
                _targetLineObj.transform.position = new Vector3(0f, targetY, 0f);
                _targetLineObj.SetActive(true);
            }

            _isDropping = false;
            _isActive = true;

            // Start quake if stage 5
            if (_quakeEnabled)
                _quakeCoroutine = StartCoroutine(QuakeRoutine());

            PrepareNextCoin();
        }

        public void Deactivate()
        {
            _isActive = false;
            if (_quakeCoroutine != null) StopCoroutine(_quakeCoroutine);
            if (_sliderRenderer != null)
                _sliderRenderer.gameObject.SetActive(false);
            if (_targetLineObj != null)
                _targetLineObj.SetActive(false);
        }

        void Update()
        {
            if (!_isActive || _isDropping) return;

            // Slide coin
            _sliderBound = _camWidth - 0.6f;
            _sliderX += _sliderDir * _slideSpeed * Time.deltaTime;
            if (_sliderX > _sliderBound) { _sliderX = _sliderBound; _sliderDir = -1f; }
            if (_sliderX < -_sliderBound) { _sliderX = -_sliderBound; _sliderDir = 1f; }

            if (_sliderRenderer != null)
                _sliderRenderer.transform.position = new Vector3(_sliderX, Camera.main.orthographicSize - 0.5f, 0f);

            // Input
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                StartCoroutine(DropCoin(_sliderX));
            }
        }

        void PrepareNextCoin()
        {
            if (!_isActive) return;

            // Determine coin type
            if (_stageIndex >= 3)
            {
                float r = Random.value;
                if (r < 0.7f) _currentCoinType = CoinType.Normal;
                else if (r < 0.85f) _currentCoinType = CoinType.Heavy;
                else _currentCoinType = CoinType.Light;
            }
            else
            {
                _currentCoinType = CoinType.Normal;
            }

            // Stage 2: random size
            if (_stageIndex >= 1)
                _currentCoinScale = Random.Range(0.8f, 1.2f);
            else
                _currentCoinScale = 1.0f;

            // Override scale for special coins
            if (_currentCoinType == CoinType.Heavy) _currentCoinScale = 1.4f;
            else if (_currentCoinType == CoinType.Light) _currentCoinScale = 0.7f;

            Sprite spr = _currentCoinType == CoinType.Heavy ? _heavyCoinSprite
                       : _currentCoinType == CoinType.Light ? _lightCoinSprite
                       : _normalCoinSprite;

            if (_sliderRenderer != null)
            {
                _sliderRenderer.sprite = spr;
                _sliderRenderer.transform.localScale = new Vector3(_currentCoinScale, 1f, 1f);
                _sliderRenderer.gameObject.SetActive(true);
            }

            _sliderX = 0f;
            _sliderDir = Random.value > 0.5f ? 1f : -1f;
        }

        IEnumerator DropCoin(float startX)
        {
            _isDropping = true;
            if (_sliderRenderer != null)
                _sliderRenderer.gameObject.SetActive(false);

            // Create falling coin GO
            var coinGO = new GameObject("Coin_" + _placedCoins);
            coinGO.transform.position = new Vector3(startX, Camera.main.orthographicSize - 0.5f, 0f);

            Sprite spr = _currentCoinType == CoinType.Heavy ? _heavyCoinSprite
                       : _currentCoinType == CoinType.Light ? _lightCoinSprite
                       : _normalCoinSprite;

            var sr = coinGO.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingOrder = 10;
            float baseWidth = spr != null ? (spr.rect.width / spr.pixelsPerUnit) : 1.0f;
            coinGO.transform.localScale = new Vector3(_currentCoinScale, 1f, 1f);

            float dropSpeed = 8f;
            float targetY = _stackTopY + _coinHeight * 0.5f;

            // Wind drift
            float windX = 0f;
            if (_windEnabled)
                windX = Random.Range(-0.3f, 0.3f);

            // Drop animation
            while (coinGO.transform.position.y > targetY)
            {
                if (!_isActive) { Destroy(coinGO); yield break; }
                float dt = Time.deltaTime;
                coinGO.transform.position += new Vector3(windX * dt, -dropSpeed * dt, 0f);
                yield return null;
            }

            // Snap to stack
            float finalX = coinGO.transform.position.x;
            coinGO.transform.position = new Vector3(finalX, targetY, 0f);
            coinGO.transform.SetParent(_coinStackRoot, true);

            _stackedCoins.Add(coinGO);
            _stackTopY += _coinHeight;

            // Calculate offset from ideal position
            float idealX = _stackedCoins.Count > 1 ? _stackedCoins[_stackedCoins.Count - 2].transform.position.x : 0f;
            float offset = Mathf.Abs(finalX - idealX);

            // Collapse check
            bool collapsed = CheckCollapse();

            if (collapsed)
            {
                yield return StartCoroutine(CollapseAnimation());
                _gameManager.OnTowerCollapsed();
                yield break;
            }

            // Visual feedback
            yield return StartCoroutine(PlaceFeedback(coinGO, offset));

            _placedCoins++;
            _gameManager.OnCoinPlaced(offset, Mathf.Max(0, _goalCoins - _placedCoins));

            // Check goal
            if (_placedCoins >= _goalCoins)
            {
                _gameManager.OnStageGoalReached();
                yield break;
            }

            _isDropping = false;
            PrepareNextCoin();
        }

        bool CheckCollapse()
        {
            if (_stackedCoins.Count < 2) return false;

            // Check if any coin is too far from center
            float cumulativeOffset = 0f;
            for (int i = 1; i < _stackedCoins.Count; i++)
            {
                float offset = _stackedCoins[i].transform.position.x - _stackedCoins[i-1].transform.position.x;
                cumulativeOffset += offset;
                if (Mathf.Abs(cumulativeOffset) > 1.5f)
                    return true;
            }

            return false;
        }

        IEnumerator PlaceFeedback(GameObject coinGO, float offset)
        {
            bool isPerfect = offset < 0.1f;
            bool isGood = offset < 0.3f;

            if (isPerfect)
            {
                // Scale pop
                Vector3 orig = coinGO.transform.localScale;
                float t = 0f;
                while (t < 0.2f)
                {
                    t += Time.deltaTime;
                    float s = Mathf.Lerp(1f, 1.3f, Mathf.Sin(t / 0.2f * Mathf.PI));
                    coinGO.transform.localScale = new Vector3(orig.x * s, orig.y * s, 1f);
                    yield return null;
                }
                coinGO.transform.localScale = orig;

                // Gold tint
                var sr = coinGO.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(1f, 1f, 0.5f, 1f);
                    yield return new WaitForSeconds(0.1f);
                    sr.color = Color.white;
                }
            }
            else if (!isGood)
            {
                // Red flash
                var sr = coinGO.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(1f, 0.3f, 0.3f, 1f);
                    yield return StartCoroutine(CameraShake(0.15f, 0.1f));
                    sr.color = Color.white;
                }
                else
                {
                    yield return StartCoroutine(CameraShake(0.15f, 0.1f));
                }
            }
        }

        IEnumerator CollapseAnimation()
        {
            yield return StartCoroutine(CameraShake(0.4f, 0.25f));

            // Make coins fall
            foreach (var c in _stackedCoins)
            {
                if (c == null) continue;
                c.transform.SetParent(null);
                var rb = c.AddComponent<Rigidbody2D>();
                rb.gravityScale = 2f;
                rb.linearVelocity = new Vector2(Random.Range(-2f, 2f), Random.Range(-1f, 1f));
            }

            yield return new WaitForSeconds(0.5f);

            foreach (var c in _stackedCoins)
                if (c != null) Destroy(c);
            _stackedCoins.Clear();
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            Vector3 orig = cam.transform.position;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                cam.transform.position = new Vector3(orig.x + x, orig.y + y, orig.z);
                yield return null;
            }
            cam.transform.position = orig;
        }

        IEnumerator QuakeRoutine()
        {
            while (_isActive)
            {
                yield return new WaitForSeconds(3f);
                if (!_isActive) yield break;

                float quakeOffset = Random.Range(-0.1f, 0.1f);
                if (_coinStackRoot != null)
                {
                    Vector3 orig = _coinStackRoot.position;
                    _coinStackRoot.position = orig + new Vector3(quakeOffset, 0f, 0f);
                    yield return new WaitForSeconds(0.3f);
                    if (_coinStackRoot != null)
                        _coinStackRoot.position = orig;
                }
            }
        }
    }
}
