using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game099_TouchMemory
{
    public class TouchMemoryUI : MonoBehaviour
    {
        [SerializeField, Tooltip("ラウンドテキスト")] private TextMeshProUGUI _roundText;
        [SerializeField, Tooltip("ステータステキスト")] private TextMeshProUGUI _statusText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアテキスト")] private TextMeshProUGUI _clearText;
        [SerializeField, Tooltip("ゲームオーバーパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("ゲームオーバーテキスト")] private TextMeshProUGUI _gameOverText;

        public void UpdateRound(int round, int max)
        {
            if (_roundText) _roundText.text = $"ラウンド {round} / {max}";
        }

        public void UpdateStatus(string text)
        {
            if (_statusText) _statusText.text = text;
        }

        public void ShowClearPanel(int round)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearText) _clearText.text = $"完全クリア！\n\n全{round}ラウンド達成！";
        }

        public void HideClearPanel()
        {
            if (_clearPanel) _clearPanel.SetActive(false);
        }

        public void ShowGameOverPanel(int reachedRound)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverText) _gameOverText.text = $"ゲームオーバー\n\n到達ラウンド: {reachedRound}";
        }

        public void HideGameOverPanel()
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
        }
    }
}
