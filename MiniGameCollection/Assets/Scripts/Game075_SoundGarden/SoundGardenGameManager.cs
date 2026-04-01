using UnityEngine;

namespace Game075_SoundGarden
{
    public class SoundGardenGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private GardenManager _gardenManager;
        [SerializeField, Tooltip("UI管理")] private SoundGardenUI _ui;
        [SerializeField, Tooltip("全植物種類")] private int _totalPlantTypes = 4;

        private bool _isPlaying;

        private void Start()
        {
            _isPlaying = true;
            _gardenManager.StartGame();
            _ui.UpdateCollection(_gardenManager.GrownTypes, _totalPlantTypes);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _ui.UpdateCollection(_gardenManager.GrownTypes, _totalPlantTypes);

            if (_gardenManager.GrownTypes >= _totalPlantTypes)
            {
                _isPlaying = false;
                _gardenManager.StopGame();
                _ui.ShowClear(_gardenManager.GrownTypes);
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
