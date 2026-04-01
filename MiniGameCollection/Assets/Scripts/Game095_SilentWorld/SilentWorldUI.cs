using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game095_SilentWorld
{
    public class SilentWorldUI : MonoBehaviour
    {
        [SerializeField, Tooltip("経過時間テキスト")] private TextMeshProUGUI _timerText;
        [SerializeField, Tooltip("収集アイテム数テキスト")] private TextMeshProUGUI _itemText;
        [SerializeField, Tooltip("ヒント残回数テキスト")] private TextMeshProUGUI _hintText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコアテキスト")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライボタン")] private Button _clearRetryButton;
        [SerializeField, Tooltip("ゲームオーバーパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("ゲームオーバーリトライボタン")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニューへ戻るボタン")] private Button _menuButton;

        public void UpdateTimer(float t)
        { if (_timerText) _timerText.text = $"{(int)t / 60:00}:{t % 60:00.0}"; }

        public void UpdateItems(int c, int total)
        { if (_itemText) _itemText.text = $"音符 {c}/{total}"; }

        public void UpdateHint(int remaining)
        { if (_hintText) _hintText.text = $"ヒント {remaining}"; }

        public void ShowClear(float time)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearScoreText) _clearScoreText.text = $"クリア！\n{(int)time / 60:00}:{time % 60:00.0}";
        }

        public void ShowGameOver()
        { if (_gameOverPanel) _gameOverPanel.SetActive(true); }
    }
}
