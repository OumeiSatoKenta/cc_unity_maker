using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game018_TimeRewind
{
    public class TimeRewindUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _moveCountText;
        [SerializeField] private TextMeshProUGUI _rewindCountText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private Button _rewindButton;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private TimeRewindGameManager _gameManager;

        private void Awake()
        {
            if (_rewindButton != null)
                _rewindButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RewindAction(); });
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.NextStage(); });
        }

        public void UpdateMoveCount(int count)
        {
            if (_moveCountText != null) _moveCountText.text = $"移動: {count}";
        }

        public void UpdateRewindCount(int count)
        {
            if (_rewindCountText != null) _rewindCountText.text = $"巻戻: {count}";
        }

        public void UpdateStageText(int stageNum)
        {
            if (_stageText != null) _stageText.text = $"ステージ {stageNum}";
        }

        public void ShowClearPanel(int moves, int rewinds, int stageNum)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = $"クリア!\nステージ {stageNum}\n移動{moves} / 巻戻{rewinds}";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }
    }
}
