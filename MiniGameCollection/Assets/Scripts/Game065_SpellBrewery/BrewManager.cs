using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game065_SpellBrewery
{
    public class BrewManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ハーブスプライト")] private Sprite _herbSprite;
        [SerializeField, Tooltip("クリスタルスプライト")] private Sprite _crystalSprite;
        [SerializeField, Tooltip("キノコスプライト")] private Sprite _mushroomSprite;
        [SerializeField, Tooltip("ポーションスプライト")] private Sprite _potionSprite;
        [SerializeField, Tooltip("材料収集間隔")] private float _gatherInterval = 3f;

        private bool _isActive;
        private float _gatherTimer;
        private int[] _ingredients; // 0=herb, 1=crystal, 2=mushroom
        private HashSet<int> _discoveredRecipes = new HashSet<int>();
        private int _selectedSlot0 = -1;
        private int _selectedSlot1 = -1;

        // Recipes: pair of ingredient indices -> recipe id
        // 0+1=recipe0, 0+2=recipe1, 1+2=recipe2, 0+0=recipe3, 1+1=recipe4
        private static readonly int[,] RecipeTable = {
            {3, 0, 1},  // herb+herb=3, herb+crystal=0, herb+mushroom=1
            {0, 4, 2},  // crystal+herb=0, crystal+crystal=4, crystal+mushroom=2
            {1, 2, -1}, // mushroom+herb=1, mushroom+crystal=2, mushroom+mushroom=invalid
        };

        public void StartGame()
        {
            _isActive = true;
            _ingredients = new int[] { 5, 5, 5 }; // Start with some materials
            _gatherTimer = _gatherInterval;
            _selectedSlot0 = -1;
            _selectedSlot1 = -1;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Auto-gather ingredients
            _gatherTimer -= Time.deltaTime;
            if (_gatherTimer <= 0f)
            {
                _gatherTimer = _gatherInterval;
                int idx = Random.Range(0, 3);
                _ingredients[idx]++;
            }
        }

        public void SelectIngredient(int index)
        {
            if (!_isActive) return;
            if (index < 0 || index >= 3) return;
            if (_ingredients[index] <= 0) return;

            if (_selectedSlot0 < 0)
            {
                _selectedSlot0 = index;
            }
            else if (_selectedSlot1 < 0)
            {
                _selectedSlot1 = index;
                TryBrew();
            }
        }

        public void ClearSelection()
        {
            _selectedSlot0 = -1;
            _selectedSlot1 = -1;
        }

        private void TryBrew()
        {
            if (_selectedSlot0 < 0 || _selectedSlot1 < 0) return;

            int a = _selectedSlot0;
            int b = _selectedSlot1;

            int recipeId = RecipeTable[a, b];
            if (recipeId >= 0)
            {
                _ingredients[a]--;
                _ingredients[b]--;
                _discoveredRecipes.Add(recipeId);
            }

            _selectedSlot0 = -1;
            _selectedSlot1 = -1;
        }

        public string GetIngredientText()
        {
            string[] names = { "ハーブ", "クリスタル", "キノコ" };
            return $"{names[0]}: {_ingredients[0]}  {names[1]}: {_ingredients[1]}  {names[2]}: {_ingredients[2]}";
        }

        public int DiscoveredRecipes => _discoveredRecipes.Count;
        public int SelectedSlot0 => _selectedSlot0;
        public int SelectedSlot1 => _selectedSlot1;
    }
}
