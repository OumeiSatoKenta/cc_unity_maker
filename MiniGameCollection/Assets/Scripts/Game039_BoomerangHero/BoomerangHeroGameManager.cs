using UnityEngine;

namespace Game039_BoomerangHero
{
    public class BoomerangHeroGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private BoomerangManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private BoomerangHeroUI _ui;

        [SerializeField, Tooltip("総敵数")]
        private int _totalEnemies = 8;

        [SerializeField, Tooltip("最大投擲回数")]
        private int _maxThrows = 3;

        private int _killedCount;
        private int _throwCount;
        private bool _isPlaying;

        private void Start()
        {
            _killedCount = 0;
            _throwCount = 0;
            _isPlaying = true;
            _ui.UpdateKills(0, _totalEnemies);
            _ui.UpdateThrows(_maxThrows);
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
                int stars = _throwCount <= 1 ? 3 : (_throwCount == 2 ? 2 : 1);
                _ui.ShowClear(stars);
            }
        }

        public void OnThrowUsed()
        {
            if (!_isPlaying) return;
            _throwCount++;
            _ui.UpdateThrows(_maxThrows - _throwCount);
        }

        public void OnBoomerangReturned()
        {
            if (!_isPlaying) return;
            if (_throwCount >= _maxThrows && _killedCount < _totalEnemies)
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
    }
}
