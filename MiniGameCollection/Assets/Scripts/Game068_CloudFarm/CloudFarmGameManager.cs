using UnityEngine;

namespace Game068_CloudFarm
{
    public class CloudFarmGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private FarmManager _farmManager;
        [SerializeField, Tooltip("UI管理")] private CloudFarmUI _ui;
        [SerializeField, Tooltip("全作物種類数")] private int _totalCropTypes = 4;

        private int _coins;
        private bool _isPlaying;

        private void Start()
        {
            _coins = 15;
            _isPlaying = true;
            _farmManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            UpdateUI();

            if (_farmManager.CollectedCropTypes >= _totalCropTypes)
            {
                _isPlaying = false;
                _farmManager.StopGame();
                _ui.ShowClear(_farmManager.CollectedCropTypes, _coins);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateCoins(_coins);
            _ui.UpdateCollection(_farmManager.CollectedCropTypes, _totalCropTypes);
        }

        public void AddCoins(int amount)
        {
            _coins += amount;
            UpdateUI();
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
        public int Coins => _coins;
    }
}
