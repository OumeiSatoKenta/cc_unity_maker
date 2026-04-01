using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game040_DashDungeon
{
    public class DashDungeonUI : MonoBehaviour
    {
        [SerializeField, Tooltip("HPテキスト")] private TextMeshProUGUI _hpText;
        [SerializeField, Tooltip("スコアテキスト")] private TextMeshProUGUI _scoreText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateHp(int hp, int max) { if (_hpText != null) _hpText.text = $"HP: {hp}/{max}"; }
        public void UpdateScore(int s) { if (_scoreText != null) _scoreText.text = $"Score: {s}"; }
        public void ShowClear(int s) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"Score: {s}"; }
        public void ShowGameOver(int s) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {s}"; }
    }
}
