using UnityEngine;

namespace Game031v2_BounceKing
{
    public enum ItemType { PaddleExpand, MultiBall, PaddleShrink }

    public class ItemController : MonoBehaviour
    {
        public ItemType itemType;
        const float FallSpeed = 3.0f;
        BounceKingGameManager _gameManager;
        bool _collected;

        public void Initialize(ItemType type, BounceKingGameManager gm)
        {
            itemType = type;
            _gameManager = gm;
        }

        void Update()
        {
            if (_collected) return;
            transform.position += Vector3.down * FallSpeed * Time.deltaTime;

            float camBottom = -Camera.main.orthographicSize;
            if (transform.position.y < camBottom - 1f)
                Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (other.CompareTag("Paddle"))
            {
                _collected = true;
                _gameManager.OnItemCollected(itemType);
                Destroy(gameObject);
            }
        }
    }
}
