using UnityEngine;
using UnityEngine.InputSystem;

namespace Game056_InflateFloat
{
    public class FloatManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private InflateFloatGameManager _gameManager;
        [SerializeField, Tooltip("風船スプライト")] private Sprite _balloonSprite;
        [SerializeField, Tooltip("障害物スプライト")] private Sprite _spikeSprite;
        [SerializeField, Tooltip("ゴールスプライト")] private Sprite _goalSprite;
        [SerializeField, Tooltip("膨張速度")] private float _inflateSpeed = 0.5f;
        [SerializeField, Tooltip("収縮速度")] private float _deflateSpeed = 0.3f;
        [SerializeField, Tooltip("最大サイズ")] private float _maxSize = 2f;
        [SerializeField, Tooltip("最小サイズ")] private float _minSize = 0.3f;
        [SerializeField, Tooltip("破裂サイズ")] private float _popSize = 2.2f;
        [SerializeField, Tooltip("横移動速度")] private float _moveSpeed = 2f;

        private Camera _mainCamera;
        private GameObject _balloon;
        private Rigidbody2D _balloonRb;
        private float _currentSize;
        private bool _isActive;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _currentSize = 0.8f;

            // Create balloon
            _balloon = new GameObject("Balloon");
            _balloon.transform.position = new Vector3(-3f, 0f, 0f);
            var sr = _balloon.AddComponent<SpriteRenderer>();
            sr.sprite = _balloonSprite; sr.sortingOrder = 5;
            _balloonRb = _balloon.AddComponent<Rigidbody2D>();
            _balloonRb.gravityScale = 0f;
            _balloonRb.mass = 0.2f;
            var col = _balloon.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            var bc = _balloon.AddComponent<BalloonController>();
            bc.Initialize(_gameManager);

            UpdateBalloonScale();
            SpawnObstacles();
            SpawnGoal();
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive || _balloon == null) return;

            // Inflate on hold, deflate on release
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                _currentSize += _inflateSpeed * Time.deltaTime;
                if (_currentSize >= _popSize)
                {
                    _gameManager.OnBalloonPopped();
                    return;
                }
            }
            else
            {
                _currentSize -= _deflateSpeed * Time.deltaTime;
                _currentSize = Mathf.Max(_currentSize, _minSize);
            }

            UpdateBalloonScale();

            // Buoyancy: larger = more upward force
            float buoyancy = (_currentSize - 0.7f) * 4f;
            _balloonRb.linearVelocity = new Vector2(_moveSpeed, buoyancy);

            // Fall check
            if (_balloon.transform.position.y < -7f)
            {
                _gameManager.OnBalloonFallen();
            }

            // Camera follow horizontally
            var camPos = _mainCamera.transform.position;
            camPos.x = Mathf.Lerp(camPos.x, _balloon.transform.position.x + 2f, Time.deltaTime * 2f);
            _mainCamera.transform.position = camPos;
        }

        private void UpdateBalloonScale()
        {
            if (_balloon != null)
                _balloon.transform.localScale = Vector3.one * _currentSize;
        }

        private void SpawnObstacles()
        {
            float[] positions = { 2f, 5f, 8f, 11f, 14f };
            foreach (float x in positions)
            {
                float y = Random.Range(-3f, 3f);
                var obj = new GameObject("Spike");
                obj.transform.position = new Vector3(x, y, 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _spikeSprite; sr.sortingOrder = 3;
                obj.transform.localScale = Vector3.one * 0.8f;
                var col = obj.AddComponent<CircleCollider2D>();
                col.radius = 0.3f;
                col.isTrigger = true;
                obj.name = "Spike";
            }
        }

        private void SpawnGoal()
        {
            var obj = new GameObject("Goal");
            obj.transform.position = new Vector3(18f, 0f, 0f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _goalSprite; sr.sortingOrder = 4;
            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 1f);
            col.isTrigger = true;
        }

        public float SizeRatio => Mathf.Clamp01((_currentSize - _minSize) / (_maxSize - _minSize));
    }
}
