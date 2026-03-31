using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game019_PathCut
{
    public class PathCutUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _cutCountText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private GameObject _failPanel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private PathCutGameManager _gameManager;

        private void Awake()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.NextStage(); });
            if (_retryButton != null)
                _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
        }

        public void UpdateCutCount(int count)
        {
            if (_cutCountText != null) _cutCountText.text = $"カット: {count}";
        }

        public void UpdateStageText(int stageNum)
        {
            if (_stageText != null) _stageText.text = $"ステージ {stageNum}";
        }

        public void ShowClearPanel(int cuts, int stageNum)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = $"クリア!\nステージ {stageNum}\n{cuts} カット";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }

        public void ShowFailPanel()
        {
            if (_failPanel != null) _failPanel.SetActive(true);
        }

        public void HideFailPanel()
        {
            if (_failPanel != null) _failPanel.SetActive(false);
        }
    }
}
