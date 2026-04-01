using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game067_TapDojo
{
    public class TapDojoUI : MonoBehaviour
    {
        [SerializeField, Tooltip("段位テキスト")] private TextMeshProUGUI _rankText;
        [SerializeField, Tooltip("ポイントテキスト")] private TextMeshProUGUI _pointsText;
        [SerializeField, Tooltip("技ボタン")] private Button _techniqueButton;
        [SerializeField, Tooltip("技ボタンテキスト")] private TextMeshProUGUI _techniqueButtonText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateRank(string rank) { if (_rankText != null) _rankText.text = rank; }
        public void UpdatePoints(int pts, int nextRank) { if (_pointsText != null) _pointsText.text = $"修行: {pts}/{nextRank}"; }
        public void UpdateTechnique(int lv, int cost) { if (_techniqueButtonText != null) _techniqueButtonText.text = $"技Lv{lv}\n{cost}pt"; }
        public void ShowClear(string rank) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"{rank}に昇段！"; }
    }
}
