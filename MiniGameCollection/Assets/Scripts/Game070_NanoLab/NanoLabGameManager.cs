using UnityEngine;

namespace Game070_NanoLab
{
    public class NanoLabGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private NanoManager _nanoManager;
        [SerializeField, Tooltip("UI管理")] private NanoLabUI _ui;
        [SerializeField, Tooltip("技術ツリー総数")] private int _totalTech = 6;

        private long _nanobots;
        private bool _isPlaying;

        private void Start()
        {
            _nanobots = 10;
            _isPlaying = true;
            _nanoManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;

            long autoGen = _nanoManager.AutoGenerate;
            if (autoGen > 0)
            {
                _nanobots += autoGen;
                UpdateUI();
            }

            if (_nanoManager.UnlockedTech >= _totalTech)
            {
                _isPlaying = false;
                _nanoManager.StopGame();
                _ui.ShowClear(_nanoManager.UnlockedTech);
            }
        }

        public void OnTap()
        {
            if (!_isPlaying) return;
            _nanobots += 1 + _nanoManager.TapBonus;
            UpdateUI();
        }

        private void UpdateUI()
        {
            _ui.UpdateNanobots(_nanobots);
            _ui.UpdateTech(_nanoManager.UnlockedTech, _totalTech);
            _ui.UpdateMultiplier(_nanoManager.MultiplierLevel, _nanoManager.NextMultiplierCost);
            _ui.UpdateResearch(_nanoManager.NextResearchCost);
        }

        public bool TrySpend(long cost)
        {
            if (_nanobots >= cost) { _nanobots -= cost; UpdateUI(); return true; }
            return false;
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public long Nanobots => _nanobots;
    }
}
