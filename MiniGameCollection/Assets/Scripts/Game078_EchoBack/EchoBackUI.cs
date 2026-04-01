using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game078_EchoBack
{
    public class EchoBackUI : MonoBehaviour
    {
        [SerializeField, Tooltip("ラウンド")] private TextMeshProUGUI _roundText;
        [SerializeField, Tooltip("ミス")] private TextMeshProUGUI _missText;
        [SerializeField, Tooltip("フィードバック")] private TextMeshProUGUI _feedbackText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        private float _feedbackTimer;

        private void Update()
        {
            if (_feedbackTimer > 0f) { _feedbackTimer -= Time.deltaTime; if (_feedbackTimer <= 0f && _feedbackText != null) _feedbackText.text = ""; }
        }

        public void UpdateRound(int r, int total) { if (_roundText != null) _roundText.text = $"Round {r}/{total}"; }
        public void UpdateMisses(int m, int max) { if (_missText != null) _missText.text = $"Miss: {m}/{max}"; }
        public void ShowFeedback(bool correct) { if (_feedbackText != null) { _feedbackText.text = correct ? "正解！" : "不正解…"; _feedbackText.color = correct ? Color.green : Color.red; _feedbackTimer = 0.6f; } }
        public void ShowClear(int rounds) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{rounds}ラウンドクリア！"; }
        public void ShowGameOver(int rounds) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"Round {rounds}まで"; }
    }
}
