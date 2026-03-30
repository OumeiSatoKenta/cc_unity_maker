using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game002_MirrorMaze
{
    public class MirrorMazeUI : MonoBehaviour
    {
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private MirrorMazeGameManager _gameManager;

        private void Awake()
        {
            _restartButton?.onClick.AddListener(OnRestart);
            _menuButton?.onClick.AddListener(OnMenu);
        }

        public void ShowClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = "クリア！\nレーザーをゴールに届けた！";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }

        private void OnRestart() => _gameManager?.RestartGame();
        private void OnMenu()    => SceneLoader.BackToMenu();
    }
}
