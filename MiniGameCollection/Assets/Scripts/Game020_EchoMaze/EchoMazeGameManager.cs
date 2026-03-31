using UnityEngine;

namespace Game020_EchoMaze
{
    public class EchoMazeGameManager : MonoBehaviour
    {
        [SerializeField] private EchoMazeManager _mazeManager;
        [SerializeField] private EchoMazeUI _ui;

        private int _moveCount;
        private int _wallHits;
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
            _wallHits = 0;
            _isCleared = false;
            if (_mazeManager != null) _mazeManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateMoveCount(_moveCount);
                _ui.UpdateWallHits(_wallHits);
                _ui.UpdateStageText(_currentStage + 1);
                _ui.HideClearPanel();
            }
        }

        public void OnPlayerMoved()
        {
            if (_isCleared) return;
            _moveCount++;
            if (_ui != null) _ui.UpdateMoveCount(_moveCount);
        }

        public void OnWallHit()
        {
            if (_isCleared) return;
            _wallHits++;
            if (_ui != null) _ui.UpdateWallHits(_wallHits);
        }

        public void OnPuzzleSolved()
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel(_moveCount, _wallHits, _currentStage + 1);
        }

        public void RestartGame() { StartGame(); }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= EchoMazeManager.StageCount) _currentStage = 0;
            StartGame();
        }
    }
}
