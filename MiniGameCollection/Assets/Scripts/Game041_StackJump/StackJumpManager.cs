using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game041_StackJump
{
    public class StackJumpManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private StackJumpGameManager _gameManager;

        [SerializeField, Tooltip("ブロックスプライト")]
        private Sprite _blockSprite;

        private Transform _movingBlock;
        private float _blockWidth;
        private float _lastBlockX;
        private float _nextBlockY;
        private float _moveSpeed = 4f;
        private float _moveDirection = 1f;
        private bool _waitingForTap;
        private List<GameObject> _stackedBlocks = new List<GameObject>();

        private const float BlockHeight = 0.2f;
        private const float InitialWidth = 2.5f;
        private const float BaseY = -4f;
        private const float MoveRange = 4f;
        private const float MinWidth = 0.3f;

        public void StartGame()
        {
            _blockWidth = InitialWidth;
            _lastBlockX = 0f;
            _nextBlockY = BaseY;

            // 最初の固定ブロック
            CreateStackedBlock(0f, BaseY, InitialWidth);
            _nextBlockY += BlockHeight;
            SpawnMovingBlock();
        }

        private void CreateStackedBlock(float x, float y, float width)
        {
            var obj = new GameObject($"Stack_{_stackedBlocks.Count}");
            obj.transform.position = new Vector3(x, y, 0f);
            obj.transform.localScale = new Vector3(width, BlockHeight * 3f, 1f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _blockSprite;
            sr.sortingOrder = 2;
            // 色をグラデーション
            float hue = (_stackedBlocks.Count * 0.07f) % 1f;
            sr.color = Color.HSVToRGB(hue, 0.6f, 0.9f);
            _stackedBlocks.Add(obj);
        }

        private void SpawnMovingBlock()
        {
            if (!_gameManager.IsPlaying) return;
            var obj = new GameObject("MovingBlock");
            obj.transform.position = new Vector3(-MoveRange, _nextBlockY, 0f);
            obj.transform.localScale = new Vector3(_blockWidth, BlockHeight * 3f, 1f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _blockSprite;
            sr.sortingOrder = 3;
            float hue = (_stackedBlocks.Count * 0.07f) % 1f;
            sr.color = Color.HSVToRGB(hue, 0.7f, 1f);
            _movingBlock = obj.transform;
            _moveDirection = 1f;
            _waitingForTap = true;
            _moveSpeed = 4f + _gameManager.StackedCount * 0.15f;
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying || !_waitingForTap) return;
            if (_movingBlock == null) return;

            // 左右に動かす
            var pos = _movingBlock.position;
            pos.x += _moveDirection * _moveSpeed * Time.deltaTime;
            if (pos.x >= MoveRange) _moveDirection = -1f;
            if (pos.x <= -MoveRange) _moveDirection = 1f;
            _movingBlock.position = pos;

            // タップで停止
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                DropBlock();
            }
        }

        private void DropBlock()
        {
            _waitingForTap = false;
            if (_movingBlock == null) return;

            float dropX = _movingBlock.position.x;
            float overlap = _blockWidth - Mathf.Abs(dropX - _lastBlockX);

            if (overlap <= 0f)
            {
                // 完全ミス
                var sr = _movingBlock.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.red;
                Destroy(_movingBlock.gameObject, 0.5f);
                _movingBlock = null;
                _gameManager.OnBlockMissed();
                return;
            }

            // はみ出し部分を切り落とす
            float newWidth = Mathf.Min(overlap, _blockWidth);
            float newX;
            if (dropX > _lastBlockX)
                newX = _lastBlockX + (_blockWidth - newWidth) / 2f + (dropX - _lastBlockX) / 2f;
            else
                newX = _lastBlockX - (_blockWidth - newWidth) / 2f + (dropX - _lastBlockX) / 2f;

            // ブロックをスタック
            Destroy(_movingBlock.gameObject);
            _movingBlock = null;
            CreateStackedBlock(newX, _nextBlockY, newWidth);

            _lastBlockX = newX;
            _blockWidth = newWidth;
            _nextBlockY += BlockHeight;

            // カメラを上に移動
            if (Camera.main != null && _nextBlockY > 0f)
            {
                var camPos = Camera.main.transform.position;
                camPos.y = _nextBlockY - 2f;
                Camera.main.transform.position = camPos;
            }

            _gameManager.OnBlockStacked(overlap);

            if (_blockWidth < MinWidth)
            {
                _gameManager.OnBlockMissed();
                return;
            }

            SpawnMovingBlock();
        }
    }
}
