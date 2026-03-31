using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game017_CrystalSort
{
    public class CrystalSortUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _sortedText;
        [SerializeField] private TextMeshProUGUI _missText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private CrystalSortGameManager _gameManager;

        private void Awake()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.NextStage(); });
        }

        public void UpdateSortedCount(int count)
        {
            if (_sortedText != null) _sortedText.text = $"分類: {count}";
        }

        public void UpdateMissCount(int count)
        {
            if (_missText != null) _missText.text = $"ミス: {count}";
        }

        public void UpdateStageText(int stageNum)
        {
            if (_stageText != null) _stageText.text = $"ステージ {stageNum}";
        }

        public void ShowClearPanel(int sorted, int misses, int stageNum)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = $"クリア!\nステージ {stageNum}\n{sorted} 個分類 / ミス {misses}";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }
    }
}
