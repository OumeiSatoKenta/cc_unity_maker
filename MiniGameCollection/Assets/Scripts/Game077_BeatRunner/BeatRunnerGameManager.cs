using UnityEngine;

namespace Game077_BeatRunner
{
    public class BeatRunnerGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private RunManager _runManager;
        [SerializeField, Tooltip("UI管理")] private BeatRunnerUI _ui;
        [SerializeField, Tooltip("楽曲長(秒)")] private float _songLength = 30f;
        [SerializeField, Tooltip("衝突上限")] private int _maxHits = 3;

        private int _score;
        private int _combo;
        private int _hitCount;
        private float _songTimer;
        private bool _isPlaying;

        private void Start()
        {
            _score = 0; _combo = 0; _hitCount = 0; _songTimer = 0f;
            _isPlaying = true;
            _runManager.StartGame();
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_combo);
            _ui.UpdateHits(_hitCount, _maxHits);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _songTimer += Time.deltaTime;
            _ui.UpdateProgress(_songTimer / _songLength);

            if (_songTimer >= _songLength)
            {
                _isPlaying = false;
                _runManager.StopGame();
                _ui.ShowClear(_score);
            }
        }

        public void OnBeatHit()
        {
            if (!_isPlaying) return;
            _combo++;
            _score += 50 + _combo * 10;
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_combo);
        }

        public void OnBeatMiss()
        {
            if (!_isPlaying) return;
            _combo = 0;
            _ui.UpdateCombo(_combo);
        }

        public void OnObstacleHit()
        {
            if (!_isPlaying) return;
            _hitCount++;
            _combo = 0;
            _ui.UpdateHits(_hitCount, _maxHits);
            _ui.UpdateCombo(_combo);

            if (_hitCount >= _maxHits)
            {
                _isPlaying = false;
                _runManager.StopGame();
                _ui.ShowGameOver(_score);
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
