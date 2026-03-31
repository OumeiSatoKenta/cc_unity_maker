using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game029_MeteorShield
{
    public class MeteorShieldUI : MonoBehaviour
    {
        [SerializeField, Tooltip("HP表示テキスト")]
        private TextMeshProUGUI _hpText;

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

        [SerializeField, Tooltip("クリア時リスタートボタン")]
        private Button _restartButton;

        [SerializeField, Tooltip("ゲームオーバー時リトライボタン")]
        private Button _retryButton;

        [SerializeField, Tooltip("メニューボタン")]
        private Button _menuButton;

        private MeteorShieldGameManager _gameManager;

        private void Awake()
        {
            _gameManager = GetComponentInParent<MeteorShieldGameManager>();

            if (_restartButton != null)
                _restartButton.onClick.AddListener(() =>
                {
                    if (_gameManager != null) _gameManager.RestartGame();
                });

            if (_retryButton != null)
                _retryButton.onClick.AddListener(() =>
                {
                    if (_gameManager != null) _gameManager.RestartGame();
                });

            if (_menuButton != null)
                _menuButton.onClick.AddListener(() => SceneLoader.BackToMenu());
        }

        public void UpdateHp(int current, int max)
        {
            if (_hpText != null)
            {
                string hearts = new string('\u2665', current) + new string('\u2661', max - current);
                _hpText.text = hearts;
            }
        }

        public void UpdateTime(float remaining)
        {
            if (_timeText != null)
                _timeText.text = $"{remaining:F1}s";
        }

        public void ShowClearPanel(int remainingHp, int maxHp)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null)
                _clearText.text = $"クリア!\nHP: {remainingHp}/{maxHp}";
        }

        public void ShowGameOverPanel(float survivedTime)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverText != null)
                _gameOverText.text = $"ゲームオーバー\n生存: {survivedTime:F1}秒";
        }

        public void HidePanels()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
