using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game096_DualControl
{
    public class DualControlUI : MonoBehaviour
    {
        [SerializeField, Tooltip("タイマーテキスト")] private TextMeshProUGUI _timerText;
        [SerializeField, Tooltip("ステージテキスト（左）")] private TextMeshProUGUI _stageLeftText;
        [SerializeField, Tooltip("ステージテキスト（右）")] private TextMeshProUGUI _stageRightText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコアテキスト")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライボタン")] private Button _clearRetryButton;
        [SerializeField, Tooltip("ゲームオーバーパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("ゲームオーバーリトライボタン")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニューへ戻るボタン")] private Button _menuButton;

        public void UpdateTimer(float t)
        { if (_timerText) _timerText.text = $"{(int)t / 60:00}:{t % 60:00.0}"; }

        public void UpdateStage(int left, int right, int goal)
        {
            if (_stageLeftText)  _stageLeftText.text  = $"左 {left}/{goal}";
            if (_stageRightText) _stageRightText.text = $"右 {right}/{goal}";
        }

        public void ShowClear(float time)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearScoreText) _clearScoreText.text = $"クリア！\n{(int)time / 60:00}:{time % 60:00.0}";
        }

        public void ShowGameOver()
        { if (_gameOverPanel) _gameOverPanel.SetActive(true); }
    }
}
