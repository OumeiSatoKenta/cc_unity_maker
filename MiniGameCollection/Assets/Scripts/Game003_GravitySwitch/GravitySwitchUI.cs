using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game003_GravitySwitch
{
    public class GravitySwitchUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _moveCountText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;
        [SerializeField] private GravitySwitchGameManager _gameManager;

        private void Awake()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(OnNextStageClicked);
            if (_upButton != null)
                _upButton.onClick.AddListener(() => OnDirectionClicked(Vector2Int.up));
            if (_downButton != null)
                _downButton.onClick.AddListener(() => OnDirectionClicked(Vector2Int.down));
            if (_leftButton != null)
                _leftButton.onClick.AddListener(() => OnDirectionClicked(Vector2Int.left));
            if (_rightButton != null)
                _rightButton.onClick.AddListener(() => OnDirectionClicked(Vector2Int.right));
        }

        public void UpdateMoveCount(int count)
        {
            if (_moveCountText != null)
                _moveCountText.text = $"手数: {count}";
        }

        public void UpdateStageText(int stageNum)
        {
            if (_stageText != null)
                _stageText.text = $"ステージ {stageNum}";
        }

        public void ShowClearPanel(int moveCount, int stageNum)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null)
                _clearText.text = $"クリア!\nステージ {stageNum}\n{moveCount} 手";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }

        private void OnDirectionClicked(Vector2Int direction)
        {
            if (_gameManager != null) _gameManager.ApplyGravityDirection(direction);
        }

        private void OnRestartClicked()
        {
            if (_gameManager != null) _gameManager.RestartGame();
        }

        private void OnNextStageClicked()
        {
            if (_gameManager != null) _gameManager.NextStage();
        }
    }
}
