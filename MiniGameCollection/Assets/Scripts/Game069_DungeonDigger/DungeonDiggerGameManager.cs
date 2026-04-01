using UnityEngine;

namespace Game069_DungeonDigger
{
    public class DungeonDiggerGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private DigManager _digManager;
        [SerializeField, Tooltip("UI管理")] private DungeonDiggerUI _ui;
        [SerializeField, Tooltip("最深部")] private int _maxDepth = 50;

        private int _gems;
        private int _depth;
        private bool _isPlaying;

        private void Start()
        {
            _gems = 0;
            _depth = 0;
            _isPlaying = true;
            _digManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;

            int autoDig = _digManager.AutoDig;
            if (autoDig > 0)
            {
                _depth += autoDig;
                // Random gem find
                if (Random.value < 0.3f) _gems += autoDig;
                UpdateUI();
                CheckClear();
            }
        }

        public void OnDig()
        {
            if (!_isPlaying) return;
            int power = 1 + _digManager.DrillLevel;
            _depth += power;
            // Deeper = more gems
            if (Random.value < 0.2f + _depth * 0.005f)
                _gems += 1 + _depth / 10;
            UpdateUI();
            CheckClear();
        }

        private void CheckClear()
        {
            if (_depth >= _maxDepth)
            {
                _isPlaying = false;
                _digManager.StopGame();
                _ui.ShowClear(_depth, _gems);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateDepth(_depth, _maxDepth);
            _ui.UpdateGems(_gems);
            _ui.UpdateDrill(_digManager.DrillLevel, _digManager.NextDrillCost);
        }

        public bool TrySpend(int cost)
        {
            if (_gems >= cost) { _gems -= cost; UpdateUI(); return true; }
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
