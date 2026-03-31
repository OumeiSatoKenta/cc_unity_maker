using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game055_DustSweep
{
    public class DustSweepUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _cleanText;
        [SerializeField] private TextMeshProUGUI _starText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearResultText;
        [SerializeField] private DustSweepGameManager _gameManager;

        public void UpdateTimer(float t) { if (_timerText != null) _timerText.text = t.ToString("F1") + "s"; }
        public void UpdateClean(float p) { if (_cleanText != null) _cleanText.text = Mathf.RoundToInt(p * 100f) + "%"; }
        public void UpdateStars(int s) { if (_starText != null) _starText.text = "\u2605 x" + s; }
        public void ShowClearPanel(float t, int s) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearResultText != null) _clearResultText.text = t.ToString("F1") + "秒 / \u2605 x" + s; }
        public void HideClearPanel() { if (_clearPanel != null) _clearPanel.SetActive(false); }
        public void OnRetryButton() { if (_gameManager != null) _gameManager.StartGame(); }
    }
}
