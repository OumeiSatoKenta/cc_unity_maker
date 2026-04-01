using UnityEngine;

namespace Game052_HammerNail
{
    public class HammerNailGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private NailManager _nailManager;
        [SerializeField, Tooltip("UI管理")] private HammerNailUI _ui;
        [SerializeField, Tooltip("釘の本数")] private int _totalNails = 5;
        [SerializeField, Tooltip("ミス上限")] private int _maxMisses = 3;

        private int _nailsComplete;
        private int _missCount;
        private bool _isPlaying;

        private void Start()
        {
            _nailsComplete = 0;
            _missCount = 0;
            _isPlaying = true;
            _nailManager.StartGame(_totalNails);
            _ui.UpdateProgress(_nailsComplete, _totalNails);
            _ui.UpdateMisses(_missCount, _maxMisses);
        }

        public void OnNailComplete()
        {
            if (!_isPlaying) return;
            _nailsComplete++;
            _ui.UpdateProgress(_nailsComplete, _totalNails);
            if (_nailsComplete >= _totalNails)
            {
                _isPlaying = false;
                _nailManager.StopGame();
                _ui.ShowClear(_nailsComplete);
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
                _nailManager.StopGame();
                _ui.ShowGameOver(_nailsComplete);
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
