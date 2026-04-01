using UnityEngine;

namespace Game065_SpellBrewery
{
    public class SpellBreweryGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private BrewManager _brewManager;
        [SerializeField, Tooltip("UI管理")] private SpellBreweryUI _ui;
        [SerializeField, Tooltip("全レシピ数")] private int _totalRecipes = 5;

        private bool _isPlaying;

        private void Start()
        {
            _isPlaying = true;
            _brewManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            UpdateUI();

            if (_brewManager.DiscoveredRecipes >= _totalRecipes)
            {
                _isPlaying = false;
                _brewManager.StopGame();
                _ui.ShowClear(_brewManager.DiscoveredRecipes);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateIngredients(_brewManager.GetIngredientText());
            _ui.UpdateRecipes(_brewManager.DiscoveredRecipes, _totalRecipes);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
