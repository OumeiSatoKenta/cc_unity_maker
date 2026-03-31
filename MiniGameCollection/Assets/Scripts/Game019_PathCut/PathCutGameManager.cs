using UnityEngine;

namespace Game019_PathCut
{
    public class PathCutGameManager : MonoBehaviour
    {
        [SerializeField] private PathCutManager _cutManager;
        [SerializeField] private PathCutUI _ui;

        private int _cutCount;
        private bool _isCleared;
        private int _currentStage;

        private void Start()
        {
            _currentStage = 0;
            StartGame();
        }

        public void StartGame()
        {
            _cutCount = 0;
            _isCleared = false;
            if (_cutManager != null) _cutManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateCutCount(_cutCount);
                _ui.UpdateStageText(_currentStage + 1);
                _ui.HideClearPanel();
                _ui.HideFailPanel();
            }
        }

        public void OnRopeCut()
        {
            if (_isCleared) return;
            _cutCount++;
            if (_ui != null) _ui.UpdateCutCount(_cutCount);
        }

        public void OnPuzzleSolved()
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel(_cutCount, _currentStage + 1);
        }

        public void OnBallLost()
        {
            if (_isCleared) return;
            if (_ui != null) _ui.ShowFailPanel();
        }

        public void RestartGame() { StartGame(); }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= PathCutManager.StageCount) _currentStage = 0;
            StartGame();
        }
    }
}
