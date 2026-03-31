using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game023_ChainSlash
{
    public class ChainSlashUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private ChainSlashGameManager _gameManager;

        private void Awake()
        {
            if (_retryButton != null)
                _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"スコア: {score}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText != null) _comboText.text = combo > 0 ? $"{combo} コンボ!" : "";
        }

        public void UpdateTime(float remaining)
        {
            if (_timeText != null) _timeText.text = $"残り: {Mathf.CeilToInt(remaining)}秒";
        }

        public void ShowResultPanel(int score, int maxCombo)
        {
            if (_resultPanel != null) _resultPanel.SetActive(true);
            if (_resultText != null) _resultText.text = $"タイムアップ!\nスコア: {score}\n最大コンボ: {maxCombo}";
        }

        public void HideResultPanel()
        {
            if (_resultPanel != null) _resultPanel.SetActive(false);
        }
    }
}
