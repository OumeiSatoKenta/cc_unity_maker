using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game040_DashDungeon
{
    public class DungeonManager : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private GameObject _exitPrefab;
        [SerializeField] private float _dashSpeed = 12f;
        [SerializeField] private float _cellSize = 1.0f;

        private GameObject _playerObj;
        private Vector2 _playerPos;
        private Vector2Int _dashDir; // current dash direction
        private bool _isDashing;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();
        private bool[,] _wallGrid;
        private Vector2Int _exitCell;
        private int _gridW = 9, _gridH = 9;
        private bool _isRunning;

        private DashDungeonGameManager _gameManager;

        private void Awake() { _gameManager = GetComponentInParent<DashDungeonGameManager>(); }

        private void Update()
        {
            if (!_isRunning) return;
            HandleInput();
            if (_isDashing) UpdateDash();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame && !_isDashing)
            {
                // Rotate direction 90 degrees clockwise
                if (_dashDir == Vector2Int.right) _dashDir = Vector2Int.down;
                else if (_dashDir == Vector2Int.down) _dashDir = Vector2Int.left;
                else if (_dashDir == Vector2Int.left) _dashDir = Vector2Int.up;
                else _dashDir = Vector2Int.right;
                _isDashing = true;
            }
        }

        private void UpdateDash()
        {
            if (_playerObj == null) return;
            Vector2 target = _playerPos + (Vector2)_dashDir * _dashSpeed * Time.deltaTime;

            // Check wall collision
            Vector2Int gridPos = WorldToGrid(target);
            if (gridPos.x < 0 || gridPos.x >= _gridW || gridPos.y < 0 || gridPos.y >= _gridH || _wallGrid[gridPos.x, gridPos.y])
            {
                // Stop at wall
                _isDashing = false;
                // Snap to grid
                _playerPos = GridToWorld(WorldToGrid(_playerPos));
                _playerObj.transform.position = _playerPos;
                return;
            }

            _playerPos = target;
            _playerObj.transform.position = _playerPos;

            // Check exit
            if (WorldToGrid(_playerPos) == _exitCell)
            {
                _isRunning = false;
                if (_gameManager != null) _gameManager.OnLevelComplete();
            }

            // Check coins
            foreach (var obj in _stageObjects)
            {
                if (obj == null || !obj.activeSelf) continue;
                if (obj.CompareTag("Untagged") && obj.name.StartsWith("Coin"))
                {
                    if (Vector2.Distance(_playerPos, obj.transform.position) < 0.4f)
                    {
                        obj.SetActive(false);
                        if (_gameManager != null) _gameManager.OnCoinCollected();
                    }
                }
            }
        }

        private Vector2Int WorldToGrid(Vector2 worldPos)
        {
            float offsetX = (_gridW - 1) * _cellSize * 0.5f;
            float offsetY = (_gridH - 1) * _cellSize * 0.5f;
            int x = Mathf.RoundToInt((worldPos.x + offsetX) / _cellSize);
            int y = Mathf.RoundToInt((worldPos.y + offsetY) / _cellSize);
            return new Vector2Int(x, y);
        }

        private Vector2 GridToWorld(Vector2Int gridPos)
        {
            float offsetX = (_gridW - 1) * _cellSize * 0.5f;
            float offsetY = (_gridH - 1) * _cellSize * 0.5f;
            return new Vector2(gridPos.x * _cellSize - offsetX, gridPos.y * _cellSize - offsetY);
        }

        public void StartGame()
        {
            ClearAll();
            _gridW = 9; _gridH = 9;
            _wallGrid = new bool[_gridW, _gridH];
            _dashDir = Vector2Int.right;
            _isDashing = false;
            _isRunning = true;
            BuildLevel();
        }

        private void BuildLevel()
        {
            // Border walls
            for (int x = 0; x < _gridW; x++) { PlaceWall(x, 0); PlaceWall(x, _gridH - 1); }
            for (int y = 1; y < _gridH - 1; y++) { PlaceWall(0, y); PlaceWall(_gridW - 1, y); }
            // Some inner walls
            PlaceWall(3, 2); PlaceWall(3, 3); PlaceWall(3, 4);
            PlaceWall(5, 4); PlaceWall(5, 5); PlaceWall(5, 6);
            PlaceWall(2, 6); PlaceWall(6, 2);

            // Exit
            _exitCell = new Vector2Int(7, 7);
            if (_exitPrefab != null) { var e = Instantiate(_exitPrefab, transform); e.transform.position = GridToWorld(_exitCell); _stageObjects.Add(e); }

            // Coins
            Vector2Int[] coinPositions = { new Vector2Int(2, 2), new Vector2Int(4, 4), new Vector2Int(6, 6), new Vector2Int(2, 5), new Vector2Int(6, 3) };
            foreach (var cp in coinPositions)
            {
                if (_coinPrefab != null) { var c = Instantiate(_coinPrefab, transform); c.transform.position = GridToWorld(cp); c.name = $"Coin_{cp.x}_{cp.y}"; _stageObjects.Add(c); }
            }

            // Player
            _playerPos = GridToWorld(new Vector2Int(1, 1));
            if (_playerPrefab != null) { _playerObj = Instantiate(_playerPrefab, transform); _playerObj.transform.position = _playerPos; }
        }

        private void PlaceWall(int x, int y)
        {
            _wallGrid[x, y] = true;
            if (_wallPrefab != null) { var w = Instantiate(_wallPrefab, transform); w.transform.position = GridToWorld(new Vector2Int(x, y)); _stageObjects.Add(w); }
        }

        public void StopGame() { _isRunning = false; }
        private void ClearAll() { foreach (var o in _stageObjects) if (o != null) Destroy(o); _stageObjects.Clear(); if (_playerObj != null) { Destroy(_playerObj); _playerObj = null; } }
    }
}
