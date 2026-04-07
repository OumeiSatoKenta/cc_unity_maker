using UnityEngine;
using System.Collections;

namespace Game060v2_MeltIce
{
    public class MirrorController : MonoBehaviour
    {
        SpriteRenderer _sr;
        float _angleDeg = 45f; // 0, 45, 90, 135
        public Vector2Int GridPosition { get; set; }

        // Reflection directions for given angle:
        // 45deg mirror (\): horizontal↔vertical
        // 0deg mirror (|): horizontal flips horizontal axis
        // 90deg mirror (-): vertical flips vertical axis
        // 135deg mirror (/): horizontal↔vertical with swap

        public void Setup(Vector2Int gridPos, Sprite sprite, float cellSize)
        {
            GridPosition = gridPos;
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = sprite;
            _sr.sortingOrder = 5;
            _angleDeg = 45f;
            transform.localScale = new Vector3(cellSize, cellSize, 1f);
            UpdateVisualAngle();

            var col = gameObject.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }

        public void Rotate45()
        {
            _angleDeg = (_angleDeg + 45f) % 180f;
            UpdateVisualAngle();
            StartCoroutine(RotateBounce());
        }

        void UpdateVisualAngle()
        {
            transform.rotation = Quaternion.Euler(0f, 0f, _angleDeg);
        }

        // Given an incoming direction, returns the reflected direction
        // dir is in grid space (e.g. Vector2.down = moving downward)
        public Vector2 GetReflectedDirection(Vector2 inDir)
        {
            // Mirror line direction depends on _angleDeg:
            // 0deg = vertical mirror |: reflects horizontal component
            // 45deg = \ mirror: swaps and negates
            // 90deg = horizontal mirror -: reflects vertical component
            // 135deg = / mirror: swaps
            float rad = _angleDeg * Mathf.Deg2Rad;
            Vector2 normal = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
            return inDir - 2f * Vector2.Dot(inDir, normal) * normal;
        }

        public IEnumerator PlaceBounceAnimation()
        {
            float elapsed = 0f;
            float duration = 0.15f;
            float baseScale = transform.localScale.x;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = baseScale * (1f + 0.15f * Mathf.Sin(t * Mathf.PI));
                transform.localScale = new Vector3(scale, scale, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = new Vector3(baseScale, baseScale, 1f);
        }

        IEnumerator RotateBounce()
        {
            float elapsed = 0f;
            float duration = 0.1f;
            float baseScale = transform.localScale.x;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = baseScale * (1f + 0.1f * Mathf.Sin(t * Mathf.PI));
                float sx = transform.localScale.x;
                transform.localScale = new Vector3(scale, scale, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = new Vector3(baseScale, baseScale, 1f);
        }
    }
}
