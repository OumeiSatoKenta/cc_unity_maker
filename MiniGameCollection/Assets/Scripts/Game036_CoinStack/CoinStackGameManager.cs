using UnityEngine;

namespace Game036_CoinStack
{
    public class CoinStackGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private StackManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private CoinStackUI _ui;

        [SerializeField, Tooltip("目標段数")]
        private int _goalHeight = 15;

        [SerializeField, Tooltip("最大コイン数")]
        private int _maxCoins = 20;

        private int _stackedCount;
        private int _usedCoins;
        private bool _isPlaying;

        private void Start()
        {
            _stackedCount = 0;
            _usedCoins = 0;
            _isPlaying = true;
            _ui.UpdateHeight(_stackedCount, _goalHeight);
            _ui.UpdateRemaining(_maxCoins - _usedCoins);
            _manager.StartGame();
        }

        public void OnCoinStacked()
        {
            if (!_isPlaying) return;
            _stackedCount++;
            _usedCoins++;
            _ui.UpdateHeight(_stackedCount, _goalHeight);
            _ui.UpdateRemaining(_maxCoins - _usedCoins);

            if (_stackedCount >= _goalHeight)
            {
                _isPlaying = false;
                _ui.ShowClear(_stackedCount);
            }
        }

        public void OnCoinMissed()
        {
            if (!_isPlaying) return;
            _usedCoins++;
            _ui.UpdateRemaining(_maxCoins - _usedCoins);

            if (_usedCoins >= _maxCoins && _stackedCount < _goalHeight)
            {
                _isPlaying = false;
                _ui.ShowGameOver(_stackedCount);
            }
        }

        public void OnTowerCollapse()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _ui.ShowGameOver(_stackedCount);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int StackedCount => _stackedCount;
    }
}
