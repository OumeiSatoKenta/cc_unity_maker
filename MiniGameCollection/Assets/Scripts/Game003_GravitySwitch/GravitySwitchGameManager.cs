using UnityEngine;

namespace Game003_GravitySwitch
{
    public class GravitySwitchGameManager : MonoBehaviour
    {
        [SerializeField] private GravityManager _gravityManager;
        [SerializeField] private GravitySwitchUI _ui;

        private int _moveCount;
        private bool _isCleared;
        private int _currentStage;

        private void Start()
        {
            _currentStage = 0;
            StartGame();
        }

        public void StartGame()
        {
            _moveCount = 0;
            _isCleared = false;
            if (_gravityManager != null) _gravityManager.SetupStage(_currentStage);
            if (_ui != null) _ui.UpdateMoveCount(_moveCount);
            if (_ui != null) _ui.UpdateStageText(_currentStage + 1);
            if (_ui != null) _ui.HideClearPanel();
        }

        public void OnGravityChanged()
        {
            if (_isCleared) return;

            _moveCount++;
            if (_ui != null) _ui.UpdateMoveCount(_moveCount);

            if (_gravityManager != null && _gravityManager.IsGoalReached())
            {
                _isCleared = true;
                if (_ui != null) _ui.ShowClearPanel(_moveCount, _currentStage + 1);
            }
        }

        public void ApplyGravityDirection(Vector2Int direction)
        {
            if (_isCleared) return;
            if (_gravityManager != null && _gravityManager.ApplyGravity(direction))
            {
                OnGravityChanged();
            }
        }

        public void RestartGame()
        {
            StartGame();
        }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= GravityManager.StageCount)
                _currentStage = 0;
            StartGame();
        }
    }
}
