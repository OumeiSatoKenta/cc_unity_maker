using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game004_WordCrystal
{
    public class WordCrystalUI : MonoBehaviour
    {
        [SerializeField] private Text _timerText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _currentWordText;
        [SerializeField] private Text _feedbackText;
        [SerializeField] private Text _finalScoreText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private WordCrystalGameManager _gameManager;
        [SerializeField] private CrystalManager _crystalManager;

        private Coroutine _feedbackCoroutine;

        private void Start()
        {
            _gameManager.OnScoreChanged.AddListener(UpdateScore);
            _gameManager.OnTimeChanged.AddListener(UpdateTimer);
            _gameManager.OnGameOver.AddListener(ShowGameOverPanel);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
            if (_feedbackText) _feedbackText.gameObject.SetActive(false);
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateTimer(float time)
        {
            if (_timerText) _timerText.text = $"Time: {Mathf.CeilToInt(time)}";
        }

        public void UpdateCurrentWord(string word)
        {
            if (_currentWordText) _currentWordText.text = word;
        }

        public void ShowInvalidFeedback()
        {
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(FeedbackCoroutine("Not a word!"));
        }

        private IEnumerator FeedbackCoroutine(string msg)
        {
            if (_feedbackText)
            {
                _feedbackText.text = msg;
                _feedbackText.gameObject.SetActive(true);
                yield return new WaitForSeconds(1.5f);
                _feedbackText.gameObject.SetActive(false);
            }
        }

        public void ShowGameOverPanel()
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_finalScoreText) _finalScoreText.text = $"Score: {_gameManager.GetScore()}";
        }

        public void HideGameOverPanel()
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
        }

        public void OnSubmitClicked() => _crystalManager?.SubmitWord();
        public void OnClearClicked() => _crystalManager?.ClearSelection();
        public void OnRestartClicked() => _gameManager?.RestartGame();
        public void OnMenuClicked() => _gameManager?.LoadMenu();
    }
}
