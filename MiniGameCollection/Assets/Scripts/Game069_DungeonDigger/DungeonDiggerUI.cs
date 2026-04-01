using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game069_DungeonDigger
{
    public class DungeonDiggerUI : MonoBehaviour
    {
        [SerializeField, Tooltip("深度テキスト")] private TextMeshProUGUI _depthText;
        [SerializeField, Tooltip("宝石テキスト")] private TextMeshProUGUI _gemText;
        [SerializeField, Tooltip("ドリルボタン")] private Button _drillButton;
        [SerializeField, Tooltip("ドリルボタンテキスト")] private TextMeshProUGUI _drillButtonText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateDepth(int d, int max) { if (_depthText != null) _depthText.text = $"深度: {d}/{max}m"; }
        public void UpdateGems(int g) { if (_gemText != null) _gemText.text = $"宝石: {g}"; }
        public void UpdateDrill(int lv, int cost) { if (_drillButtonText != null) _drillButtonText.text = $"ドリルLv{lv}\n{cost}個"; }
        public void ShowClear(int depth, int gems) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"深度{depth}m / 宝石{gems}個！"; }
    }
}
