using UnityEngine;

namespace Game050_BubbleSort
{
    public class BubbleSortGameManager : MonoBehaviour
    {
        [SerializeField] private BubbleSortManager _sortManager;
        [SerializeField] private BubbleSortUI _ui;

        private int _moveCount;
        private int _currentStage;
        private bool _isCleared;

        private void Start()
        {
            _currentStage = 1;
            StartGame();
        }

        public void StartGame()
        {
            _moveCount = 0;
            _isCleared = false;
            if (_ui != null)
            {
                _ui.UpdateMoves(_moveCount);
                _ui.UpdateStage(_currentStage);
                _ui.HideClearPanel();
            }
            if (_sortManager != null) _sortManager.GenerateStage(_currentStage);
        }

        public void OnBubbleMoved()
        {
            if (_isCleared) return;
            _moveCount++;
            if (_ui != null) _ui.UpdateMoves(_moveCount);

            if (_sortManager != null && _sortManager.CheckSorted())
            {
                _isCleared = true;
                if (_ui != null) _ui.ShowClearPanel(_moveCount);
            }
        }

        public void OnNextStage()
        {
            _currentStage++;
            StartGame();
        }

        public void OnRetry()
        {
            StartGame();
        }
    }
}
