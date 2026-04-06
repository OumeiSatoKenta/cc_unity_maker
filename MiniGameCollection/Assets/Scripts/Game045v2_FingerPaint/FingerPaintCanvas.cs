using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Game045v2_FingerPaint
{
    /// <summary>
    /// 描画キャンバスのコアメカニクス。Texture2Dベースで描画を管理する。
    /// </summary>
    public class FingerPaintCanvas : MonoBehaviour
    {
        [SerializeField] FingerPaintGameManager _gameManager;
        [SerializeField] SpriteRenderer _canvasRenderer;
        [SerializeField] SpriteRenderer _templateRenderer;

        private const int TexSize = 512;
        private Texture2D _canvasTex;
        private Texture2D _templateTex;
        private Sprite _canvasSprite;
        private Sprite _templateSprite;

        // Brush state
        private Color _currentColor = Color.black;
        private bool _isEraserMode = false;
        private bool _isThinBrush = false;
        private int BrushRadius => _isEraserMode ? 20 : (_isThinBrush ? 6 : 18);

        // Ink
        private float _inkAmount = 1.0f;
        private float _maxInk = 1.0f;
        private float _inkPerPixel; // ink consumed per pixel drawn
        private const float InkPerPixelBase = 0.00002f;

        // Match rate
        private float _cachedMatchRate = 0f;
        private float _matchUpdateInterval = 0.5f;
        private float _matchTimer = 0f;

        // Double tap detection
        private float _lastTapTime = -1f;
        private const float DoubleTapThreshold = 0.35f;
        private bool _templateVisible = true;

        // Drawing state
        private bool _isDrawing = false;
        private Vector2 _lastDrawPos;
        private bool _isActive = false;

        // Stage5: fading ink
        private bool _inkFades = false;
        private float _fadingInkTimer = 0f;
        private const float FadeDelay = 30f;

        // Color accuracy combo tracking
        private Color[] _paletteColors;
        private int _currentStageIndex;

        private void Awake()
        {
            _canvasTex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
            _canvasTex.filterMode = FilterMode.Bilinear;
            _templateTex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
            _templateTex.filterMode = FilterMode.Bilinear;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex, float inkAmount)
        {
            _currentStageIndex = stageIndex;
            _isActive = true;
            _isDrawing = false;
            _inkFades = (stageIndex >= 4);
            _fadingInkTimer = 0f;

            // Ink setup
            _maxInk = inkAmount;
            _inkAmount = inkAmount;
            _inkPerPixel = InkPerPixelBase;

            // Setup palette colors for stage
            SetupPaletteForStage(stageIndex);

            // Clear canvas
            ClearTexture(_canvasTex, new Color(1f, 1f, 1f, 0f));

            // Generate template pattern
            GenerateTemplate(stageIndex);

            // Update sprites
            _canvasTex.Apply();
            _templateTex.Apply();
            UpdateSprites();

            // Show template
            _templateVisible = true;
            if (_templateRenderer != null)
                _templateRenderer.color = new Color(1f, 1f, 1f, 0.35f);

            _cachedMatchRate = 0f;
        }

        private void SetupPaletteForStage(int stageIndex)
        {
            var colors = GetPaletteColors(stageIndex);
            _paletteColors = colors;
            _currentColor = colors[0];
        }

        public static Color[] GetPaletteColors(int stageIndex)
        {
            // Each stage unlocks more colors
            Color[] all = {
                new Color(0.20f, 0.60f, 0.86f, 1f), // blue
                new Color(0.95f, 0.35f, 0.25f, 1f), // red
                new Color(0.25f, 0.75f, 0.35f, 1f), // green
                new Color(0.95f, 0.75f, 0.10f, 1f), // yellow
                new Color(0.65f, 0.30f, 0.85f, 1f), // purple
            };
            int count = Mathf.Clamp(stageIndex + 1, 1, all.Length);
            Color[] result = new Color[count];
            for (int i = 0; i < count; i++) result[i] = all[i];
            return result;
        }

        private void GenerateTemplate(int stageIndex)
        {
            ClearTexture(_templateTex, new Color(1f, 1f, 1f, 0f));
            Color[] palette = GetPaletteColors(stageIndex);

            int cx = TexSize / 2;
            int cy = TexSize / 2;

            switch (stageIndex)
            {
                case 0: // Single-color circle
                    DrawCircleOnTex(_templateTex, cx, cy, 180, palette[0]);
                    break;
                case 1: // 2-color simple pattern
                    DrawCircleOnTex(_templateTex, cx - 80, cy, 130, palette[0]);
                    DrawCircleOnTex(_templateTex, cx + 80, cy, 130, palette[1]);
                    break;
                case 2: // 3-color animal silhouette (simplified cat)
                    DrawCircleOnTex(_templateTex, cx, cy + 40, 140, palette[0]); // body
                    DrawCircleOnTex(_templateTex, cx, cy - 100, 90, palette[1]); // head
                    DrawEllipseOnTex(_templateTex, cx - 55, cy - 160, 30, 50, palette[2]); // ear L
                    DrawEllipseOnTex(_templateTex, cx + 55, cy - 160, 30, 50, palette[2]); // ear R
                    break;
                case 3: // 4-color landscape
                    DrawRectOnTex(_templateTex, 0, 0, TexSize, TexSize / 2, palette[0]); // sky
                    DrawRectOnTex(_templateTex, 0, TexSize / 2, TexSize, TexSize, palette[1]); // ground
                    DrawTriangleOnTex(_templateTex, cx - 130, TexSize / 2, cx, TexSize / 4, cx + 130, TexSize / 2, palette[2]); // mountain
                    DrawCircleOnTex(_templateTex, cx + 150, cy - 100, 60, palette[3]); // sun
                    break;
                case 4: // 5-color complex
                    DrawRectOnTex(_templateTex, 0, 0, TexSize, TexSize / 3, palette[0]); // sky
                    DrawRectOnTex(_templateTex, 0, TexSize * 2 / 3, TexSize, TexSize, palette[1]); // ground
                    DrawTriangleOnTex(_templateTex, cx - 160, TexSize * 2 / 3, cx - 30, TexSize / 4, cx + 100, TexSize * 2 / 3, palette[2]); // mountain1
                    DrawTriangleOnTex(_templateTex, cx - 60, TexSize * 2 / 3, cx + 80, TexSize / 3, cx + 210, TexSize * 2 / 3, palette[2]); // mountain2
                    DrawCircleOnTex(_templateTex, cx - 150, TexSize / 5, 50, palette[3]); // sun
                    DrawCircleOnTex(_templateTex, cx + 140, TexSize / 5, 35, palette[3]);
                    DrawCircleOnTex(_templateTex, cx, TexSize / 3, 30, palette[4]); // cloud
                    DrawCircleOnTex(_templateTex, cx + 40, TexSize / 3 - 10, 30, palette[4]);
                    DrawCircleOnTex(_templateTex, cx - 40, TexSize / 3 - 10, 30, palette[4]);
                    break;
            }
            _templateTex.Apply();
        }

        private void Update()
        {
            if (!_isActive) return;

            HandleInput();
            UpdateMatchRate();

            if (_inkFades)
            {
                _fadingInkTimer += Time.deltaTime;
                if (_fadingInkTimer >= FadeDelay)
                {
                    FadeCanvas();
                }
            }
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;

            bool pressed = Mouse.current.leftButton.isPressed;
            bool justPressed = Mouse.current.leftButton.wasPressedThisFrame;
            bool justReleased = Mouse.current.leftButton.wasReleasedThisFrame;

            if (justPressed)
            {
                // Double tap check
                float now = Time.time;
                if (now - _lastTapTime < DoubleTapThreshold)
                {
                    ToggleTemplate();
                    _lastTapTime = -1f;
                    return;
                }
                _lastTapTime = now;

                Vector2 worldPos = GetWorldPos();
                if (IsOnCanvas(worldPos))
                {
                    _isDrawing = true;
                    _lastDrawPos = WorldToTexCoord(worldPos);
                    DrawAtPos(_lastDrawPos);
                    _canvasTex.Apply();
                }
            }

            if (pressed && _isDrawing)
            {
                Vector2 worldPos = GetWorldPos();
                if (IsOnCanvas(worldPos))
                {
                    Vector2 texPos = WorldToTexCoord(worldPos);
                    DrawLine(_lastDrawPos, texPos);
                    _lastDrawPos = texPos;
                }
            }

            if (justReleased)
            {
                _isDrawing = false;
            }
        }

        private Vector2 GetWorldPos()
        {
            if (Camera.main == null) return Vector2.zero;
            Vector3 screen = Mouse.current.position.ReadValue();
            return Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
        }

        private bool IsOnCanvas(Vector2 worldPos)
        {
            if (_canvasRenderer == null) return false;
            var bounds = _canvasRenderer.bounds;
            return bounds.Contains(new Vector3(worldPos.x, worldPos.y, 0f));
        }

        private Vector2 WorldToTexCoord(Vector2 worldPos)
        {
            var bounds = _canvasRenderer.bounds;
            float nx = (worldPos.x - bounds.min.x) / bounds.size.x;
            float ny = (worldPos.y - bounds.min.y) / bounds.size.y;
            return new Vector2(nx * TexSize, ny * TexSize);
        }

        private void DrawLine(Vector2 from, Vector2 to)
        {
            float dist = Vector2.Distance(from, to);
            int steps = Mathf.Max(1, Mathf.RoundToInt(dist / 3f));
            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0f : (float)i / steps;
                DrawAtPos(Vector2.Lerp(from, to, t));
            }
            _canvasTex.Apply();
        }

        private void DrawAtPos(Vector2 texPos)
        {
            if (_inkAmount <= 0f && !_isEraserMode) return;

            int px = Mathf.RoundToInt(texPos.x);
            int py = Mathf.RoundToInt(texPos.y);
            int r = BrushRadius;
            Color drawColor = _isEraserMode ? Color.clear : _currentColor;

            // Ink consumption (eraser uses ink x3, but still allowed at 0)
            float inkCost = _isEraserMode ? _inkPerPixel * 3f : _inkPerPixel;
            int pixelCount = 0;

            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    if (dx * dx + dy * dy > r * r) continue;
                    int tx = px + dx;
                    int ty = py + dy;
                    if (tx < 0 || tx >= TexSize || ty < 0 || ty >= TexSize) continue;

                    float alpha = 1f - (float)Mathf.Sqrt(dx * dx + dy * dy) / r * 0.4f;
                    if (_isEraserMode)
                        _canvasTex.SetPixel(tx, ty, Color.clear);
                    else
                        _canvasTex.SetPixel(tx, ty, new Color(drawColor.r, drawColor.g, drawColor.b, alpha));
                    pixelCount++;
                }
            }

            _inkAmount = Mathf.Max(0f, _inkAmount - inkCost * pixelCount);

            // Color accuracy combo (Stage2+)
            if (_currentStageIndex >= 1 && !_isEraserMode)
            {
                CheckColorAccuracy(px, py);
            }
        }

        private void CheckColorAccuracy(int px, int py)
        {
            if (px < 0 || px >= TexSize || py < 0 || py >= TexSize) return;
            Color templateColor = _templateTex.GetPixel(px, py);
            if (templateColor.a < 0.1f) return; // not on template

            float tolerance = 0.25f;
            bool correct = ColorDistance(templateColor, _currentColor) < tolerance;
            if (correct)
                _gameManager.AddCombo();
            else
                _gameManager.ResetCombo();
        }

        private float ColorDistance(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
        }

        private void UpdateMatchRate()
        {
            _matchTimer += Time.deltaTime;
            if (_matchTimer < _matchUpdateInterval) return;
            _matchTimer = 0f;

            int total = 0;
            int matched = 0;
            float tolerance = 0.3f;
            int sampleStep = 8; // sample every 8 pixels for performance

            for (int x = 0; x < TexSize; x += sampleStep)
            {
                for (int y = 0; y < TexSize; y += sampleStep)
                {
                    Color tmpl = _templateTex.GetPixel(x, y);
                    if (tmpl.a < 0.1f) continue;
                    total++;
                    Color canvas = _canvasTex.GetPixel(x, y);
                    if (canvas.a > 0.3f && ColorDistance(tmpl, canvas) < tolerance)
                        matched++;
                }
            }

            _cachedMatchRate = total > 0 ? (float)matched / total : 0f;
        }

        private void FadeCanvas()
        {
            // Gradually fade drawn pixels (Stage5)
            float fadeAmount = Time.deltaTime * 0.05f;
            bool changed = false;
            for (int x = 0; x < TexSize; x += 4)
            {
                for (int y = 0; y < TexSize; y += 4)
                {
                    Color c = _canvasTex.GetPixel(x, y);
                    if (c.a > 0.01f)
                    {
                        c.a = Mathf.Max(0f, c.a - fadeAmount);
                        _canvasTex.SetPixel(x, y, c);
                        changed = true;
                    }
                }
            }
            if (changed) _canvasTex.Apply();
        }

        public void SetColor(Color color) { _currentColor = color; }
        public void SetEraserMode(bool eraser) { _isEraserMode = eraser; }
        public void SetThinBrush(bool thin) { _isThinBrush = thin; }
        public void SetActive(bool active) { _isActive = active; }
        public float GetMatchRate() => _cachedMatchRate;
        public float GetInkAmount() => _inkAmount / Mathf.Max(_maxInk, 0.001f);

        private void ToggleTemplate()
        {
            _templateVisible = !_templateVisible;
            if (_templateRenderer != null)
                _templateRenderer.color = new Color(1f, 1f, 1f, _templateVisible ? 0.35f : 0f);
        }

        private void UpdateSprites()
        {
            if (_canvasRenderer != null)
            {
                if (_canvasSprite != null) Destroy(_canvasSprite);
                _canvasSprite = Sprite.Create(_canvasTex, new Rect(0, 0, TexSize, TexSize), Vector2.one * 0.5f, TexSize / 4f);
                _canvasRenderer.sprite = _canvasSprite;
            }
            if (_templateRenderer != null)
            {
                if (_templateSprite != null) Destroy(_templateSprite);
                _templateSprite = Sprite.Create(_templateTex, new Rect(0, 0, TexSize, TexSize), Vector2.one * 0.5f, TexSize / 4f);
                _templateRenderer.sprite = _templateSprite;
                _templateRenderer.color = new Color(1f, 1f, 1f, 0.35f);
            }
        }

        // --- Texture drawing helpers ---
        private void ClearTexture(Texture2D tex, Color color)
        {
            Color[] pixels = new Color[TexSize * TexSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
        }

        private void DrawCircleOnTex(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx * dx + dy * dy > radius * radius) continue;
                    int tx = cx + dx;
                    int ty = cy + dy;
                    if (tx >= 0 && tx < TexSize && ty >= 0 && ty < TexSize)
                        tex.SetPixel(tx, ty, color);
                }
        }

        private void DrawEllipseOnTex(Texture2D tex, int cx, int cy, int rx, int ry, Color color)
        {
            for (int dx = -rx; dx <= rx; dx++)
                for (int dy = -ry; dy <= ry; dy++)
                {
                    float nx = (float)dx / rx;
                    float ny = (float)dy / ry;
                    if (nx * nx + ny * ny > 1f) continue;
                    int tx = cx + dx;
                    int ty = cy + dy;
                    if (tx >= 0 && tx < TexSize && ty >= 0 && ty < TexSize)
                        tex.SetPixel(tx, ty, color);
                }
        }

        private void DrawRectOnTex(Texture2D tex, int x1, int y1, int x2, int y2, Color color)
        {
            for (int x = x1; x < x2; x++)
                for (int y = y1; y < y2; y++)
                    if (x >= 0 && x < TexSize && y >= 0 && y < TexSize)
                        tex.SetPixel(x, y, color);
        }

        private void DrawTriangleOnTex(Texture2D tex, int x0, int y0, int x1, int y1, int x2, int y2, Color color)
        {
            int minX = Mathf.Max(0, Mathf.Min(x0, Mathf.Min(x1, x2)));
            int maxX = Mathf.Min(TexSize - 1, Mathf.Max(x0, Mathf.Max(x1, x2)));
            int minY = Mathf.Max(0, Mathf.Min(y0, Mathf.Min(y1, y2)));
            int maxY = Mathf.Min(TexSize - 1, Mathf.Max(y0, Mathf.Max(y1, y2)));

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    if (PointInTriangle(x, y, x0, y0, x1, y1, x2, y2))
                        tex.SetPixel(x, y, color);
        }

        private bool PointInTriangle(int px, int py, int x0, int y0, int x1, int y1, int x2, int y2)
        {
            float d1 = Sign(px, py, x0, y0, x1, y1);
            float d2 = Sign(px, py, x1, y1, x2, y2);
            float d3 = Sign(px, py, x2, y2, x0, y0);
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        private float Sign(int px, int py, int x1, int y1, int x2, int y2)
            => (px - x2) * (y1 - y2) - (x1 - x2) * (py - y2);

        private void OnDestroy()
        {
            if (_canvasTex != null) Destroy(_canvasTex);
            if (_templateTex != null) Destroy(_templateTex);
            if (_canvasSprite != null) Destroy(_canvasSprite);
            if (_templateSprite != null) Destroy(_templateSprite);
        }
    }
}
