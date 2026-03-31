using UnityEngine;

namespace Game023_ChainSlash
{
    public class Enemy : MonoBehaviour
    {
        public bool IsChained { get; private set; }
        public bool IsAlive { get; private set; }
        public int ChainOrder { get; private set; }

        private SpriteRenderer _spriteRenderer;

        public void Initialize()
        {
            IsChained = false;
            IsAlive = true;
            ChainOrder = -1;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Chain(int order, Sprite chainedSprite)
        {
            IsChained = true;
            ChainOrder = order;
            if (_spriteRenderer != null && chainedSprite != null)
                _spriteRenderer.sprite = chainedSprite;
        }

        public void Slash()
        {
            IsAlive = false;
            gameObject.SetActive(false);
        }
    }
}
