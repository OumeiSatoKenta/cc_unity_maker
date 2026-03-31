using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game027_DotDodge
{
    public class DotDodgeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private DotDodgeGameManager _gameManager;

        private void Awake()
        {
            if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
        }

        public void UpdateTime(float time) { if (_timeText != null) _timeText.text = $"生存: {time:F1}秒"; }

        public void ShowGameOverPanel(float time)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverText != null) _gameOverText.text = $"ゲームオーバー\n生存時間: {time:F1}秒";
        }

        public void HideGameOverPanel() { if (_gameOverPanel != null) _gameOverPanel.SetActive(false); }
    }
}
