using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game045_FingerPaint
{
    public class FingerPaintUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _strokeText;
        [SerializeField] private GameObject _finishPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private FingerPaintGameManager _gameManager;

        public void UpdateTimer(float time)
        {
            if (_timerText != null) _timerText.text = Mathf.CeilToInt(time) + "s";
        }

        public void UpdateStrokes(int count)
        {
            if (_strokeText != null) _strokeText.text = "ストローク: " + count;
        }

        public void ShowFinishPanel(int strokes)
        {
            if (_finishPanel != null) _finishPanel.SetActive(true);
            if (_resultText != null) _resultText.text = strokes + " ストロークの作品!";
        }

        public void HideFinishPanel()
        {
            if (_finishPanel != null) _finishPanel.SetActive(false);
        }

        public void OnRetryButton()
        {
            if (_gameManager != null) _gameManager.StartGame();
        }
    }
}
