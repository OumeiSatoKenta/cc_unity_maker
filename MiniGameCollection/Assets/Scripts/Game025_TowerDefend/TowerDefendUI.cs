using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game025_TowerDefend
{
    public class TowerDefendUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private TextMeshProUGUI _towersText;
        [SerializeField] private TextMeshProUGUI _waveText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private TowerDefendGameManager _gameManager;

        private void Awake()
        {
            if (_retryButton != null)
                _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
        }

        public void UpdateScore(int score) { if (_scoreText != null) _scoreText.text = $"スコア: {score}"; }
        public void UpdateLives(int lives) { if (_livesText != null) _livesText.text = $"ライフ: {lives}"; }
        public void UpdateTowers(int remaining) { if (_towersText != null) _towersText.text = $"タワー残: {remaining}"; }
        public void UpdateWave(int wave, int total) { if (_waveText != null) _waveText.text = $"Wave {wave}/{total}"; }

        public void ShowResultPanel(int score, bool won)
        {
            if (_resultPanel != null) _resultPanel.SetActive(true);
            if (_resultText != null) _resultText.text = won ? $"勝利!\nスコア: {score}" : $"敗北…\nスコア: {score}";
        }

        public void HideResultPanel() { if (_resultPanel != null) _resultPanel.SetActive(false); }
    }
}
