using UnityEngine;
using System.Collections;

namespace Game030v2_FingerRacer
{
    public class CheckpointMarker : MonoBehaviour
    {
        public int Index;
        public FingerRacerGameManager GameManager;

        bool _passed;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_passed) return;
            if (other.GetComponent<CarController>() == null) return;
            _passed = true;
            GameManager?.OnCheckpointPassed(Index);
            StartCoroutine(PassedAnimation());
        }

        IEnumerator PassedAnimation()
        {
            var sr = GetComponent<SpriteRenderer>();
            var origColor = sr != null ? sr.color : Color.green;
            float elapsed = 0f;
            float duration = 0.3f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = t < 0.5f ? Mathf.Lerp(1f, 1.5f, t * 2f) : Mathf.Lerp(1.5f, 1f, (t - 0.5f) * 2f);
                transform.localScale = Vector3.one * scale;
                if (sr != null) sr.color = Color.Lerp(Color.white, origColor, t);
                yield return null;
            }
            transform.localScale = Vector3.one;
            if (sr != null) sr.color = new Color(0.5f, 0.5f, 0.5f, 0.4f);
        }
    }
}
