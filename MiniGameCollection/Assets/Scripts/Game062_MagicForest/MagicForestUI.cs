using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game062_MagicForest
{
    public class MagicForestUI : MonoBehaviour
    {
        [SerializeField, Tooltip("木の数")] private TextMeshProUGUI _treeCountText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateTrees(int count, int target) { if (_treeCountText != null) _treeCountText.text = $"森: {count}/{target}本"; }
        public void ShowClear(int trees) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"{trees}本の森が完成！"; }
    }
}
