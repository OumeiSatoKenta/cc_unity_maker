using UnityEngine;
using TMPro;

namespace Game004v2_WordCrystal
{
    public class LetterTile : MonoBehaviour
    {
        public char Letter { get; private set; }
        public bool IsBonus { get; private set; }
        public bool IsUsed { get; private set; }

        private SpriteRenderer _sr;
        private TextMeshPro _text;

        public void Initialize(char letter, bool isBonus)
        {
            Letter = letter;
            IsBonus = isBonus;
            IsUsed = false;

            _sr = GetComponent<SpriteRenderer>();
            _text = GetComponentInChildren<TextMeshPro>();

            if (_text != null)
            {
                _text.text = letter.ToString().ToUpper();
                _text.color = isBonus ? new Color(1f, 0.85f, 0f) : Color.white;
            }

            if (_sr != null && isBonus)
                _sr.color = new Color(1f, 0.95f, 0.7f);

            StartCoroutine(PopAnim());
        }

        public void SetUsed(bool used)
        {
            IsUsed = used;
            if (_sr != null)
                _sr.color = used
                    ? new Color(0.5f, 0.5f, 0.5f, 0.6f)
                    : (IsBonus ? new Color(1f, 0.95f, 0.7f) : Color.white);
        }

        private System.Collections.IEnumerator PopAnim()
        {
            float t = 0f;
            Vector3 orig = transform.localScale;
            while (t < 0.2f)
            {
                if (this == null) yield break;
                t += Time.deltaTime;
                float ratio = t / 0.2f;
                float s = ratio < 0.5f
                    ? Mathf.Lerp(0f, 1.3f, ratio / 0.5f)
                    : Mathf.Lerp(1.3f, 1f, (ratio - 0.5f) / 0.5f);
                transform.localScale = orig * s;
                yield return null;
            }
            transform.localScale = orig;
        }
    }
}
