using UnityEngine;

namespace Game031_BounceKing
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class Block : MonoBehaviour
    {
        private int _hp;
        private int _maxHp;
        // 引数: (このBlock, 獲得ポイント) — BreakoutManager側でリストから即時除外するためselfを渡す
        private System.Action<Block, int> _onDestroyed;

        public void Initialize(int hp, Sprite sprite, System.Action<Block, int> onDestroyed)
        {
            _hp = hp;
            _maxHp = hp;
            _onDestroyed = onDestroyed;
            GetComponent<SpriteRenderer>().sprite = sprite;
        }

        public void Hit()
        {
            _hp--;
            if (_hp <= 0)
            {
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
