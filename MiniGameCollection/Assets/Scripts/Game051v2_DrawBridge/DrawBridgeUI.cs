using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game051v2_DrawBridge
{
    public class DrawBridgeUI : MonoBehaviour
    {
        [SerializeField] DrawBridgeGameManager _gameManager;
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] Slider _inkSlider;
        [SerializeField] Image _inkSliderFill;
        [SerializeField] Button _goButton;
        [SerializeField] Button _eraseButton;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] Button _allClearRetryButton;
        [SerializeField] Button _allClearMenuButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;

        [SerializeField] Button _backToMenuButton;

        void Start()
        {
            if (_goButton != null)
                _goButton.onClick.AddListener(() => _gameManager.OnGoPressed());
            if (_eraseButton != null)
                _eraseButton.onClick.AddListener(() => _gameManager.OnErasePressed());
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(() => _gameManager.GoNextStage());
            if (_retryButton != null)
                _retryButton.onClick.AddListener(() => _gameManager.RestartGame());
            if (_allClearRetryButton != null)
                _allClearRetryButton.onClick.AddListener(() => _gameManager.RestartGame());
            if (_allClearMenuButton != null)
                _allClearMenuButton.onClick.AddListener(() => _gameManager.GoToMenu());
            if (_backToMenuButton != null)
                _backToMenuButton.onClick.AddListener(() => _gameManager.GoToMenu());

            HideStageClear();
            HideGameOver();
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        public void UpdateInk(float ratio)
        {
            ratio = Mathf.Clamp01(ratio);
            if (_inkSlider != null)
                _inkSlider.value = ratio;
            if (_inkSliderFill != null)
            {
                if (ratio <= 0.2f)
                    _inkSliderFill.color = Color.red;
                else if (ratio <= 0.5f)
                    _inkSliderFill.color = new Color(1f, 0.6f, 0f);
                else
                    _inkSliderFill.color = new Color(0.3f, 0.7f, 0.3f);
            }
        }

        public void SetGoButtonEnabled(bool enabled)
        {
            if (_goButton != null)
                _goButton.interactable = enabled;
        }

        public void ShowStageClear(int score)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText != null)
                    _stageClearScoreText.text = $"Score: {score}";
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null)
                _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText != null)
                    _allClearScoreText.text = $"Total Score: {score}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null)
                    _gameOverScoreText.text = $"Score: {score}";
            }
        }

        public void HideGameOver()
        {
            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(false);
        }
    }
}
