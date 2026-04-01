using UnityEngine;

namespace Game083_StarChef
{
    public class StarChefGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private KitchenManager _kitchenManager;
        [SerializeField, Tooltip("UI管理")] private StarChefUI _ui;
        [SerializeField, Tooltip("全レシピ数")] private int _totalRecipes = 5;

        private bool _isPlaying;

        private void Start()
        {
            _isPlaying = true;
            _kitchenManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            UpdateUI();
            if (_kitchenManager.CompletedRecipes >= _totalRecipes)
            {
                _isPlaying = false;
                _kitchenManager.StopGame();
                _ui.ShowClear(_kitchenManager.CompletedRecipes);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateRecipes(_kitchenManager.CompletedRecipes, _totalRecipes);
            _ui.UpdateIngredients(_kitchenManager.GetIngredientText());
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
