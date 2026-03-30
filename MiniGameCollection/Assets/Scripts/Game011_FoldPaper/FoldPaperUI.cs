using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game011_FoldPaper
{
    /// <summary>
    /// FoldPaper のUI制御。折り手数表示・ステージ表示・クリアパネル・Undoボタンを管理する。
    /// </summary>
    public class FoldPaperUI : MonoBehaviour
    {
        [SerializeField, Tooltip("折り手数を表示するテキスト")]
        private TextMeshProUGUI _foldCountText;

        [SerializeField, Tooltip("ステージ番号テキスト")]
        private TextMeshProUGUI _stageText;

        [SerializeField, Tooltip("クリア時に表示するパネル")]
        private GameObject _clearPanel;

        [SerializeField, Tooltip("クリアメッセージのテキスト")]
        private TextMeshProUGUI _clearText;

        [SerializeField, Tooltip("リスタートボタン")]
        private Button _restartButton;

        [SerializeField, Tooltip("次のステージボタン")]
        private Button _nextStageButton;

        [SerializeField, Tooltip("メニューへ戻るボタン（クリアパネル内）")]
        private Button _menuButton;

        [SerializeField, Tooltip("Undoボタン")]
        private Button _undoButton;

        [SerializeField, Tooltip("ゲームマネージャー参照")]
        private FoldPaperGameManager _gameManager;

        private void Awake()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(OnNextStageClicked);
            if (_menuButton != null)
                _menuButton.onClick.AddListener(OnMenuClicked);
            if (_undoButton != null)
                _undoButton.onClick.AddListener(OnUndoClicked);
        }

        public void UpdateFoldCount(int count)
        {
            if (_foldCountText != null)
                _foldCountText.text = $"折り: {count} 回";
        }

        public void UpdateStageText(int stageIndex)
        {
            if (_stageText != null)
                _stageText.text = $"ステージ {stageIndex + 1}";
        }

        public void ShowClearPanel(int foldCount)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = $"完成!\n{foldCount} 回で折れました";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }

        private void OnRestartClicked()
        {
            if (_gameManager != null) _gameManager.RestartGame();
        }

        private void OnNextStageClicked()
        {
            if (_gameManager != null) _gameManager.NextStage();
        }

        private void OnMenuClicked()
        {
            SceneLoader.BackToMenu();
        }

        private void OnUndoClicked()
        {
            if (_gameManager != null) _gameManager.UndoFold();
        }
    }
}
