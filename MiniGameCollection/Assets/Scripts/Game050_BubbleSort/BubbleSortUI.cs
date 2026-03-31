using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game050_BubbleSort
{
    public class BubbleSortUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _moveText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearMoveText;
        [SerializeField] private BubbleSortGameManager _gameManager;

        public void UpdateMoves(int count)
        {
            if (_moveText != null) _moveText.text = "手数: " + count;
        }

        public void UpdateStage(int stage)
        {
            if (_stageText != null) _stageText.text = "ステージ " + stage;
        }

        public void ShowClearPanel(int moves)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearMoveText != null) _clearMoveText.text = moves + " 手でクリア!";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }

        public void OnNextButton()
        {
            if (_gameManager != null) _gameManager.OnNextStage();
        }

        public void OnRetryButton()
        {
            if (_gameManager != null) _gameManager.OnRetry();
        }
    }
}
