using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game035_WaveRider
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private WaveRiderGameManager _gameManager;

        [SerializeField, Tooltip("サーファーTransform")]
        private Transform _surferTransform;

        [SerializeField, Tooltip("岩スプライト")]
        private Sprite _rockSprite;

        private Camera _mainCamera;
        private float _surferX;
        private float _balance = 50f; // 0-100, 50が中央
        private bool _isJumping;
        private float _jumpTimer;
        private float _obstacleTimer;
        private float _waveTime;
        private List<Obstacle> _obstacles = new List<Obstacle>();

        private const float SurferBaseY = -1.5f;
        private const float WaveAmplitude = 0.8f;
        private const float WaveFrequency = 1.2f;
        private const float BalanceDecayRate = 8f;
        private const float BalanceRecoverRate = 15f;
        private const float ObstacleInterval = 2.5f;
        private const float ObstacleSpeed = 4f;
        private const float JumpDuration = 0.6f;
        private const float JumpHeight = 2f;
        private const float HitRadius = 0.5f;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void StartGame()
        {
            _surferX = 0f;
            _balance = 50f;
            _isJumping = false;
            _jumpTimer = 0f;
            _obstacleTimer = 0f;
            _waveTime = 0f;
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying) return;

            _waveTime += Time.deltaTime;
            HandleInput();
            UpdateSurfer();
            UpdateObstacles();
            CheckCollision();
            CheckBalance();
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;

            // ドラッグで左右バランス操作
            if (Mouse.current.leftButton.isPressed)
            {
                Vector3 mousePos = Mouse.current.position.ReadValue();
                mousePos.z = -_mainCamera.transform.position.z;
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

                // 画面中央より左→バランス左寄り、右→右寄り
                float targetBalance = Mathf.InverseLerp(-4f, 4f, worldPos.x) * 100f;
                _balance = Mathf.Lerp(_balance, targetBalance, Time.deltaTime * BalanceRecoverRate);
                _surferX = Mathf.Lerp(-3f, 3f, _balance / 100f);
            }

            // タップでジャンプ
            if (Mouse.current.leftButton.wasPressedThisFrame && !_isJumping)
            {
                _isJumping = true;
                _jumpTimer = 0f;
                _gameManager.AddTrickScore(50);
            }
        }

        private void UpdateSurfer()
        {
            if (_surferTransform == null) return;

            // 波のY座標
            float waveY = SurferBaseY + Mathf.Sin(_waveTime * WaveFrequency) * WaveAmplitude;

            // ジャンプ
            float jumpOffset = 0f;
            if (_isJumping)
            {
                _jumpTimer += Time.deltaTime;
                float t = _jumpTimer / JumpDuration;
                if (t >= 1f)
                {
                    _isJumping = false;
                    jumpOffset = 0f;
                }
                else
                {
                    jumpOffset = Mathf.Sin(t * Mathf.PI) * JumpHeight;
                }
            }

            _surferTransform.position = new Vector3(_surferX, waveY + jumpOffset, 0f);

            // バランスに応じて傾き
            float tilt = (_balance - 50f) * 0.3f;
            _surferTransform.rotation = Quaternion.Euler(0f, 0f, -tilt);

            // バランスの自然な崩れ（中央から離れるほど加速）
            float drift = (Mathf.PerlinNoise(_waveTime * 0.5f, 0f) - 0.5f) * BalanceDecayRate;
            _balance += drift * Time.deltaTime;
            _balance = Mathf.Clamp(_balance, 0f, 100f);
        }

        private void UpdateObstacles()
        {
            _obstacleTimer += Time.deltaTime;
            if (_obstacleTimer >= ObstacleInterval)
            {
                _obstacleTimer = 0f;
                SpawnObstacle();
            }

            _obstacles.RemoveAll(o => o == null);
        }

        private void SpawnObstacle()
        {
            var obj = new GameObject("Rock");
            float y = SurferBaseY + Random.Range(-0.5f, 0.5f);
            obj.transform.position = new Vector3(6f, y, 0f);
            obj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;

            var obstacle = obj.AddComponent<Obstacle>();
            obstacle.Initialize(_rockSprite, ObstacleSpeed);
            _obstacles.Add(obstacle);
        }

        private void CheckCollision()
        {
            if (_surferTransform == null || _isJumping) return;

            Vector2 surferPos = _surferTransform.position;
            foreach (var obs in _obstacles)
            {
                if (obs == null) continue;
                float dist = Vector2.Distance(surferPos, obs.transform.position);
                if (dist < HitRadius + 0.3f)
                {
                    _gameManager.OnCrash();
                    return;
                }
            }
        }

        private void CheckBalance()
        {
            if (_balance <= 2f || _balance >= 98f)
            {
                _gameManager.OnCrash();
            }
        }

        public float Balance => _balance;
    }
}
