using UnityEngine;

namespace Game008_IcePath
{
    public class IcePathGameManager : MonoBehaviour
    {
        [SerializeField] private IcePathManager _iceManager;
        [SerializeField] private IcePathUI _ui;

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
            if (_iceManager != null) _iceManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateMoveCount(_moveCount);
                _ui.UpdateStageText(_currentStage + 1);
                _ui.UpdateProgress(1, 0);
                _ui.HideClearPanel();
            }
        }

        public void OnPlayerMoved(int visited, int total)
        {
            if (_isCleared) return;
            _moveCount++;
            if (_ui != null)
            {
                _ui.UpdateMoveCount(_moveCount);
                _ui.UpdateProgress(visited, total);
            }
        }

        public void OnPuzzleSolved()
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel(_moveCount, _currentStage + 1);
        }

        public void RestartGame()
        {
            _moveCount = 0;
            _isCleared = false;
            if (_iceManager != null) _iceManager.ResetStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateMoveCount(0);
                _ui.UpdateProgress(1, 0);
                _ui.HideClearPanel();
            }
        }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= IcePathManager.StageCount) _currentStage = 0;
            StartGame();
        }
    }
}
