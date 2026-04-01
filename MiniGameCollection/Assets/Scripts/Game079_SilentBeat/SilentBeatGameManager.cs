using UnityEngine;

namespace Game079_SilentBeat
{
    public class SilentBeatGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private BeatManager _beatManager;
        [SerializeField, Tooltip("UI管理")] private SilentBeatUI _ui;
        [SerializeField, Tooltip("制限時間")] private float _duration = 30f;
        [SerializeField, Tooltip("許容ズレ(秒)")] private float _tolerance = 0.3f;

        private float _timer;
        private int _tapCount;
        private int _perfectCount;
        private bool _isPlaying;

        private void Start()
        {
            _timer = _duration; _tapCount = 0; _perfectCount = 0;
            _isPlaying = true;
            _beatManager.StartGame(_tolerance);
            _ui.UpdateTimer(_timer);
            _ui.UpdateAccuracy(1f);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _timer -= Time.deltaTime;
            _ui.UpdateTimer(Mathf.Max(0f, _timer));

            if (_timer <= 0f)
            {
                _isPlaying = false;
                _beatManager.StopGame();
                float accuracy = _tapCount > 0 ? (float)_perfectCount / _tapCount : 0f;
                _ui.ShowClear(Mathf.RoundToInt(accuracy * 100), _tapCount);
            }
        }

        public void OnTap(float deviation)
        {
            if (!_isPlaying) return;
            _tapCount++;
            if (Mathf.Abs(deviation) <= _tolerance) _perfectCount++;
            float accuracy = _tapCount > 0 ? (float)_perfectCount / _tapCount : 0f;
            _ui.UpdateAccuracy(accuracy);

            if (Mathf.Abs(deviation) > _tolerance * 2f)
            {
                _isPlaying = false;
                _beatManager.StopGame();
                _ui.ShowGameOver(Mathf.RoundToInt(accuracy * 100), _tapCount);
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
