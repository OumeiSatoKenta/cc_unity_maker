using UnityEngine;

namespace Game017_CrystalSort
{
    public class CrystalSortGameManager : MonoBehaviour
    {
        [SerializeField] private SortManager _sortManager;
        [SerializeField] private CrystalSortUI _ui;

        private int _sortedCount;
        private int _missCount;
        private bool _isCleared;
        private int _currentStage;

        private void Start()
        {
            _currentStage = 0;
            StartGame();
        }

        public void StartGame()
        {
            _sortedCount = 0;
            _missCount = 0;
            _isCleared = false;
            if (_sortManager != null) _sortManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateSortedCount(_sortedCount);
                _ui.UpdateMissCount(_missCount);
                _ui.UpdateStageText(_currentStage + 1);
                _ui.HideClearPanel();
            }
        }

        public void OnCrystalSorted()
        {
            if (_isCleared) return;
            _sortedCount++;
            if (_ui != null) _ui.UpdateSortedCount(_sortedCount);
        }

        public void OnMiss()
        {
            if (_isCleared) return;
            _missCount++;
            if (_ui != null) _ui.UpdateMissCount(_missCount);
        }

        public void OnPuzzleSolved()
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel(_sortedCount, _missCount, _currentStage + 1);
        }

        public void RestartGame() { StartGame(); }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= SortManager.StageCount) _currentStage = 0;
            StartGame();
        }
    }
}
