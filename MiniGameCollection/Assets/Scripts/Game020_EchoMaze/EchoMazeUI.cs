using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game020_EchoMaze
{
    public class EchoMazeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _moveCountText;
        [SerializeField] private TextMeshProUGUI _wallHitText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private EchoMazeGameManager _gameManager;

        private void Awake()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.NextStage(); });
        }

        public void UpdateMoveCount(int count)
        {
            if (_moveCountText != null) _moveCountText.text = $"移動: {count}";
        }

        public void UpdateWallHits(int count)
        {
            if (_wallHitText != null) _wallHitText.text = $"衝突: {count}";
        }

        public void UpdateStageText(int stageNum)
        {
            if (_stageText != null) _stageText.text = $"ステージ {stageNum}";
        }

        public void ShowClearPanel(int moves, int hits, int stageNum)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = $"クリア!\nステージ {stageNum}\n移動{moves} / 衝突{hits}";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }
    }
}
