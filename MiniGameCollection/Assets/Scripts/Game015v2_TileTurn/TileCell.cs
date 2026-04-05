using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game015v2_TileTurn
{
    public enum TileType { Normal, Linked, Locked, Flipped }

    public class TileCell : MonoBehaviour
    {
        public TileType TileType { get; private set; }
        public int CurrentRotation { get; private set; }  // 0/1/2/3 (x90 degrees)
        public bool IsCorrect => CurrentRotation == 0;
        public bool IsFlipped { get; private set; }

        // Linked tiles: which neighbors rotate with this tile
        public List<TileCell> LinkedNeighbors { get; private set; } = new List<TileCell>();

        SpriteRenderer _sr;
        Sprite _normalSprite;
        Sprite _correctSprite;
        bool _isAnimating;

        public void Initialize(TileType type, int initialRotation, bool initialFlipped, Sprite normalSprite, Sprite correctSprite)
        {
            TileType = type;
            CurrentRotation = initialRotation;
            IsFlipped = initialFlipped;
            _normalSprite = normalSprite;
            _correctSprite = correctSprite;
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
            if (normalSprite != null) _sr.sprite = normalSprite;
            ApplyRotationVisual();
        }

        void ApplyRotationVisual()
        {
            transform.localRotation = Quaternion.Euler(0, 0, -90f * CurrentRotation);
            if (IsFlipped)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // Returns true if this rotation makes the tile correct
        public bool Rotate()
        {
            if (TileType == TileType.Locked) return false;
            CurrentRotation = (CurrentRotation + 1) % 4;
            ApplyRotationVisual();
            if (IsCorrect)
                StartCoroutine(PlayCorrectAnimation());
            return IsCorrect;
        }

        // For Flipped tiles: flip horizontally instead of rotate
        public void Flip()
        {
            if (TileType != TileType.Flipped) return;
            IsFlipped = !IsFlipped;
            ApplyRotationVisual();
        }

        // Called by linked neighbor's rotation
        public void RotateLinked()
        {
            if (TileType == TileType.Locked) return;
            CurrentRotation = (CurrentRotation + 1) % 4;
            ApplyRotationVisual();
            if (IsCorrect)
                StartCoroutine(PlayCorrectAnimation());
        }

        IEnumerator PlayCorrectAnimation()
        {
            if (_isAnimating) yield break;
            _isAnimating = true;
            if (_correctSprite != null) _sr.sprite = _correctSprite;
            Vector3 origScale = transform.localScale;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.2f;
                float scale = t < 0.5f ? Mathf.Lerp(1f, 1.3f, t * 2f) : Mathf.Lerp(1.3f, 1f, (t - 0.5f) * 2f);
                transform.localScale = origScale * scale;
                yield return null;
            }
            transform.localScale = origScale;
            _isAnimating = false;
        }

        public void PlayGameOverFlash()
        {
            StartCoroutine(RedFlash());
        }

        IEnumerator RedFlash()
        {
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                _sr.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), Color.white, t);
                yield return null;
            }
            _sr.color = Color.white;
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
