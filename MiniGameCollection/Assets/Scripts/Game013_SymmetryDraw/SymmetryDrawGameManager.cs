using UnityEngine;

namespace Game013_SymmetryDraw
{
    public class SymmetryDrawGameManager : MonoBehaviour
    {
        [SerializeField] private CanvasDrawManager _drawManager;
        [SerializeField] private SymmetryDrawUI _ui;

        private bool _isCleared;
        private int _currentStage;

        private void Start()
        {
            _currentStage = 0;
            StartGame();
        }

        public void StartGame()
        {
            _isCleared = false;
            if (_drawManager != null) _drawManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateStageText(_currentStage + 1);
                _ui.UpdateProgress(0, 0);
                _ui.HideClearPanel();
            }
        }

        public void OnCellPainted(int painted, int total)
        {
            if (_isCleared) return;
            if (_ui != null) _ui.UpdateProgress(painted, total);
        }

        public void OnPuzzleSolved()
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel(_currentStage + 1);
        }

        public void RestartGame()
        {
            _isCleared = false;
            if (_drawManager != null) _drawManager.ResetStage(_currentStage);
            if (_ui != null) _ui.UpdateProgress(0, 0);
        }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= CanvasDrawManager.StageCount) _currentStage = 0;
            StartGame();
        }
    }
}
