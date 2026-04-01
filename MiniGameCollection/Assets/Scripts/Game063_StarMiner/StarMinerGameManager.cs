using UnityEngine;

namespace Game063_StarMiner
{
    public class StarMinerGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private MineManager _mineManager;
        [SerializeField, Tooltip("UI管理")] private StarMinerUI _ui;
        [SerializeField, Tooltip("目標鉱石数")] private int _targetOre = 100;

        private int _totalOre;
        private bool _isPlaying;

        private void Start()
        {
            _totalOre = 0;
            _isPlaying = true;
            _mineManager.StartGame();
            _ui.UpdateOre(_totalOre, _targetOre);
            _ui.UpdateDrill(_mineManager.DrillLevel, _mineManager.NextDrillCost);
        }

        private void Update()
        {
            if (!_isPlaying) return;

            int autoOre = _mineManager.AutoMine;
            if (autoOre > 0)
            {
                _totalOre += autoOre;
                _ui.UpdateOre(_totalOre, _targetOre);
            }

            if (_totalOre >= _targetOre)
            {
                _isPlaying = false;
                _mineManager.StopGame();
                _ui.ShowClear(_totalOre);
            }
        }

        public void OnOreMined(int amount)
        {
            if (!_isPlaying) return;
            _totalOre += amount;
            _ui.UpdateOre(_totalOre, _targetOre);
        }

        public bool TrySpend(int cost)
        {
            if (_totalOre >= cost)
            {
                _totalOre -= cost;
                _ui.UpdateOre(_totalOre, _targetOre);
                _ui.UpdateDrill(_mineManager.DrillLevel, _mineManager.NextDrillCost);
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
        public int Ore => _totalOre;
    }
}
