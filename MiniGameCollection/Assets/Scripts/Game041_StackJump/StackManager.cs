using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game041_StackJump
{
    public class StackManager : MonoBehaviour
    {
        [SerializeField] private StackJumpGameManager _gameManager;

        private const float BlockHeight = 0.3f;
        private const float StartWidth = 3f;
        private const float MoveSpeed = 5f;
        private const float MoveRange = 4f;
        private const float PerfectThreshold = 0.1f;

        private List<GameObject> _placedBlocks = new List<GameObject>();
        private GameObject _movingBlock;
        private float _currentWidth;
        private float _currentX;
        private int _stackHeight;
        private bool _movingRight;
        private Sprite _platformSprite;
        private Camera _mainCamera;

        public void Init()
        {
            _mainCamera = Camera.main;
            _platformSprite = Resources.Load<Sprite>("Sprites/Game041_StackJump/platform");

            foreach (var b in _placedBlocks)
                if (b != null) Destroy(b);
            _placedBlocks.Clear();
            if (_movingBlock != null) Destroy(_movingBlock);

            _currentWidth = StartWidth;
            _currentX = 0f;
            _stackHeight = 0;
            _movingRight = true;

            var baseBlock = CreateBlock(0f, 0f, StartWidth);
            _placedBlocks.Add(baseBlock);
            _stackHeight = 1;

            SpawnMovingBlock();
        }

        private void Update()
        {
            if (_gameManager != null && _gameManager.IsGameOver) return;
            if (_movingBlock == null) return;

            float dir = _movingRight ? 1f : -1f;
            float speed = MoveSpeed + _stackHeight * 0.15f;
            _movingBlock.transform.position += Vector3.right * dir * speed * Time.deltaTime;

            if (_movingBlock.transform.position.x > MoveRange)
                _movingRight = false;
            else if (_movingBlock.transform.position.x < -MoveRange)
                _movingRight = true;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                PlaceBlock();
            }
        }

        private void PlaceBlock()
        {
            if (_movingBlock == null) return;

            float movingX = _movingBlock.transform.position.x;
            float movingW = _movingBlock.transform.localScale.x;

            float overlap = CalculateOverlap(_currentX, _currentWidth, movingX, movingW);

            if (overlap <= 0f)
            {
                Destroy(_movingBlock);
                _movingBlock = null;
                if (_gameManager != null) _gameManager.OnGameOver();
                return;
            }

            bool isPerfect = Mathf.Abs(movingX - _currentX) < PerfectThreshold;

            if (isPerfect)
            {
                _movingBlock.transform.position = new Vector3(_currentX, _stackHeight * BlockHeight, 0f);
                ShowPerfectEffect(_movingBlock.transform.position);
            }
            else
            {
                float leftEdgeA = _currentX - _currentWidth / 2f;
                float rightEdgeA = _currentX + _currentWidth / 2f;
                float leftEdgeB = movingX - movingW / 2f;
                float rightEdgeB = movingX + movingW / 2f;

                float overlapLeft = Mathf.Max(leftEdgeA, leftEdgeB);
                float overlapRight = Mathf.Min(rightEdgeA, rightEdgeB);
                float newCenter = (overlapLeft + overlapRight) / 2f;

                _movingBlock.transform.position = new Vector3(newCenter, _stackHeight * BlockHeight, 0f);
                _movingBlock.transform.localScale = new Vector3(overlap, BlockHeight, 1f);

                CreateFallingPiece(movingX, movingW, overlapLeft, overlapRight);

                _currentX = newCenter;
                _currentWidth = overlap;
            }

            _placedBlocks.Add(_movingBlock);
            _movingBlock = null;
            _stackHeight++;

            if (_gameManager != null) _gameManager.OnBlockPlaced(isPerfect);

            AdjustCamera();
            SpawnMovingBlock();
        }

        private float CalculateOverlap(float ax, float aw, float bx, float bw)
        {
            float aLeft = ax - aw / 2f;
            float aRight = ax + aw / 2f;
            float bLeft = bx - bw / 2f;
            float bRight = bx + bw / 2f;

            float overlapLeft = Mathf.Max(aLeft, bLeft);
            float overlapRight = Mathf.Min(aRight, bRight);

            return Mathf.Max(0f, overlapRight - overlapLeft);
        }

        private void CreateFallingPiece(float movingX, float movingW, float overlapLeft, float overlapRight)
        {
            float leftEdgeB = movingX - movingW / 2f;
            float rightEdgeB = movingX + movingW / 2f;

            float cutWidth = 0f;
            float cutCenter = 0f;

            if (leftEdgeB < overlapLeft)
            {
                cutWidth = overlapLeft - leftEdgeB;
                cutCenter = (leftEdgeB + overlapLeft) / 2f;
            }
            else if (rightEdgeB > overlapRight)
            {
                cutWidth = rightEdgeB - overlapRight;
                cutCenter = (overlapRight + rightEdgeB) / 2f;
            }

            if (cutWidth > 0.01f)
            {
                var fall = CreateBlock(cutCenter, _stackHeight * BlockHeight, cutWidth);
                var sr = fall.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    var c = sr.color;
                    c.a = 0.6f;
                    sr.color = c;
                }
                var rb = fall.AddComponent<Rigidbody2D>();
                rb.gravityScale = 2f;
                Destroy(fall, 2f);
            }
        }

        private void ShowPerfectEffect(Vector3 pos)
        {
            var perfSprite = Resources.Load<Sprite>("Sprites/Game041_StackJump/perfect");
            if (perfSprite == null) return;
            var go = new GameObject("PerfectFX");
            go.transform.position = pos + Vector3.up * 0.3f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = perfSprite;
            sr.sortingOrder = 20;
            go.transform.localScale = Vector3.one * 0.5f;
            Destroy(go, 0.5f);
        }

        private void SpawnMovingBlock()
        {
            float y = _stackHeight * BlockHeight;
            float startX = _movingRight ? -MoveRange : MoveRange;
            _movingBlock = CreateBlock(startX, y, _currentWidth);

            var sr = _movingBlock.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float hue = (_stackHeight * 0.07f) % 1f;
                sr.color = Color.HSVToRGB(hue, 0.5f, 1f);
            }
        }

        private GameObject CreateBlock(float x, float y, float width)
        {
            var go = new GameObject("Block");
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(width, BlockHeight, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _platformSprite;
            sr.sortingOrder = 5;
            float hue = (_stackHeight * 0.07f) % 1f;
            sr.color = Color.HSVToRGB(hue, 0.5f, 1f);
            return go;
        }

        private void AdjustCamera()
        {
            if (_mainCamera == null) return;
            float targetY = Mathf.Max(1.5f, _stackHeight * BlockHeight - 1f);
            var pos = _mainCamera.transform.position;
            _mainCamera.transform.position = new Vector3(pos.x, Mathf.Lerp(pos.y, targetY, 0.3f), pos.z);
        }
    }
}
