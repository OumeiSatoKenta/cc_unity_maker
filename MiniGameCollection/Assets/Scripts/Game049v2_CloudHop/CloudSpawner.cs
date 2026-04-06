using System.Collections.Generic;
using UnityEngine;

namespace Game049v2_CloudHop
{
    public class CloudSpawner : MonoBehaviour
    {
        [SerializeField] CloudHopGameManager _gameManager;
        [SerializeField] Sprite _spriteNormal;
        [SerializeField] Sprite _spriteSpring;
        [SerializeField] Sprite _spriteThunder;
        [SerializeField] Sprite _spriteMoving;
        [SerializeField] Sprite _spriteCoin;

        private int _stageNumber;
        private float _cloudLifetime = 4f;
        private float _cloudCount = 1.3f;
        private bool _randomFade;
        private bool _enableSpring;
        private bool _enableThunder;
        private bool _enableMoving;
        private float _moveSpeed = 1.5f;

        private float _spawnIntervalBase = 1.2f;
        private float _spawnTimer;

        private Camera _cam;
        private float _highestCloudY;
        private bool _isActive;

        private List<CloudObject> _activeClouds = new List<CloudObject>();

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            _stageNumber = stageNumber;
            _cloudLifetime = 4f / config.speedMultiplier;
            _cloudCount = config.countMultiplier;
            _randomFade = stageNumber >= 5;
            _enableSpring = stageNumber >= 2;
            _enableThunder = stageNumber >= 3;
            _enableMoving = stageNumber >= 4;
            _moveSpeed = 1.5f * config.speedMultiplier;

            _spawnIntervalBase = 0.8f / config.countMultiplier;

            _cam = Camera.main;
            float camSize = _cam.orthographicSize;
            _highestCloudY = -camSize + 1.5f;
            _isActive = true;
            _spawnTimer = 0f;

            // Spawn initial clouds
            SpawnInitialClouds();
        }

        void SpawnInitialClouds()
        {
            float camSize = _cam != null ? _cam.orthographicSize : 5f;
            float camWidth = _cam != null ? camSize * _cam.aspect : 2.8f;
            float startY = -camSize + 1.5f;

            int initialCount = Mathf.RoundToInt(6 * _cloudCount);
            for (int i = 0; i < initialCount; i++)
            {
                float y = startY + i * (camSize * 1.5f / initialCount);
                SpawnCloud(y, true);
            }
        }

        void Update()
        {
            if (!_isActive || _cam == null) return;

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _spawnIntervalBase)
            {
                _spawnTimer = 0f;
                float camSize = _cam.orthographicSize;
                float spawnY = _cam.transform.position.y + camSize + 1.5f;
                SpawnCloud(spawnY, false);
            }

            // Cleanup null refs
            _activeClouds.RemoveAll(c => c == null);
        }

        void SpawnCloud(float y, bool isInitial)
        {
            if (_cam == null) return;
            float camSize = _cam.orthographicSize;
            float camWidth = camSize * _cam.aspect;

            float x = Random.Range(-camWidth + 0.8f, camWidth - 0.8f);

            CloudType type = CloudType.Normal;
            float rand = Random.value;
            if (_enableMoving && rand < 0.15f) type = CloudType.Moving;
            else if (_enableThunder && rand < 0.30f) type = CloudType.Thunder;
            else if (_enableSpring && rand < 0.50f) type = CloudType.Spring;
            else type = CloudType.Normal;

            Sprite sprite = GetSprite(type);

            var obj = new GameObject("Cloud_" + type);
            obj.transform.position = new Vector3(x, y, 0f);
            obj.layer = LayerMask.NameToLayer("Default");
            obj.tag = "Cloud";

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;

            float cloudW = 2.0f;
            float cloudH = 0.8f;
            if (sprite != null)
            {
                float pw = sprite.rect.width / sprite.pixelsPerUnit;
                float ph = sprite.rect.height / sprite.pixelsPerUnit;
                obj.transform.localScale = new Vector3(cloudW / pw, cloudH / ph, 1f);
            }

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(cloudW, 0.3f);
            col.offset = new Vector2(0f, -cloudH * 0.3f);
            col.isTrigger = false;

            bool moving = (type == CloudType.Moving);
            float moveRange = moving ? 1.5f : 0f;

            var cloud = obj.AddComponent<CloudObject>();
            cloud.Initialize(type, _cloudLifetime, _randomFade, moving, _moveSpeed, moveRange, _gameManager);

            _activeClouds.Add(cloud);

            // Spawn coin near this cloud (probability based on stage)
            float coinChance = 0.2f + (_stageNumber - 1) * 0.05f;
            if (type != CloudType.Thunder && Random.value < coinChance)
            {
                SpawnCoin(x + Random.Range(-1.5f, 1.5f), y + 0.8f);
            }
        }

        Sprite GetSprite(CloudType type)
        {
            switch (type)
            {
                case CloudType.Spring: return _spriteSpring != null ? _spriteSpring : _spriteNormal;
                case CloudType.Thunder: return _spriteThunder != null ? _spriteThunder : _spriteNormal;
                case CloudType.Moving: return _spriteMoving != null ? _spriteMoving : _spriteNormal;
                default: return _spriteNormal;
            }
        }

        void SpawnCoin(float x, float y)
        {
            var coinObj = new GameObject("Coin");
            coinObj.transform.position = new Vector3(x, y, 0f);
            coinObj.tag = "Coin";

            var sr = coinObj.AddComponent<SpriteRenderer>();
            sr.sprite = _spriteCoin;
            sr.sortingOrder = 3;

            float size = 0.5f;
            if (_spriteCoin != null)
            {
                float pw = _spriteCoin.rect.width / _spriteCoin.pixelsPerUnit;
                float ph = _spriteCoin.rect.height / _spriteCoin.pixelsPerUnit;
                coinObj.transform.localScale = new Vector3(size / pw, size / ph, 1f);
            }

            var col = coinObj.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            col.isTrigger = true;

            var coin = coinObj.AddComponent<CoinObject>();
            coin.Initialize(_gameManager);
        }

        public void ClearClouds()
        {
            foreach (var c in _activeClouds)
            {
                if (c != null) Destroy(c.gameObject);
            }
            _activeClouds.Clear();

            // Also destroy all coins
            var coins = FindObjectsByType<CoinObject>(FindObjectsSortMode.None);
            foreach (var coin in coins)
            {
                Destroy(coin.gameObject);
            }
        }

        public void SetActive(bool active) => _isActive = active;
    }
}
