using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game086_CityBonsai
{
    public class CityBonsaiUI : MonoBehaviour
    {
        [SerializeField, Tooltip("コイン")] private TextMeshProUGUI _coinText;
        [SerializeField, Tooltip("人口")] private TextMeshProUGUI _populationText;
        [SerializeField, Tooltip("建物数")] private TextMeshProUGUI _buildingText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateCoins(int c) { if (_coinText != null) _coinText.text = $"コイン: {c}"; }
        public void UpdatePopulation(int p, int target) { if (_populationText != null) _populationText.text = $"人口: {p}/{target}"; }
        public void UpdateBuildings(int b) { if (_buildingText != null) _buildingText.text = $"建物: {b}"; }
        public void ShowClear(int pop, int buildings) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"人口{pop} / {buildings}棟！"; }
    }
}
