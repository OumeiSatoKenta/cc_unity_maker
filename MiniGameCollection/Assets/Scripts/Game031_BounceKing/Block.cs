using UnityEngine;

namespace Game031_BounceKing
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class Block : MonoBehaviour
    {
        private int _hp;
        private int _maxHp;
        private bool _isDead; // 同フレーム内の多重Hit防止
        private System.Action<Block, int> _onDestroyed;

        public void Initialize(int hp, Sprite sprite, System.Action<Block, int> onDestroyed)
        {
            _hp = hp;
            _maxHp = hp;
            _isDead = false;
            _onDestroyed = onDestroyed;
            GetComponent<SpriteRenderer>().sprite = sprite;
        }

        public void Hit()
        {
            if (_isDead) return; // 多重呼び出し防止
            _hp--;
            if (_hp <= 0)
            {
                _isDead = true;
                // コールバック前にDestroyするとUnityの遅延破棄でnullにならず
                // Count==0チェックが正しく動かないため、先にコールバックを呼ぶ
                // BreakoutManager側でリストからthisを削除してからCount確認する
                _onDestroyed?.Invoke(this, 100);
                Destroy(gameObject);
            }
            else
            {
                // HP残りに応じて明度を下げる
                var sr = GetComponent<SpriteRenderer>();
                sr.color = Color.Lerp(Color.gray, Color.white, (float)_hp / _maxHp);
            }
        }

        // 衝突検出はBallController(Dynamic)側で行うため、ここでは実装しない
        // StaticなRigidbody2DはOnCollisionEnter2Dを受け取れないためBallController側に移動
    }
}
