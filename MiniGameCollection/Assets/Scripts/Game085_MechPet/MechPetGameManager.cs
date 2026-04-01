using UnityEngine;

namespace Game085_MechPet
{
    public class MechPetGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private MechManager _mechManager;
        [SerializeField, Tooltip("UI管理")] private MechPetUI _ui;
        [SerializeField, Tooltip("目標パワー")] private int _targetPower = 50;

        private int _coins;
        private bool _isPlaying;

        private void Start()
        {
            _coins = 15;
            _isPlaying = true;
            _mechManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            int autoCoins = _mechManager.AutoIncome;
            if (autoCoins > 0) { _coins += autoCoins; UpdateUI(); }

            if (_mechManager.TotalPower >= _targetPower)
            {
                _isPlaying = false;
                _mechManager.StopGame();
                _ui.ShowClear(_mechManager.TotalPower);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateCoins(_coins);
            _ui.UpdatePower(_mechManager.TotalPower, _targetPower);
            _ui.UpdateUpgradeCost(_mechManager.NextUpgradeCost);
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
