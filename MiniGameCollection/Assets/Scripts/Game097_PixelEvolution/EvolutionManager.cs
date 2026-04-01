using UnityEngine;

namespace Game097_PixelEvolution
{
    public class EvolutionManager : MonoBehaviour
    {
        private SpriteRenderer _creatureRenderer;
        private Texture2D _currentTexture;
        private Sprite _currentSprite;
        private Coroutine _pulseCoroutine;
        private float _displayScale = 3f;

        private void Awake()
        {
            var creatureObj = new GameObject("Creature");
            creatureObj.transform.SetParent(transform);
            creatureObj.transform.position = new Vector3(0f, 0.5f, 0f);
            _creatureRenderer = creatureObj.AddComponent<SpriteRenderer>();
            _creatureRenderer.sortingOrder = 5;
        }

        public void UpdateCreature(int generation, int[] choices, int choiceCount)
        {
            int size = 1 + generation * 2; // gen1=3, gen2=5, ... gen10=21
            var pattern = GeneratePattern(size, generation, choices, choiceCount);
            var color = GetCreatureColor(choices, choiceCount, generation);
            ApplyPixelArt(pattern, size, color);

            // スケールパルスアニメーション
            var t = _creatureRenderer.transform;
            t.localScale = Vector3.one * (_displayScale * 1.2f);
            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = StartCoroutine(PulseAnimation(t));
        }

        private System.Collections.IEnumerator PulseAnimation(Transform t)
        {
            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 from = t.localScale;
            Vector3 to = Vector3.one * _displayScale;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / duration;
                t.localScale = Vector3.Lerp(from, to, p * p);
                yield return null;
            }
            t.localScale = to;
        }

        private bool[,] GeneratePattern(int size, int generation, int[] choices, int choiceCount)
        {
            var pattern = new bool[size, size];
            int half = size / 2;
            Random.InitState(generation * 1000 + (choiceCount > 0 ? choices[0] * 100 : 0)
                + (choiceCount > 1 ? choices[1] * 10 : 0) + (choiceCount > 2 ? choices[2] : 0));

            // 中心から外側に広がるパターン
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x <= half; x++)
                {
                    float dist = Mathf.Sqrt((x - half) * (x - half) + (y - half) * (y - half));
                    float maxDist = half * 1.2f;
                    float prob = Mathf.Clamp01(1f - (dist / maxDist));

                    // 世代が上がるほど複雑になる
                    prob *= 0.3f + (generation / 10f) * 0.7f;

                    // 選択による形状変化
                    if (choiceCount > 0 && choices[0] == 0) // 水辺: 横長
                        prob *= 1f + Mathf.Abs(y - half) * 0.05f;
                    if (choiceCount > 0 && choices[0] == 1) // 陸地: 縦長
                        prob *= 1f + Mathf.Abs(x - half) * 0.05f;
                    if (choiceCount > 1 && choices[1] == 0) // 熱帯: 突起多め
                        prob += Random.value * 0.2f;
                    if (choiceCount > 2 && choices[2] == 0) // 捕食者: 角張り
                        prob = prob > 0.4f ? 1f : 0f;

                    bool filled = Random.value < prob;
                    pattern[y, x] = filled;
                    pattern[y, size - 1 - x] = filled; // 左右対称
                }
            }

            // 中心は常に埋める
            pattern[half, half] = true;
            if (half > 0)
            {
                pattern[half - 1, half] = true;
                pattern[half + 1, half] = true;
                pattern[half, half - 1] = true;
                pattern[half, half + 1] = true;
            }

            return pattern;
        }

        private Color GetCreatureColor(int[] choices, int choiceCount, int generation)
        {
            float r = 0.5f, g = 0.5f, b = 0.8f;

            if (choiceCount > 0)
            {
                if (choices[0] == 0) { r = 0.2f; g = 0.5f; b = 0.9f; } // 水辺 = 青
                else                 { r = 0.3f; g = 0.7f; b = 0.2f; } // 陸地 = 緑
            }

            if (choiceCount > 1)
            {
                if (choices[1] == 0) { r += 0.2f; b -= 0.1f; } // 熱帯 = 暖色寄り
                else                 { b += 0.2f; r -= 0.1f; } // 寒冷 = 寒色寄り
            }

            if (choiceCount > 2)
            {
                if (choices[2] == 0) { r *= 1.2f; g *= 0.8f; } // 捕食者 = 濃い
                else                 { r += 0.15f; g += 0.15f; b += 0.15f; } // 草食者 = 明るい
            }

            // 世代で明るさ調整
            float brightness = 0.7f + (generation / 10f) * 0.3f;
            return new Color(
                Mathf.Clamp01(r * brightness),
                Mathf.Clamp01(g * brightness),
                Mathf.Clamp01(b * brightness)
            );
        }

        private void ApplyPixelArt(bool[,] pattern, int size, Color color)
        {
            if (_currentSprite != null) Destroy(_currentSprite);
            if (_currentTexture != null) Destroy(_currentTexture);

            _currentTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            _currentTexture.filterMode = FilterMode.Point;
            _currentTexture.wrapMode = TextureWrapMode.Clamp;

            Color bgColor = new Color(0, 0, 0, 0);
            Color outlineColor = new Color(color.r * 0.4f, color.g * 0.4f, color.b * 0.4f, 1f);

            // 塗りつぶし
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (pattern[y, x])
                    {
                        // 隣接チェックでアウトライン
                        bool isEdge = false;
                        if (x == 0 || !pattern[y, x - 1]) isEdge = true;
                        if (x == size - 1 || !pattern[y, x + 1]) isEdge = true;
                        if (y == 0 || !pattern[y - 1, x]) isEdge = true;
                        if (y == size - 1 || !pattern[y + 1, x]) isEdge = true;

                        _currentTexture.SetPixel(x, y, isEdge ? outlineColor : color);
                    }
                    else
                    {
                        _currentTexture.SetPixel(x, y, bgColor);
                    }
                }
            }

            // 目を追加（世代3以降）
            if (size >= 7)
            {
                int eyeY = size / 2 + size / 4;
                int eyeXL = size / 2 - size / 5;
                int eyeXR = size / 2 + size / 5;
                Color eyeColor = Color.white;
                Color pupilColor = Color.black;

                if (eyeXL >= 0 && eyeXL < size && eyeY >= 0 && eyeY < size)
                    _currentTexture.SetPixel(eyeXL, eyeY, eyeColor);
                if (eyeXR >= 0 && eyeXR < size && eyeY >= 0 && eyeY < size)
                    _currentTexture.SetPixel(eyeXR, eyeY, eyeColor);
                // 瞳
                int pupilY = eyeY - 1 >= 0 ? eyeY - 1 : eyeY;
                if (eyeXL >= 0 && eyeXL < size && pupilY >= 0 && pupilY < size)
                    _currentTexture.SetPixel(eyeXL, pupilY, pupilColor);
                if (eyeXR >= 0 && eyeXR < size && pupilY >= 0 && pupilY < size)
                    _currentTexture.SetPixel(eyeXR, pupilY, pupilColor);
            }

            _currentTexture.Apply();

            _currentSprite = Sprite.Create(
                _currentTexture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size / 2f // pixelsPerUnit
            );
            _creatureRenderer.sprite = _currentSprite;
        }

        private void OnDestroy()
        {
            if (_currentSprite != null) Destroy(_currentSprite);
            if (_currentTexture != null) Destroy(_currentTexture);
        }
    }
}
