using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game085_MechPet
{
    public class MechPetUI : MonoBehaviour
    {
        [SerializeField, Tooltip("コイン")] private TextMeshProUGUI _coinText;
        [SerializeField, Tooltip("パワー")] private TextMeshProUGUI _powerText;
        [SerializeField, Tooltip("強化ボタン")] private Button _upgradeButton;
        [SerializeField, Tooltip("強化テキスト")] private TextMeshProUGUI _upgradeButtonText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateCoins(int c) { if (_coinText != null) _coinText.text = $"コイン: {c}"; }
        public void UpdatePower(int p, int target) { if (_powerText != null) _powerText.text = $"パワー: {p}/{target}"; }
        public void UpdateUpgradeCost(int cost) { if (_upgradeButtonText != null) _upgradeButtonText.text = $"強化\n{cost}"; }
        public void ShowClear(int power) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"パワー{power}達成！"; }
    }
}
