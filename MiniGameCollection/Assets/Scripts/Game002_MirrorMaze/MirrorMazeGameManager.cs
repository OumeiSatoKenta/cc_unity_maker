using UnityEngine;

namespace Game002_MirrorMaze
{
    public class MirrorMazeGameManager : MonoBehaviour
    {
        [SerializeField] private MazeManager _mazeManager;
        [SerializeField] private MirrorMazeUI _ui;

        private int _moveCount;
        private bool _isCleared;
        private int _currentStage;

        public int MoveCount => _moveCount;
        public bool IsCleared => _isCleared;

        private void Start()
        {
            _currentStage = 0;
            StartGame();
        }

        public void StartGame()
        {
            _moveCount = 0;
            _isCleared = false;

            if (_mazeManager != null) _mazeManager.SetupStage(_currentStage);
            if (_ui != null) _ui.UpdateMoveCount(_moveCount);
            if (_ui != null) _ui.UpdateStageText(_currentStage + 1);
            if (_ui != null) _ui.HideClearPanel();
        }

        public void OnMirrorMoved() => HandlePlayerAction();
        public void OnMirrorRotated() => HandlePlayerAction();

        private void HandlePlayerAction()
        {
            if (_isCleared) return;

            _moveCount++;
            if (_ui != null) _ui.UpdateMoveCount(_moveCount);
            if (_mazeManager != null) _mazeManager.UpdateLaser();

            if (_mazeManager != null && _mazeManager.IsGoalReached())
            {
                _isCleared = true;
                if (_ui != null) _ui.ShowClearPanel(_moveCount, _currentStage + 1);
            }
        }

        public void RestartGame()
        {
            StartGame();
        }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= MazeManager.StageCount)
                _currentStage = 0;
            StartGame();
        }
    }
}
