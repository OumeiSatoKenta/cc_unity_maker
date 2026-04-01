using UnityEngine;

namespace Game033_AimSniper
{
    public class AimSniperGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private SniperManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private AimSniperUI _ui;

        [SerializeField, Tooltip("総ターゲット数")]
        private int _totalTargets = 5;

        [SerializeField, Tooltip("最大弾数")]
        private int _maxBullets = 8;

        private int _hitCount;
        private int _shotCount;
        private int _remainingBullets;
        private bool _isPlaying;

        private void Start()
        {
            _hitCount = 0;
            _shotCount = 0;
            _remainingBullets = _maxBullets;
            _isPlaying = true;
            _ui.UpdateBullets(_remainingBullets);
            _ui.UpdateAccuracy(0f);
            _manager.StartStage();
        }

        public void AddHit()
        {
            if (!_isPlaying) return;
            _hitCount++;
            UpdateAccuracyDisplay();

            if (_hitCount >= _totalTargets)
            {
                _isPlaying = false;
                float accuracy = _shotCount > 0 ? (float)_hitCount / _shotCount * 100f : 0f;
                int stars = accuracy >= 90f ? 3 : (accuracy >= 70f ? 2 : 1);
                _ui.ShowClear(stars);
            }
        }

        public void OnShot()
        {
            if (!_isPlaying) return;
            _shotCount++;
            _remainingBullets--;
            _ui.UpdateBullets(_remainingBullets);
            UpdateAccuracyDisplay();

            if (_remainingBullets <= 0 && _hitCount < _totalTargets)
            {
                _isPlaying = false;
                _ui.ShowGameOver();
            }
        }

        private void UpdateAccuracyDisplay()
        {
            float accuracy = _shotCount > 0 ? (float)_hitCount / _shotCount * 100f : 0f;
            _ui.UpdateAccuracy(accuracy);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int RemainingBullets => _remainingBullets;
    }
}
