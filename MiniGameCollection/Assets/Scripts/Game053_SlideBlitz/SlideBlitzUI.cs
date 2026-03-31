using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game053_SlideBlitz
{
    public class SlideBlitzUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _moveText;
        [SerializeField] private TextMeshProUGUI _sizeText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearResultText;
        [SerializeField] private SlideBlitzGameManager _gameManager;

        public void UpdateTimer(float t)
        {
            if (_timerText != null) _timerText.text = t.ToString("F1") + "s";
        }

        public void UpdateMoves(int m)
        {
            if (_moveText != null) _moveText.text = "手数: " + m;
        }

        public void UpdateSize(int s)
        {
            if (_sizeText != null) _sizeText.text = s + "x" + s;
        }

        public void ShowClearPanel(float time, int moves)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearResultText != null) _clearResultText.text = time.ToString("F1") + "秒 / " + moves + "手";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }

        public void OnNextButton()
        {
            if (_gameManager != null) _gameManager.OnNextSize();
        }

        public void OnRetryButton()
        {
            if (_gameManager != null) _gameManager.OnRetry();
        }
    }
}
