using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game038v2_FlyBird
{
    public class FlyBirdUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _progressText;
        [SerializeField] TextMeshProUGUI _comboText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        public void UpdateStage(int stage)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / 5";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateProgress(int current, int target)
        {
            if (_progressText) _progressText.text = $"{current} / {target}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo >= 5)
            {
                _comboText.text = combo >= 10 ? $"x3 COMBO! {combo}" : $"x2 COMBO! {combo}";
                _comboText.gameObject.SetActive(true);
                StopAllCoroutines();
                StartCoroutine(ComboEffect());
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        IEnumerator ComboEffect()
        {
            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 origScale = _comboText.transform.localScale;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = 1f + 0.5f * Mathf.Sin(t * Mathf.PI);
                _comboText.transform.localScale = origScale * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            _comboText.transform.localScale = origScale;
        }

        public void ShowStageClearPanel(int score)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText) _stageClearScoreText.text = $"Score: {score}";
        }

        public void HideStageClearPanel()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowFinalClearPanel(int score)
        {
            if (_finalClearPanel) _finalClearPanel.SetActive(true);
            if (_finalScoreText) _finalScoreText.text = $"全ステージクリア！\nScore: {score}";
        }

        public void ShowGameOverPanel(int score)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText) _gameOverScoreText.text = $"ゲームオーバー\nScore: {score}";
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_finalClearPanel) _finalClearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
        }
    }
}
