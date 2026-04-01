using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game043_BallSort3D
{
    public class BallSortUI : MonoBehaviour
    {
        [SerializeField, Tooltip("手数テキスト")] private TextMeshProUGUI _movesText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateMoves(int m) { if (_movesText != null) _movesText.text = $"手数: {m}"; }
        public void ShowClear(int m) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"{m}手でクリア！"; }
    }
}
