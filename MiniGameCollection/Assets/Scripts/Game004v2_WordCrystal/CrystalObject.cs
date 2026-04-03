using UnityEngine;

namespace Game004v2_WordCrystal
{
    public enum CrystalType { Normal, Hidden, Bonus, Poison }

    public class CrystalObject : MonoBehaviour
    {
        public CrystalType CrystalType { get; private set; }
        public bool IsDestroyed { get; private set; }

        private SpriteRenderer _sr;
        private WordManager _wordManager;

        public void Initialize(CrystalType type, WordManager manager)
        {
            CrystalType = type;
            _wordManager = manager;
            _sr = GetComponent<SpriteRenderer>();
            IsDestroyed = false;
        }

        public void PlayDestroyAnimation()
        {
            IsDestroyed = true;
            StartCoroutine(DestroyAnim());
        }

        private System.Collections.IEnumerator DestroyAnim()
        {
            float t = 0f;
            Vector3 origScale = transform.localScale;
            while (t < 0.25f)
            {
                if (this == null) yield break;
                t += Time.deltaTime;
                float ratio = t / 0.25f;
                float s = ratio < 0.4f
                    ? Mathf.Lerp(1f, 1.4f, ratio / 0.4f)
                    : Mathf.Lerp(1.4f, 0f, (ratio - 0.4f) / 0.6f);
                transform.localScale = origScale * s;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
