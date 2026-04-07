using UnityEngine;
using System.Collections;

namespace Game060v2_MeltIce
{
    public class IceBlockController : MonoBehaviour
    {
        SpriteRenderer _sr;
        public Vector2Int CurrentGridPos { get; private set; }
        public bool IsForbidden { get; private set; }
        public bool IsHitByLight { get; set; }

        bool _isMobile;
        Vector2Int _moveMin;
        Vector2Int _moveMax;
        float _moveSpeed = 0.6f;
        int _moveDir = 1;
        float _moveProgress;
        Vector2Int _moveDirAxis;

        MeltIceGameManager _manager;

        static readonly Color TargetColor = new Color(0.5f, 0.85f, 1f, 1f);
        static readonly Color ForbiddenColor = new Color(1f, 0.3f, 0.2f, 1f);
        static readonly Color HitGlowColor = new Color(1f, 1f, 0.5f, 1f);

        public void Setup(Vector2Int gridPos, bool isForbidden, Sprite sprite, float cellSize,
            bool isMobile, Vector2Int maxBound)
        {
            CurrentGridPos = gridPos;
            IsForbidden = isForbidden;
            _isMobile = isMobile;

            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = sprite;
            _sr.sortingOrder = 3;
            transform.localScale = new Vector3(cellSize * 0.85f, cellSize * 0.85f, 1f);

            var col = gameObject.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            if (_isMobile)
            {
                // Horizontal movement between col 1 and col 4
                _moveMin = new Vector2Int(1, gridPos.y);
                _moveMax = new Vector2Int(maxBound.x - 1, gridPos.y);
                _moveDirAxis = Vector2Int.right;
                _moveProgress = Random.value; // stagger start
            }

            _manager = GetComponentInParent<MeltIceGameManager>();
            UpdateVisual();
        }

        void Update()
        {
            if (!_isMobile) return;
            if (_manager == null) return;

            // Simple lerp movement
            _moveProgress += Time.deltaTime * _moveSpeed * _moveDir;

            if (_moveProgress >= 1f) { _moveProgress = 1f; _moveDir = -1; }
            else if (_moveProgress <= 0f) { _moveProgress = 0f; _moveDir = 1; }

            Vector2Int newGrid = Vector2Int.RoundToInt(
                Vector2.Lerp(_moveMin, _moveMax, _moveProgress)
            );

            if (newGrid != CurrentGridPos)
            {
                CurrentGridPos = newGrid;
                transform.position = _manager.GridToWorld(CurrentGridPos);
                // Trigger recalculation
                var lrs = FindFirstObjectByType<LightRaySystem>();
                if (lrs != null) lrs.RecalculateLightPath();
            }
            else
            {
                // Smooth position
                Vector3 target = _manager.GridToWorld(CurrentGridPos);
                transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 8f);
            }
        }

        public void UpdateVisual()
        {
            if (_sr == null) return;
            if (IsHitByLight)
                _sr.color = HitGlowColor;
            else if (IsForbidden)
                _sr.color = ForbiddenColor;
            else
                _sr.color = TargetColor;
        }

        public IEnumerator MeltAnimation()
        {
            float elapsed = 0f;
            float duration = 0.5f;
            Vector3 startScale = transform.localScale;
            Color startColor = _sr.color;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                // Pop then shrink
                float scaleFactor = t < 0.3f ? (1f + 0.3f * (t / 0.3f)) : (1.3f - 1.3f * ((t - 0.3f) / 0.7f));
                transform.localScale = startScale * scaleFactor;
                _sr.color = Color.Lerp(startColor, new Color(1f, 1f, 1f, 0f), t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }

        public IEnumerator ForbiddenHitFlash()
        {
            for (int i = 0; i < 5; i++)
            {
                _sr.color = Color.white;
                yield return new WaitForSeconds(0.05f);
                _sr.color = ForbiddenColor;
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}
