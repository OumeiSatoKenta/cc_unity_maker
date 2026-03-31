using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game004_WordCrystal
{
    public class WordCrystalUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _wordSlotsText;
        [SerializeField] private TextMeshProUGUI _missCountText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private WordCrystalGameManager _gameManager;

        private void Awake()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);
            if (_retryButton != null)
                _retryButton.onClick.AddListener(OnRestartClicked);
        }

        public void UpdateWordSlots(string targetWord, int filledCount)
        {
            if (_wordSlotsText == null) return;
            string display = "";
            for (int i = 0; i < targetWord.Length; i++)
            {
                if (i < filledCount)
                    display += targetWord[i] + " ";
                else
                    display += "_ ";
            }
            _wordSlotsText.text = display.TrimEnd();
        }

        public void UpdateMissCount(int misses, int maxMisses)
        {
            if (_missCountText != null)
                _missCountText.text = $"ミス: {misses}/{maxMisses}";
        }

        public void UpdateStageText(int stageNum)
        {
            if (_stageText != null)
                _stageText.text = $"ステージ {stageNum}";
        }

        public void ShowClearPanel(int totalStages)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null)
                _clearText.text = $"全クリア!\n{totalStages} ステージ完了";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }

        public void ShowGameOverPanel(int stageNum)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverText != null)
                _gameOverText.text = $"ゲームオーバー\nステージ {stageNum} で失敗";
        }

        public void HideGameOverPanel()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        private void OnRestartClicked()
        {
            if (_gameManager != null) _gameManager.RestartGame();
        }
    }
}
