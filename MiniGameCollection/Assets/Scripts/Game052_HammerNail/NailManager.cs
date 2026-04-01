using UnityEngine;
using UnityEngine.InputSystem;

namespace Game052_HammerNail
{
    public class NailManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private HammerNailGameManager _gameManager;
        [SerializeField, Tooltip("釘スプライト")] private Sprite _nailSprite;
        [SerializeField, Tooltip("ハンマースプライト")] private Sprite _hammerSprite;
        [SerializeField, Tooltip("目標深度")] private float _targetDepth = 2f;
        [SerializeField, Tooltip("タイミング幅")] private float _sweetSpotDuration = 0.3f;

        private Camera _mainCamera;
        private bool _isActive;
        private GameObject _currentNail;
        private SpriteRenderer _currentNailSr;
        private float _currentDepth;
        private int _currentIndex;
        private int _totalNails;
        private float _gaugeValue;
        private float _gaugeSpeed = 3f;
        private bool _gaugeGoingUp = true;
        private GameObject _hammerObj;
        private SpriteRenderer _hammerSr;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame(int count)
        {
            _totalNails = count;
            _currentIndex = 0;
            _isActive = true;

            // Create hammer visual
            _hammerObj = new GameObject("Hammer");
            _hammerSr = _hammerObj.AddComponent<SpriteRenderer>();
            _hammerSr.sprite = _hammerSprite;
            _hammerSr.sortingOrder = 10;
            _hammerObj.transform.position = new Vector3(0f, 3f, 0f);
            _hammerObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            SpawnNail();
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Oscillate gauge (timing meter)
            if (_gaugeGoingUp)
            {
                _gaugeValue += Time.deltaTime * _gaugeSpeed;
                if (_gaugeValue >= 1f) { _gaugeValue = 1f; _gaugeGoingUp = false; }
            }
            else
            {
                _gaugeValue -= Time.deltaTime * _gaugeSpeed;
                if (_gaugeValue <= 0f) { _gaugeValue = 0f; _gaugeGoingUp = true; }
            }

            if (Mouse.current == null) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HammerHit();
            }
        }

        private void HammerHit()
        {
            // Sweet spot is center of gauge (0.4-0.6)
            float center = 0.5f;
            float distance = Mathf.Abs(_gaugeValue - center);
            bool isGoodHit = distance < _sweetSpotDuration;

            // Hammer animation
            if (_hammerObj != null)
            {
                _hammerObj.transform.position = new Vector3(0f, 1.5f, 0f);
                // Will reset next frame via LateUpdate-like approach
                Invoke(nameof(ResetHammer), 0.15f);
            }

            if (isGoodHit)
            {
                // Good timing - nail goes deeper
                float hitStrength = 1f - (distance / _sweetSpotDuration);
                float depthAdd = Mathf.Lerp(0.3f, 0.8f, hitStrength);
                _currentDepth += depthAdd;

                if (_currentNail != null)
                {
                    _currentNail.transform.position += new Vector3(0f, -depthAdd * 0.3f, 0f);
                }

                if (_currentDepth >= _targetDepth)
                {
                    _gameManager.OnNailComplete();
                    _currentIndex++;
                    if (_currentIndex < _totalNails) SpawnNail();
                }
            }
            else
            {
                // Bad timing - miss (nail bends)
                if (_currentNailSr != null) _currentNailSr.color = Color.red;
                _gameManager.OnMiss();
                _currentIndex++;
                if (_currentIndex < _totalNails && _gameManager.IsPlaying) SpawnNail();
            }

            // Speed up gauge slightly each hit
            _gaugeSpeed = Mathf.Min(_gaugeSpeed + 0.2f, 8f);
        }

        private void ResetHammer()
        {
            if (_hammerObj != null) _hammerObj.transform.position = new Vector3(0f, 3f, 0f);
        }

        private void SpawnNail()
        {
            _currentDepth = 0f;
            if (_currentNail != null) Destroy(_currentNail);

            _currentNail = new GameObject($"Nail_{_currentIndex}");
            _currentNail.transform.position = new Vector3(0f, 0f, 0f);
            _currentNailSr = _currentNail.AddComponent<SpriteRenderer>();
            _currentNailSr.sprite = _nailSprite;
            _currentNailSr.sortingOrder = 5;
            _currentNailSr.color = Color.white;
            _currentNail.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            _gaugeValue = 0f;
            _gaugeGoingUp = true;
        }

        public float GaugeValue => _gaugeValue;
    }
}
