using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

namespace Game053_SlideBlitz
{
    public class SlideManager : MonoBehaviour
    {
        [SerializeField] private SlideBlitzGameManager _gameManager;

        private int _size;
        private int[,] _board;
        private GameObject[,] _tiles;
        private int _emptyR, _emptyC;
        private float _cellSize;
        private float _offsetX, _offsetY;
        private Sprite _tileSprite;
        private Camera _mainCamera;
        private List<GameObject> _allObjects = new List<GameObject>();

        public void GenerateBoard(int size)
        {
            _mainCamera = Camera.main;
            _tileSprite = Resources.Load<Sprite>("Sprites/Game053_SlideBlitz/tile");

            CleanUp();

            _size = size;
            _cellSize = Mathf.Min(6f / size, 1.5f);
            _offsetX = -(_size - 1) * _cellSize / 2f;
            _offsetY = (_size - 1) * _cellSize / 2f;

            _board = new int[_size, _size];
            _tiles = new GameObject[_size, _size];

            int num = 1;
            for (int r = 0; r < _size; r++)
                for (int c = 0; c < _size; c++)
                {
                    _board[r, c] = num;
                    num++;
                }
            _board[_size - 1, _size - 1] = 0;
            _emptyR = _size - 1;
            _emptyC = _size - 1;

            Shuffle(200 + size * 50);

            for (int r = 0; r < _size; r++)
                for (int c = 0; c < _size; c++)
                    if (_board[r, c] != 0)
                        _tiles[r, c] = CreateTile(r, c, _board[r, c]);
        }

        private GameObject CreateTile(int r, int c, int number)
        {
            float x = _offsetX + c * _cellSize;
            float y = _offsetY - r * _cellSize;
            var go = new GameObject("Tile_" + number);
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = Vector3.one * (_cellSize * 0.95f / 0.48f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _tileSprite;
            sr.sortingOrder = 2;
            float hue = (float)(number - 1) / (_size * _size) * 0.6f + 0.55f;
            sr.color = Color.HSVToRGB(hue % 1f, 0.5f, 1f);

            var textGo = new GameObject("Num");
            textGo.transform.SetParent(go.transform, false);
            textGo.transform.localPosition = Vector3.zero;
            var tmp = textGo.AddComponent<TextMeshPro>();
            tmp.text = number.ToString();
            tmp.fontSize = 4f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.sortingOrder = 3;

            _allObjects.Add(go);
            return go;
        }

        private void Shuffle(int moves)
        {
            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };
            int lastDir = -1;
            for (int i = 0; i < moves; i++)
            {
                var valid = new List<int>();
                for (int d = 0; d < 4; d++)
                {
                    if (d == (lastDir ^ 1) && lastDir >= 0) continue;
                    int nr = _emptyR + dr[d], nc = _emptyC + dc[d];
                    if (nr >= 0 && nr < _size && nc >= 0 && nc < _size)
                        valid.Add(d);
                }
                int dir = valid[Random.Range(0, valid.Count)];
                int sr = _emptyR + dr[dir], sc = _emptyC + dc[dir];
                _board[_emptyR, _emptyC] = _board[sr, sc];
                _board[sr, sc] = 0;
                _emptyR = sr; _emptyC = sc;
                lastDir = dir;
            }
        }

        private void CleanUp()
        {
            foreach (var o in _allObjects) if (o != null) Destroy(o);
            _allObjects.Clear();
        }

        private void Update()
        {
            if (Mouse.current == null) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));

                int col = Mathf.RoundToInt((wp.x - _offsetX) / _cellSize);
                int row = Mathf.RoundToInt((_offsetY - wp.y) / _cellSize);

                if (row < 0 || row >= _size || col < 0 || col >= _size) return;
                if (_board[row, col] == 0) return;

                if ((row == _emptyR && Mathf.Abs(col - _emptyC) == 1) ||
                    (col == _emptyC && Mathf.Abs(row - _emptyR) == 1))
                {
                    _board[_emptyR, _emptyC] = _board[row, col];
                    _tiles[_emptyR, _emptyC] = _tiles[row, col];
                    float tx = _offsetX + _emptyC * _cellSize;
                    float ty = _offsetY - _emptyR * _cellSize;
                    _tiles[_emptyR, _emptyC].transform.position = new Vector3(tx, ty, 0f);

                    _board[row, col] = 0;
                    _tiles[row, col] = null;
                    _emptyR = row; _emptyC = col;

                    if (_gameManager != null) _gameManager.OnTileMoved();
                }
            }
        }

        public bool CheckSolved()
        {
            int expected = 1;
            for (int r = 0; r < _size; r++)
                for (int c = 0; c < _size; c++)
                {
                    if (r == _size - 1 && c == _size - 1) return _board[r, c] == 0;
                    if (_board[r, c] != expected) return false;
                    expected++;
                }
            return true;
        }
    }
}
