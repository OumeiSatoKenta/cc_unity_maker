using UnityEngine;
using UnityEngine.Events;

namespace Game008_IcePath
{
    /// <summary>
    /// IcePath のゲーム全体を制御する。
    /// レベル管理、クリア判定、シーン遷移を担当。
    /// </summary>
    public class IcePathGameManager : MonoBehaviour
    {
        [SerializeField] private IcePathManager _icePathManager;
        [SerializeField] private IcePathUI _ui;

        public bool IsPlaying { get; private set; }

        public UnityEvent<int> OnLevelCleared = new();

        private int _currentLevel;

        // ── Level data ──────────────────────────────────────────────
        // 0 = ice (passable), 1 = wall (impassable)
        // startRow, startCol = player start position
        private static readonly (int[,] layout, int startRow, int startCol)[] Levels =
        {
            // Level 1 – simple 5x5 open field
            (
                new int[5, 5]
                {
                    { 0, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 0 },
                },
                0, 0
            ),
            // Level 2 – walls forming channels
            (
                new int[5, 5]
                {
                    { 0, 0, 0, 0, 0 },
                    { 0, 1, 0, 1, 0 },
                    { 0, 0, 0, 0, 0 },
                    { 0, 1, 0, 1, 0 },
                    { 0, 0, 0, 0, 0 },
                },
                0, 0
            ),
            // Level 3 – more complex wall layout
            (
                new int[5, 5]
                {
                    { 0, 0, 1, 0, 0 },
                    { 0, 0, 0, 0, 1 },
                    { 1, 0, 0, 0, 0 },
                    { 0, 0, 0, 1, 0 },
                    { 0, 1, 0, 0, 0 },
                },
                0, 0
            ),
        };

        private void Start() => LoadLevel(0);

        public void LoadLevel(int level)
        {
            _currentLevel = Mathf.Clamp(level, 0, Levels.Length - 1);
            IsPlaying = true;

            var (layout, startRow, startCol) = Levels[_currentLevel];
            _icePathManager.LoadLevel(layout, startRow, startCol);

            _ui?.SetLevelText($"Level {_currentLevel + 1} / {Levels.Length}");
            _ui?.HideClearPanel();
        }

        public void OnCleared()
        {
            IsPlaying = false;
            _ui?.ShowClearPanel(_currentLevel);
            OnLevelCleared?.Invoke(_currentLevel);
        }

        public void ResetLevel()
        {
            IsPlaying = true;
            var (layout, startRow, startCol) = Levels[_currentLevel];
            _icePathManager.LoadLevel(layout, startRow, startCol);
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
