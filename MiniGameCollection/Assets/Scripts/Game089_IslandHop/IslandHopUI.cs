using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game089_IslandHop
{
    public class IslandHopUI : MonoBehaviour
    {
        [SerializeField, Tooltip("資源")] private TextMeshProUGUI _resourceText;
        [SerializeField, Tooltip("島数")] private TextMeshProUGUI _islandText;
        [SerializeField, Tooltip("開拓ボタン")] private Button _expandButton;
        [SerializeField, Tooltip("開拓テキスト")] private TextMeshProUGUI _expandButtonText;
        [SerializeField, Tooltip("IslandManager")] private IslandManager _islandManager;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        private void Update()
        {
            if (_expandButtonText != null && _islandManager != null)
                _expandButtonText.text = $"開拓\n{_islandManager.NextExpandCost}";
        }

        public void UpdateResources(int r) { if (_resourceText != null) _resourceText.text = $"資源: {r}"; }
        public void UpdateIslands(int i, int target) { if (_islandText != null) _islandText.text = $"島: {i}/{target}"; }
        public void ShowClear(int islands) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"リゾート{islands}島完成！"; }
    }
}
