using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game058_ThreadNeedle
{
    public class ThreadNeedleUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _missText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private ThreadNeedleGameManager _gameManager;

        public void UpdateScore(int s) { if (_scoreText != null) _scoreText.text = "スコア: " + s; }
        public void UpdateMisses(int m, int max) { if (_missText != null) _missText.text = "ミス: " + m + "/" + max; }
        public void ShowGameOverPanel(int s) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_finalScoreText != null) _finalScoreText.text = "スコア: " + s; }
        public void HideGameOverPanel() { if (_gameOverPanel != null) _gameOverPanel.SetActive(false); }
        public void OnRetryButton() { if (_gameManager != null) _gameManager.StartGame(); }
    }
}
