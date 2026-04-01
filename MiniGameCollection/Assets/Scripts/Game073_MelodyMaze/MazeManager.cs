using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game073_MelodyMaze
{
    public class MazeManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private MelodyMazeGameManager _gameManager;
        [SerializeField, Tooltip("壁スプライト")] private Sprite _wallSprite;
        [SerializeField, Tooltip("パススプライト")] private Sprite _pathSprite;
        [SerializeField, Tooltip("ノートスプライト")] private Sprite _noteSprite;
        [SerializeField, Tooltip("プレイヤースプライト")] private Sprite _playerSprite;
        [SerializeField, Tooltip("ゴールスプライト")] private Sprite _goalSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int _gridSize = 7;
        private float _cellSize = 0.9f;
        private int[,] _maze; // 0=wall, 1=path, 2=note, 3=goal
        private int _playerR, _playerC;
        private GameObject _playerObj;
        private Vector2 _gridOrigin;
        private GameObject[,] _cellObjects;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            GenerateMaze();
            RenderMaze();
        }

        public void StopGame() { _isActive = false; }

        private void GenerateMaze()
        {
            _maze = new int[_gridSize, _gridSize];
            // Simple maze: paths in odd rows/cols
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    _maze[r, c] = (r % 2 == 1 && c % 2 == 1) ? 1 : 0;

            // Connect paths
            for (int r = 1; r < _gridSize; r += 2)
                for (int c = 1; c < _gridSize - 2; c += 2)
                    _maze[r, c + 1] = 1;
            for (int c = 1; c < _gridSize; c += 2)
                for (int r = 1; r < _gridSize - 2; r += 2)
                    if (Random.value > 0.4f) _maze[r + 1, c] = 1;

            // Place notes on some path cells
            int notesPlaced = 0;
            for (int r = 1; r < _gridSize; r += 2)
                for (int c = 1; c < _gridSize; c += 2)
                    if (_maze[r, c] == 1 && Random.value < 0.4f && notesPlaced < 6)
                    { _maze[r, c] = 2; notesPlaced++; }

            // Start and goal
            _playerR = 1; _playerC = 1;
            _maze[_playerR, _playerC] = 1;
            _maze[_gridSize - 2, _gridSize - 2] = 3; // goal
        }

        private void RenderMaze()
        {
            float totalSize = _gridSize * _cellSize;
            _gridOrigin = new Vector2(-totalSize / 2f + _cellSize / 2f, totalSize / 2f - _cellSize / 2f);
            _cellObjects = new GameObject[_gridSize, _gridSize];

            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                {
                    Vector3 pos = CellToWorld(r, c);
                    var obj = new GameObject($"Cell_{r}_{c}");
                    obj.transform.position = pos;
                    var sr = obj.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 1;

                    switch (_maze[r, c])
                    {
                        case 0: sr.sprite = _wallSprite; break;
                        case 1: sr.sprite = _pathSprite; break;
                        case 2: sr.sprite = _noteSprite; sr.color = Color.yellow; break;
                        case 3: sr.sprite = _goalSprite; break;
                    }

                    obj.transform.localScale = Vector3.one * (_cellSize * 0.03f);
                    _cellObjects[r, c] = obj;
                }

            _playerObj = new GameObject("Player");
            _playerObj.transform.position = CellToWorld(_playerR, _playerC);
            var psr = _playerObj.AddComponent<SpriteRenderer>();
            psr.sprite = _playerSprite; psr.sortingOrder = 5;
            _playerObj.transform.localScale = Vector3.one * (_cellSize * 0.03f);
        }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                // Determine direction from tap relative to player
                Vector2 playerPos = CellToWorld(_playerR, _playerC);
                Vector2 diff = wp - playerPos;

                int dr = 0, dc = 0;
                if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
                    dc = diff.x > 0 ? 1 : -1;
                else
                    dr = diff.y > 0 ? -1 : 1;

                TryMove(dr, dc);
            }
        }

        private void TryMove(int dr, int dc)
        {
            int nr = _playerR + dr;
            int nc = _playerC + dc;

            if (nr < 0 || nr >= _gridSize || nc < 0 || nc >= _gridSize) return;
            if (_maze[nr, nc] == 0) return;

            _playerR = nr; _playerC = nc;
            _playerObj.transform.position = CellToWorld(_playerR, _playerC);
            _gameManager.OnPlayerMoved();

            if (_maze[nr, nc] == 2)
            {
                _maze[nr, nc] = 1;
                if (_cellObjects[nr, nc] != null)
                {
                    var sr = _cellObjects[nr, nc].GetComponent<SpriteRenderer>();
                    sr.sprite = _pathSprite; sr.color = Color.white;
                }
                _gameManager.OnNoteCollected();
            }
            else if (_maze[nr, nc] == 3)
            {
                _gameManager.OnReachedGoal();
            }
        }

        private Vector3 CellToWorld(int r, int c)
        {
            return new Vector3(_gridOrigin.x + c * _cellSize, _gridOrigin.y - r * _cellSize, 0f);
        }
    }
}
