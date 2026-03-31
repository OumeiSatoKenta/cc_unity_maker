using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game032_SpinCutter
{
    public class SpinCutterUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private SpinCutterGameManager _gameManager;

        private void Awake() { if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); }); }

        public void UpdateScore(int score) { if (_scoreText != null) _scoreText.text = $"スコア: {score}"; }
        public void UpdateTime(float t) { if (_timeText != null) _timeText.text = $"残り: {Mathf.CeilToInt(Mathf.Max(t,0))}秒"; }

        public void ShowResultPanel(int score, bool survived)
        {
            if (_resultPanel != null) _resultPanel.SetActive(true);
            if (_resultText != null) _resultText.text = survived ? $"タイムアップ!\nスコア: {score}" : $"やられた…\nスコア: {score}";
        }

        public void HideResultPanel() { if (_resultPanel != null) _resultPanel.SetActive(false); }
    }
}
