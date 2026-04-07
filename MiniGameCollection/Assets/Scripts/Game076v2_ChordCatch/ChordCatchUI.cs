using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game076v2_ChordCatch
{
    public class ChordCatchUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _missText;
        [SerializeField] TextMeshProUGUI _progressText;
        [SerializeField] TextMeshProUGUI _judgementText;
        [SerializeField] Image _beatIndicator;
        [SerializeField] Sprite _beatNormal;
        [SerializeField] Sprite _beatActive;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearTitle;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        Coroutine _judgementCoroutine;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score\n{score:N0}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo >= 2)
            {
                _comboText.text = $"{combo} Combo!";
                _comboText.gameObject.SetActive(true);
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        public void UpdateMiss(int miss)
        {
            if (_missText) _missText.text = $"Miss {miss}/5";
        }

        public void UpdateProgress(int current, int total)
        {
            if (_progressText) _progressText.text = $"{current}/{total}";
        }

        public void ShowJudgement(string text, Color color)
        {
            if (_judgementText == null) return;
            if (_judgementCoroutine != null) StopCoroutine(_judgementCoroutine);
            _judgementCoroutine = StartCoroutine(JudgementAnimation(text, color));
        }

        IEnumerator JudgementAnimation(string text, Color color)
        {
            _judgementText.text = text;
            _judgementText.color = color;
            _judgementText.gameObject.SetActive(true);

            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1.3f, t / 0.15f);
                if (t > 0.15f) scale = Mathf.Lerp(1.3f, 1f, (t - 0.15f) / 0.15f);
                _judgementText.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            _judgementText.transform.localScale = Vector3.one;

            yield return new WaitForSeconds(0.5f);
            _judgementText.gameObject.SetActive(false);
        }

        public void SetBeatActive(bool active)
        {
            if (_beatIndicator == null) return;
            if (_beatNormal != null && _beatActive != null)
                _beatIndicator.sprite = active ? _beatActive : _beatNormal;
            else
                _beatIndicator.color = active ? new Color(0.88f, 0.25f, 0.98f) : new Color(0f, 0.74f, 0.83f);
        }

        public void ShowStageClear(int completedStage)
        {
            if (_stageClearPanel == null) return;
            if (_stageClearTitle != null)
                _stageClearTitle.text = $"Stage {completedStage} クリア！";
            _stageClearPanel.SetActive(true);
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel == null) return;
            if (_allClearScoreText != null)
                _allClearScoreText.text = $"Total Score\n{score:N0}";
            _allClearPanel.SetActive(true);
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel == null) return;
            if (_gameOverScoreText != null)
                _gameOverScoreText.text = $"Score\n{score:N0}";
            _gameOverPanel.SetActive(true);
        }
    }
}
