using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game053_SlideBlitz
{
    public class PuzzleManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private SlideBlitzGameManager _gameManager;
        [SerializeField, Tooltip("タイルスプライト")] private Sprite _tileSprite;
        [SerializeField, Tooltip("グリッドサイズ")] private int _gridSize = 4;

        private Camera _mainCamera;
        private bool _isActive;
        private int[,] _grid;
        private GameObject[,] _tiles;
        private int _emptyR, _emptyC;
        private float _tileSize = 1.1f;
        private Vector2 _gridOrigin;

        private static readonly Color[] TileColors = {
            new Color(0.3f, 0.6f, 0.9f), new Color(0.4f, 0.7f, 0.4f),
            new Color(0.9f, 0.5f, 0.3f), new Color(0.7f, 0.4f, 0.8f),
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            int n = _gridSize;
            _grid = new int[n, n];
            _tiles = new GameObject[n, n];

            float totalSize = n * _tileSize;
            _gridOrigin = new Vector2(-totalSize / 2f + _tileSize / 2f, totalSize / 2f - _tileSize / 2f);

            // Initialize sorted
            int val = 1;
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    _grid[r, c] = val;
                    val++;
                }
            _grid[n - 1, n - 1] = 0;
            _emptyR = n - 1; _emptyC = n - 1;

            // Shuffle with valid moves
            Shuffle(100);

            // Create tile objects
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    if (_grid[r, c] != 0)
                        CreateTile(r, c, _grid[r, c]);
        }

        public void StopGame() { _isActive = false; }

        private void Shuffle(int moves)
        {
            int n = _gridSize;
            int lastDir = -1;
            for (int i = 0; i < moves; i++)
            {
                var dirs = new List<int>();
                if (_emptyR > 0 && lastDir != 2) dirs.Add(0); // up
                if (_emptyR < n - 1 && lastDir != 0) dirs.Add(2); // down
                if (_emptyC > 0 && lastDir != 3) dirs.Add(1); // left
                if (_emptyC < n - 1 && lastDir != 1) dirs.Add(3); // right

                int dir = dirs[Random.Range(0, dirs.Count)];
                int nr = _emptyR + (dir == 0 ? -1 : dir == 2 ? 1 : 0);
                int nc = _emptyC + (dir == 1 ? -1 : dir == 3 ? 1 : 0);

                _grid[_emptyR, _emptyC] = _grid[nr, nc];
                _grid[nr, nc] = 0;
                _emptyR = nr; _emptyC = nc;
                lastDir = dir;
            }
        }

        private void CreateTile(int r, int c, int val)
        {
            var obj = new GameObject($"Tile_{val}");
            obj.transform.position = GridToWorld(r, c);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _tileSprite;
            sr.sortingOrder = 2;
            sr.color = TileColors[(val - 1) % TileColors.Length];

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            // Number label via child TextMesh
            var textObj = new GameObject("Num");
            textObj.transform.SetParent(obj.transform);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            var tm = textObj.AddComponent<TextMesh>();
            tm.text = val.ToString();
            tm.fontSize = 48;
            tm.characterSize = 0.15f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;

            _tiles[r, c] = obj;
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

                var hit = Physics2D.OverlapPoint(wp);
                if (hit != null)
                {
                    // Find which tile was clicked
                    for (int r = 0; r < _gridSize; r++)
                        for (int c = 0; c < _gridSize; c++)
                            if (_tiles[r, c] == hit.gameObject)
                            {
                                TryMove(r, c);
                                return;
                            }
                }
            }
        }

        private void TryMove(int r, int c)
        {
            if (Mathf.Abs(r - _emptyR) + Mathf.Abs(c - _emptyC) != 1) return;

            // Swap in grid
            _grid[_emptyR, _emptyC] = _grid[r, c];
            _grid[r, c] = 0;
            _tiles[_emptyR, _emptyC] = _tiles[r, c];
            _tiles[r, c] = null;

            // Move tile visually
            _tiles[_emptyR, _emptyC].transform.position = GridToWorld(_emptyR, _emptyC);
            _emptyR = r; _emptyC = c;

            _gameManager.OnTileMoved();
        }

        private Vector3 GridToWorld(int r, int c)
        {
            return new Vector3(_gridOrigin.x + c * _tileSize, _gridOrigin.y - r * _tileSize, 0f);
        }

        public bool IsSolved
        {
            get
            {
                int val = 1;
                int n = _gridSize;
                for (int r = 0; r < n; r++)
                    for (int c = 0; c < n; c++)
                    {
                        if (r == n - 1 && c == n - 1) return _grid[r, c] == 0;
                        if (_grid[r, c] != val) return false;
                        val++;
                    }
                return true;
            }
        }
    }
}
