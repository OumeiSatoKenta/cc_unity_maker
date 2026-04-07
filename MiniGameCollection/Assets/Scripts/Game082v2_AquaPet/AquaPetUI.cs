using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game082v2_AquaPet
{
    public class AquaPetUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _waterQualityText;
        [SerializeField] TextMeshProUGUI _feedCountText;
        [SerializeField] TextMeshProUGUI _collectionText;
        [SerializeField] Slider _waterQualitySlider;
        [SerializeField] Slider _healthSlider;
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

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

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo <= 0)
            {
                _comboText.text = "";
            }
            else
            {
                _comboText.text = $"COMBO x{combo}";
                _comboText.color = combo >= 3 ? new Color(1f, 0.8f, 0.1f) : Color.white;
            }
        }

        public void UpdateWaterQuality(float quality)
        {
            if (_waterQualityText != null)
            {
                _waterQualityText.text = $"水質: {quality:F0}%";
                _waterQualityText.color = quality < 40f ? new Color(1f, 0.3f, 0.3f)
                    : quality < 70f ? new Color(1f, 0.9f, 0.3f)
                    : new Color(0.4f, 1f, 0.7f);
            }
            if (_waterQualitySlider != null)
                _waterQualitySlider.value = quality / 100f;
        }

        public void UpdateAverageHealth(float health)
        {
            if (_healthSlider != null)
                _healthSlider.value = health / 100f;
        }

        public void UpdateFeedCount(int count)
        {
            if (_feedCountText != null)
                _feedCountText.text = $"餌: {count}";
        }

        public void UpdateCollection(int collected, int total)
        {
            if (_collectionText != null)
                _collectionText.text = $"図鑑: {collected}/{total}";
        }

        public void TriggerComboPop()
        {
            if (_comboText != null)
                StartCoroutine(ComboPopAnim());
        }

        IEnumerator ComboPopAnim()
        {
            if (_comboText == null) yield break;
            Vector3 orig = _comboText.transform.localScale;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / 0.3f;
                float s = ratio < 0.5f
                    ? Mathf.Lerp(1f, 1.4f, ratio * 2f)
                    : Mathf.Lerp(1.4f, 1f, (ratio - 0.5f) * 2f);
                _comboText.transform.localScale = orig * s;
                yield return null;
            }
            _comboText.transform.localScale = orig;
        }

        public void ShowStageClear(int stage)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearText != null)
                    _stageClearText.text = $"Stage {stage} クリア！";
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
                    _allClearScoreText.text = $"Final Score: {score}";
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
    }
}
