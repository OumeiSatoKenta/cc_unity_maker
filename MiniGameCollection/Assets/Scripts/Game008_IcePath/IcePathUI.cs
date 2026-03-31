using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game008_IcePath
{
    public class IcePathUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _moveCountText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private IcePathGameManager _gameManager;

        private void Awake()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.NextStage(); });
        }

        public void UpdateMoveCount(int count)
        {
            if (_moveCountText != null) _moveCountText.text = $"手数: {count}";
        }

        public void UpdateStageText(int stageNum)
        {
            if (_stageText != null) _stageText.text = $"ステージ {stageNum}";
        }

        public void UpdateProgress(int visited, int total)
        {
            if (_progressText != null)
                _progressText.text = total > 0 ? $"{visited}/{total}" : "";
        }

        public void ShowClearPanel(int moveCount, int stageNum)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = $"クリア!\nステージ {stageNum}\n{moveCount} 手";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }
    }
}
