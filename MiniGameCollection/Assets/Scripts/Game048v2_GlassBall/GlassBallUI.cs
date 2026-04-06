using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game048v2_GlassBall
{
    public class GlassBallUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _coinText;
        [SerializeField] Slider _impactSlider;
        [SerializeField] Slider _inkSlider;
        [SerializeField] Image _impactFill;

        [Header("Buttons")]
        [SerializeField] Button _launchButton;
        [SerializeField] Button _clearRailButton;

        [Header("Panels")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        void Start()
        {
            HideAllPanels();
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

        public void UpdateCoinCount(int current, int total)
        {
            if (_coinText != null)
            {
                if (total <= 0)
                    _coinText.text = "";
                else
                    _coinText.text = $"Coin: {current}/{total}";
            }
        }

        public void UpdateImpact(float value01)
        {
            if (_impactSlider != null)
                _impactSlider.value = value01;
            if (_impactFill != null)
                _impactFill.color = Color.Lerp(new Color(0.2f, 0.8f, 0.2f), Color.red, value01);
        }

        public void UpdateInk(float value01)
        {
            if (_inkSlider != null)
                _inkSlider.value = value01;
        }

        public void SetLaunchButtonInteractable(bool interactable)
        {
            if (_launchButton != null)
                _launchButton.interactable = interactable;
        }

        public void ShowStageClear(int score)
        {
            HideAllPanels();
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText != null)
                    _stageClearScoreText.text = $"Score: {score}";
            }
        }

        public void ShowAllClear(int score)
        {
            HideAllPanels();
            if (_allClearPanel != null)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText != null)
                    _allClearScoreText.text = $"Total Score: {score}";
            }
        }

        public void ShowGameOver(int score)
        {
            HideAllPanels();
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null)
                    _gameOverScoreText.text = $"Score: {score}";
            }
        }

        void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
