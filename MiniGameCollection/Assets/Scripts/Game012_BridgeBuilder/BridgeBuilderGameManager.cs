using UnityEngine;
using UnityEngine.Events;

namespace Game012_BridgeBuilder
{
    public class BridgeBuilderGameManager : MonoBehaviour
    {
        [SerializeField] private BridgeManager _bridgeManager;
        [SerializeField] private BridgeBuilderUI _ui;

        public bool IsPlaying { get; private set; }
        public bool IsTesting { get; private set; }

        public UnityEvent<int> OnLevelCleared = new();

        private int _currentLevel;

        // Level definitions: anchor points and gap info
        private static readonly LevelData[] Levels =
        {
            new LevelData
            {
                LeftEdge = new Vector2(-3f, -1f),
                RightEdge = new Vector2(3f, -1f),
                GapWidth = 6f,
                Budget = 5,
                RequiredSupports = 1,
            },
            new LevelData
            {
                LeftEdge = new Vector2(-3.5f, -1f),
                RightEdge = new Vector2(3.5f, -0.5f),
                GapWidth = 7f,
                Budget = 7,
                RequiredSupports = 2,
            },
            new LevelData
            {
                LeftEdge = new Vector2(-4f, -1f),
                RightEdge = new Vector2(4f, -1f),
                GapWidth = 8f,
                Budget = 8,
                RequiredSupports = 2,
            },
        };

        private void Start() => LoadLevel(0);

        public void LoadLevel(int level)
        {
            _currentLevel = Mathf.Clamp(level, 0, Levels.Length - 1);
            IsPlaying = true;
            IsTesting = false;
            _bridgeManager.LoadLevel(Levels[_currentLevel]);
            _ui?.SetLevelText($"Level {_currentLevel + 1} / {Levels.Length}");
            _ui?.SetBudgetText(Levels[_currentLevel].Budget);
            _ui?.HideClearPanel();
            _ui?.SetTestMode(false);
        }

        public void StartTest()
        {
            if (!IsPlaying || IsTesting) return;
            IsTesting = true;
            _ui?.SetTestMode(true);
            _bridgeManager.StartTest();
        }

        public void OnTestResult(bool success)
        {
            IsTesting = false;
            if (success)
            {
                IsPlaying = false;
                _ui?.ShowClearPanel();
                OnLevelCleared?.Invoke(_currentLevel);
            }
            else
            {
                _ui?.SetTestMode(false);
            }
        }

        public void ResetLevel()
        {
            LoadLevel(_currentLevel);
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

    [System.Serializable]
    public class LevelData
    {
        public Vector2 LeftEdge;
        public Vector2 RightEdge;
        public float GapWidth;
        public int Budget;
        public int RequiredSupports;
    }
}
