using UnityEngine;
using UnityEngine.InputSystem;

namespace Game044_TiltMaze
{
    public class MazeManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private TiltMazeGameManager _gameManager;

        [SerializeField, Tooltip("ボールスプライト")]
        private Sprite _ballSprite;

        [SerializeField, Tooltip("壁スプライト")]
        private Sprite _wallSprite;

        [SerializeField, Tooltip("ゴールスプライト")]
        private Sprite _goalSprite;

        private Camera _mainCamera;
        private Transform _ball;
        private Rigidbody2D _ballRb;
        private Vector2Int _goalGridPos;

        private const float CellSize = 0.7f;
        private const float TiltForce = 8f;
        private const int W = 9;
        private const int H = 11;
        private static readonly Vector2 Offset = new Vector2(-2.8f, -3.5f);

        // 1=wall, 0=path, 2=start, 3=goal
        private static readonly int[,] Map = {
            {1,1,1,1,1,1,1,1,1},
            {1,2,0,0,1,0,0,0,1},
            {1,0,1,0,1,0,1,0,1},
            {1,0,1,0,0,0,1,0,1},
            {1,0,0,0,1,1,1,0,1},
            {1,1,1,0,0,0,0,0,1},
            {1,0,0,0,1,0,1,0,1},
            {1,0,1,1,1,0,1,0,1},
            {1,0,0,0,0,0,0,0,1},
            {1,0,1,0,1,1,0,3,1},
            {1,1,1,1,1,1,1,1,1},
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            BuildMaze();
            SpawnBall();
        }

        private void BuildMaze()
        {
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    int cell = Map[y, x];
                    Vector3 pos = GridToWorld(x, y);

                    if (cell == 1)
                    {
                        var obj = new GameObject($"Wall_{x}_{y}");
                        obj.transform.position = pos;
                        obj.transform.localScale = new Vector3(CellSize, CellSize, 1f);
                        var sr = obj.AddComponent<SpriteRenderer>();
                        sr.sprite = _wallSprite; sr.sortingOrder = 1;
                        var col = obj.AddComponent<BoxCollider2D>();
                        col.size = Vector2.one;
                        var rb = obj.AddComponent<Rigidbody2D>();
                        rb.bodyType = RigidbodyType2D.Static;
                        obj.transform.SetParent(transform);
                    }
                    else if (cell == 3)
                    {
                        var obj = new GameObject("Goal");
                        obj.transform.position = pos;
                        obj.transform.localScale = new Vector3(CellSize * 0.8f, CellSize * 0.8f, 1f);
                        var sr = obj.AddComponent<SpriteRenderer>();
                        sr.sprite = _goalSprite; sr.sortingOrder = 1;
                        obj.transform.SetParent(transform);
                        _goalGridPos = new Vector2Int(x, y);
                    }
                }
            }
        }

        private void SpawnBall()
        {
            // スタート位置を見つける
            Vector3 startPos = Vector3.zero;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (Map[y, x] == 2) startPos = GridToWorld(x, y);

            var ballObj = new GameObject("Ball");
            ballObj.transform.position = startPos;
            ballObj.transform.localScale = new Vector3(CellSize * 0.6f, CellSize * 0.6f, 1f);
            var bsr = ballObj.AddComponent<SpriteRenderer>();
            bsr.sprite = _ballSprite; bsr.sortingOrder = 5;

            _ballRb = ballObj.AddComponent<Rigidbody2D>();
            _ballRb.gravityScale = 0f;
            _ballRb.linearDamping = 3f;
            _ballRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = ballObj.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            _ball = ballObj.transform;
        }

        private void FixedUpdate()
        {
            if (!_gameManager.IsPlaying || _ballRb == null) return;

            // マウス位置で傾きをシミュレート（画面中央からの距離）
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector3 wp = _mainCamera.ScreenToWorldPoint(mp);
                Vector2 dir = ((Vector2)wp - (Vector2)_ball.position).normalized;
                _ballRb.AddForce(dir * TiltForce);
            }

            // ゴール判定
            Vector3 goalWorld = GridToWorld(_goalGridPos.x, _goalGridPos.y);
            if (Vector2.Distance(_ball.position, goalWorld) < CellSize * 0.5f)
            {
                _gameManager.OnReachGoal();
            }
        }

        private Vector3 GridToWorld(int x, int y)
        {
            return new Vector3(Offset.x + x * CellSize, Offset.y + (H - 1 - y) * CellSize, 0f);
        }
    }
}
