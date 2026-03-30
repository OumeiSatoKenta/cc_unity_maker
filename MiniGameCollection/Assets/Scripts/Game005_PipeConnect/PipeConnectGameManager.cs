using UnityEngine;
using UnityEngine.Events;

namespace Game005_PipeConnect
{
    /// <summary>
    /// ゲーム状態・レベル管理を担当する。
    /// レベルデータは 5x5 int[,] で定義（型別・初期回転別に保持）。
    /// </summary>
    public class PipeConnectGameManager : MonoBehaviour
    {
        [SerializeField] private PipeManager _pipeManager;
        [SerializeField] private PipeConnectUI _ui;

        public bool IsPlaying { get; private set; }

        public UnityEvent<int> OnMoveCountChanged = new();
        public UnityEvent<int> OnLevelCleared = new();

        private int _currentLevel;
        private int _moveCount;

        // ── Level data ───────────────────────────────────────────────
        // 0=empty, 1=straight, 2=bend, 3=T, 4=cross, 5=source, 6=goal
        private static readonly int[][,] LevelTypes =
        {
            // Level 1: horizontal line
            new int[5, 5]
            {
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 },
                { 5, 1, 1, 1, 6 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 },
            },
            // Level 2: L-shape
            new int[5, 5]
            {
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 6, 0 },
                { 0, 0, 0, 1, 0 },
                { 0, 5, 1, 2, 0 },
                { 0, 0, 0, 0, 0 },
            },
            // Level 3: zigzag
            new int[5, 5]
            {
                { 0, 0, 0, 0, 6 },
                { 0, 0, 0, 2, 2 },
                { 0, 0, 0, 1, 0 },
                { 2, 1, 1, 2, 0 },
                { 5, 0, 0, 0, 0 },
            },
        };

        // Initial (scrambled) rotations
        // Rotation: 0=0°, 1=90°CW, 2=180°, 3=270°CW
        // Fixed tiles (source/goal/empty) keep their rotation as-is.
        private static readonly int[][,] LevelInitialRotations =
        {
            // Level 1: all 0; straights (2,1-3) at rot 0 = vertical (wrong; solution = rot 1)
            new int[5, 5]
            {
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 },
            },
            // Level 2: goal(1,3)=rot3; straight(2,3)=rot1(wrong); bend(3,3)=rot0(wrong); straight(3,2)=rot0(wrong)
            new int[5, 5]
            {
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 3, 0 },
                { 0, 0, 0, 1, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 },
            },
            // Level 3: source(4,0)=rot3; goal(0,4)=rot3; pipes scrambled
            new int[5, 5]
            {
                { 0, 0, 0, 0, 3 },
                { 0, 0, 0, 3, 0 },
                { 0, 0, 0, 1, 0 },
                { 0, 0, 0, 0, 0 },
                { 3, 0, 0, 0, 0 },
            },
        };

        private void Start() => LoadLevel(0);

        public void LoadLevel(int level)
        {
            _currentLevel = Mathf.Clamp(level, 0, LevelTypes.Length - 1);
            _moveCount = 0;
            IsPlaying = true;
            _pipeManager.LoadLevel(LevelTypes[_currentLevel], LevelInitialRotations[_currentLevel]);
            OnMoveCountChanged?.Invoke(_moveCount);
            _ui?.SetLevelText($"Level {_currentLevel + 1} / {LevelTypes.Length}");
        }

        public void OnTileMoved()
        {
            _moveCount++;
            OnMoveCountChanged?.Invoke(_moveCount);
        }

        public void OnSolved()
        {
            IsPlaying = false;
            OnLevelCleared?.Invoke(_currentLevel);
        }

        public int GetMoveCount() => _moveCount;

        public void LoadNextLevel()
        {
            int next = (_currentLevel + 1) % LevelTypes.Length;
            LoadLevel(next);
            _ui?.HideClearPanel();
        }

        public void LoadMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu");
        }
    }
}
