using System.Collections.Generic;
using UnityEngine;

namespace Game003_GravitySwitch
{
    public enum GravityDirection { Up = 0, Down = 1, Left = 2, Right = 3 }

    /// <summary>
    /// 重力方向の管理、ボール移動シミュレーション、タイル生成を担当する。
    /// ボタンからは ApplyGravity(int) を呼び出す。
    /// </summary>
    public class GravityManager : MonoBehaviour
    {
        [SerializeField] private GravitySwitchGameManager _gameManager;
        [SerializeField] private float _cellSize = 1.0f;

        private int _moveCount;
        private Vector2Int _ballPos;
        private GameObject _ballObject;
        private int[,] _currentGrid;
        private int _rows, _cols;
        private int _currentLevel;
        private readonly List<GameObject> _tileObjects = new();

        public int CurrentLevel => _currentLevel;
        public static int LevelCount => _levels.Length;

        // グリッド値: 0=空マス, 1=壁, 2=ゴール
        private static readonly int[][,] _levels =
        {
            // Level 1 - ボール初期位置 (row=3, col=1) / 2手解答
            new int[,]
            {
                {1,1,1,1,1,1,1},
                {1,0,0,0,0,2,1},
                {1,0,0,1,0,0,1},
                {1,0,0,0,0,0,1},
                {1,0,1,0,0,0,1},
                {1,0,0,0,0,0,1},
                {1,1,1,1,1,1,1},
            },
            // Level 2 - ボール初期位置 (row=3, col=3) / 2手解答
            new int[,]
            {
                {1,1,1,1,1,1,1},
                {1,0,0,0,0,0,1},
                {1,1,0,0,1,0,1},
                {1,0,0,0,0,0,1},
                {1,0,1,0,0,2,1},
                {1,0,0,0,0,0,1},
                {1,1,1,1,1,1,1},
            },
            // Level 3 - ボール初期位置 (row=4, col=3) / 3手解答
            new int[,]
            {
                {1,1,1,1,1,1,1},
                {1,0,0,0,0,2,1},
                {1,0,1,0,0,0,1},
                {1,0,0,0,1,0,1},
                {1,0,0,0,0,0,1},
                {1,0,0,0,0,0,1},
                {1,1,1,1,1,1,1},
            },
        };

        private static readonly Vector2Int[] _ballStarts =
        {
            new(3, 1),
            new(3, 3),
            new(4, 3),
        };

        public void InitLevel(int level)
        {
            _currentLevel = level;
            _moveCount = 0;

            var src = _levels[level];
            _rows = src.GetLength(0);
            _cols = src.GetLength(1);
            _currentGrid = new int[_rows, _cols];
            for (int r = 0; r < _rows; r++)
                for (int c = 0; c < _cols; c++)
                    _currentGrid[r, c] = src[r, c];

            _ballPos = _ballStarts[level];

            BuildTiles();
            CreateBall();
            UpdateBallVisual();
        }

        private void BuildTiles()
        {
            foreach (var go in _tileObjects)
                if (go != null) Destroy(go);
            _tileObjects.Clear();

            var wallSprite = Resources.Load<Sprite>("Sprites/Game003_GravitySwitch/tile_wall");
            var goalSprite = Resources.Load<Sprite>("Sprites/Game003_GravitySwitch/tile_goal");

            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _cols; c++)
                {
                    int val = _currentGrid[r, c];
                    if (val == 0) continue; // 空マスはカメラ背景に任せる

                    var go = new GameObject($"Tile_{r}_{c}");
                    go.transform.SetParent(transform);
                    go.transform.position = GridToWorld(r, c);

                    var sr = go.AddComponent<SpriteRenderer>();

                    if (val == 1)
                    {
                        go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
                        sr.sortingOrder = 0;
                        if (wallSprite != null) sr.sprite = wallSprite;
                        else sr.color = new Color(0.27f, 0.22f, 0.18f);
                    }
                    else if (val == 2)
                    {
                        go.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                        sr.sortingOrder = 1;
                        if (goalSprite != null) sr.sprite = goalSprite;
                        else sr.color = new Color(1f, 0.84f, 0f);
                    }

                    var tileView = go.AddComponent<TileView>();
                    tileView.Init(r, c, val);
                    _tileObjects.Add(go);
                }
            }
        }

        private void CreateBall()
        {
            if (_ballObject != null) Destroy(_ballObject);

            _ballObject = new GameObject("Ball");
            _ballObject.transform.SetParent(transform);
            _ballObject.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

            var sr = _ballObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            var ballSprite = Resources.Load<Sprite>("Sprites/Game003_GravitySwitch/ball");
            if (ballSprite != null) sr.sprite = ballSprite;
            else sr.color = Color.white;
        }

        private Vector3 GridToWorld(int r, int c)
        {
            float offsetX = -(_cols - 1) * _cellSize * 0.5f;
            float offsetY = (_rows - 1) * _cellSize * 0.5f;
            return new Vector3(offsetX + c * _cellSize, offsetY - r * _cellSize, 0f);
        }

        private void UpdateBallVisual()
        {
            if (_ballObject == null) return;
            var pos = GridToWorld(_ballPos.x, _ballPos.y);
            _ballObject.transform.position = new Vector3(pos.x, pos.y, -0.5f);
        }

        /// <summary>
        /// 重力方向を適用してボールを滑らせる。
        /// 0=Up, 1=Down, 2=Left, 3=Right
        /// </summary>
        public void ApplyGravity(int dirInt)
        {
            var dir = (GravityDirection)dirInt;
            int dr = 0, dc = 0;
            switch (dir)
            {
                case GravityDirection.Up:    dr = -1; break;
                case GravityDirection.Down:  dr =  1; break;
                case GravityDirection.Left:  dc = -1; break;
                case GravityDirection.Right: dc =  1; break;
            }

            int r = _ballPos.x, c = _ballPos.y;
            bool moved = false;
            bool reachedGoal = false;

            while (true)
            {
                int nr = r + dr, nc = c + dc;
                if (nr < 0 || nr >= _rows || nc < 0 || nc >= _cols) break;
                if (_currentGrid[nr, nc] == 1) break;
                r = nr; c = nc;
                moved = true;
                if (_currentGrid[nr, nc] == 2) { reachedGoal = true; break; }
            }

            if (!moved) return;

            _ballPos = new Vector2Int(r, c);
            _moveCount++;
            UpdateBallVisual();

            _gameManager?.OnMoved(_moveCount);
            if (reachedGoal) _gameManager?.OnCleared(_moveCount);
        }

        public void ApplyGravityUp()    => ApplyGravity(0);
        public void ApplyGravityDown()  => ApplyGravity(1);
        public void ApplyGravityLeft()  => ApplyGravity(2);
        public void ApplyGravityRight() => ApplyGravity(3);
    }
}
