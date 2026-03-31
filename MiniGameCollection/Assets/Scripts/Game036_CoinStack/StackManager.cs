using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game036_CoinStack
{
    public class StackManager : MonoBehaviour
    {
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private float _dropSpeed = 4f;
        [SerializeField] private float _swingSpeed = 3f;
        [SerializeField] private float _coinHeight = 0.25f;

        private GameObject _currentCoin;
        private readonly List<GameObject> _stackedCoins = new List<GameObject>();
        private float _swingX;
        private float _swingDir = 1f;
        private float _stackTop;
        private bool _isRunning;
        private bool _isDropping;

        private CoinStackGameManager _gameManager;

        private void Awake() { _gameManager = GetComponentInParent<CoinStackGameManager>(); }

        private void Update()
        {
            if (!_isRunning) return;
            if (_isDropping) DropCoin();
            else SwingCoin();
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame && !_isDropping)
                _isDropping = true;
        }

        private void SwingCoin()
        {
            if (_currentCoin == null) return;
            _swingX += _swingDir * _swingSpeed * Time.deltaTime;
            if (_swingX > 3f) { _swingX = 3f; _swingDir = -1f; }
            if (_swingX < -3f) { _swingX = -3f; _swingDir = 1f; }
            _currentCoin.transform.position = new Vector3(_swingX, 4f, 0);
        }

        private void DropCoin()
        {
            if (_currentCoin == null) return;
            var pos = _currentCoin.transform.position;
            pos.y -= _dropSpeed * Time.deltaTime;

            if (pos.y <= _stackTop + _coinHeight)
            {
                pos.y = _stackTop + _coinHeight;
                _currentCoin.transform.position = pos;

                // Check alignment
                float lastX = _stackedCoins.Count > 0 ? _stackedCoins[_stackedCoins.Count - 1].transform.position.x : 0f;
                float offset = Mathf.Abs(pos.x - lastX);

                if (offset > 1.2f)
                {
                    // Too far off - game over
                    _isRunning = false;
                    if (_gameManager != null) _gameManager.OnStackFall();
                    return;
                }

                _stackedCoins.Add(_currentCoin);
                _stackTop += _coinHeight;
                _isDropping = false;

                if (_gameManager != null) _gameManager.OnCoinStacked(_stackedCoins.Count, offset);

                SpawnNextCoin();

                // Increase speed
                _swingSpeed = 3f + _stackedCoins.Count * 0.15f;
            }
            else
            {
                _currentCoin.transform.position = pos;
            }
        }

        private void SpawnNextCoin()
        {
            if (_coinPrefab == null) return;
            _currentCoin = Instantiate(_coinPrefab, transform);
            _swingX = -3f;
            _swingDir = 1f;
            _currentCoin.transform.position = new Vector3(_swingX, 4f, 0);
        }

        public void StartGame()
        {
            ClearAll();
            _stackTop = -3.5f;
            _swingSpeed = 3f;
            _isDropping = false;
            _isRunning = true;
            SpawnNextCoin();
        }

        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var c in _stackedCoins) if (c != null) Destroy(c);
            _stackedCoins.Clear();
            if (_currentCoin != null) { Destroy(_currentCoin); _currentCoin = null; }
        }
    }
}
