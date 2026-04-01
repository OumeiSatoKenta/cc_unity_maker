using UnityEngine;

namespace Game064_AquaCity
{
    public class AquaCityGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private CityManager _cityManager;
        [SerializeField, Tooltip("UI管理")] private AquaCityUI _ui;
        [SerializeField, Tooltip("目標人口")] private int _targetPopulation = 50;

        private int _coins;
        private bool _isPlaying;

        private void Start()
        {
            _coins = 20;
            _isPlaying = true;
            _cityManager.StartGame();
            _ui.UpdateCoins(_coins);
            _ui.UpdatePopulation(_cityManager.Population, _targetPopulation);
            _ui.UpdateFish(_cityManager.FishCount);
        }

        private void Update()
        {
            if (!_isPlaying) return;

            // Auto coin generation from buildings
            int autoCoins = _cityManager.AutoIncome;
            if (autoCoins > 0)
            {
                _coins += autoCoins;
                _ui.UpdateCoins(_coins);
            }

            _ui.UpdatePopulation(_cityManager.Population, _targetPopulation);
            _ui.UpdateFish(_cityManager.FishCount);

            if (_cityManager.Population >= _targetPopulation)
            {
                _isPlaying = false;
                _cityManager.StopGame();
                _ui.ShowClear(_cityManager.Population, _cityManager.FishCount);
            }
        }

        public void OnBuildingTapped()
        {
            if (!_isPlaying) return;
            _coins += 3;
            _ui.UpdateCoins(_coins);
        }

        public bool TrySpend(int cost)
        {
            if (_coins >= cost)
            {
                _coins -= cost;
                _ui.UpdateCoins(_coins);
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
        public int Coins => _coins;
    }
}
