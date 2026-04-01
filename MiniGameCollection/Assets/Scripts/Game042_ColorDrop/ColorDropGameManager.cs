using UnityEngine;

namespace Game042_ColorDrop
{
    public class ColorDropGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private ColorDropManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private ColorDropUI _ui;

        [SerializeField, Tooltip("総ドロップ数")]
        private int _totalDrops = 20;

        [SerializeField, Tooltip("最大ミス数")]
        private int _maxMisses = 3;

        private int _score;
        private int _combo;
        private int _missCount;
        private int _processedCount;
        private bool _isPlaying;

        private void Start()
        {
            _score = 0; _combo = 0; _missCount = 0; _processedCount = 0;
            _isPlaying = true;
            _ui.UpdateScore(_score);
            _ui.UpdateMisses(_missCount, _maxMisses);
            _manager.StartGame();
        }

        public void OnCorrectDrop()
        {
            if (!_isPlaying) return;
            _combo++;
            _score += 100 + _combo * 30;
            _processedCount++;
            _ui.UpdateScore(_score);
            if (_processedCount >= _totalDrops) { _isPlaying = false; _ui.ShowClear(_score); }
        }

        public void OnWrongDrop()
        {
            if (!_isPlaying) return;
            _combo = 0;
            _missCount++;
            _processedCount++;
            _ui.UpdateMisses(_missCount, _maxMisses);
            if (_missCount >= _maxMisses) { _isPlaying = false; _ui.ShowGameOver(_score); }
            else if (_processedCount >= _totalDrops) { _isPlaying = false; _ui.ShowClear(_score); }
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int TotalDrops => _totalDrops;
    }
}
