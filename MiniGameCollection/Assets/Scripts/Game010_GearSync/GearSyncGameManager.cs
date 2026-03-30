using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game010_GearSync
{
    /// <summary>
    /// GearSync ゲームの全体状態を管理する。
    /// ステージ進行・クリア判定・UI制御を担当。
    /// </summary>
    public class GearSyncGameManager : MonoBehaviour
    {
        public enum GameState { Playing, Clear }

        [SerializeField] private GearManager _gearManager;
        [SerializeField] private GearSyncUI _ui;

        private GameState _state = GameState.Playing;
        private int _currentLevel = 1;
        private const int TotalLevels = 5;
        private int _rotationCount = 0;

        public GameState State => _state;
        public int CurrentLevel => _currentLevel;
        public int RotationCount => _rotationCount;

        private void Start()
        {
            _rotationCount = 0;
            _gearManager.SetupLevel(_currentLevel);
            _ui.UpdateLevelText(_currentLevel, TotalLevels);
            _ui.UpdateRotationText(_rotationCount);
            _ui.HideClearPanel();
        }

        /// <summary>歯車を回転させた際に呼ばれる</summary>
        public void OnGearRotated()
        {
            if (_state != GameState.Playing) return;
            _rotationCount++;
            _ui.UpdateRotationText(_rotationCount);
            CheckClear();
        }

        private void CheckClear()
        {
            if (!_gearManager.IsAllGearsSynced()) return;

            _state = GameState.Clear;
            _ui.ShowClearPanel(_rotationCount);
        }

        public void OnNextLevel()
        {
            if (_currentLevel >= TotalLevels)
            {
                LoadMenu();
                return;
            }
            _currentLevel++;
            _rotationCount = 0;
            _state = GameState.Playing;
            _gearManager.SetupLevel(_currentLevel);
            _ui.UpdateLevelText(_currentLevel, TotalLevels);
            _ui.UpdateRotationText(_rotationCount);
            _ui.HideClearPanel();
        }

        public void OnRestart()
        {
            _rotationCount = 0;
            _state = GameState.Playing;
            _gearManager.SetupLevel(_currentLevel);
            _ui.UpdateLevelText(_currentLevel, TotalLevels);
            _ui.UpdateRotationText(_rotationCount);
            _ui.HideClearPanel();
        }

        public void LoadMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }
    }
}
