using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game077_BeatRunner
{
    public class RunManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private BeatRunnerGameManager _gameManager;
        [SerializeField, Tooltip("ランナースプライト")] private Sprite _runnerSprite;
        [SerializeField, Tooltip("障害物スプライト")] private Sprite _obstacleSprite;
        [SerializeField, Tooltip("ビートマーカースプライト")] private Sprite _beatSprite;
        [SerializeField, Tooltip("走行速度")] private float _runSpeed = 4f;
        [SerializeField, Tooltip("ジャンプ力")] private float _jumpForce = 8f;
        [SerializeField, Tooltip("ビート間隔")] private float _beatInterval = 0.8f;

        private bool _isActive;
        private GameObject _runner;
        private Rigidbody2D _runnerRb;
        private float _groundY = -3f;
        private float _beatTimer;
        private float _scrollOffset;
        private List<GameObject> _obstacles = new List<GameObject>();
        private List<GameObject> _beatMarkers = new List<GameObject>();
        private bool _isGrounded;

        public void StartGame()
        {
            _isActive = true;
            _beatTimer = _beatInterval;
            _scrollOffset = 0f;

            // Create runner
            _runner = new GameObject("Runner");
            _runner.transform.position = new Vector3(-3f, _groundY + 0.5f, 0f);
            var sr = _runner.AddComponent<SpriteRenderer>();
            sr.sprite = _runnerSprite; sr.sortingOrder = 5;
            _runner.transform.localScale = Vector3.one * 0.8f;
            _runnerRb = _runner.AddComponent<Rigidbody2D>();
            _runnerRb.gravityScale = 3f;
            _runnerRb.freezeRotation = true;
            var col = _runner.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.3f, 0.4f);

            // Ground collider
            var ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0f, _groundY, 0f);
            var gCol = ground.AddComponent<BoxCollider2D>();
            gCol.size = new Vector2(20f, 0.2f);
            ground.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

            // Pre-spawn obstacles
            for (int i = 0; i < 10; i++)
            {
                SpawnObstacle(6f + i * 4f);
            }
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Scroll world
            _scrollOffset += _runSpeed * Time.deltaTime;

            // Move obstacles left
            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                if (_obstacles[i] == null) { _obstacles.RemoveAt(i); continue; }
                _obstacles[i].transform.position += Vector3.left * _runSpeed * Time.deltaTime;
                if (_obstacles[i].transform.position.x < -8f)
                {
                    Destroy(_obstacles[i]);
                    _obstacles.RemoveAt(i);
                    SpawnObstacle(10f);
                }
            }

            // Ground check
            _isGrounded = _runner.transform.position.y < _groundY + 0.7f;

            // Beat timer
            _beatTimer -= Time.deltaTime;
            if (_beatTimer <= 0f)
            {
                _beatTimer = _beatInterval;
                // Visual beat pulse (on runner)
                _runner.transform.localScale = Vector3.one * 0.9f;
                Invoke(nameof(ResetScale), 0.1f);
            }

            // Input
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (_isGrounded)
                {
                    _runnerRb.linearVelocity = new Vector2(0f, _jumpForce);

                    // Check if jump is on beat
                    float beatPhase = _beatTimer / _beatInterval;
                    if (beatPhase > 0.8f || beatPhase < 0.2f)
                        _gameManager.OnBeatHit();
                    else
                        _gameManager.OnBeatMiss();
                }
            }

            // Check obstacle collision
            CheckObstacleCollision();
        }

        private void ResetScale()
        {
            if (_runner != null) _runner.transform.localScale = Vector3.one * 0.8f;
        }

        private void SpawnObstacle(float x)
        {
            var obj = new GameObject("Obstacle");
            obj.transform.position = new Vector3(x, _groundY + 0.35f, 0f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _obstacleSprite; sr.sortingOrder = 3;
            obj.transform.localScale = Vector3.one * 0.6f;
            _obstacles.Add(obj);
        }

        private void CheckObstacleCollision()
        {
            if (_runner == null) return;
            foreach (var obs in _obstacles)
            {
                if (obs == null) continue;
                float dist = Vector2.Distance(_runner.transform.position, obs.transform.position);
                if (dist < 0.5f && _runner.transform.position.y < _groundY + 1f)
                {
                    obs.transform.position = new Vector3(-10f, -10f, 0f); // move away
                    _gameManager.OnObstacleHit();
                    break;
                }
            }
        }
    }
}
