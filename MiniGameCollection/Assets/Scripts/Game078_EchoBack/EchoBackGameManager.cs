using UnityEngine;

namespace Game078_EchoBack
{
    public class EchoBackGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private EchoManager _echoManager;
        [SerializeField, Tooltip("UI管理")] private EchoBackUI _ui;
        [SerializeField, Tooltip("全ラウンド数")] private int _totalRounds = 8;
        [SerializeField, Tooltip("連続ミス上限")] private int _maxConsecutiveMisses = 3;

        private int _currentRound;
        private int _consecutiveMisses;
        private bool _isPlaying;

        private void Start()
        {
            _currentRound = 0; _consecutiveMisses = 0;
            _isPlaying = true;
            _echoManager.StartGame();
            _ui.UpdateRound(_currentRound + 1, _totalRounds);
            _ui.UpdateMisses(_consecutiveMisses, _maxConsecutiveMisses);
        }

        public void OnPatternCorrect()
        {
            if (!_isPlaying) return;
            _currentRound++;
            _consecutiveMisses = 0;
            _ui.UpdateRound(_currentRound, _totalRounds);
            _ui.UpdateMisses(_consecutiveMisses, _maxConsecutiveMisses);
            _ui.ShowFeedback(true);

            if (_currentRound >= _totalRounds)
            {
                _isPlaying = false;
                _echoManager.StopGame();
                _ui.ShowClear(_currentRound);
                return;
            }
            _echoManager.NextRound(_currentRound);
        }

        public void OnPatternWrong()
        {
            if (!_isPlaying) return;
            _consecutiveMisses++;
            _ui.UpdateMisses(_consecutiveMisses, _maxConsecutiveMisses);
            _ui.ShowFeedback(false);

            if (_consecutiveMisses >= _maxConsecutiveMisses)
            {
                _isPlaying = false;
                _echoManager.StopGame();
                _ui.ShowGameOver(_currentRound);
            }
            else
            {
                _echoManager.ReplayPattern();
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
