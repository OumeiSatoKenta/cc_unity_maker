using UnityEngine;
using UnityEngine.InputSystem;

namespace Game080_FreqFight
{
    public class FreqManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private FreqFightGameManager _gameManager;
        [SerializeField, Tooltip("敵スプライト")] private Sprite _enemySprite;
        [SerializeField, Tooltip("ロックオン許容範囲")] private float _lockTolerance = 0.1f;
        [SerializeField, Tooltip("攻撃間隔")] private float _attackInterval = 3f;

        private Camera _mainCamera;
        private bool _isActive;
        private float _targetFreq;
        private float _playerFreq;
        private float _attackTimer;
        private GameObject _enemyObj;
        private SpriteRenderer _enemySr;
        private bool _isDragging;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _playerFreq = 0.5f;
            _attackTimer = _attackInterval;
            SpawnEnemy();
        }

        public void StopGame() { _isActive = false; }

        public void NextEnemy() { SpawnEnemy(); }

        private void SpawnEnemy()
        {
            _targetFreq = Random.Range(0.1f, 0.9f);
            _attackTimer = _attackInterval;

            if (_enemyObj != null) Destroy(_enemyObj);
            _enemyObj = new GameObject("Enemy");
            _enemyObj.transform.position = new Vector3(0f, 2f, 0f);
            _enemySr = _enemyObj.AddComponent<SpriteRenderer>();
            _enemySr.sprite = _enemySprite; _enemySr.sortingOrder = 5;
            _enemyObj.transform.localScale = Vector3.one * 1.2f;
        }

        private void Update()
        {
            if (!_isActive) return;

            // Enemy attacks on timer
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _attackTimer = _attackInterval;
                _gameManager.OnPlayerDamaged();
            }

            // Player adjusts frequency with drag
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    _isDragging = true;
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                    _isDragging = false;

                if (_isDragging)
                {
                    float screenX = Mouse.current.position.ReadValue().x;
                    _playerFreq = Mathf.Clamp01(screenX / Screen.width);
                }

                // Check lock-on
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    float diff = Mathf.Abs(_playerFreq - _targetFreq);
                    if (diff <= _lockTolerance)
                    {
                        // Hit!
                        if (_enemySr != null) _enemySr.color = Color.yellow;
                        _gameManager.OnEnemyDefeated();
                    }
                }
            }

            // Visual: enemy color indicates proximity
            if (_enemySr != null)
            {
                float diff = Mathf.Abs(_playerFreq - _targetFreq);
                if (diff < _lockTolerance)
                    _enemySr.color = Color.Lerp(Color.green, Color.white, diff / _lockTolerance);
                else if (diff < _lockTolerance * 3f)
                    _enemySr.color = Color.Lerp(Color.yellow, Color.red, (diff - _lockTolerance) / (_lockTolerance * 2f));
                else
                    _enemySr.color = Color.red;
            }
        }

        public float PlayerFreq => _playerFreq;
        public float TargetFreq => _targetFreq;
    }
}
