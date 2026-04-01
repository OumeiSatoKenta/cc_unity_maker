using UnityEngine;

namespace Game053_SlideBlitz
{
    public class SlideBlitzGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("パズル管理")] private PuzzleManager _puzzleManager;
        [SerializeField, Tooltip("UI管理")] private SlideBlitzUI _ui;
        [SerializeField, Tooltip("制限時間")] private float _timeLimit = 60f;

        private float _timer;
        private int _moveCount;
        private bool _isPlaying;

        private void Start()
        {
            _timer = _timeLimit;
            _moveCount = 0;
            _isPlaying = true;
            _puzzleManager.StartGame();
            _ui.UpdateTimer(_timer);
            _ui.UpdateMoves(_moveCount);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _timer -= Time.deltaTime;
            _ui.UpdateTimer(Mathf.Max(0f, _timer));

            if (_timer <= 0f)
            {
                _isPlaying = false;
                _puzzleManager.StopGame();
                _ui.ShowGameOver(_moveCount);
            }
        }

        public void OnTileMoved()
        {
            if (!_isPlaying) return;
            _moveCount++;
            _ui.UpdateMoves(_moveCount);

            if (_puzzleManager.IsSolved)
            {
                _isPlaying = false;
                _puzzleManager.StopGame();
                float remaining = Mathf.Max(0f, _timer);
                _ui.ShowClear(_moveCount, remaining);
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
