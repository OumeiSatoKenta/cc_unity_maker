using UnityEngine;

namespace Game032_SpinCutter
{
    public class SpinCutterGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private SpinCutterManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private SpinCutterUI _ui;

        [SerializeField, Tooltip("総敵数")]
        private int _totalEnemies = 8;

        [SerializeField, Tooltip("最大発射回数")]
        private int _maxLaunches = 3;

        private int _killedCount;
        private int _launchesUsed;
        private bool _isPlaying;

        private void Start()
        {
            _killedCount = 0;
            _launchesUsed = 0;
            _isPlaying = true;
            _ui.UpdateLaunches(_maxLaunches - _launchesUsed);
            _ui.UpdateKills(0, _totalEnemies);
            _manager.StartStage();
        }

        public void AddKill()
        {
            if (!_isPlaying) return;
            _killedCount++;
            _ui.UpdateKills(_killedCount, _totalEnemies);

            if (_killedCount >= _totalEnemies)
            {
                _isPlaying = false;
                // AddKill はOnLaunchUsed より前に呼ばれるため+1して発射中の1回を含める
                int effectiveLaunches = _launchesUsed + 1;
                int stars = effectiveLaunches <= 1 ? 3 : (effectiveLaunches == 2 ? 2 : 1);
                _ui.ShowClear(stars);
            }
        }

        public void OnLaunchUsed()
        {
            if (!_isPlaying) return;
            _launchesUsed++;
            _ui.UpdateLaunches(_maxLaunches - _launchesUsed);

            if (_launchesUsed >= _maxLaunches && _killedCount < _totalEnemies)
            {
                _isPlaying = false;
                _ui.ShowGameOver();
            }
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int MaxLaunches => _maxLaunches;
        public int LaunchesUsed => _launchesUsed;
    }
}
