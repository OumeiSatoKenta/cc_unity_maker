using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game013_SymmetryDraw
{
    /// <summary>
    /// SymmetryDraw のUI制御。ストローク数表示・クリアパネル・メニューへ戻るボタンを管理する。
    /// </summary>
    public class SymmetryDrawUI : MonoBehaviour
    {
        [SerializeField, Tooltip("ストローク数を表示するテキスト")]
        private TextMeshProUGUI _strokeCountText;

        [SerializeField, Tooltip("クリア時に表示するパネル")]
        private GameObject _clearPanel;

        [SerializeField, Tooltip("クリアメッセージのテキスト")]
        private TextMeshProUGUI _clearText;

        [SerializeField, Tooltip("リスタートボタン")]
        private Button _restartButton;

        [SerializeField, Tooltip("メニューへ戻るボタン")]
        private Button _menuButton;

        [SerializeField, Tooltip("ゲームマネージャー参照")]
        private SymmetryDrawGameManager _gameManager;

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

        public void UpdateStrokeCount(int count)
        {
            if (_strokeCountText != null)
            {
                _strokeCountText.text = $"ストローク: {count}";
            }
        }

        public void ShowClearPanel(int strokeCount)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = $"クリア!\n{strokeCount} ストローク";
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
