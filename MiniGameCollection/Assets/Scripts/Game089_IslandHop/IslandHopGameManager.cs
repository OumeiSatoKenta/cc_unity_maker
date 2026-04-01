using UnityEngine;

namespace Game089_IslandHop
{
    public class IslandHopGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private IslandManager _islandManager;
        [SerializeField, Tooltip("UI管理")] private IslandHopUI _ui;
        [SerializeField, Tooltip("目標島数")] private int _targetIslands = 5;

        private int _resources;
        private bool _isPlaying;

        private void Start()
        {
            _resources = 10;
            _isPlaying = true;
            _islandManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            int autoRes = _islandManager.AutoGather;
            if (autoRes > 0) { _resources += autoRes; UpdateUI(); }

            if (_islandManager.IslandCount >= _targetIslands)
            {
                _isPlaying = false;
                _islandManager.StopGame();
                _ui.ShowClear(_islandManager.IslandCount);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateResources(_resources);
            _ui.UpdateIslands(_islandManager.IslandCount, _targetIslands);
        }

        public bool TrySpend(int cost)
        {
            if (_resources >= cost) { _resources -= cost; UpdateUI(); return true; }
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
