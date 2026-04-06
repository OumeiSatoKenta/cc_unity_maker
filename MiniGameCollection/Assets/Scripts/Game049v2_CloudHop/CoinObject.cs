using System.Collections;
using UnityEngine;

namespace Game049v2_CloudHop
{
    public class CoinObject : MonoBehaviour
    {
        private CloudHopGameManager _gameManager;
        private bool _collected;

        public void Initialize(CloudHopGameManager gameManager)
        {
            _gameManager = gameManager;
            _collected = false;
        }

        void Update()
        {
            // Gentle rotation animation
            transform.Rotate(0f, 0f, 90f * Time.deltaTime);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected || !other.CompareTag("Player")) return;
            _collected = true;
            _gameManager?.OnCoinCollected();
            StartCoroutine(CollectAnimation());
        }

        IEnumerator CollectAnimation()
        {
            float t = 0f;
            Vector3 startPos = transform.position;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.4f;
                transform.position = startPos + Vector3.up * ratio * 1.5f;
                var sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f - ratio;
                    sr.color = c;
                }
                transform.localScale = Vector3.one * (1f + ratio * 0.5f);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
