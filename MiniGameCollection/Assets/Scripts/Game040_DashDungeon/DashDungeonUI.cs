using UnityEngine;using UnityEngine.UI;using TMPro;
namespace Game040_DashDungeon
{
    public class DashDungeonUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private DashDungeonGameManager _gameManager;
        private void Awake() { if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); }); }
        public void UpdateCoins(int c) { if (_coinText != null) _coinText.text = $"コイン: {c}"; }
        public void UpdateTime(float t) { if (_timeText != null) _timeText.text = $"時間: {t:F1}秒"; }
        public void ShowResultPanel(int coins, float time) { if (_resultPanel != null) _resultPanel.SetActive(true); if (_resultText != null) _resultText.text = $"クリア!\nコイン: {coins}\n時間: {time:F1}秒"; }
        public void HideResultPanel() { if (_resultPanel != null) _resultPanel.SetActive(false); }
    }
}
