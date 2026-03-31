using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game030_FingerRacer
{
    public class FingerRacerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _checkpointText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _hintText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private FingerRacerGameManager _gameManager;

        private void Awake()
        {
            if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
        }

        public void UpdateCheckpoints(int hit, int total) { if (_checkpointText != null) _checkpointText.text = $"CP: {hit}/{total}"; }
        public void UpdateTime(float time) { if (_timeText != null) _timeText.text = $"残り: {Mathf.CeilToInt(Mathf.Max(time, 0))}秒"; }
        public void ShowHint(bool show) { if (_hintText != null) _hintText.gameObject.SetActive(show); }

        public void ShowResultPanel(int checkpoints)
        {
            if (_resultPanel != null) _resultPanel.SetActive(true);
            if (_resultText != null) _resultText.text = $"レース終了!\nチェックポイント: {checkpoints}/5";
        }

        public void HideResultPanel() { if (_resultPanel != null) _resultPanel.SetActive(false); }
    }
}
