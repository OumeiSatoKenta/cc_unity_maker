using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game076_ChordCatch
{
    public class ChordCatchUI : MonoBehaviour
    {
        [SerializeField, Tooltip("問題番号")] private TextMeshProUGUI _questionText;
        [SerializeField, Tooltip("スコア")] private TextMeshProUGUI _scoreText;
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

        public void UpdateQuestion(int q, int total) { if (_questionText != null) _questionText.text = $"Q{q}/{total}"; }
        public void UpdateScore(int s) { if (_scoreText != null) _scoreText.text = $"正解: {s}"; }
        public void UpdateMisses(int m, int max) { if (_missText != null) _missText.text = $"Miss: {m}/{max}"; }
        public void ShowFeedback(bool correct) { if (_feedbackText != null) { _feedbackText.text = correct ? "正解！" : "不正解…"; _feedbackText.color = correct ? Color.green : Color.red; _feedbackTimer = 0.8f; } }
        public void ShowClear(int correct, int total) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"{correct}/{total}問正解！"; }
        public void ShowGameOver(int correct, int total) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"{correct}/{total}問正解"; }
    }
}
