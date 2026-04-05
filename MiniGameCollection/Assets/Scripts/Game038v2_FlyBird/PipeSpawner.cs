using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game038v2_FlyBird
{
    public class PipeSpawner : MonoBehaviour
    {
        [SerializeField] FlyBirdGameManager _gameManager;
        [SerializeField] BirdController _birdController;
        [SerializeField] Sprite _spritePipeBody;
        [SerializeField] Sprite _spritePipeCap;
        [SerializeField] Sprite _spriteCoin;
        [SerializeField] Sprite _spriteWindEffect;

        bool _isActive;
        int _currentStage;
        Rigidbody2D _birdRbCached;

        // Stage config
        float _gapSize;
        float _scrollSpeed;
        bool _hasCoin;
        bool _hasMovingPipe;
        bool _hasWind;
        bool _hasRotatingPipe;

        float _spawnInterval;
        float _spawnTimer;

        Camera _camera;
        List<GameObject> _activePipes = new List<GameObject>();
        List<GameObject> _activeCoins = new List<GameObject>();
        List<GameObject> _windEffects = new List<GameObject>();

        float _windTimer;
        float _windInterval = 3.0f;
        float _windForce = 2.5f;
        bool _windActive;
        float _windDuration = 1.5f;
        float _windActiveTimer;

        void Awake()
        {
            _camera = Camera.main;
        }

        public void SetupStage(int stage)
        {
            _currentStage = stage;
            switch (stage)
            {
                case 0: _gapSize = 3.5f; _scrollSpeed = 3.0f; _hasCoin = false; _hasMovingPipe = false; _hasWind = false; _hasRotatingPipe = false; break;
                case 1: _gapSize = 3.0f; _scrollSpeed = 3.5f; _hasCoin = true;  _hasMovingPipe = false; _hasWind = false; _hasRotatingPipe = false; break;
                case 2: _gapSize = 2.6f; _scrollSpeed = 4.0f; _hasCoin = true;  _hasMovingPipe = true;  _hasWind = false; _hasRotatingPipe = false; break;
                case 3: _gapSize = 2.4f; _scrollSpeed = 4.5f; _hasCoin = true;  _hasMovingPipe = true;  _hasWind = true;  _hasRotatingPipe = false; break;
                case 4: _gapSize = 2.0f; _scrollSpeed = 5.0f; _hasCoin = true;  _hasMovingPipe = true;  _hasWind = true;  _hasRotatingPipe = true;  break;
                default: _gapSize = 2.0f; _scrollSpeed = 5.0f; _hasCoin = true; _hasMovingPipe = true;  _hasWind = true;  _hasRotatingPipe = true;  break;
            }
            _spawnInterval = Mathf.Max(0.8f, 2.5f - stage * 0.1f);
            _spawnTimer = _spawnInterval * 0.5f; // first pipe spawns early
            _windTimer = _windInterval;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active) _windActive = false;
        }

        public void ClearPipes()
        {
            foreach (var p in _activePipes) if (p != null) Destroy(p);
            foreach (var c in _activeCoins) if (c != null) Destroy(c);
            foreach (var w in _windEffects) if (w != null) Destroy(w);
            _activePipes.Clear();
            _activeCoins.Clear();
            _windEffects.Clear();
            _isActive = true;
            if (_birdRbCached == null && _birdController != null)
                _birdRbCached = _birdController.GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (!_isActive) return;

            float camSize = _camera.orthographicSize;
            float camWidth = camSize * _camera.aspect;
            float destroyX = -camWidth - 2f;

            // Move pipes
            List<GameObject> toRemove = new List<GameObject>();
            foreach (var pipe in _activePipes)
            {
                if (pipe == null) { toRemove.Add(pipe); continue; }
                pipe.transform.Translate(Vector3.left * _scrollSpeed * Time.deltaTime);
                if (pipe.transform.position.x < destroyX) toRemove.Add(pipe);
            }
            foreach (var p in toRemove) { _activePipes.Remove(p); if (p != null) Destroy(p); }

            // Move coins
            List<GameObject> toRemoveC = new List<GameObject>();
            foreach (var coin in _activeCoins)
            {
                if (coin == null) { toRemoveC.Add(coin); continue; }
                if (!coin.activeInHierarchy) { toRemoveC.Add(coin); continue; }
                coin.transform.Translate(Vector3.left * _scrollSpeed * Time.deltaTime);
                if (coin.transform.position.x < destroyX) toRemoveC.Add(coin);
            }
            foreach (var c in toRemoveC) { _activeCoins.Remove(c); if (c != null) Destroy(c); }

            // Move wind effects
            List<GameObject> toRemoveW = new List<GameObject>();
            foreach (var w in _windEffects)
            {
                if (w == null) { toRemoveW.Add(w); continue; }
                w.transform.Translate(Vector3.left * _scrollSpeed * Time.deltaTime);
                if (w.transform.position.x < destroyX) toRemoveW.Add(w);
            }
            foreach (var w in toRemoveW) { _windEffects.Remove(w); if (w != null) Destroy(w); }

            // Spawn
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnPipe();
                _spawnTimer = _spawnInterval;
            }

            // Wind logic
            if (_hasWind)
            {
                if (_windActive)
                {
                    if (_birdRbCached != null)
                        _birdRbCached.AddForce(Vector2.up * _windForce * Time.deltaTime * 60f);

                    _windActiveTimer -= Time.deltaTime;
                    if (_windActiveTimer <= 0f) _windActive = false;
                }
                else
                {
                    _windTimer -= Time.deltaTime;
                    if (_windTimer <= 0f)
                    {
                        _windActive = true;
                        _windActiveTimer = _windDuration;
                        _windTimer = _windInterval;
                        _windForce = Random.value > 0.5f ? 2.5f : -2.5f;
                    }
                }
            }
        }

        void SpawnPipe()
        {
            float camSize = _camera.orthographicSize;
            float camWidth = camSize * _camera.aspect;
            float spawnX = camWidth + 1.0f;

            float minY = -camSize + 1.8f;
            float maxY = camSize - 1.8f;
            float gapCenterY = Random.Range(minY, maxY);

            float topPipeBottom = gapCenterY + _gapSize / 2f;
            float botPipeTop = gapCenterY - _gapSize / 2f;

            bool isMoving = _hasMovingPipe && Random.value > 0.5f;
            bool isRotating = _hasRotatingPipe && Random.value > 0.4f;

            // Create pair root
            var pairRoot = new GameObject("PipePair");
            pairRoot.transform.position = new Vector3(spawnX, 0f, 0f);
            var pairComp = pairRoot.AddComponent<PipePair>();
            pairComp.CurrentStage = _currentStage;

            if (isMoving) pairComp.SetMoving(true, 1.5f, 0.8f, Random.value * Mathf.PI * 2f);
            if (isRotating) pairComp.SetRotating(true);

            // Top pipe (hangs from top)
            float topPipeHeight = (camSize - topPipeBottom) + 0.5f;
            if (topPipeHeight > 0.2f)
            {
                CreatePipeSegment(pairRoot.transform, new Vector3(0f, topPipeBottom + topPipeHeight / 2f - 0.5f, 0f), topPipeHeight, true, isRotating);
            }

            // Bottom pipe
            float botPipeHeight = (botPipeTop + camSize) + 0.5f;
            if (botPipeHeight > 0.2f)
            {
                CreatePipeSegment(pairRoot.transform, new Vector3(0f, botPipeTop - botPipeHeight / 2f + 0.5f, 0f), botPipeHeight, false, isRotating);
            }

            // Score trigger (invisible collider in the gap)
            // pairRoot is at (spawnX, 0, 0), so localPosition Y = gapCenterY to reach world Y = gapCenterY
            var triggerObj = new GameObject("ScoreTrigger");
            triggerObj.transform.SetParent(pairRoot.transform);
            triggerObj.transform.localPosition = new Vector3(0.1f, gapCenterY, 0f);
            triggerObj.tag = "ScoreTrigger";
            var triggerCol = triggerObj.AddComponent<BoxCollider2D>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector2(0.2f, _gapSize * 0.8f);

            _activePipes.Add(pairRoot);

            // Coin
            if (_hasCoin && Random.value > 0.5f)
            {
                SpawnCoin(spawnX, gapCenterY, _gapSize);
            }

            // Wind visual
            if (_hasWind && _windActive)
            {
                SpawnWindVisual(spawnX, gapCenterY);
            }
        }

        void CreatePipeSegment(Transform parent, Vector3 localPos, float height, bool isTop, bool noCollider = false)
        {
            var pipeObj = new GameObject(isTop ? "TopPipe" : "BotPipe");
            pipeObj.transform.SetParent(parent);
            pipeObj.transform.localPosition = localPos;
            pipeObj.tag = "Obstacle";

            var sr = pipeObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;
            if (_spritePipeBody != null)
            {
                sr.sprite = _spritePipeBody;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(0.6f, height);
            }
            else
            {
                pipeObj.transform.localScale = new Vector3(0.6f, height, 1f);
                sr.color = new Color(0.2f, 0.7f, 0.2f);
            }

            if (!noCollider)
            {
                var col = pipeObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.6f, height);
            }

            // Cap
            if (_spritePipeCap != null)
            {
                var capObj = new GameObject("Cap");
                capObj.transform.SetParent(pipeObj.transform);
                var capSr = capObj.AddComponent<SpriteRenderer>();
                capSr.sprite = _spritePipeCap;
                capSr.sortingOrder = 3;
                capObj.tag = "Obstacle";

                float capY = isTop ? -height / 2f : height / 2f;
                capObj.transform.localPosition = new Vector3(0f, capY, 0f);
                capObj.transform.localScale = new Vector3(1.3f, 0.25f / height, 1f);

                var capCol = capObj.AddComponent<BoxCollider2D>();
                capCol.size = new Vector2(0.8f, 0.4f);
            }
        }

        void SpawnCoin(float x, float gapCenterY, float gapSize)
        {
            // Place coin just outside the gap
            float sign = Random.value > 0.5f ? 1f : -1f;
            float coinY = gapCenterY + sign * (gapSize / 2f + 0.4f);

            var coinObj = new GameObject("Coin");
            coinObj.transform.position = new Vector3(x + Random.Range(-0.2f, 0.2f), coinY, 0f);
            coinObj.tag = "Coin";

            var sr = coinObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            if (_spriteCoin != null)
            {
                sr.sprite = _spriteCoin;
                coinObj.transform.localScale = Vector3.one * 0.4f;
            }
            else
            {
                coinObj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
                sr.color = new Color(1f, 0.85f, 0f);
            }

            var col = coinObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            _activeCoins.Add(coinObj);
        }

        void SpawnWindVisual(float x, float centerY)
        {
            if (_spriteWindEffect == null) return;
            var wObj = new GameObject("WindEffect");
            wObj.transform.position = new Vector3(x + 1f, centerY, 0f);
            var sr = wObj.AddComponent<SpriteRenderer>();
            sr.sprite = _spriteWindEffect;
            sr.sortingOrder = 1;
            sr.color = new Color(0.6f, 0.8f, 1f, 0.5f);
            wObj.transform.localScale = new Vector3(2f, 1f, 1f);
            _windEffects.Add(wObj);
        }
    }
}
