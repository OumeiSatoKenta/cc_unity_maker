using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game034_DropZone
{
    public class DropZoneUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _missText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private DropZoneGameManager _gameManager;

        private void Awake() { if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); }); }

        public void UpdateScore(int s) { if (_scoreText != null) _scoreText.text = $"スコア: {s}"; }
        public void UpdateMisses(int m) { if (_missText != null) _missText.text = $"ミス: {m}"; }
        public void UpdateTime(float t) { if (_timeText != null) _timeText.text = $"残り: {Mathf.CeilToInt(Mathf.Max(t,0))}秒"; }

        public void ShowResultPanel(int score, int misses)
        {
            if (_resultPanel != null) _resultPanel.SetActive(true);
            if (_resultText != null) _resultText.text = $"タイムアップ!\nスコア: {score}\nミス: {misses}";
        }

        public void HideResultPanel() { if (_resultPanel != null) _resultPanel.SetActive(false); }
    }
}
