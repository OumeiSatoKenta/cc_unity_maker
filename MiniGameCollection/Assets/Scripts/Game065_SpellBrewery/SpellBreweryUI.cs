using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game065_SpellBrewery
{
    public class SpellBreweryUI : MonoBehaviour
    {
        [SerializeField, Tooltip("材料テキスト")] private TextMeshProUGUI _ingredientText;
        [SerializeField, Tooltip("レシピテキスト")] private TextMeshProUGUI _recipeText;
        [SerializeField, Tooltip("ハーブボタン")] private Button _herbButton;
        [SerializeField, Tooltip("クリスタルボタン")] private Button _crystalButton;
        [SerializeField, Tooltip("キノコボタン")] private Button _mushroomButton;
        [SerializeField, Tooltip("リセットボタン")] private Button _resetButton;
        [SerializeField, Tooltip("BrewManager")] private BrewManager _brewManager;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateIngredients(string text) { if (_ingredientText != null) _ingredientText.text = text; }
        public void UpdateRecipes(int found, int total) { if (_recipeText != null) _recipeText.text = $"レシピ: {found}/{total}"; }
        public void ShowClear(int recipes) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{recipes}レシピ発見！"; }
    }
}
