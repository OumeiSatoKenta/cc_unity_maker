using UnityEngine;

namespace Game007_NumberFlow
{
    public class NumberFlowGameManager : MonoBehaviour
    {
        [SerializeField] private NumberFlowManager _flowManager;
        [SerializeField] private NumberFlowUI _ui;

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
            if (_flowManager != null) _flowManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateProgress(0, 0);
                _ui.UpdateStageText(_currentStage + 1);
                _ui.HideClearPanel();
            }
        }

        public void OnCellPlaced(int current, int total)
        {
            if (_isCleared) return;
            if (_ui != null) _ui.UpdateProgress(current, total);
        }

        public void OnPuzzleSolved()
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel(_currentStage + 1);
        }

        public void RestartGame()
        {
            if (_flowManager != null) _flowManager.ResetPath();
            _isCleared = false;
            if (_ui != null) _ui.UpdateProgress(0, 0);
        }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= NumberFlowManager.StageCount)
                _currentStage = 0;
            StartGame();
        }
    }
}
