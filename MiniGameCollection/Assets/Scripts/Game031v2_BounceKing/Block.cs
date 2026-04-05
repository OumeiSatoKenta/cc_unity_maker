using UnityEngine;
using System.Collections;

namespace Game031v2_BounceKing
{
    public enum BlockType { Normal, Hard, Boss }

    public class Block : MonoBehaviour
    {
        public BlockType blockType;
        public int maxHp;
        int _hp;
        SpriteRenderer _sr;
        Color _baseColor;

        public bool IsAlive => _hp > 0;
        public System.Action<Block> OnDestroyed;
        public System.Action<Block> OnHit;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Initialize(BlockType type, Sprite sprite)
        {
            blockType = type;
            maxHp = type switch
            {
                BlockType.Hard => 2,
                BlockType.Boss => 4,
                _ => 1
            };
            _hp = maxHp;
            if (_sr != null)
            {
                _sr.sprite = sprite;
                _baseColor = _sr.color;
            }
        }

        public void TakeHit()
        {
            if (!IsAlive) return;
            _hp--;
            OnHit?.Invoke(this);

            if (_hp <= 0)
            {
                StartCoroutine(DestroyFlash());
            }
            else
            {
                StartCoroutine(HitFlash());
            }
        }

        IEnumerator HitFlash()
        {
            if (_sr != null) _sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            if (_sr != null) _sr.color = _baseColor;
        }

        IEnumerator DestroyFlash()
        {
            if (_sr != null) _sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            OnDestroyed?.Invoke(this);
            Destroy(gameObject);
        }

        public int GetScore()
        {
            return blockType switch
            {
                BlockType.Hard => 30,
                BlockType.Boss => 50,
                _ => 10
            };
        }

        public float GetDropRate()
        {
            return blockType switch
            {
                BlockType.Hard => 0.4f,
                BlockType.Boss => 1.0f,
                _ => 0.2f
            };
        }
    }
}
