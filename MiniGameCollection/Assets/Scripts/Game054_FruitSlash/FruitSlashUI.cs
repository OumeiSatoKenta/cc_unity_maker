using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game054_FruitSlash
{
    public class FruitSlashUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private TextMeshProUGUI _lifeText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private FruitSlashGameManager _gameManager;

        public void UpdateScore(int s) { if (_scoreText != null) _scoreText.text = "スコア: " + s; }
        public void UpdateCombo(int c) { if (_comboText != null) _comboText.text = c > 1 ? c + " コンボ!" : ""; }
        public void UpdateLife(int l) { if (_lifeText != null) { string h = ""; for (int i = 0; i < l; i++) h += "\u2665 "; _lifeText.text = h.TrimEnd(); } }
        public void ShowGameOverPanel(int s) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_finalScoreText != null) _finalScoreText.text = "スコア: " + s; }
        public void HideGameOverPanel() { if (_gameOverPanel != null) _gameOverPanel.SetActive(false); }
        public void OnRetryButton() { if (_gameManager != null) _gameManager.StartGame(); }
    }
}
