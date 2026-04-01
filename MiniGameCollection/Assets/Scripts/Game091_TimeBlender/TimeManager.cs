using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game091_TimeBlender
{
    public class TimeManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private TimeBlenderGameManager _gameManager;
        [SerializeField, Tooltip("ブロックスプライト")] private Sprite _blockSprite;
        [SerializeField, Tooltip("木スプライト")] private Sprite _treeSprite;
        [SerializeField, Tooltip("プレイヤースプライト")] private Sprite _playerSprite;
        [SerializeField, Tooltip("ゴールスプライト")] private Sprite _goalSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private bool _isPresent = true;
        private int _gridSize = 5;
        private float _cellSize = 1.2f;
        private int _playerR, _playerC;
        private int _goalR, _goalC;
        private GameObject _playerObj;
        private GameObject _goalObj;
        private List<GameObject> _blocks = new List<GameObject>();
        private Vector2 _gridOrigin;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            SetupPuzzle();
        }

        public void StopGame() { _isActive = false; }
        public void NextPuzzle() { ClearPuzzle(); SetupPuzzle(); }

        private void SetupPuzzle()
        {
            float totalSize = _gridSize * _cellSize;
            _gridOrigin = new Vector2(-totalSize / 2f + _cellSize / 2f, totalSize / 2f - _cellSize / 2f - 0.5f);

            _playerR = 0; _playerC = 0;
            _goalR = _gridSize - 1; _goalC = _gridSize - 1;

            _playerObj = CreateObj("Player", _playerSprite, _playerR, _playerC, 5, Color.white);
            _goalObj = CreateObj("Goal", _goalSprite, _goalR, _goalC, 2, Color.white);

            // Place some blocks (present only)
            for (int i = 0; i < 4; i++)
            {
                int r = Random.Range(1, _gridSize - 1);
                int c = Random.Range(1, _gridSize - 1);
                var obj = CreateObj($"Block_{i}", _blockSprite, r, c, 1, Color.white);
                _blocks.Add(obj);
            }

            _isPresent = true;
        }

        private void ClearPuzzle()
        {
            if (_playerObj != null) Destroy(_playerObj);
            if (_goalObj != null) Destroy(_goalObj);
            foreach (var b in _blocks) if (b != null) Destroy(b);
            _blocks.Clear();
        }

        private GameObject CreateObj(string name, Sprite sprite, int r, int c, int order, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.position = CellToWorld(r, c);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite; sr.sortingOrder = order; sr.color = color;
            obj.transform.localScale = Vector3.one * 0.4f;
            return obj;
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

        public void ToggleTime()
        {
            if (!_isActive) return;
            _isPresent = !_isPresent;
            _gameManager.OnEraChanged(_isPresent);

            // Toggle block visibility (blocks only in present)
            foreach (var b in _blocks)
            {
                if (b != null)
                {
                    var sr = b.GetComponent<SpriteRenderer>();
                    sr.color = _isPresent ? Color.white : new Color(1f, 1f, 1f, 0.2f);
                }
            }

            // Camera tint
            if (_mainCamera != null)
                _mainCamera.backgroundColor = _isPresent
                    ? new Color(0.16f, 0.14f, 0.24f)
                    : new Color(0.24f, 0.16f, 0.14f);
        }

        private void TryMove(int dr, int dc)
        {
            int nr = _playerR + dr;
            int nc = _playerC + dc;
            if (nr < 0 || nr >= _gridSize || nc < 0 || nc >= _gridSize) return;

            // Check block collision (only in present era)
            if (_isPresent)
            {
                Vector3 targetPos = CellToWorld(nr, nc);
                foreach (var b in _blocks)
                {
                    if (b != null && Vector2.Distance(b.transform.position, targetPos) < 0.3f)
                        return;
                }
            }

            _playerR = nr; _playerC = nc;
            _playerObj.transform.position = CellToWorld(_playerR, _playerC);

            if (_playerR == _goalR && _playerC == _goalC)
                _gameManager.OnPuzzleSolved();
        }

        private Vector3 CellToWorld(int r, int c)
        {
            return new Vector3(_gridOrigin.x + c * _cellSize, _gridOrigin.y - r * _cellSize, 0f);
        }

        public bool IsPresent => _isPresent;
    }
}
