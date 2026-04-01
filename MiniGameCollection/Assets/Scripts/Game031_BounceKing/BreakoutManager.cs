using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game031_BounceKing
{
    public class BreakoutManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理コンポーネント")]
        private BounceKingGameManager _gameManager;

        [SerializeField, Tooltip("パドルのTransform")]
        private Transform _paddleTransform;

        [SerializeField, Tooltip("ボールのスプライト")]
        private Sprite _ballSprite;

        [SerializeField, Tooltip("ブロックスプライト配列 [0]=赤,[1]=橙,[2]=黄,[3]=緑,[4]=青")]
        private Sprite[] _blockSprites;

        [SerializeField, Tooltip("ボール用バウンスPhysicsMaterial2D")]
        private PhysicsMaterial2D _bouncyMaterial;

        // ステージ設定
        private const int Cols = 8;
        private const int Rows = 5;
        private const float BlockWidth = 0.88f;
        private const float BlockHeight = 0.35f;
        private const float BlockSpacingX = 0.92f;
        private const float BlockSpacingY = 0.40f;
        private const float BlockStartX = -3.22f;
        private const float BlockStartY = 2.8f;

        private const float PaddleMinX = -3.6f;
        private const float PaddleMaxX = 3.6f;
        private const float PaddleY = -4.2f;
        private const float BallStartOffsetY = 0.4f;
        private const float BallOutY = -5.8f;

        private List<Block> _blocks = new List<Block>();
        private BallController _ball;
        private Camera _mainCamera;
        private bool _isPlaying;
        private bool _waitingForLaunch;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void StartGame()
        {
            // リスタート時に前回のボールが残っている場合は破棄
            if (_ball != null)
            {
                Destroy(_ball.gameObject);
                _ball = null;
            }

            _isPlaying = true;
            SpawnBlocks();
            SpawnBall();
        }

        public void StopGame()
        {
            _isPlaying = false;
            if (_ball != null) _ball.Stop();
        }

        public void ResetBall()
        {
            if (_ball != null)
            {
                Destroy(_ball.gameObject);
                _ball = null;
            }
            SpawnBall();
        }

        private void SpawnBlocks()
        {
            foreach (var b in _blocks)
                if (b != null) Destroy(b.gameObject);
            _blocks.Clear();

            if (_blockSprites == null || _blockSprites.Length == 0)
            {
                Debug.LogWarning("[BreakoutManager] blockSprites が設定されていません");
            }

            for (int row = 0; row < Rows; row++)
            {
                int spriteIndex = (_blockSprites != null && _blockSprites.Length > 0)
                    ? row % _blockSprites.Length
                    : -1;

                for (int col = 0; col < Cols; col++)
                {
                    float x = BlockStartX + col * BlockSpacingX;
                    float y = BlockStartY - row * BlockSpacingY;

                    var blockObj = new GameObject($"Block_{row}_{col}");
                    blockObj.transform.position = new Vector3(x, y, 0f);
                    blockObj.transform.localScale = new Vector3(BlockWidth, BlockHeight, 1f);
                    blockObj.transform.SetParent(transform);

                    blockObj.AddComponent<SpriteRenderer>();
                    var col2d = blockObj.AddComponent<BoxCollider2D>();
                    col2d.size = Vector2.one; // localScale(0.88, 0.35)適用でワールドサイズ(0.88, 0.35)になる
                    var rb = blockObj.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Static;

                    var block = blockObj.AddComponent<Block>();
                    Sprite sprite = (spriteIndex >= 0) ? _blockSprites[spriteIndex] : null;
                    block.Initialize(1, sprite, OnBlockDestroyed);
                    _blocks.Add(block);
                }
            }
        }

        private void SpawnBall()
        {
            float px = _paddleTransform != null ? _paddleTransform.position.x : 0f;
            var ballObj = new GameObject("Ball");
            ballObj.transform.position = new Vector3(px, PaddleY + BallStartOffsetY, 0f);
            ballObj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            var sr = ballObj.AddComponent<SpriteRenderer>();
            sr.sprite = _ballSprite;
            sr.sortingOrder = 5;

            // RequireComponentに依存せず明示的にコンポーネントを追加してから初期化
            ballObj.AddComponent<Rigidbody2D>();
            ballObj.AddComponent<CircleCollider2D>();
            _ball = ballObj.AddComponent<BallController>();
            _ball.Initialize(_bouncyMaterial);
            _waitingForLaunch = true;
        }

        private void Update()
        {
            if (!_isPlaying) return;
            UpdatePaddle();
            UpdateBallLaunch();
            CheckBallOut();
        }

        private void UpdatePaddle()
        {
            if (_paddleTransform == null || Mouse.current == null) return;
            if (!Mouse.current.leftButton.isPressed) return;

            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = -_mainCamera.transform.position.z;
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
            float clampedX = Mathf.Clamp(worldPos.x, PaddleMinX, PaddleMaxX);
            _paddleTransform.position = new Vector3(clampedX, PaddleY, 0f);
        }

        private void UpdateBallLaunch()
        {
            if (!_waitingForLaunch) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                _waitingForLaunch = false;
                if (_ball != null)
                {
                    Vector2 dir = new Vector2(Random.Range(-0.5f, 0.5f), 1f).normalized;
                    _ball.Launch(dir);
                }
            }
            else
            {
                // 発射前はパドルにボールを追従させる
                if (_ball != null && _paddleTransform != null)
                {
                    float px = _paddleTransform.position.x;
                    _ball.transform.position = new Vector3(px, PaddleY + BallStartOffsetY, 0f);
                }
            }
        }

        private void CheckBallOut()
        {
            // 発射待機中は落下判定をスキップ
            if (_waitingForLaunch) return;
            if (_ball == null || !_ball.IsActive) return;

            if (_ball.transform.position.y < BallOutY)
            {
                Destroy(_ball.gameObject);
                _ball = null;
                if (_gameManager != null) _gameManager.OnBallLost();
            }
        }

        private void OnBlockDestroyed(Block destroyedBlock, int points)
        {
            // Destroy の遅延実行でnullにならないため、thisを受け取って即時除外
            _blocks.Remove(destroyedBlock);

            if (_gameManager != null) _gameManager.AddScore(points);

            if (_blocks.Count == 0)
            {
                if (_gameManager != null) _gameManager.OnAllBlocksDestroyed();
            }
        }
    }
}
