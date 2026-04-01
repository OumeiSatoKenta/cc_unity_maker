using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game064_AquaCity
{
    public class AquaCityUI : MonoBehaviour
    {
        [SerializeField, Tooltip("人口")] private TextMeshProUGUI _populationText;
        [SerializeField, Tooltip("コイン")] private TextMeshProUGUI _coinText;
        [SerializeField, Tooltip("魚数")] private TextMeshProUGUI _fishText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdatePopulation(int pop, int target) { if (_populationText != null) _populationText.text = $"人口: {pop}/{target}"; }
        public void UpdateCoins(int c) { if (_coinText != null) _coinText.text = $"コイン: {c}"; }
        public void UpdateFish(int f) { if (_fishText != null) _fishText.text = $"魚: {f}種"; }
        public void ShowClear(int pop, int fish) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"人口{pop} / 魚{fish}種！"; }
    }
}
