using UnityEngine;

namespace Game091_TimeBlender
{
    public class TimeBlenderGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private TimeManager _timeManager;
        [SerializeField, Tooltip("UI管理")] private TimeBlenderUI _ui;
        [SerializeField, Tooltip("全パズル数")] private int _totalPuzzles = 5;

        private int _solvedPuzzles;
        private bool _isPlaying;

        private void Start()
        {
            _solvedPuzzles = 0;
            _isPlaying = true;
            _timeManager.StartGame();
            _ui.UpdatePuzzles(_solvedPuzzles, _totalPuzzles);
            _ui.UpdateEra(_timeManager.IsPresent);
        }

        public void OnPuzzleSolved()
        {
            if (!_isPlaying) return;
            _solvedPuzzles++;
            _ui.UpdatePuzzles(_solvedPuzzles, _totalPuzzles);

            if (_solvedPuzzles >= _totalPuzzles)
            {
                _isPlaying = false;
                _timeManager.StopGame();
                _ui.ShowClear(_solvedPuzzles);
                return;
            }
            _timeManager.NextPuzzle();
        }

        public void OnParadox()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _timeManager.StopGame();
            _ui.ShowGameOver(_solvedPuzzles);
        }

        public void OnEraChanged(bool isPresent)
        {
            _ui.UpdateEra(isPresent);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
