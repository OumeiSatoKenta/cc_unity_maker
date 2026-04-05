using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game028v2_RopeSwing
{
    public class PlatformData
    {
        public GameObject go;
        public SpriteRenderer sr;
        public bool isGoal;
        public bool isMoving;
        public bool isCollapsing;
        public float moveRange;
        public float moveSpeed;
        public float moveOriginX;
        public float width;
        public float topY;  // top edge Y in world space
        public bool collapsed;
        public Coroutine collapseCoroutine;
    }

    public class RopeAnchorData
    {
        public GameObject go;
        public Vector2 position;
        public float ropeLength;
    }

    public class PlatformManager : MonoBehaviour
    {
        [SerializeField] RopeSwingGameManager _gameManager;
        [SerializeField] Sprite _platformSprite;
        [SerializeField] Sprite _goalSprite;
        [SerializeField] Sprite _anchorSprite;

        List<PlatformData> _platforms = new List<PlatformData>();
        List<RopeAnchorData> _anchors = new List<RopeAnchorData>();

        int _platformCount;
        float _platformWidth;
        bool _hasMobile;
        bool _hasWind;
        bool _hasCollapse;
        float _mobileFraction;
        float _collapseFraction;
        float _windStrength;
        float _moveSpeedBase;

        public float WindStrength => _hasWind ? _windStrength : 0f;
        public List<PlatformData> Platforms => _platforms;
        public List<RopeAnchorData> Anchors => _anchors;

        public void SetupStage(StageManager.StageConfig config, int stage)
        {
            ClearAll();

            _platformCount = config.countMultiplier;
            float complexity = config.complexityFactor;

            // Stage-specific settings
            switch (stage)
            {
                case 1:
                    _platformWidth = 2.0f;
                    _hasMobile = false; _hasWind = false; _hasCollapse = false;
                    _mobileFraction = 0f; _collapseFraction = 0f;
                    _windStrength = 0f; _moveSpeedBase = 0f;
                    break;
                case 2:
                    _platformWidth = 1.6f;
                    _hasMobile = false; _hasWind = false; _hasCollapse = false;
                    _mobileFraction = 0f; _collapseFraction = 0f;
                    _windStrength = 0f; _moveSpeedBase = 0f;
                    break;
                case 3:
                    _platformWidth = 1.4f;
                    _hasMobile = true; _hasWind = false; _hasCollapse = false;
                    _mobileFraction = 0.3f; _collapseFraction = 0f;
                    _windStrength = 0f; _moveSpeedBase = 0.8f * config.speedMultiplier;
                    break;
                case 4:
                    _platformWidth = 1.2f;
                    _hasMobile = true; _hasWind = true; _hasCollapse = false;
                    _mobileFraction = 0.3f; _collapseFraction = 0f;
                    _windStrength = Random.Range(0.5f, 1.5f) * (Random.value > 0.5f ? 1f : -1f);
                    _moveSpeedBase = 1.0f * config.speedMultiplier;
                    break;
                default: // stage 5
                    _platformWidth = 1.0f;
                    _hasMobile = true; _hasWind = true; _hasCollapse = true;
                    _mobileFraction = 0.4f; _collapseFraction = 0.4f;
                    _windStrength = Random.Range(0.8f, 2.0f) * (Random.value > 0.5f ? 1f : -1f);
                    _moveSpeedBase = 1.2f * config.speedMultiplier;
                    break;
            }

            BuildLevel(stage);
        }

        void BuildLevel(int stage)
        {
            if (_platformCount <= 0)
            {
                Debug.LogError($"[PlatformManager] platformCount is {_platformCount}. Aborting BuildLevel.");
                return;
            }
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float bottomMargin = 2.8f;
            float platformY = -camSize + bottomMargin + 0.32f; // platform top edge Y

            // Distribute platforms evenly across screen width
            float totalWidth = camWidth * 2f;
            float spacing = totalWidth / (_platformCount + 1);
            float startX = -camWidth + spacing;

            // Anchor points are between platforms (slightly above)
            float anchorY = platformY + 2.2f;

            for (int i = 0; i < _platformCount; i++)
            {
                float px = startX + spacing * i;
                bool isGoal = (i == _platformCount - 1);
                bool isMoving = !isGoal && _hasMobile && i > 0 && Random.value < _mobileFraction;
                bool isCollapse = !isGoal && !isMoving && _hasCollapse && i > 0 && Random.value < _collapseFraction;

                var pd = CreatePlatform(px, platformY, isGoal, isMoving, isCollapse);
                _platforms.Add(pd);

                // Create anchor between this platform and next (except after last)
                if (i < _platformCount - 1)
                {
                    float ax = px + spacing * 0.5f;
                    float rl = stage == 1 ? 2.5f : Random.Range(1.8f, 3.0f);
                    CreateAnchor(ax, anchorY, rl);
                }
            }
        }

        PlatformData CreatePlatform(float x, float topY, bool isGoal, bool isMoving, bool isCollapse)
        {
            var go = new GameObject(isGoal ? "GoalPlatform" : "Platform");
            go.transform.position = new Vector3(x, topY, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = isGoal ? _goalSprite : _platformSprite;
            sr.sortingOrder = 5;

            // Scale sprite to desired width
            float spriteWidth = sr.sprite != null ? sr.sprite.rect.width / sr.sprite.pixelsPerUnit : 1f;
            float spriteHeight = sr.sprite != null ? sr.sprite.rect.height / sr.sprite.pixelsPerUnit : 0.25f;
            float scaleX = _platformWidth / spriteWidth;
            float scaleY = 0.4f / spriteHeight;
            go.transform.localScale = new Vector3(scaleX, scaleY, 1f);

            var pd = new PlatformData
            {
                go = go,
                sr = sr,
                isGoal = isGoal,
                isMoving = isMoving,
                isCollapsing = isCollapse,
                width = _platformWidth,
                topY = topY + 0.2f,
                moveOriginX = x,
                moveRange = 1.2f,
                moveSpeed = _moveSpeedBase,
                collapsed = false
            };
            return pd;
        }

        void CreateAnchor(float x, float y, float ropeLength)
        {
            var go = new GameObject("Anchor");
            go.transform.position = new Vector3(x, y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _anchorSprite;
            sr.sortingOrder = 4;
            float spriteSize = _anchorSprite != null ? _anchorSprite.rect.width / _anchorSprite.pixelsPerUnit : 0.5f;
            float targetSize = 0.5f;
            float s = targetSize / spriteSize;
            go.transform.localScale = new Vector3(s, s, 1f);

            _anchors.Add(new RopeAnchorData
            {
                go = go,
                position = new Vector2(x, y),
                ropeLength = ropeLength
            });
        }

        public void TriggerCollapse(PlatformData pd)
        {
            if (pd.collapseCoroutine != null) return;
            pd.collapseCoroutine = StartCoroutine(CollapseRoutine(pd));
        }

        IEnumerator CollapseRoutine(PlatformData pd)
        {
            float elapsed = 0f;
            float duration = 2.0f;
            Color origColor = pd.sr.color;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float blink = Mathf.Sin(elapsed * Mathf.PI * 6f);
                float t = elapsed / duration;
                pd.sr.color = Color.Lerp(origColor, new Color(1f, 0.3f, 0.2f, 1f - t * 0.5f), Mathf.Clamp01(t + blink * 0.2f));
                yield return null;
            }
            pd.collapsed = true;
            pd.go.SetActive(false);
        }

        void Update()
        {
            foreach (var pd in _platforms)
            {
                if (!pd.isMoving || pd.collapsed || !pd.go.activeSelf) continue;
                float t = Time.time * pd.moveSpeed;
                float offsetX = Mathf.Sin(t) * pd.moveRange;
                var pos = pd.go.transform.position;
                pos.x = pd.moveOriginX + offsetX;
                pd.go.transform.position = pos;
                // Update center x for landing detection
            }
        }

        void ClearAll()
        {
            foreach (var pd in _platforms)
            {
                if (pd.collapseCoroutine != null) StopCoroutine(pd.collapseCoroutine);
                if (pd.go != null) Destroy(pd.go);
            }
            foreach (var a in _anchors)
                if (a.go != null) Destroy(a.go);
            _platforms.Clear();
            _anchors.Clear();
        }

        void OnDestroy() { ClearAll(); }
    }
}
