using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game009_ColorMix
{
    public class ColorMixUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _matchText;
        [SerializeField] private Slider _redSlider;
        [SerializeField] private Slider _greenSlider;
        [SerializeField] private Slider _blueSlider;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private ColorMixGameManager _gameManager;
        [SerializeField] private ColorMixManager _colorManager;

        private void Awake()
        {
            if (_redSlider != null)
                _redSlider.onValueChanged.AddListener(v => { if (_colorManager != null) _colorManager.SetRedValue(v); });
            if (_greenSlider != null)
                _greenSlider.onValueChanged.AddListener(v => { if (_colorManager != null) _colorManager.SetGreenValue(v); });
            if (_blueSlider != null)
                _blueSlider.onValueChanged.AddListener(v => { if (_colorManager != null) _colorManager.SetBlueValue(v); });
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); });
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.NextStage(); });
        }

        public void UpdateStageText(int stageNum)
        {
            if (_stageText != null) _stageText.text = $"ステージ {stageNum}";
        }

        public void UpdateMatchText(float percentage)
        {
            if (_matchText != null) _matchText.text = $"一致度: {percentage:F0}%";
        }

        public void ResetSliders()
        {
            if (_redSlider != null) _redSlider.value = 0.5f;
            if (_greenSlider != null) _greenSlider.value = 0.5f;
            if (_blueSlider != null) _blueSlider.value = 0.5f;
        }

        public void ShowClearPanel(int stageNum)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = $"クリア!\nステージ {stageNum}\n色が一致!";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }
    }
}
