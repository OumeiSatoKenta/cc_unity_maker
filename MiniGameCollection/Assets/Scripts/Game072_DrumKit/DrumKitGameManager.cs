using UnityEngine;

namespace Game072_DrumKit
{
    public class DrumKitGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private DrumManager _drumManager;
        [SerializeField, Tooltip("UI管理")] private DrumKitUI _ui;
        [SerializeField, Tooltip("パターン長")] private int _patternLength = 8;
        [SerializeField, Tooltip("ミス上限")] private int _maxMisses = 5;

        private int _score;
        private int _currentStep;
        private int _missCount;
        private bool _isPlaying;

        private void Start()
        {
            _score = 0; _currentStep = 0; _missCount = 0;
            _isPlaying = true;
            _drumManager.StartGame(_patternLength);
            _ui.UpdateScore(_score);
            _ui.UpdateStep(_currentStep, _patternLength);
            _ui.UpdateMisses(_missCount, _maxMisses);
        }

        public void OnCorrectHit()
        {
            if (!_isPlaying) return;
            _score += 100;
            _currentStep++;
            _ui.UpdateScore(_score);
            _ui.UpdateStep(_currentStep, _patternLength);

            if (_currentStep >= _patternLength)
            {
                _isPlaying = false;
                _drumManager.StopGame();
                _ui.ShowClear(_score);
            }
        }

        public void OnMiss()
        {
            if (!_isPlaying) return;
            _missCount++;
            _ui.UpdateMisses(_missCount, _maxMisses);

            if (_missCount >= _maxMisses)
            {
                _isPlaying = false;
                _drumManager.StopGame();
                _ui.ShowGameOver(_score);
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
