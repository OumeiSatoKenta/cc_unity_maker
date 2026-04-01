using UnityEngine;

namespace Game046_SqueezePop
{
    public class SqueezePopGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private PopManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private SqueezePopUI _ui;

        [SerializeField, Tooltip("総バブル数")]
        private int _totalBubbles = 15;

        [SerializeField, Tooltip("制限時間")]
        private float _timeLimit = 30f;

        private int _poppedCount;
        private float _timer;
        private bool _isPlaying;

        private void Start()
        {
            _poppedCount = 0;
            _timer = _timeLimit;
            _isPlaying = true;
            _ui.UpdateRemaining(_totalBubbles);
            _ui.UpdateTimer(_timer);
            _manager.StartGame();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _timer -= Time.deltaTime;
            _ui.UpdateTimer(Mathf.Max(0, _timer));
            if (_timer <= 0f) { _isPlaying = false; _ui.ShowGameOver(_poppedCount); }
        }

        public void OnBubblePopped()
        {
            if (!_isPlaying) return;
            _poppedCount++;
            _ui.UpdateRemaining(_totalBubbles - _poppedCount);
            if (_poppedCount >= _totalBubbles)
            { _isPlaying = false; _ui.ShowClear(_timer); }
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int TotalBubbles => _totalBubbles;
    }
}
