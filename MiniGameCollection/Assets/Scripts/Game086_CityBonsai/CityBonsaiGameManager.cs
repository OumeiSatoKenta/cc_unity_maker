using UnityEngine;

namespace Game086_CityBonsai
{
    public class CityBonsaiGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private CityBonsaiManager _cityManager;
        [SerializeField, Tooltip("UI管理")] private CityBonsaiUI _ui;
        [SerializeField, Tooltip("目標人口")] private int _targetPopulation = 30;

        private int _coins;
        private bool _isPlaying;

        private void Start()
        {
            _coins = 15;
            _isPlaying = true;
            _cityManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            int autoCoins = _cityManager.AutoIncome;
            if (autoCoins > 0) { _coins += autoCoins; UpdateUI(); }

            if (_cityManager.Population >= _targetPopulation)
            {
                _isPlaying = false;
                _cityManager.StopGame();
                _ui.ShowClear(_cityManager.Population, _cityManager.BuildingCount);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateCoins(_coins);
            _ui.UpdatePopulation(_cityManager.Population, _targetPopulation);
            _ui.UpdateBuildings(_cityManager.BuildingCount);
        }

        public bool TrySpend(int cost)
        {
            if (_coins >= cost) { _coins -= cost; UpdateUI(); return true; }
            return false;
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
