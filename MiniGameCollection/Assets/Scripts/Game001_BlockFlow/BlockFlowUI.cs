using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game001_BlockFlow
{
    /// <summary>
    /// BlockFlow のUI制御。手数表示・クリアパネル・メニューへ戻るボタンを管理する。
    /// </summary>
    public class BlockFlowUI : MonoBehaviour
    {
        [SerializeField, Tooltip("手数を表示するテキスト")]
        private TextMeshProUGUI _moveCountText;

        [SerializeField, Tooltip("クリア時に表示するパネル")]
        private GameObject _clearPanel;

        [SerializeField, Tooltip("クリアメッセージのテキスト")]
        private TextMeshProUGUI _clearText;

        [SerializeField, Tooltip("リスタートボタン")]
        private Button _restartButton;

        [SerializeField, Tooltip("メニューへ戻るボタン")]
        private Button _menuButton;

        [SerializeField, Tooltip("ゲームマネージャー参照")]
        private BlockFlowGameManager _gameManager;

        private void Awake()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }
            if (_menuButton != null)
            {
                _menuButton.onClick.AddListener(OnMenuClicked);
            }
        }

        public void UpdateMoveCount(int count)
        {
            if (_moveCountText != null)
            {
                _moveCountText.text = $"手数: {count}";
            }
        }

        public void ShowClearPanel(int moveCount)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = $"クリア!\n{moveCount} 手";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }

        private void OnRestartClicked()
        {
            if (_gameManager != null) _gameManager.RestartGame();
        }

        private void OnMenuClicked()
        {
            SceneLoader.BackToMenu();
        }
    }
}
