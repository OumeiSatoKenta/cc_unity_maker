using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game003_GravitySwitch
{
    public class GravitySwitchUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _moveText;

        [SerializeField] private GameObject _stageClearPanel;
        [SerializeField] private TextMeshProUGUI _stageClearText;
        [SerializeField] private Button _nextStageButton;

        [SerializeField] private GameObject _gameClearPanel;
        [SerializeField] private TextMeshProUGUI _gameClearScoreText;

        [SerializeField] private GameObject _gameOverPanel;

        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _menuButton2;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;

        [SerializeField] private GravitySwitchGameManager _gameManager;

        private void Awake()
        {
            // 方向ボタン・リセットは Setup の Persistent Listener で登録済みのため AddListener しない
            // GravityDirection 引数が必要な方向ボタンのみここで登録
            if (_upButton != null) _upButton.onClick.AddListener(() => _gameManager?.OnGravityButton(GravityDirection.Up));
            if (_downButton != null) _downButton.onClick.AddListener(() => _gameManager?.OnGravityButton(GravityDirection.Down));
            if (_leftButton != null) _leftButton.onClick.AddListener(() => _gameManager?.OnGravityButton(GravityDirection.Left));
            if (_rightButton != null) _rightButton.onClick.AddListener(() => _gameManager?.OnGravityButton(GravityDirection.Right));
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateMoves(int moves, int limit)
        {
            if (_moveText != null)
            {
                if (limit > 0)
                    _moveText.text = $"手数: {moves} / {limit}";
                else
                    _moveText.text = $"手数: {moves}";
            }
        }

        public void ShowStageClearPanel(int stageNum, int stageScore, int starRating)
        {
            HideAllPanels();
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            string stars = starRating == 3 ? "★★★" : starRating == 2 ? "★★☆" : "★☆☆";
            if (_stageClearText != null)
                _stageClearText.text = $"ステージ {stageNum} クリア！\n{stars}\n+{stageScore}点";
        }

        public void ShowClearPanel(int totalScore)
        {
            HideAllPanels();
            if (_gameClearPanel != null) _gameClearPanel.SetActive(true);
            if (_gameClearScoreText != null) _gameClearScoreText.text = $"Total: {totalScore}";
        }

        public void ShowGameOverPanel()
        {
            HideAllPanels();
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
