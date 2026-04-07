using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game074v2_NoteRain
{
    public class NoteRainUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _judgementText;
        [SerializeField] Image[] _lifeImages;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        [SerializeField] NoteRainGameManager _gameManager;

        Coroutine _judgementCoroutine;

        void Start()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_allClearPanel)   _allClearPanel.SetActive(false);
            if (_gameOverPanel)   _gameOverPanel.SetActive(false);
            if (_judgementText)   _judgementText.gameObject.SetActive(false);
            if (_nextStageButton) _nextStageButton.onClick.AddListener(() => _gameManager.NextStage());
            UpdateScore(0);
            UpdateCombo(0);
            UpdateLife(5);
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score\n{score:N0}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText) _comboText.text = combo > 1 ? $"Combo\n{combo}" : "";
        }

        public void UpdateLife(int life)
        {
            for (int i = 0; i < _lifeImages.Length; i++)
            {
                if (_lifeImages[i]) _lifeImages[i].color = i < life ? Color.red : new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
        }

        public void ShowJudgement(string text, Color color)
        {
            if (_judgementText == null) return;
            if (_judgementCoroutine != null) StopCoroutine(_judgementCoroutine);
            _judgementCoroutine = StartCoroutine(JudgementAnim(text, color));
        }

        IEnumerator JudgementAnim(string text, Color color)
        {
            _judgementText.text = text;
            _judgementText.color = color;
            _judgementText.gameObject.SetActive(true);
            _judgementText.transform.localScale = Vector3.one * 1.3f;
            float t = 0f;
            while (t < 0.12f) { t += Time.deltaTime; _judgementText.transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, t / 0.12f); yield return null; }
            yield return new WaitForSeconds(0.4f);
            _judgementText.gameObject.SetActive(false);
        }

        public void ShowStageClear(int clearedStage)
        {
            if (_stageClearPanel)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearText) _stageClearText.text = $"Stage {clearedStage} Clear!";
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText) _allClearScoreText.text = $"Score: {score:N0}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText) _gameOverScoreText.text = $"Score: {score:N0}";
            }
        }
    }
}
