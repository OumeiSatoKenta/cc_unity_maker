using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game031_BounceKing
{
    public class BounceManager : MonoBehaviour
    {
        [SerializeField] private GameObject _ballPrefab;
        [SerializeField] private GameObject _paddlePrefab;
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private float _ballSpeed = 6f;
        [SerializeField] private int _blockRows = 4;
        [SerializeField] private int _blockCols = 7;

        private GameObject _ballObj;
        private GameObject _paddleObj;
        private Vector2 _ballVel;
        private readonly List<GameObject> _blocks = new List<GameObject>();
        private bool _isRunning;
        private int _totalBlocks;

        private BounceKingGameManager _gameManager;
        private Camera _mainCamera;

        private void Awake()
        {
            _gameManager = GetComponentInParent<BounceKingGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isRunning) return;
            HandleInput();
            MoveBall();
            CheckCollisions();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null || _paddleObj == null) return;

            if (mouse.leftButton.isPressed)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                float worldX = _mainCamera.ScreenToWorldPoint(sp).x;
                var pos = _paddleObj.transform.position;
                pos.x = Mathf.Clamp(worldX, -4.5f, 4.5f);
                _paddleObj.transform.position = pos;
            }
        }

        private void MoveBall()
        {
            if (_ballObj == null) return;
            Vector2 pos = _ballObj.transform.position;
            pos += _ballVel * Time.deltaTime;

            // Wall bounces
            if (pos.x < -5.5f || pos.x > 5.5f) _ballVel.x = -_ballVel.x;
            if (pos.y > 4.5f) _ballVel.y = -_ballVel.y;

            // Ball fell below
            if (pos.y < -5.5f)
            {
                _isRunning = false;
                if (_gameManager != null) _gameManager.OnBallLost();
                return;
            }

            pos.x = Mathf.Clamp(pos.x, -5.5f, 5.5f);
            _ballObj.transform.position = pos;
        }

        private void CheckCollisions()
        {
            if (_ballObj == null || _paddleObj == null) return;
            Vector2 ballPos = _ballObj.transform.position;

            // Paddle collision
            Vector2 paddlePos = _paddleObj.transform.position;
            if (_ballVel.y < 0 &&
                ballPos.y < paddlePos.y + 0.3f && ballPos.y > paddlePos.y - 0.3f &&
                Mathf.Abs(ballPos.x - paddlePos.x) < 1.5f)
            {
                _ballVel.y = Mathf.Abs(_ballVel.y);
                float offset = (ballPos.x - paddlePos.x) / 1.5f;
                _ballVel.x = offset * _ballSpeed * 0.8f;
                _ballVel = _ballVel.normalized * _ballSpeed;
            }

            // Block collisions
            for (int i = _blocks.Count - 1; i >= 0; i--)
            {
                if (_blocks[i] == null) { _blocks.RemoveAt(i); continue; }
                Vector2 bPos = _blocks[i].transform.position;
                if (Mathf.Abs(ballPos.x - bPos.x) < 0.8f && Mathf.Abs(ballPos.y - bPos.y) < 0.3f)
                {
                    Destroy(_blocks[i]);
                    _blocks.RemoveAt(i);
                    _ballVel.y = -_ballVel.y;
                    if (_gameManager != null) _gameManager.OnBlockDestroyed(_totalBlocks - _blocks.Count, _totalBlocks);
                    if (_blocks.Count == 0 && _gameManager != null) _gameManager.OnAllBlocksDestroyed();
                    break;
                }
            }
        }

        public void StartGame()
        {
            ClearAll();
            _isRunning = true;

            // Paddle
            if (_paddlePrefab != null)
            {
                _paddleObj = Instantiate(_paddlePrefab, transform);
                _paddleObj.transform.position = new Vector3(0, -4f, 0);
            }

            // Ball
            if (_ballPrefab != null)
            {
                _ballObj = Instantiate(_ballPrefab, transform);
                _ballObj.transform.position = new Vector3(0, -3.5f, 0);
                float angle = Random.Range(30f, 150f) * Mathf.Deg2Rad;
                _ballVel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _ballSpeed;
            }

            // Blocks
            _totalBlocks = _blockRows * _blockCols;
            float startX = -(_blockCols - 1) * 0.85f * 0.5f;
            Color[] colors = { new Color(0.9f,0.3f,0.2f), new Color(0.2f,0.7f,0.3f), new Color(0.2f,0.4f,0.9f), new Color(0.9f,0.7f,0.2f) };

            for (int row = 0; row < _blockRows; row++)
            {
                for (int col = 0; col < _blockCols; col++)
                {
                    if (_blockPrefab == null) continue;
                    var block = Instantiate(_blockPrefab, transform);
                    block.transform.position = new Vector3(startX + col * 0.85f, 3f - row * 0.45f, 0);
                    var sr = block.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = colors[row % colors.Length];
                    _blocks.Add(block);
                }
            }
        }

        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var b in _blocks) if (b != null) Destroy(b);
            _blocks.Clear();
            if (_ballObj != null) { Destroy(_ballObj); _ballObj = null; }
            if (_paddleObj != null) { Destroy(_paddleObj); _paddleObj = null; }
        }
    }
}
