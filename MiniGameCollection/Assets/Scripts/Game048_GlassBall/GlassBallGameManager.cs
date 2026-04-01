using UnityEngine;

namespace Game048_GlassBall
{
    public class GlassBallGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("レール管理")] private RailManager _railManager;
        [SerializeField, Tooltip("UI管理")] private GlassBallUI _ui;
        [SerializeField, Tooltip("衝撃上限")] private float _impactMax = 100f;

        private float _impactGauge;
        private float _elapsedTime;
        private bool _isPlaying;

        private void Start()
        {
            _impactGauge = 0f;
            _elapsedTime = 0f;
            _isPlaying = true;
            _railManager.StartGame();
            _ui.UpdateImpact(0f);
            _ui.UpdateInk(1f);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _elapsedTime += Time.deltaTime;
            _ui.UpdateInk(_railManager.InkRatio);
        }

        public void AddImpact(float amount)
        {
            if (!_isPlaying) return;
            _impactGauge += amount;
            _ui.UpdateImpact(_impactGauge / _impactMax);
            if (_impactGauge >= _impactMax) OnBallBroken();
        }

        public void OnBallReachedGoal()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _railManager.StopGame();
            _ui.ShowClear(_elapsedTime);
        }

        public void OnBallFallen()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _railManager.StopGame();
            _ui.ShowGameOver();
        }

        private void OnBallBroken()
        {
            _isPlaying = false;
            _railManager.StopGame();
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
