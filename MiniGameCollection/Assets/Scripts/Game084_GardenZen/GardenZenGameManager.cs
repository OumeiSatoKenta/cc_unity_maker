using UnityEngine;

namespace Game084_GardenZen
{
    public class GardenZenGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private ZenManager _zenManager;
        [SerializeField, Tooltip("UI管理")] private GardenZenUI _ui;
        [SerializeField, Tooltip("目標配置数")] private int _targetPlacements = 10;

        private bool _isPlaying;

        private void Start()
        {
            _isPlaying = true;
            _zenManager.StartGame();
            _ui.UpdatePlacements(_zenManager.PlacementCount, _targetPlacements);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _ui.UpdatePlacements(_zenManager.PlacementCount, _targetPlacements);

            if (_zenManager.PlacementCount >= _targetPlacements)
            {
                _isPlaying = false;
                _zenManager.StopGame();
                _ui.ShowClear(_zenManager.PlacementCount);
            }
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
