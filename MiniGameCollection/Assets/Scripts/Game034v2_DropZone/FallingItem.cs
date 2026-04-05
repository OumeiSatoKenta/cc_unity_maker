using UnityEngine;
using System.Collections;

namespace Game034v2_DropZone
{
    public enum ItemType
    {
        Fruit,
        Trash,
        Recycle,
        TrickyFruit,
        TrickyTrash,
        Bonus
    }

    public class FallingItem : MonoBehaviour
    {
        public ItemType ItemType { get; private set; }
        public bool IsBeingDragged { get; private set; }

        SpriteRenderer _sr;
        Vector3 _dragOffset;
        bool _isActive = true;
        bool _callbackFired;
        float _fallSpeed;
        DropZoneMechanic _mechanic;

        public bool HasBeenProcessed { get; private set; }
        public void MarkProcessed() => HasBeenProcessed = true;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Initialize(ItemType type, Sprite sprite, float fallSpeed, DropZoneMechanic mechanic)
        {
            ItemType = type;
            _fallSpeed = fallSpeed;
            _mechanic = mechanic;
            if (_sr != null) _sr.sprite = sprite;
            _isActive = true;
            _callbackFired = false;
            HasBeenProcessed = false;
        }

        void Update()
        {
            if (!_isActive || IsBeingDragged) return;

            transform.position += Vector3.down * _fallSpeed * Time.deltaTime;

            // fell off screen
            if (transform.position.y < -7f)
            {
                _isActive = false;
                _mechanic.OnItemFellOff(this);
            }
        }

        public void StartDrag(Vector3 worldPos)
        {
            IsBeingDragged = true;
            _dragOffset = transform.position - worldPos;
        }

        public void UpdateDrag(Vector3 worldPos)
        {
            transform.position = worldPos + _dragOffset;
        }

        public void EndDrag()
        {
            IsBeingDragged = false;
        }

        public void PlayCorrectAnimation(Vector3 targetPos, System.Action onComplete)
        {
            _isActive = false;
            StartCoroutine(CorrectAnim(targetPos, onComplete));
        }

        IEnumerator CorrectAnim(Vector3 targetPos, System.Action onComplete)
        {
            Vector3 startPos = transform.position;
            Vector3 startScale = transform.localScale;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.3f;
                transform.position = Vector3.Lerp(startPos, targetPos, ratio);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, ratio);
                yield return null;
            }
            if (!_callbackFired) { _callbackFired = true; onComplete?.Invoke(); }
            Destroy(gameObject);
        }

        public void PlayWrongAnimation(System.Action onComplete)
        {
            _isActive = false;
            StartCoroutine(WrongAnim(onComplete));
        }

        IEnumerator WrongAnim(System.Action onComplete)
        {
            Color origColor = _sr != null ? _sr.color : Color.white;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                if (_sr != null) _sr.color = Color.Lerp(Color.red, origColor, t / 0.2f);
                yield return null;
            }
            if (_sr != null) _sr.color = origColor;
            if (!_callbackFired) { _callbackFired = true; onComplete?.Invoke(); }
            Destroy(gameObject);
        }

        public void Deactivate()
        {
            _isActive = false;
            IsBeingDragged = false;
            _callbackFired = true;
            Destroy(gameObject, 0.05f);
        }
    }
}
