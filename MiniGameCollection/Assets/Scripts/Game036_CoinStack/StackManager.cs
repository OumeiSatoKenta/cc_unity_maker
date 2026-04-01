using UnityEngine;
using UnityEngine.InputSystem;

namespace Game036_CoinStack
{
    public class StackManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private CoinStackGameManager _gameManager;

        [SerializeField, Tooltip("コインスプライト")]
        private Sprite _coinSprite;

        [SerializeField, Tooltip("台座スプライト")]
        private Sprite _platformSprite;

        private Transform _movingCoin;
        private float _coinSpeed = 4f;
        private float _coinDirection = 1f;
        private float _nextCoinY;
        private float _stackCenterX = 0f;
        private bool _waitingForTap;

        private const float CoinWidth = 1.2f;
        private const float CoinHeight = 0.25f;
        private const float PlatformY = -3.5f;
        private const float MoveRangeX = 3.5f;
        private const float MaxOffsetForStack = 0.8f;

        public void StartGame()
        {
            _nextCoinY = PlatformY + CoinHeight;
            _stackCenterX = 0f;
            SpawnPlatform();
            SpawnMovingCoin();
        }

        private void SpawnPlatform()
        {
            var obj = new GameObject("Platform");
            obj.transform.position = new Vector3(0f, PlatformY, 0f);
            obj.transform.localScale = new Vector3(2f, 0.5f, 1f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _platformSprite;
            sr.sortingOrder = 1;
        }

        private void SpawnMovingCoin()
        {
            if (!_gameManager.IsPlaying) return;

            var obj = new GameObject("MovingCoin");
            obj.transform.position = new Vector3(-MoveRangeX, _nextCoinY, 0f);
            obj.transform.localScale = new Vector3(CoinWidth, CoinHeight * 3f, 1f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _coinSprite;
            sr.sortingOrder = 3;

            _movingCoin = obj.transform;
            _coinDirection = 1f;
            _waitingForTap = true;

            // 速度を段数に応じて上げる
            _coinSpeed = 4f + _gameManager.StackedCount * 0.3f;
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying) return;

            if (_waitingForTap && _movingCoin != null)
            {
                // コインを左右に動かす
                var pos = _movingCoin.position;
                pos.x += _coinDirection * _coinSpeed * Time.deltaTime;
                if (pos.x >= MoveRangeX) _coinDirection = -1f;
                if (pos.x <= -MoveRangeX) _coinDirection = 1f;
                _movingCoin.position = pos;

                // タップで落下
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    DropCoin();
                }
            }
        }

        private void DropCoin()
        {
            _waitingForTap = false;
            if (_movingCoin == null) return;

            float dropX = _movingCoin.position.x;
            float offset = Mathf.Abs(dropX - _stackCenterX);

            if (offset > MaxOffsetForStack)
            {
                // ミス: コインが落下して消える
                var sr = _movingCoin.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.red;
                Destroy(_movingCoin.gameObject, 0.5f);
                _movingCoin = null;

                if (offset > MaxOffsetForStack * 2f)
                {
                    // 大きくずれ → タワー崩壊
                    _gameManager.OnTowerCollapse();
                    return;
                }

                _gameManager.OnCoinMissed();
                SpawnMovingCoin();
            }
            else
            {
                // 成功: コインをスタック位置に固定
                _movingCoin.position = new Vector3(dropX, _nextCoinY, 0f);

                // スタック中心をやや移動（ずれが蓄積）
                _stackCenterX = Mathf.Lerp(_stackCenterX, dropX, 0.3f);
                _nextCoinY += CoinHeight;
                _movingCoin = null;

                _gameManager.OnCoinStacked();
                SpawnMovingCoin();
            }
        }
    }
}
