using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game060_MeltIce
{
    public class MeltIceUI : MonoBehaviour
    {
        [SerializeField, Tooltip("残り鏡")] private TextMeshProUGUI _mirrorsText;
        [SerializeField, Tooltip("残り氷")] private TextMeshProUGUI _iceText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateMirrors(int remaining) { if (_mirrorsText != null) _mirrorsText.text = $"鏡: {remaining}"; }
        public void UpdateIce(int remaining, int total) { if (_iceText != null) _iceText.text = $"氷: {remaining}/{total}"; }
        public void ShowClear(int mirrors) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"鏡{mirrors}枚でクリア！"; }
        public void ShowGameOver() { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); }
    }
}
