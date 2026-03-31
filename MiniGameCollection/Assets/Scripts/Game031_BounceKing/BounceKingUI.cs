using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game031_BounceKing
{
    public class BounceKingUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private BounceKingGameManager _gameManager;

        private void Awake()
        {
            if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
        }

        public void UpdateScore(int score) { if (_scoreText != null) _scoreText.text = $"スコア: {score}"; }
        public void UpdateLives(int lives) { if (_livesText != null) _livesText.text = $"ライフ: {lives}"; }

        public void ShowWinPanel(int score)
        {
            if (_resultPanel != null) _resultPanel.SetActive(true);
            if (_resultText != null) _resultText.text = $"全破壊!\nスコア: {score}";
        }

        public void ShowLosePanel(int score)
        {
            if (_resultPanel != null) _resultPanel.SetActive(true);
            if (_resultText != null) _resultText.text = $"ゲームオーバー\nスコア: {score}";
        }

        public void HidePanel() { if (_resultPanel != null) _resultPanel.SetActive(false); }
    }
}
