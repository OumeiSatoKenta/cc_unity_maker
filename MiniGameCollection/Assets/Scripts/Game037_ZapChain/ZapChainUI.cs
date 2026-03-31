using UnityEngine;using UnityEngine.UI;using TMPro;
namespace Game037_ZapChain
{
    public class ZapChainUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _chainText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private ZapChainGameManager _gameManager;

        private void Awake() { if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); }); }
        public void UpdateScore(int s) { if (_scoreText != null) _scoreText.text = $"スコア: {s}"; }
        public void UpdateChain(int c) { if (_chainText != null) _chainText.text = c > 1 ? $"{c}連鎖!" : ""; }
        public void UpdateTime(float t) { if (_timeText != null) _timeText.text = $"残り: {Mathf.CeilToInt(Mathf.Max(t,0))}秒"; }
        public void ShowResultPanel(int score, int maxChain)
        {
            if (_resultPanel != null) _resultPanel.SetActive(true);
            if (_resultText != null) _resultText.text = $"タイムアップ!\nスコア: {score}\n最大連鎖: {maxChain}";
        }
        public void HideResultPanel() { if (_resultPanel != null) _resultPanel.SetActive(false); }
    }
}
