using UnityEngine;
using UnityEngine.UI;

namespace Game005_PipeConnect
{
    public class PipeConnectUI : MonoBehaviour
    {
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _moveText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private Text _clearResultText;
        [SerializeField] private PipeConnectGameManager _gameManager;

        private void Start()
        {
            _gameManager.OnMoveCountChanged.AddListener(UpdateMoveCount);
            _gameManager.OnLevelCleared.AddListener(ShowClearPanel);
            if (_clearPanel) _clearPanel.SetActive(false);
        }

        public void SetLevelText(string text)
        {
            if (_levelText) _levelText.text = text;
        }

        private void UpdateMoveCount(int count)
        {
            if (_moveText) _moveText.text = $"Moves: {count}";
        }

        private void ShowClearPanel(int level)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearResultText)
                _clearResultText.text = $"Cleared in {_gameManager.GetMoveCount()} moves!";
        }

        public void HideClearPanel()
        {
            if (_clearPanel) _clearPanel.SetActive(false);
        }

        public void OnNextLevelClicked() => _gameManager?.LoadNextLevel();
        public void OnMenuClicked() => _gameManager?.LoadMenu();
    }
}
