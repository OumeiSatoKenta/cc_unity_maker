using UnityEngine;

namespace Game055_DustSweep
{
    public class DustSweepGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private SweepManager _sweepManager;
        [SerializeField, Tooltip("UI管理")] private DustSweepUI _ui;
        [SerializeField, Tooltip("制限時間")] private float _timeLimit = 30f;

        private float _timer;
        private bool _isPlaying;

        private void Start()
        {
            _timer = _timeLimit;
            _isPlaying = true;
            _sweepManager.StartGame();
            _ui.UpdateTimer(_timer);
            _ui.UpdateProgress(0f);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _timer -= Time.deltaTime;
            _ui.UpdateTimer(Mathf.Max(0f, _timer));
            _ui.UpdateProgress(_sweepManager.CleanRatio);

            if (_sweepManager.CleanRatio >= 1f)
            {
                _isPlaying = false;
                _sweepManager.StopGame();
                _ui.ShowClear(_timeLimit - _timer);
                return;
            }

            if (_timer <= 0f)
            {
                _isPlaying = false;
                _sweepManager.StopGame();
                _ui.ShowGameOver(_sweepManager.CleanRatio);
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
