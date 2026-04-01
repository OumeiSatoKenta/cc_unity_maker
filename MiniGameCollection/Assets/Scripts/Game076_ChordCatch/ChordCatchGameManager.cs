using UnityEngine;

namespace Game076_ChordCatch
{
    public class ChordCatchGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private ChordManager _chordManager;
        [SerializeField, Tooltip("UI管理")] private ChordCatchUI _ui;
        [SerializeField, Tooltip("全問題数")] private int _totalQuestions = 10;
        [SerializeField, Tooltip("ミス上限")] private int _maxMisses = 3;

        private int _currentQuestion;
        private int _correctCount;
        private int _missCount;
        private bool _isPlaying;

        private void Start()
        {
            _currentQuestion = 0; _correctCount = 0; _missCount = 0;
            _isPlaying = true;
            _chordManager.StartGame();
            _ui.UpdateQuestion(_currentQuestion + 1, _totalQuestions);
            _ui.UpdateScore(_correctCount);
            _ui.UpdateMisses(_missCount, _maxMisses);
        }

        public void OnCorrectAnswer()
        {
            if (!_isPlaying) return;
            _correctCount++;
            _currentQuestion++;
            _ui.UpdateScore(_correctCount);
            _ui.UpdateQuestion(_currentQuestion + 1, _totalQuestions);
            _ui.ShowFeedback(true);

            if (_currentQuestion >= _totalQuestions)
            {
                _isPlaying = false;
                _chordManager.StopGame();
                _ui.ShowClear(_correctCount, _totalQuestions);
                return;
            }
            _chordManager.NextQuestion();
        }

        public void OnWrongAnswer()
        {
            if (!_isPlaying) return;
            _missCount++;
            _ui.UpdateMisses(_missCount, _maxMisses);
            _ui.ShowFeedback(false);

            if (_missCount >= _maxMisses)
            {
                _isPlaying = false;
                _chordManager.StopGame();
                _ui.ShowGameOver(_correctCount, _totalQuestions);
            }
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
