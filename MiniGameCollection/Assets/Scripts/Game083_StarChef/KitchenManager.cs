using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game083_StarChef
{
    public class KitchenManager : MonoBehaviour
    {
        [SerializeField, Tooltip("星の粉スプライト")] private Sprite _stardustSprite;
        [SerializeField, Tooltip("月光ジュースププライト")] private Sprite _moonjuiceSprite;
        [SerializeField, Tooltip("鍋スプライト")] private Sprite _potSprite;

        private bool _isActive;
        private int[] _ingredients; // 0=stardust, 1=moonjuice, 2=nebula
        private HashSet<int> _completedRecipes = new HashSet<int>();
        private float _gatherTimer;
        private int _selectedA = -1;
        private int _selectedB = -1;

        private static readonly string[] IngredientNames = { "星の粉", "月光ジュース", "星雲エキス" };

        // Recipes: combo of 2 ingredients
        private static readonly int[,] RecipeTable = {
            { 0, 1, 2 }, // stardust+stardust=0, stardust+moon=1, stardust+nebula=2
            { 1, 3, 4 }, // moon+stardust=1, moon+moon=3, moon+nebula=4
            { 2, 4, -1 }, // nebula+stardust=2, nebula+moon=4, nebula+nebula=invalid
        };

        public void StartGame()
        {
            _isActive = true;
            _ingredients = new int[] { 3, 3, 3 };
            _gatherTimer = 0f;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;
            _gatherTimer += Time.deltaTime;
            if (_gatherTimer >= 4f)
            {
                _gatherTimer = 0f;
                _ingredients[Random.Range(0, 3)]++;
            }
        }

        public void SelectIngredient(int index)
        {
            if (!_isActive || index < 0 || index >= 3) return;
            if (_ingredients[index] <= 0) return;

            if (_selectedA < 0) { _selectedA = index; }
            else if (_selectedB < 0) { _selectedB = index; TryCook(); }
        }

        public void ClearSelection() { _selectedA = -1; _selectedB = -1; }

        private void TryCook()
        {
            int a = _selectedA, b = _selectedB;
            int recipe = RecipeTable[a, b];
            if (recipe >= 0)
            {
                _ingredients[a]--;
                _ingredients[b]--;
                _completedRecipes.Add(recipe);
            }
            _selectedA = -1; _selectedB = -1;
        }

        public string GetIngredientText()
        {
            return $"{IngredientNames[0]}: {_ingredients[0]}  {IngredientNames[1]}: {_ingredients[1]}  {IngredientNames[2]}: {_ingredients[2]}";
        }

        public int CompletedRecipes => _completedRecipes.Count;
    }
}
