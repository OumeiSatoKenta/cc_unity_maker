using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game024v2_BubblePop
{
    public enum BubbleType { Normal, Iron, Split, Ghost }
    public enum BubbleColor { None, Red, Blue, Green }

    public class BubbleData
    {
        public GameObject obj;
        public SpriteRenderer sr;
        public BubbleType type;
        public BubbleColor color;
        public float spawnTime;
        public float riseSpeed;
        public int hitCount;      // For iron bubble (needs 2 hits)
        public bool isAlive;
        public bool colorRevealed; // For ghost bubble
        public float ghostRevealTimer;
        public BubbleColor originalColor; // Ghost stores its real color
    }

    public class BubbleController : MonoBehaviour
    {
        [SerializeField] BubblePopGameManager _gameManager;

        Sprite _sprRed, _sprBlue, _sprGreen, _sprIron, _sprSplit, _sprGhost;
        Camera _mainCam;

        bool _isActive;
        float _spawnInterval;
        float _riseSpeed;
        int _colorCount;
        float _ironRatio;
        float _splitRatio;
        float _ghostRatio;
        float _stageDuration;
        float _stageTimer;
        float _spawnTimer;
        int _stageNumber;

        BubbleColor _lastPoppedColor = BubbleColor.None;
        readonly List<BubbleData> _bubbles = new();

        const int MaxBubbles = 25;
        const float GhostRevealTime = 0.8f;
        const float BubbleRadius = 0.45f;

        void Awake()
        {
            _mainCam = Camera.main;
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            ClearAllBubbles();
            _isActive = false;
            _stageNumber = stageNumber;

            // Map stage parameters
            _riseSpeed = config.speedMultiplier;
            float intervalBase = stageNumber switch
            {
                1 => 1.8f,
                2 => 1.4f,
                3 => 1.2f,
                4 => 1.0f,
                _ => 0.8f
            };
            _spawnInterval = intervalBase / Mathf.Max(config.countMultiplier, 0.5f);

            _colorCount = stageNumber >= 2 ? 3 : 1;
            _ironRatio  = stageNumber >= 3 ? 0.15f : 0f;
            _splitRatio = stageNumber >= 4 ? 0.20f : 0f;
            _ghostRatio = stageNumber >= 5 ? 0.10f : 0f;

            _stageDuration = stageNumber <= 2 ? 30f : 60f;
            _stageTimer = _stageDuration;
            _spawnTimer = 0f;
            _lastPoppedColor = BubbleColor.None;
            _isActive = true;

            LoadSprites();
        }

        void LoadSprites()
        {
            if (_sprRed != null) return; // Already loaded
            _sprRed   = Resources.Load<Sprite>("Sprites/Game024v2_BubblePop/BubbleRed");
            _sprBlue  = Resources.Load<Sprite>("Sprites/Game024v2_BubblePop/BubbleBlue");
            _sprGreen = Resources.Load<Sprite>("Sprites/Game024v2_BubblePop/BubbleGreen");
            _sprIron  = Resources.Load<Sprite>("Sprites/Game024v2_BubblePop/BubbleIron");
            _sprSplit = Resources.Load<Sprite>("Sprites/Game024v2_BubblePop/BubbleSplit");
            _sprGhost = Resources.Load<Sprite>("Sprites/Game024v2_BubblePop/BubbleGhost");
        }

        public void StopGame()
        {
            _isActive = false;
            ClearAllBubbles();
        }

        void ClearAllBubbles()
        {
            foreach (var b in _bubbles)
            {
                if (b.obj != null) Destroy(b.obj);
            }
            _bubbles.Clear();
        }

        void Update()
        {
            if (!_isActive) return;

            _stageTimer -= Time.deltaTime;
            if (_stageTimer <= 0f)
            {
                _isActive = false;
                _gameManager.OnStageTimeUp();
                return;
            }

            // Spawn
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _spawnInterval && _bubbles.Count < MaxBubbles)
            {
                _spawnTimer = 0f;
                SpawnBubble();
            }

            // Update bubbles
            for (int i = _bubbles.Count - 1; i >= 0; i--)
            {
                var b = _bubbles[i];
                if (!b.isAlive) { _bubbles.RemoveAt(i); continue; }
                if (b.obj == null) { _bubbles.RemoveAt(i); continue; }

                // Rise
                b.obj.transform.position += Vector3.up * b.riseSpeed * Time.deltaTime;

                // Ghost reveal timer
                if (b.type == BubbleType.Ghost)
                {
                    b.ghostRevealTimer -= Time.deltaTime;
                    if (!b.colorRevealed)
                    {
                        if (b.ghostRevealTimer > 0f)
                        {
                            b.sr.color = GetBubbleColor(b.originalColor);
                        }
                        else
                        {
                            // Hide color and mark as revealed (won't repeat)
                            b.sr.color = new Color(0.6f, 0.8f, 1f, 0.4f);
                            b.colorRevealed = true;
                        }
                    }
                }

                // Check escape
                float topY = _mainCam.orthographicSize - 0.8f;
                if (b.obj.transform.position.y > topY)
                {
                    Destroy(b.obj);
                    _bubbles.RemoveAt(i);
                    _gameManager.OnBubbleEscaped();
                }
            }

            // Input
            HandleInput();
        }

        void HandleInput()
        {
            Vector2 screenPos = Vector2.zero;
            bool fired = false;

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
                fired = true;
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPos = Mouse.current.position.ReadValue();
                fired = true;
            }
            if (!fired) return;
            Vector3 worldPos = _mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            Vector2 world2D = new Vector2(worldPos.x, worldPos.y);

            for (int i = _bubbles.Count - 1; i >= 0; i--)
            {
                var b = _bubbles[i];
                if (!b.isAlive || b.obj == null) continue;

                if (Vector2.Distance(world2D, b.obj.transform.position) <= BubbleRadius)
                {
                    TapBubble(b, i);
                    break;
                }
            }
        }

        void TapBubble(BubbleData b, int idx)
        {
            if (b.type == BubbleType.Iron)
            {
                b.hitCount++;
                StartCoroutine(IronHitFlash(b));
                if (b.hitCount < 2) return; // Need 2 hits
            }

            BubbleColor effectiveColor = b.type == BubbleType.Ghost ? b.originalColor : b.color;

            // Split bubble spawns 2 children
            if (b.type == BubbleType.Split)
            {
                float splitElapsed = Time.time - b.spawnTime;
                _gameManager.OnBubblePopped(b.type, effectiveColor, splitElapsed, _lastPoppedColor);
                _lastPoppedColor = effectiveColor;
                Vector3 splitPos = b.obj.transform.position;
                StartCoroutine(PopAnimation(b));
                b.isAlive = false;
                _bubbles.RemoveAt(idx);
                SpawnSplitChildren(splitPos);
            }
            else
            {
                float elapsed = Time.time - b.spawnTime;
                _gameManager.OnBubblePopped(b.type, effectiveColor, elapsed, _lastPoppedColor);
                _lastPoppedColor = effectiveColor;
                StartCoroutine(PopAnimation(b));
                b.isAlive = false;
                _bubbles.RemoveAt(idx);
            }
        }

        void SpawnSplitChildren(Vector3 pos)
        {
            // Two normal bubbles of random colors
            for (int i = 0; i < 2; i++)
            {
                float offset = i == 0 ? -0.4f : 0.4f;
                var child = CreateBubbleObject(BubbleType.Normal, RandomColor(), pos + new Vector3(offset, 0f, 0f));
                child.riseSpeed = _riseSpeed * 1.2f;
                _bubbles.Add(child);
            }
        }

        void SpawnBubble()
        {
            float camWidth = _mainCam.orthographicSize * _mainCam.aspect;
            float spawnY = -_mainCam.orthographicSize + 0.5f;
            float spawnX = Random.Range(-camWidth * 0.8f, camWidth * 0.8f);

            BubbleType type = PickBubbleType();
            BubbleColor color = type == BubbleType.Iron ? BubbleColor.None : RandomColor();

            var bd = CreateBubbleObject(type, color, new Vector3(spawnX, spawnY, 0f));
            bd.riseSpeed = _riseSpeed * Random.Range(0.85f, 1.15f);
            _bubbles.Add(bd);
        }

        BubbleData CreateBubbleObject(BubbleType type, BubbleColor color, Vector3 pos)
        {
            var go = new GameObject($"Bubble_{type}_{color}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;

            var bd = new BubbleData
            {
                obj = go, sr = sr,
                type = type, color = color,
                spawnTime = Time.time,
                isAlive = true, hitCount = 0
            };

            switch (type)
            {
                case BubbleType.Iron:
                    sr.sprite = _sprIron;
                    sr.color = Color.white;
                    go.transform.localScale = Vector3.one * 0.85f;
                    break;
                case BubbleType.Split:
                    sr.sprite = _sprSplit;
                    sr.color = GetBubbleColor(color);
                    go.transform.localScale = Vector3.one * 1.1f;
                    break;
                case BubbleType.Ghost:
                    bd.originalColor = color;
                    sr.sprite = _sprGhost;
                    sr.color = new Color(0.6f, 0.8f, 1f, 0.4f);
                    bd.ghostRevealTimer = GhostRevealTime;
                    go.transform.localScale = Vector3.one * 0.9f;
                    break;
                default:
                    sr.sprite = color == BubbleColor.Red ? _sprRed
                               : color == BubbleColor.Blue ? _sprBlue : _sprGreen;
                    sr.color = Color.white;
                    break;
            }

            // Add circle collider for overlap detection
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = BubbleRadius;
            col.isTrigger = true;

            return bd;
        }

        BubbleType PickBubbleType()
        {
            float r = Random.value;
            if (r < _ghostRatio) return BubbleType.Ghost;
            r -= _ghostRatio;
            if (r < _splitRatio) return BubbleType.Split;
            r -= _splitRatio;
            if (r < _ironRatio) return BubbleType.Iron;
            return BubbleType.Normal;
        }

        BubbleColor RandomColor()
        {
            int n = _colorCount;
            int r = Random.Range(0, n);
            return r == 0 ? BubbleColor.Red : r == 1 ? BubbleColor.Blue : BubbleColor.Green;
        }

        Color GetBubbleColor(BubbleColor c)
        {
            return c switch
            {
                BubbleColor.Red => new Color(1f, 0.4f, 0.3f),
                BubbleColor.Blue => new Color(0.3f, 0.6f, 1f),
                BubbleColor.Green => new Color(0.3f, 0.9f, 0.4f),
                _ => Color.white
            };
        }

        IEnumerator PopAnimation(BubbleData b)
        {
            if (b.obj == null) yield break;
            float t = 0f;
            Vector3 startScale = b.obj.transform.localScale;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.25f;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(1f, 1.4f, ratio * 2f)
                    : Mathf.Lerp(1.4f, 0f, (ratio - 0.5f) * 2f);
                if (b.obj != null)
                {
                    b.obj.transform.localScale = startScale * scale;
                    if (b.sr != null)
                    {
                        var c = b.sr.color;
                        c.a = 1f - ratio;
                        b.sr.color = c;
                    }
                }
                yield return null;
            }
            if (b.obj != null) Destroy(b.obj);
        }

        IEnumerator IronHitFlash(BubbleData b)
        {
            if (b.sr == null) yield break;
            b.sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            b.sr.color = new Color(0.9f, 0.2f, 0.2f);
            yield return new WaitForSeconds(0.05f);
            b.sr.color = Color.white;
        }

        void OnDestroy()
        {
            ClearAllBubbles();
        }
    }
}
