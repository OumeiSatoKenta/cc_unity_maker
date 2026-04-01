using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game045_FingerPaint
{
    public class FingerPaintUI : MonoBehaviour
    {
        [SerializeField, Tooltip("一致率テキスト")] private TextMeshProUGUI _matchText;
        [SerializeField, Tooltip("インクスライダー")] private Slider _inkSlider;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリア★テキスト")] private TextMeshProUGUI _clearStarText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOテキスト")] private TextMeshProUGUI _gameOverText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateMatch(float m) { if (_matchText != null) _matchText.text = $"一致率: {m:F0}%"; }
        public void UpdateInk(float ink, float max) { if (_inkSlider != null) _inkSlider.value = ink / max; }
        public void ShowClear(int stars, float match)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearStarText != null) _clearStarText.text = new string('\u2605', stars) + new string('\u2606', 3 - stars) + $"\n{match:F0}%";
        }
        public void ShowGameOver(float match)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverText != null) _gameOverText.text = $"一致率: {match:F0}%\n50%未満...";
        }
    }
}
