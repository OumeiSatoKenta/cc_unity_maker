using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game084_GardenZen
{
    public class GardenZenUI : MonoBehaviour
    {
        [SerializeField, Tooltip("配置数")] private TextMeshProUGUI _placementText;
        [SerializeField, Tooltip("石ボタン")] private Button _stoneButton;
        [SerializeField, Tooltip("苔ボタン")] private Button _mossButton;
        [SerializeField, Tooltip("砂紋ボタン")] private Button _rakeButton;
        [SerializeField, Tooltip("ZenManager")] private ZenManager _zenManager;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdatePlacements(int p, int target) { if (_placementText != null) _placementText.text = $"配置: {p}/{target}"; }
        public void ShowClear(int placements) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"庭完成！ {placements}個配置"; }
    }
}
