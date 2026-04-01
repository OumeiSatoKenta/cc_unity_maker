using UnityEngine;

namespace Game050_BubbleSort
{
    public class BubbleSortGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private SortManager _sortManager;
        [SerializeField, Tooltip("UI管理")] private BubbleSortUI _ui;
        [SerializeField, Tooltip("最大手数")] private int _maxMoves = 20;
        [SerializeField, Tooltip("バブル数")] private int _bubbleCount = 8;

        private int _movesUsed;
        private bool _isPlaying;

        private void Start()
        {
            _movesUsed = 0;
            _isPlaying = true;
            _sortManager.StartGame(_bubbleCount);
            _ui.UpdateMoves(_maxMoves);
            _ui.UpdateProgress(_sortManager.CorrectCount, _bubbleCount);
        }

        public void OnSwapPerformed()
        {
            if (!_isPlaying) return;
            _movesUsed++;
            int remaining = _maxMoves - _movesUsed;
            _ui.UpdateMoves(remaining);
            _ui.UpdateProgress(_sortManager.CorrectCount, _bubbleCount);

            if (_sortManager.IsSorted)
            {
                _isPlaying = false;
                _sortManager.StopGame();
                _ui.ShowClear(remaining);
                return;
            }

            if (remaining <= 0)
            {
                _isPlaying = false;
                _sortManager.StopGame();
                _ui.ShowGameOver();
            }
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
