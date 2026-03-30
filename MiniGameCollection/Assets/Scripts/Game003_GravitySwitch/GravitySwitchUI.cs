using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game003_GravitySwitch
{
    /// <summary>
    /// GravitySwitch の UI を管理する。
    /// 手数表示、クリアパネル、ボタンのコールバック登録を担当する。
    /// </summary>
    public class GravitySwitchUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _moveCountText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private GravitySwitchGameManager _gameManager;

        private void Start()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => _gameManager?.RestartGame());
            if (_nextLevelButton != null)
                _nextLevelButton.onClick.AddListener(() => _gameManager?.LoadNextLevel());
        }

        public void UpdateMoveCount(int count)
        {
            if (_moveCountText != null)
                _moveCountText.text = $"手数: {count}";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }

        public void ShowClearPanel(int moveCount)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = $"クリア!\n{moveCount}手";
        }
    }
}
