using UnityEngine;

namespace Game061_CookieFactory
{
    public class CookieFactoryGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private FactoryManager _factoryManager;
        [SerializeField, Tooltip("UI管理")] private CookieFactoryUI _ui;
        [SerializeField, Tooltip("目標売上")] private int _targetSales = 500;

        private int _totalCookies;
        private int _totalSales;
        private bool _isPlaying;

        private void Start()
        {
            _totalCookies = 0;
            _totalSales = 0;
            _isPlaying = true;
            _factoryManager.StartGame();
            _ui.UpdateCookies(_totalCookies);
            _ui.UpdateSales(_totalSales, _targetSales);
        }

        private void Update()
        {
            if (!_isPlaying) return;

            // Auto production from upgrades
            int autoCookies = _factoryManager.AutoProduction;
            if (autoCookies > 0)
            {
                _totalCookies += autoCookies;
                _totalSales += autoCookies;
                _ui.UpdateCookies(_totalCookies);
                _ui.UpdateSales(_totalSales, _targetSales);
            }

            if (_totalSales >= _targetSales)
            {
                _isPlaying = false;
                _factoryManager.StopGame();
                _ui.ShowClear(_totalSales);
            }
        }

        public void OnCookieBaked()
        {
            if (!_isPlaying) return;
            _totalCookies++;
            _totalSales++;
            _ui.UpdateCookies(_totalCookies);
            _ui.UpdateSales(_totalSales, _targetSales);
        }

        public bool TrySpend(int cost)
        {
            if (_totalCookies >= cost)
            {
                _totalCookies -= cost;
                _ui.UpdateCookies(_totalCookies);
                return true;
            }
            return false;
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int Cookies => _totalCookies;
    }
}
