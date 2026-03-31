using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game049_CloudHop
{
    public class CloudManager : MonoBehaviour
    {
        [SerializeField] private CloudHopGameManager _gameManager;

        private const float JumpForce = 8f;
        private const float MoveSpeed = 5f;
        private const float CloudSpawnInterval = 1.5f;
        private const float CloudLifeTime = 3f;
        private const float CloudFadeTime = 0.8f;

        private GameObject _player;
        private Rigidbody2D _playerRb;
        private List<GameObject> _clouds = new List<GameObject>();
        private List<float> _cloudTimers = new List<float>();
        private Sprite _cloudSprite, _playerSprite;
        private Camera _mainCamera;
        private float _spawnTimer;
        private float _highestY;
        private bool _grounded;

        public void Init()
        {
            _mainCamera = Camera.main;
            _cloudSprite = Resources.Load<Sprite>("Sprites/Game049_CloudHop/cloud");
            _playerSprite = Resources.Load<Sprite>("Sprites/Game049_CloudHop/player");

            CleanUp();

            _highestY = 0f;
            _spawnTimer = 0f;
            _grounded = false;

            // Initial clouds
            for (int i = 0; i < 5; i++)
            {
                float x = Random.Range(-3f, 3f);
                float y = -2f + i * 2f;
                SpawnCloud(x, y, CloudLifeTime + i);
            }

            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            if (_player != null) Destroy(_player);
            _player = new GameObject("Player");
            _player.transform.position = new Vector3(0f, -1f, 0f);
            _player.transform.localScale = Vector3.one * 0.8f;
            var sr = _player.AddComponent<SpriteRenderer>();
            sr.sprite = _playerSprite;
            sr.sortingOrder = 10;
            _playerRb = _player.AddComponent<Rigidbody2D>();
            _playerRb.gravityScale = 2f;
            _playerRb.freezeRotation = true;
            var cc = _player.AddComponent<BoxCollider2D>();
            cc.size = new Vector2(0.5f, 0.8f);
        }

        private void SpawnCloud(float x, float y, float lifeTime)
        {
            var go = new GameObject("Cloud");
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(1.5f, 0.6f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _cloudSprite;
            sr.sortingOrder = 2;
            var bc = go.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(0.8f, 0.3f);
            bc.offset = new Vector2(0f, 0.1f);
            _clouds.Add(go);
            _cloudTimers.Add(lifeTime);
        }

        private void CleanUp()
        {
            if (_player != null) { Destroy(_player); _player = null; }
            foreach (var c in _clouds) if (c != null) Destroy(c);
            _clouds.Clear();
            _cloudTimers.Clear();
        }

        private void Update()
        {
            if (_gameManager != null && _gameManager.IsGameOver) return;
            if (_player == null) return;

            HandleInput();
            UpdateClouds();
            CheckGrounded();
            AdjustCamera();
            CheckFall();

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= CloudSpawnInterval)
            {
                _spawnTimer = 0f;
                float x = Random.Range(-3.5f, 3.5f);
                float y = _highestY + Random.Range(2f, 4f);
                SpawnCloud(x, y, CloudLifeTime);
                _highestY = Mathf.Max(_highestY, y);
            }
        }

        private void HandleInput()
        {
            float moveX = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) moveX -= 1f;
                if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) moveX += 1f;
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                var screenPos = Mouse.current.position.ReadValue();
                if (screenPos.x < Screen.width * 0.4f) moveX -= 1f;
                else if (screenPos.x > Screen.width * 0.6f) moveX += 1f;
            }

            var vel = _playerRb.linearVelocity;
            vel.x = moveX * MoveSpeed;
            _playerRb.linearVelocity = vel;

            bool jumpInput = false;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) jumpInput = true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) jumpInput = true;

            if (jumpInput && _grounded)
            {
                _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, JumpForce);
                _grounded = false;
            }
        }

        private void CheckGrounded()
        {
            if (_playerRb.linearVelocity.y > 0.1f) { _grounded = false; return; }

            Vector2 pos = _player.transform.position;
            foreach (var cloud in _clouds)
            {
                if (cloud == null) continue;
                Vector2 cPos = cloud.transform.position;
                float halfW = cloud.transform.localScale.x * 0.4f;
                if (Mathf.Abs(pos.x - cPos.x) < halfW && pos.y > cPos.y - 0.1f && pos.y < cPos.y + 0.5f && _playerRb.linearVelocity.y <= 0.1f)
                {
                    if (!_grounded && _gameManager != null) _gameManager.OnCloudLanded();
                    _grounded = true;
                    return;
                }
            }
            _grounded = false;
        }

        private void UpdateClouds()
        {
            for (int i = _cloudTimers.Count - 1; i >= 0; i--)
            {
                _cloudTimers[i] -= Time.deltaTime;
                if (_clouds[i] != null)
                {
                    var sr = _clouds[i].GetComponent<SpriteRenderer>();
                    if (_cloudTimers[i] < CloudFadeTime)
                    {
                        float alpha = Mathf.Max(0f, _cloudTimers[i] / CloudFadeTime);
                        sr.color = new Color(1f, 1f, 1f, alpha);
                    }
                }
                if (_cloudTimers[i] <= 0f)
                {
                    if (_clouds[i] != null) Destroy(_clouds[i]);
                    _clouds.RemoveAt(i);
                    _cloudTimers.RemoveAt(i);
                }
            }
        }

        private void AdjustCamera()
        {
            if (_mainCamera == null || _player == null) return;
            float targetY = Mathf.Max(0f, _player.transform.position.y - 1f);
            var pos = _mainCamera.transform.position;
            _mainCamera.transform.position = new Vector3(pos.x, Mathf.Lerp(pos.y, targetY, 0.1f), pos.z);
        }

        private void CheckFall()
        {
            if (_player == null) return;
            float cameraBottom = _mainCamera.transform.position.y - _mainCamera.orthographicSize - 2f;
            if (_player.transform.position.y < cameraBottom)
            {
                if (_gameManager != null) _gameManager.OnGameOver();
            }
        }
    }
}
