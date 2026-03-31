using UnityEngine;
using TMPro;

namespace Game004_WordCrystal
{
    public class CrystalController : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public char Letter { get; private set; }
        public bool IsBroken { get; private set; }

        private SpriteRenderer _spriteRenderer;
        private TextMeshPro _letterText;

        public void Initialize(Vector2Int gridPos, char letter, Sprite crystalSprite)
        {
            GridPosition = gridPos;
            Letter = letter;
            IsBroken = false;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null && crystalSprite != null)
                _spriteRenderer.sprite = crystalSprite;

            _letterText = GetComponentInChildren<TextMeshPro>();
            if (_letterText != null)
                _letterText.gameObject.SetActive(false);
        }

        public void Break(Sprite brokenSprite)
        {
            if (IsBroken) return;
            IsBroken = true;

            if (_spriteRenderer != null && brokenSprite != null)
                _spriteRenderer.sprite = brokenSprite;

            if (_letterText != null)
            {
                _letterText.text = Letter.ToString();
                _letterText.gameObject.SetActive(true);
            }
        }
    }
}
