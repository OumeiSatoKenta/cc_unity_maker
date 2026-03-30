using UnityEngine;
using UnityEngine.Events;

namespace Game007_NumberFlow
{
    public class NumberFlowGameManager : MonoBehaviour
    {
        [SerializeField] private NumberFlowManager _flowManager;
        [SerializeField] private NumberFlowUI _ui;

        public bool IsPlaying { get; private set; }

        public UnityEvent<int> OnLevelCleared = new();

        private int _currentLevel;

        // ── Level data ──────────────────────────────────────────────
        // grid[row, col] = number (1-16)
        private static readonly int[][,] Levels =
        {
            // Level 1 – snake pattern (easy)
            new int[4, 4]
            {
                {  1,  2,  3,  4 },
                { 12, 13, 14,  5 },
                { 11, 16, 15,  6 },
                { 10,  9,  8,  7 },
            },
            // Level 2 – spiral-ish (medium)
            new int[4, 4]
            {
                {  1, 16, 15, 14 },
                {  2, 11, 12, 13 },
                {  3, 10,  7,  6 },
                {  4,  9,  8,  5 },
            },
            // Level 3 – zigzag (harder)
            new int[4, 4]
            {
                {  1,  2, 15, 14 },
                {  4,  3, 16, 13 },
                {  5,  8,  9, 12 },
                {  6,  7, 10, 11 },
            },
        };

        private void Start() => LoadLevel(0);

        public void LoadLevel(int level)
        {
            _currentLevel = Mathf.Clamp(level, 0, Levels.Length - 1);
            IsPlaying = true;
            _flowManager.LoadGrid(Levels[_currentLevel]);
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
            _flowManager.ResetGrid();
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
