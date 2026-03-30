using UnityEngine;

namespace Game003_GravitySwitch
{
    /// <summary>
    /// GravitySwitch のゲーム全体を制御する。
    /// 状態管理、クリア判定、リスタートを担当する。
    /// </summary>
    public class GravitySwitchGameManager : MonoBehaviour
    {
        [SerializeField] private GravityManager _gravityManager;
        [SerializeField] private GravitySwitchUI _ui;

        private bool _isCleared;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _isCleared = false;
            if (_gravityManager != null) _gravityManager.InitLevel(0);
            if (_ui != null)
            {
                _ui.UpdateMoveCount(0);
                _ui.HideClearPanel();
            }
        }

        public void OnMoved(int moveCount)
        {
            if (_ui != null) _ui.UpdateMoveCount(moveCount);
        }

        public void OnCleared(int moveCount)
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel(moveCount);
        }

        public void RestartGame()
        {
            _isCleared = false;
            if (_gravityManager != null) _gravityManager.InitLevel(_gravityManager.CurrentLevel);
            if (_ui != null)
            {
                _ui.UpdateMoveCount(0);
                _ui.HideClearPanel();
            }
        }

        public void LoadNextLevel()
        {
            _isCleared = false;
            if (_gravityManager != null)
            {
                int next = (_gravityManager.CurrentLevel + 1) % GravityManager.LevelCount;
                _gravityManager.InitLevel(next);
            }
            if (_ui != null)
            {
                _ui.UpdateMoveCount(0);
                _ui.HideClearPanel();
            }
        }
    }
}
