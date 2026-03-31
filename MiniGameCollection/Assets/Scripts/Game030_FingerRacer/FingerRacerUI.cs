using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game030_FingerRacer
{
    public class FingerRacerUI : MonoBehaviour
    {
        [SerializeField, Tooltip("フェーズ表示テキスト")]
        private TextMeshProUGUI _phaseText;

        [SerializeField, Tooltip("残り時間テキスト")]
        private TextMeshProUGUI _timeText;

        [SerializeField, Tooltip("クリアパネル")]
        private GameObject _clearPanel;

        [SerializeField, Tooltip("クリアテキスト")]
        private TextMeshProUGUI _clearText;

        [SerializeField, Tooltip("ゲームオーバーパネル")]
        private GameObject _gameOverPanel;

        [SerializeField, Tooltip("ゲームオーバーテキスト")]
        private TextMeshProUGUI _gameOverText;

        [SerializeField, Tooltip("ゲームオーバー時リトライボタン")]
        private Button _retryButton;

        [SerializeField, Tooltip("クリア時リトライボタン")]
        private Button _clearRetryButton;

        [SerializeField, Tooltip("メニューボタン")]
        private Button _menuButton;

        private FingerRacerGameManager _gameManager;

        private void Awake()
        {
            _gameManager = GetComponentInParent<FingerRacerGameManager>();

            if (_retryButton != null)
                _retryButton.onClick.AddListener(() =>
                {
                    if (_gameManager != null) _gameManager.RestartGame();
                });

            if (_clearRetryButton != null)
                _clearRetryButton.onClick.AddListener(() =>
                {
                    if (_gameManager != null) _gameManager.RestartGame();
                });

            if (_menuButton != null)
                _menuButton.onClick.AddListener(() => SceneLoader.BackToMenu());
        }

        public void ShowDrawingPhase()
        {
            if (_phaseText != null)
                _phaseText.text = "コースを描いてください";
            if (_timeText != null)
                _timeText.gameObject.SetActive(false);
        }

        public void ShowRacingPhase(float timeLimit)
        {
            if (_phaseText != null)
                _phaseText.text = "レース中！";
            if (_timeText != null)
            {
                _timeText.gameObject.SetActive(true);
                _timeText.text = $"{timeLimit:F1}s";
            }
        }

        public void UpdateTime(float remaining)
        {
            if (_timeText != null)
                _timeText.text = $"{remaining:F1}s";
        }

        public void ShowClearPanel(float raceTime)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null)
                _clearText.text = $"ゴール！\nタイム: {raceTime:F2}秒";
        }

        public void ShowGameOverPanel()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverText != null)
                _gameOverText.text = "タイムオーバー！";
        }

        public void HidePanels()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
