using UnityEngine;

namespace Game058_ThreadNeedle
{
    public class ThreadNeedleGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private NeedleManager _needleManager;
        [SerializeField, Tooltip("UI管理")] private ThreadNeedleUI _ui;
        [SerializeField, Tooltip("ステージ数")] private int _totalStages = 5;
        [SerializeField, Tooltip("ミス上限")] private int _maxMisses = 3;

        private int _currentStage;
        private int _missCount;
        private bool _isPlaying;

        private void Start()
        {
            _currentStage = 0;
            _missCount = 0;
            _isPlaying = true;
            _needleManager.StartGame();
            _ui.UpdateStage(_currentStage + 1, _totalStages);
            _ui.UpdateMisses(_missCount, _maxMisses);
        }

        public void OnThreadPassed()
        {
            if (!_isPlaying) return;
            _currentStage++;
            _ui.UpdateStage(_currentStage, _totalStages);
            if (_currentStage >= _totalStages)
            {
                _isPlaying = false;
                _needleManager.StopGame();
                _ui.ShowClear(_currentStage);
                return;
            }
            _needleManager.NextStage(_currentStage);
        }

        public void OnMiss()
        {
            if (!_isPlaying) return;
            _missCount++;
            _ui.UpdateMisses(_missCount, _maxMisses);
            if (_missCount >= _maxMisses)
            {
                _isPlaying = false;
                _needleManager.StopGame();
                _ui.ShowGameOver(_currentStage);
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
