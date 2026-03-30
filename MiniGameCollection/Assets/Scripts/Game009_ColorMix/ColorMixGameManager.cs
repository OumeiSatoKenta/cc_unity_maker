using UnityEngine;
using UnityEngine.Events;

namespace Game009_ColorMix
{
    /// <summary>
    /// ColorMix のゲーム全体を制御する。
    /// レベル管理、クリア判定、シーン遷移を担当。
    /// </summary>
    public class ColorMixGameManager : MonoBehaviour
    {
        [SerializeField] private ColorMixManager _colorMixManager;
        [SerializeField] private ColorMixUI _ui;

        public bool IsPlaying { get; private set; }

        public UnityEvent<int> OnLevelCleared = new();

        private int _currentLevel;

        // ── Level data ──────────────────────────────────────────────
        // targetR, targetG, targetB: 目標色 (0-255)
        // tolerancePercent: クリアに必要な色差の許容範囲 (0-1)
        private static readonly (int r, int g, int b, float tolerance, string name)[] Levels =
        {
            // Level 1 – オレンジ (赤 + 黄 ≈ 赤+緑混合)
            (220, 120, 30, 0.12f, "Orange"),
            // Level 2 – 紫 (赤 + 青)
            (140, 30, 180, 0.10f, "Purple"),
            // Level 3 – 緑 (黄 + 青 ≈ 緑)
            (50, 180, 80, 0.10f, "Green"),
            // Level 4 – ピンク (赤 + 白)
            (230, 100, 140, 0.08f, "Pink"),
            // Level 5 – 茶色 (赤 + 緑 + 少し青)
            (160, 90, 40, 0.08f, "Brown"),
        };

        private void Start() => LoadLevel(0);

        public void LoadLevel(int level)
        {
            _currentLevel = Mathf.Clamp(level, 0, Levels.Length - 1);
            IsPlaying = true;

            var (r, g, b, tolerance, name) = Levels[_currentLevel];
            Color targetColor = new Color(r / 255f, g / 255f, b / 255f);
            _colorMixManager.LoadLevel(targetColor, tolerance);

            _ui?.SetLevelText($"Level {_currentLevel + 1} / {Levels.Length}");
            _ui?.SetTargetColorName(name);
            _ui?.HideClearPanel();
        }

        public void OnMixSubmitted(Color mixedColor)
        {
            if (!IsPlaying) return;

            var (r, g, b, tolerance, name) = Levels[_currentLevel];
            Color targetColor = new Color(r / 255f, g / 255f, b / 255f);

            float diff = ColorDistance(mixedColor, targetColor);
            if (diff <= tolerance)
            {
                int score = Mathf.RoundToInt((1f - diff / tolerance) * 100f);
                IsPlaying = false;
                _ui?.ShowClearPanel(_currentLevel, score);
                OnLevelCleared?.Invoke(_currentLevel);
            }
            else
            {
                _ui?.ShowFeedback(diff);
            }
        }

        private float ColorDistance(Color a, Color b)
        {
            float dr = a.r - b.r;
            float dg = a.g - b.g;
            float db = a.b - b.b;
            return Mathf.Sqrt(dr * dr + dg * dg + db * db) / Mathf.Sqrt(3f);
        }

        public void ResetLevel()
        {
            IsPlaying = true;
            var (r, g, b, tolerance, name) = Levels[_currentLevel];
            Color targetColor = new Color(r / 255f, g / 255f, b / 255f);
            _colorMixManager.LoadLevel(targetColor, tolerance);
            _ui?.HideClearPanel();
        }

        public void LoadNextLevel()
        {
            LoadLevel((_currentLevel + 1) % Levels.Length);
        }

        public void LoadMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu");
        }
    }
}
