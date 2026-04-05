using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game037v2_ZapChain
{
    public class ZapChainUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] Slider _energySlider;
        [SerializeField] Image _energyFill;
        [SerializeField] TextMeshProUGUI _chainText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        void Start()
        {
            _stageClearPanel?.SetActive(false);
            _finalClearPanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
        }

        public void UpdateStage(int stage)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / 5";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateEnergy(float energy, float maxEnergy)
        {
            if (_energySlider != null)
            {
                _energySlider.maxValue = maxEnergy;
                _energySlider.value = energy;
            }
            if (_energyFill != null)
            {
                float ratio = energy / maxEnergy;
                _energyFill.color = ratio < 0.3f ? Color.red : ratio < 0.6f ? Color.yellow : Color.green;
            }
        }

        public void UpdateChain(int connected, int total)
        {
            if (_chainText != null) _chainText.text = $"{connected} / {total}";
        }

        public void ShowStageClearPanel(int score)
        {
            _stageClearPanel?.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score}";
        }

        public void ShowFinalClearPanel(int score)
        {
            _finalClearPanel?.SetActive(true);
            if (_finalScoreText != null) _finalScoreText.text = $"Total Score: {score}";
        }

        public void ShowGameOverPanel(int score)
        {
            _gameOverPanel?.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
        }
    }
}
