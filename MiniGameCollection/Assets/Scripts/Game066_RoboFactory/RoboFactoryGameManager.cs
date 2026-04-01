using UnityEngine;

namespace Game066_RoboFactory
{
    public class RoboFactoryGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private RoboManager _roboManager;
        [SerializeField, Tooltip("UI管理")] private RoboFactoryUI _ui;
        [SerializeField, Tooltip("目標都市レベル")] private int _targetCityLevel = 5;

        private int _resources;
        private int _cityLevel;
        private bool _isPlaying;

        private void Start()
        {
            _resources = 10;
            _cityLevel = 1;
            _isPlaying = true;
            _roboManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;

            int autoRes = _roboManager.AutoGather;
            if (autoRes > 0)
            {
                _resources += autoRes;
                UpdateUI();
            }

            if (_cityLevel >= _targetCityLevel)
            {
                _isPlaying = false;
                _roboManager.StopGame();
                _ui.ShowClear(_cityLevel, _roboManager.RobotCount);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateResources(_resources);
            _ui.UpdateRobots(_roboManager.RobotCount);
            _ui.UpdateCityLevel(_cityLevel, _targetCityLevel);
            _ui.UpdateCosts(_roboManager.NextRobotCost, NextBuildingCost);
        }

        public bool TrySpendForRobot(int cost)
        {
            if (_resources >= cost) { _resources -= cost; UpdateUI(); return true; }
            return false;
        }

        public void BuildBuilding()
        {
            if (!_isPlaying) return;
            int cost = NextBuildingCost;
            if (_resources >= cost)
            {
                _resources -= cost;
                _cityLevel++;
                UpdateUI();
            }
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int Resources => _resources;
        private int NextBuildingCost => 20 + (_cityLevel - 1) * 15;
    }
}
