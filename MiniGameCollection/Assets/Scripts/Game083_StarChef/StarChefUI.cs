using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game083_StarChef
{
    public class StarChefUI : MonoBehaviour
    {
        [SerializeField, Tooltip("レシピ")] private TextMeshProUGUI _recipeText;
        [SerializeField, Tooltip("素材")] private TextMeshProUGUI _ingredientText;
        [SerializeField, Tooltip("星の粉ボタン")] private Button _stardustButton;
        [SerializeField, Tooltip("月光ボタン")] private Button _moonjuiceButton;
        [SerializeField, Tooltip("星雲ボタン")] private Button _nebulaButton;
        [SerializeField, Tooltip("リセットボタン")] private Button _resetButton;
        [SerializeField, Tooltip("KitchenManager")] private KitchenManager _kitchenManager;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateRecipes(int done, int total) { if (_recipeText != null) _recipeText.text = $"レシピ: {done}/{total}"; }
        public void UpdateIngredients(string text) { if (_ingredientText != null) _ingredientText.text = text; }
        public void ShowClear(int recipes) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{recipes}レシピ完成！"; }
    }
}
