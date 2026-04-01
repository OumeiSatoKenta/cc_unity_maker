using UnityEngine;

namespace Game034_DropZone
{
    public class DropZoneGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private DropManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private DropZoneUI _ui;

        [SerializeField, Tooltip("総アイテム数")]
        private int _totalItems = 20;

        [SerializeField, Tooltip("最大ミス数")]
        private int _maxMisses = 3;

        private int _score;
        private int _missCount;
        private int _processedCount;
        private bool _isPlaying;

        private void Start()
        {
            _score = 0;
            _missCount = 0;
            _processedCount = 0;
            _isPlaying = true;
            _ui.UpdateScore(_score);
            _ui.UpdateMisses(_missCount, _maxMisses);
            _ui.UpdateRemaining(_totalItems);
            _manager.StartGame();
        }

        public void OnCorrectDrop(int combo)
        {
            if (!_isPlaying) return;
            _score += 100 + combo * 50;
            _processedCount++;
            _ui.UpdateScore(_score);
            _ui.UpdateRemaining(_totalItems - _processedCount);

            if (_processedCount >= _totalItems)
            {
                _isPlaying = false;
                _ui.ShowClear(_score);
            }
        }

        public void OnWrongDrop()
        {
            if (!_isPlaying) return;
            _missCount++;
            _processedCount++;
            _ui.UpdateMisses(_missCount, _maxMisses);
            _ui.UpdateRemaining(_totalItems - _processedCount);

            if (_missCount >= _maxMisses)
            {
                _isPlaying = false;
                _ui.ShowGameOver(_score);
            }
            else if (_processedCount >= _totalItems)
            {
                _isPlaying = false;
                _ui.ShowClear(_score);
            }
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int TotalItems => _totalItems;
        public int ProcessedCount => _processedCount;
    }
}
