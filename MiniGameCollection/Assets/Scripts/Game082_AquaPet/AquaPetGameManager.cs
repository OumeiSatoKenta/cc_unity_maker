using UnityEngine;

namespace Game082_AquaPet
{
    public class AquaPetGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private TankManager _tankManager;
        [SerializeField, Tooltip("UI管理")] private AquaPetUI _ui;
        [SerializeField, Tooltip("全魚種")] private int _totalSpecies = 5;

        private int _coins;
        private bool _isPlaying;

        private void Start()
        {
            _coins = 20;
            _isPlaying = true;
            _tankManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            // Auto coins from fish
            int autoCoins = _tankManager.AutoIncome;
            if (autoCoins > 0) { _coins += autoCoins; UpdateUI(); }

            _ui.UpdateCollection(_tankManager.SpeciesCount, _totalSpecies);
            if (_tankManager.SpeciesCount >= _totalSpecies)
            {
                _isPlaying = false;
                _tankManager.StopGame();
                _ui.ShowClear(_tankManager.SpeciesCount, _tankManager.FishCount);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateCoins(_coins);
            _ui.UpdateFish(_tankManager.FishCount);
            _ui.UpdateCollection(_tankManager.SpeciesCount, _totalSpecies);
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
