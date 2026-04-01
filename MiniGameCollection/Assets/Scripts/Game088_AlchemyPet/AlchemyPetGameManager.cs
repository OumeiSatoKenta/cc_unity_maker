using UnityEngine;

namespace Game088_AlchemyPet
{
    public class AlchemyPetGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private AlchemyManager _alchemyManager;
        [SerializeField, Tooltip("UI管理")] private AlchemyPetUI _ui;
        [SerializeField, Tooltip("全ペット種類")] private int _totalPetTypes = 5;

        private bool _isPlaying;

        private void Start()
        {
            _isPlaying = true;
            _alchemyManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            UpdateUI();
            if (_alchemyManager.DiscoveredPets >= _totalPetTypes)
            {
                _isPlaying = false;
                _alchemyManager.StopGame();
                _ui.ShowClear(_alchemyManager.DiscoveredPets);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdatePets(_alchemyManager.DiscoveredPets, _totalPetTypes);
            _ui.UpdateElements(_alchemyManager.GetElementText());
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
