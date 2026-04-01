using UnityEngine;

namespace Game044_TiltMaze
{
    public class TiltMazeGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private MazeManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private TiltMazeUI _ui;

        [SerializeField, Tooltip("制限時間")]
        private float _timeLimit = 60f;

        private float _timer;
        private bool _isPlaying;

        private void Start()
        {
            _timer = _timeLimit;
            _isPlaying = true;
            _ui.UpdateTimer(_timer);
            _manager.StartGame();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _timer -= Time.deltaTime;
            _ui.UpdateTimer(Mathf.Max(0, _timer));
            if (_timer <= 0f)
            {
                _isPlaying = false;
                _ui.ShowGameOver();
            }
        }

        public void OnReachGoal()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _ui.ShowClear(_timer);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
