using UnityEngine;

namespace Game053_SlideBlitz
{
    public class SlideBlitzGameManager : MonoBehaviour
    {
        [SerializeField] private SlideManager _slideManager;
        [SerializeField] private SlideBlitzUI _ui;

        private float _timer;
        private int _moveCount;
        private int _gridSize;
        private bool _isCleared;
        private bool _isPlaying;

        private void Start()
        {
            _gridSize = 3;
            StartGame();
        }

        public void StartGame()
        {
            _timer = 0f;
            _moveCount = 0;
            _isCleared = false;
            _isPlaying = true;
            if (_ui != null)
            {
                _ui.UpdateTimer(_timer);
                _ui.UpdateMoves(_moveCount);
                _ui.UpdateSize(_gridSize);
                _ui.HideClearPanel();
            }
            if (_slideManager != null) _slideManager.GenerateBoard(_gridSize);
        }

        private void Update()
        {
            if (!_isPlaying || _isCleared) return;
            _timer += Time.deltaTime;
            if (_ui != null) _ui.UpdateTimer(_timer);
        }

        public void OnTileMoved()
        {
            if (_isCleared) return;
            _moveCount++;
            if (_ui != null) _ui.UpdateMoves(_moveCount);
            if (_slideManager != null && _slideManager.CheckSolved())
            {
                _isCleared = true;
                _isPlaying = false;
                if (_ui != null) _ui.ShowClearPanel(_timer, _moveCount);
            }
        }

        public void OnNextSize()
        {
            _gridSize = _gridSize >= 5 ? 3 : _gridSize + 1;
            StartGame();
        }

        public void OnRetry()
        {
            StartGame();
        }
    }
}
