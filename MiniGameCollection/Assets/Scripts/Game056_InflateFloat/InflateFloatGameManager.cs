using UnityEngine;

namespace Game056_InflateFloat
{
    public class InflateFloatGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private FloatManager _floatManager;
        [SerializeField, Tooltip("UI管理")] private InflateFloatUI _ui;

        private bool _isPlaying;
        private float _elapsedTime;

        private void Start()
        {
            _isPlaying = true;
            _elapsedTime = 0f;
            _floatManager.StartGame();
            _ui.UpdateSize(0.5f);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _elapsedTime += Time.deltaTime;
            _ui.UpdateSize(_floatManager.SizeRatio);
        }

        public void OnReachedGoal()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _floatManager.StopGame();
            _ui.ShowClear(_elapsedTime);
        }

        public void OnBalloonPopped()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _floatManager.StopGame();
            _ui.ShowGameOver();
        }

        public void OnBalloonFallen()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _floatManager.StopGame();
            _ui.ShowGameOver();
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
