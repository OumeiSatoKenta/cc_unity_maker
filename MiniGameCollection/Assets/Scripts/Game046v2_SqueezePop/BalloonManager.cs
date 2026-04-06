using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game046v2_SqueezePop
{
    public class BalloonManager : MonoBehaviour
    {
        [SerializeField] SqueezePopGameManager _gameManager;
        [SerializeField] Sprite _balloonSprite;
        [SerializeField] Sprite _bombSprite;
        [SerializeField] Sprite _shieldSprite;
        [SerializeField] Sprite _perfectEffectSprite;

        public int RemainingCount { get; private set; }

        private enum BalloonType { Normal, Bomb, Shield }
        private enum BalloonSize { Small, Medium, Large }

        private class BalloonItem
        {
            public GameObject go;
            public SpriteRenderer sr;
            public BalloonType type;
            public BalloonSize size;
            public float inflateRatio; // 0.0 ~ 1.0+
            public bool isPopped;
            public bool shieldUsed; // Shield: requires 2 presses
            public Vector2 moveDir;
            public float baseScale;
        }

        private List<BalloonItem> _balloons = new List<BalloonItem>();
        private BalloonItem _pressedBalloon;
        private float _inflateSpeed;
        private float _explodeTime; // seconds to reach explode from 0
        private bool _hasChain;
        private bool _hasMoving;
        private bool _isActive;
        private float _lastPopTime;
        private int _lastPopIndex = -1;
        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
            if (_cam == null) Debug.LogError("[BalloonManager] Main Camera が見つかりません。");
        }

        public void SetupStage(int count, float inflateSpeed, float explodeTime,
            int bombCount, bool hasMoving, bool hasSizeVariety, bool hasChain)
        {
            StopAll();
            _inflateSpeed = inflateSpeed;
            _explodeTime = explodeTime;
            _hasChain = hasChain;
            _hasMoving = hasMoving;
            _isActive = true;

            foreach (var b in _balloons)
                if (b.go != null) Destroy(b.go);
            _balloons.Clear();

            SpawnBalloons(count, bombCount, hasSizeVariety, hasMoving);
            RemainingCount = count;
        }

        private void SpawnBalloons(int count, int bombCount, bool hasSizeVariety, bool hasMoving)
        {
            float camSize = _cam.orthographicSize;
            float camWidth = camSize * _cam.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 2.8f;
            float availH = camSize * 2f - topMargin - bottomMargin;
            float availW = camWidth * 2f - 0.5f;

            int cols = count <= 8 ? 4 : (count <= 16 ? 4 : 5);
            int rows = Mathf.CeilToInt((float)count / cols);
            float cellW = availW / cols;
            float cellH = availH / Mathf.Max(rows, 1);
            float cellSize = Mathf.Min(cellW, cellH) * 0.85f;

            float startX = -availW / 2f + cellW / 2f;
            float startY = (camSize - topMargin) - cellH / 2f;

            List<int> bombIndices = new List<int>();
            while (bombIndices.Count < bombCount && bombIndices.Count < count)
            {
                int idx = Random.Range(0, count);
                if (!bombIndices.Contains(idx)) bombIndices.Add(idx);
            }

            for (int i = 0; i < count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                float x = startX + col * cellW + Random.Range(-cellW * 0.1f, cellW * 0.1f);
                float y = startY - row * cellH + Random.Range(-cellH * 0.1f, cellH * 0.1f);

                BalloonType btype = bombIndices.Contains(i) ? BalloonType.Bomb
                    : (hasMoving && i == count - 1 && _shieldSprite != null ? BalloonType.Shield : BalloonType.Normal);
                BalloonSize bsize = hasSizeVariety
                    ? (BalloonSize)Random.Range(0, 3)
                    : BalloonSize.Medium;

                float sizeScale = bsize == BalloonSize.Small ? 0.7f : (bsize == BalloonSize.Large ? 1.3f : 1.0f);
                float baseScale = cellSize * sizeScale;

                var go = new GameObject($"Balloon_{i}");
                go.transform.position = new Vector3(x, y, 0f);
                go.transform.localScale = Vector3.one * baseScale;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = btype == BalloonType.Bomb ? _bombSprite
                    : (btype == BalloonType.Shield ? _shieldSprite : _balloonSprite);
                sr.sortingOrder = 5;

                var item = new BalloonItem
                {
                    go = go,
                    sr = sr,
                    type = btype,
                    size = bsize,
                    inflateRatio = 0f,
                    isPopped = false,
                    shieldUsed = false,
                    baseScale = baseScale,
                    moveDir = hasMoving ? Random.insideUnitCircle.normalized * 0.8f : Vector2.zero
                };
                _balloons.Add(item);
            }
        }

        private void Update()
        {
            if (!_isActive) return;

            HandleInput();
            UpdateBalloons();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 mousePos = mouse.position.ReadValue();
            Vector3 worldPos = _cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
            Vector2 world2D = new Vector2(worldPos.x, worldPos.y);

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _pressedBalloon = null;
                foreach (var b in _balloons)
                {
                    if (b.isPopped || b.go == null) continue;
                    float radius = b.go.transform.localScale.x * 0.5f;
                    if (Vector2.Distance(world2D, b.go.transform.position) < radius)
                    {
                        _pressedBalloon = b;
                        break;
                    }
                }
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                if (_pressedBalloon != null && !_pressedBalloon.isPopped)
                {
                    ReleaseBalloon(_pressedBalloon, world2D);
                    _pressedBalloon = null;
                }
            }
        }

        private void ReleaseBalloon(BalloonItem b, Vector2 clickPos)
        {
            if (b.go == null) return;
            // Perfect window: 0.4 ~ 0.7 of explodeTime
            float perfectMin = 0.4f;
            float perfectMax = 0.7f;
            float explodeThreshold = 1.0f;
            float tooSmallThreshold = 0.15f;

            if (b.inflateRatio < tooSmallThreshold)
            {
                // Too small - fail (deflate)
                StartCoroutine(DeflateAnimation(b));
                _gameManager.OnBalloonFailed();
                return;
            }

            if (b.inflateRatio >= explodeThreshold)
            {
                // Already exploded - handled in Update
                return;
            }

            bool isPerfect = b.inflateRatio >= perfectMin && b.inflateRatio <= perfectMax;
            PopBalloon(b, isPerfect, false, clickPos);
        }

        private void PopBalloon(BalloonItem b, bool isPerfect, bool isBombExplosion, Vector2 origin)
        {
            if (b.isPopped) return;

            // Shield check
            if (b.type == BalloonType.Shield && !b.shieldUsed && !isBombExplosion)
            {
                b.shieldUsed = true;
                b.inflateRatio = 0f;
                b.sr.color = new Color(0.7f, 0.9f, 1.0f, 0.8f);
                return;
            }

            b.isPopped = true;
            RemainingCount--;

            int idx = _balloons.IndexOf(b);
            float popTime = Time.time;

            // Chain check
            bool isChain = _hasChain && (popTime - _lastPopTime < 1.0f) &&
                           IsAdjacent(idx, _lastPopIndex);
            _lastPopTime = popTime;
            _lastPopIndex = idx;

            _gameManager.OnBalloonPopped(isPerfect, isBombExplosion);

            // Bomb explosion: destroy neighbors
            if (b.type == BalloonType.Bomb && b.inflateRatio >= 1.0f)
            {
                ExplodeBomb(b, idx);
            }

            StartCoroutine(PopAnimation(b, isPerfect));
        }

        private bool IsAdjacent(int a, int b)
        {
            if (a < 0 || b < 0) return false;
            if (a >= _balloons.Count || b >= _balloons.Count) return false;
            var posA = _balloons[a].go != null ? (Vector2)_balloons[a].go.transform.position : Vector2.zero;
            var posB = _balloons[b].go != null ? (Vector2)_balloons[b].go.transform.position : Vector2.zero;
            return Vector2.Distance(posA, posB) < 1.8f;
        }

        private void ExplodeBomb(BalloonItem bomb, int bombIdx)
        {
            for (int i = 0; i < _balloons.Count; i++)
            {
                if (i == bombIdx) continue;
                var b = _balloons[i];
                if (b.isPopped || b.go == null) continue;
                if (IsAdjacent(bombIdx, i))
                {
                    PopBalloon(b, false, true, b.go.transform.position);
                }
            }
        }

        private void UpdateBalloons()
        {
            var mouse = Mouse.current;
            bool isPressed = mouse != null && mouse.leftButton.isPressed;

            for (int i = 0; i < _balloons.Count; i++)
            {
                var b = _balloons[i];
                if (b.isPopped || b.go == null) continue;

                // Inflate if pressed
                if (isPressed && _pressedBalloon == b)
                {
                    b.inflateRatio += _inflateSpeed * Time.deltaTime / _explodeTime;
                    float s = b.baseScale * (1f + b.inflateRatio * 0.5f);
                    b.go.transform.localScale = Vector3.one * s;

                    // Color shift towards explode
                    float t = Mathf.Clamp01(b.inflateRatio);
                    b.sr.color = Color.Lerp(Color.white, new Color(1f, 0.4f, 0.4f), Mathf.Max(0, (t - 0.7f) / 0.3f));

                    // Auto explode
                    if (b.inflateRatio >= 1.0f)
                    {
                        _pressedBalloon = null;
                        PopBalloon(b, false, false, b.go.transform.position);
                    }
                }

                // Moving balloons
                if (_hasMoving && b.moveDir != Vector2.zero && _pressedBalloon != b)
                {
                    b.go.transform.position += (Vector3)(b.moveDir * Time.deltaTime);
                    float camSize = _cam.orthographicSize;
                    float camW = camSize * _cam.aspect;
                    var pos = b.go.transform.position;
                    if (Mathf.Abs(pos.x) > camW - 0.5f) b.moveDir.x *= -1;
                    if (pos.y > camSize - 1.5f || pos.y < -camSize + 2.8f) b.moveDir.y *= -1;
                }
            }
        }

        private IEnumerator PopAnimation(BalloonItem b, bool isPerfect)
        {
            if (b.go == null) yield break;

            float duration = 0.25f;
            float elapsed = 0f;
            Vector3 startScale = b.go.transform.localScale;
            Vector3 targetScale = startScale * 1.5f;

            if (isPerfect && _perfectEffectSprite != null)
            {
                var efx = new GameObject("PerfectEffect");
                efx.transform.position = b.go.transform.position;
                var efxSr = efx.AddComponent<SpriteRenderer>();
                efxSr.sprite = _perfectEffectSprite;
                efxSr.sortingOrder = 10;
                efxSr.transform.localScale = Vector3.one * b.baseScale * 1.5f;
                Destroy(efx, 0.6f);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float s = Mathf.Lerp(1f, 0f, t * t);
                if (b.go != null)
                    b.go.transform.localScale = startScale * s;
                yield return null;
            }

            if (b.go != null) Destroy(b.go);
        }

        private IEnumerator DeflateAnimation(BalloonItem b)
        {
            float duration = 0.3f;
            float elapsed = 0f;
            float startRatio = b.inflateRatio;
            Vector3 startScale = b.go != null ? b.go.transform.localScale : Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                b.inflateRatio = Mathf.Lerp(startRatio, 0f, t);
                if (b.go != null)
                    b.go.transform.localScale = Vector3.Lerp(startScale, Vector3.one * b.baseScale, t);
                yield return null;
            }

            if (b.go != null)
            {
                b.go.transform.localScale = Vector3.one * b.baseScale;
                b.sr.color = Color.white;
            }
            b.inflateRatio = 0f;
        }

        public void StopAll()
        {
            _isActive = false;
            _pressedBalloon = null;
            StopAllCoroutines();
        }
    }
}
