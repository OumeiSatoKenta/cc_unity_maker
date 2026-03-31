using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game047_SpinBalance
{
    public class SpinBalanceUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _pieceText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private SpinBalanceGameManager _gameManager;

        public void UpdateTimer(float time)
        {
            if (_timerText != null) _timerText.text = time.ToString("F1") + "s";
        }

        public void UpdatePieces(int count)
        {
            if (_pieceText != null) _pieceText.text = "コマ: " + count;
        }

        public void ShowGameOverPanel(float time, int pieces)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_resultText != null) _resultText.text = time.ToString("F1") + "秒 / " + pieces + "コマ";
        }

        public void HideGameOverPanel()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void OnRetryButton()
        {
            if (_gameManager != null) _gameManager.StartGame();
        }
    }
}
