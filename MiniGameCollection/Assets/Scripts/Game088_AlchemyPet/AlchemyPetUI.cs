using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game088_AlchemyPet
{
    public class AlchemyPetUI : MonoBehaviour
    {
        [SerializeField, Tooltip("ペット数")] private TextMeshProUGUI _petText;
        [SerializeField, Tooltip("素材")] private TextMeshProUGUI _elementText;
        [SerializeField, Tooltip("火ボタン")] private Button _fireButton;
        [SerializeField, Tooltip("水ボタン")] private Button _waterButton;
        [SerializeField, Tooltip("土ボタン")] private Button _earthButton;
        [SerializeField, Tooltip("リセットボタン")] private Button _resetButton;
        [SerializeField, Tooltip("AlchemyManager")] private AlchemyManager _alchemyManager;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdatePets(int discovered, int total) { if (_petText != null) _petText.text = $"ペット: {discovered}/{total}"; }
        public void UpdateElements(string text) { if (_elementText != null) _elementText.text = text; }
        public void ShowClear(int pets) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{pets}種発見！"; }
    }
}
