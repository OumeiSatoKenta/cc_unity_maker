using UnityEngine;

namespace Game018_TimeRewind
{
    public class TimeRewindGameManager : MonoBehaviour
    {
        [SerializeField] private TimeRewindManager _rewindManager;
        [SerializeField] private TimeRewindUI _ui;

        private int _moveCount;
        private int _rewindCount;
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
            _rewindCount = 0;
            _isCleared = false;
            if (_rewindManager != null) _rewindManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateMoveCount(_moveCount);
                _ui.UpdateRewindCount(_rewindCount);
                _ui.UpdateStageText(_currentStage + 1);
                _ui.HideClearPanel();
            }
        }

        public void OnPieceMoved(int historyCount)
        {
            if (_isCleared) return;
            _moveCount++;
            if (_ui != null) _ui.UpdateMoveCount(_moveCount);
        }

        public void OnRewind(int historyCount)
        {
            if (_isCleared) return;
            _rewindCount++;
            if (_ui != null) _ui.UpdateRewindCount(_rewindCount);
        }

        public void OnPuzzleSolved()
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel(_moveCount, _rewindCount, _currentStage + 1);
        }

        public void RewindAction()
        {
            if (_isCleared) return;
            if (_rewindManager != null) _rewindManager.Rewind();
        }

        public void RestartGame() { StartGame(); }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= TimeRewindManager.StageCount) _currentStage = 0;
            StartGame();
        }
    }
}
