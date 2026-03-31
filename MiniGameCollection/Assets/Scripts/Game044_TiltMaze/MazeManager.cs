using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game044_TiltMaze
{
    public class MazeManager : MonoBehaviour
    {
        [SerializeField] private TiltMazeGameManager _gameManager;

        private const float CellSize = 0.6f;
        private const float BallSpeed = 4f;

        private int[,] _grid;
        private int _gridW, _gridH;
        private Vector2Int _startPos;
        private Vector2Int _goalPos;
        private GameObject _ball;
        private Rigidbody2D _ballRb;
        private List<GameObject> _mazeObjects = new List<GameObject>();
        private Sprite _wallSprite, _floorSprite, _goalSprite, _holeSprite, _ballSprite;
        private Camera _mainCamera;

        // 0=floor, 1=wall, 2=hole, 3=goal, 4=start
        private static readonly int[][,] Stages = {
            new int[,] {
                {1,1,1,1,1,1,1,1},
                {1,4,0,0,1,0,0,1},
                {1,0,1,0,1,0,1,1},
                {1,0,1,0,0,0,0,1},
                {1,0,0,0,1,1,0,1},
                {1,1,1,0,0,2,0,1},
                {1,0,0,0,1,0,3,1},
                {1,1,1,1,1,1,1,1},
            },
            new int[,] {
                {1,1,1,1,1,1,1,1,1},
                {1,4,0,1,0,0,0,0,1},
                {1,0,0,1,0,1,1,0,1},
                {1,1,0,0,0,0,1,0,1},
                {1,0,0,1,1,0,0,0,1},
                {1,0,1,1,2,0,1,0,1},
                {1,0,0,0,0,1,0,3,1},
                {1,1,1,1,1,1,1,1,1},
            },
            new int[,] {
                {1,1,1,1,1,1,1,1,1,1},
                {1,4,0,0,1,0,0,0,0,1},
                {1,1,1,0,1,0,1,1,0,1},
                {1,0,0,0,0,0,0,1,0,1},
                {1,0,1,1,1,1,0,0,0,1},
                {1,0,0,2,0,1,0,1,0,1},
                {1,1,0,1,0,0,0,1,0,1},
                {1,0,0,1,0,1,2,1,0,1},
                {1,0,0,0,0,1,0,0,3,1},
                {1,1,1,1,1,1,1,1,1,1},
            },
        };

        public void GenerateStage(int stage)
        {
            _mainCamera = Camera.main;
            _wallSprite = Resources.Load<Sprite>("Sprites/Game044_TiltMaze/wall");
            _floorSprite = Resources.Load<Sprite>("Sprites/Game044_TiltMaze/floor");
            _goalSprite = Resources.Load<Sprite>("Sprites/Game044_TiltMaze/goal");
            _holeSprite = Resources.Load<Sprite>("Sprites/Game044_TiltMaze/hole");
            _ballSprite = Resources.Load<Sprite>("Sprites/Game044_TiltMaze/ball");

            CleanUp();

            int idx = (stage - 1) % Stages.Length;
            var stageData = Stages[idx];
            _gridH = stageData.GetLength(0);
            _gridW = stageData.GetLength(1);
            _grid = new int[_gridH, _gridW];

            float offsetX = -(_gridW - 1) * CellSize / 2f;
            float offsetY = (_gridH - 1) * CellSize / 2f;

            for (int r = 0; r < _gridH; r++)
            {
                for (int c = 0; c < _gridW; c++)
                {
                    _grid[r, c] = stageData[r, c];
                    float x = offsetX + c * CellSize;
                    float y = offsetY - r * CellSize;

                    int val = stageData[r, c];
                    Sprite spr = _floorSprite;
                    Color col = Color.white;
                    int order = 0;

                    if (val == 1)
                    {
                        spr = _wallSprite; order = 2;
                        var wallObj = CreateCell(x, y, spr, col, order);
                        var bc = wallObj.AddComponent<BoxCollider2D>();
                        bc.size = Vector2.one * 0.95f;
                    }
                    else
                    {
                        CreateCell(x, y, _floorSprite, Color.white, 0);
                        if (val == 2) CreateCell(x, y, _holeSprite, Color.white, 1);
                        else if (val == 3) { CreateCell(x, y, _goalSprite, Color.white, 1); _goalPos = new Vector2Int(c, r); }
                        else if (val == 4) _startPos = new Vector2Int(c, r);
                    }
                }
            }

            SpawnBall(offsetX + _startPos.x * CellSize, offsetY - _startPos.y * CellSize);
        }

        private void SpawnBall(float x, float y)
        {
            if (_ball != null) Destroy(_ball);
            _ball = new GameObject("Ball");
            _ball.transform.position = new Vector3(x, y, 0f);
            _ball.transform.localScale = Vector3.one * 0.7f;
            var sr = _ball.AddComponent<SpriteRenderer>();
            sr.sprite = _ballSprite;
            sr.sortingOrder = 10;
            _ballRb = _ball.AddComponent<Rigidbody2D>();
            _ballRb.gravityScale = 0f;
            _ballRb.linearDamping = 3f;
            _ballRb.freezeRotation = true;
            var cc = _ball.AddComponent<CircleCollider2D>();
            cc.radius = 0.35f;
        }

        public void ResetBall()
        {
            float offsetX = -(_gridW - 1) * CellSize / 2f;
            float offsetY = (_gridH - 1) * CellSize / 2f;
            SpawnBall(offsetX + _startPos.x * CellSize, offsetY - _startPos.y * CellSize);
        }

        private void CleanUp()
        {
            foreach (var o in _mazeObjects) if (o != null) Destroy(o);
            _mazeObjects.Clear();
            if (_ball != null) { Destroy(_ball); _ball = null; }
        }

        private GameObject CreateCell(float x, float y, Sprite sprite, Color color, int order)
        {
            var go = new GameObject("Cell");
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = Vector3.one * (CellSize / 0.32f) * 0.95f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            _mazeObjects.Add(go);
            return go;
        }

        private void FixedUpdate()
        {
            if (_gameManager == null || !_gameManager.IsPlaying) return;
            if (_ballRb == null) return;

            Vector2 input = Vector2.zero;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) input.x -= 1f;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) input.x += 1f;
                if (kb.upArrowKey.isPressed || kb.wKey.isPressed) input.y += 1f;
                if (kb.downArrowKey.isPressed || kb.sKey.isPressed) input.y -= 1f;
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
                Vector2 dir = ((Vector2)wp - (Vector2)_ball.transform.position).normalized;
                input += dir;
            }

            if (input.sqrMagnitude > 0.01f)
                _ballRb.AddForce(input.normalized * BallSpeed);

            CheckHoleAndGoal();
        }

        private void CheckHoleAndGoal()
        {
            if (_ball == null) return;
            float offsetX = -(_gridW - 1) * CellSize / 2f;
            float offsetY = (_gridH - 1) * CellSize / 2f;

            int col = Mathf.RoundToInt((_ball.transform.position.x - offsetX) / CellSize);
            int row = Mathf.RoundToInt((offsetY - _ball.transform.position.y) / CellSize);

            if (row < 0 || row >= _gridH || col < 0 || col >= _gridW) return;

            if (_grid[row, col] == 2)
            {
                if (_gameManager != null) _gameManager.OnFallInHole();
            }
            else if (_grid[row, col] == 3)
            {
                if (_gameManager != null) _gameManager.OnReachGoal();
            }
        }
    }
}
