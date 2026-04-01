using UnityEngine;

namespace Game041_StackJump
{
    public class StackJumpGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private StackJumpManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private StackJumpUI _ui;

        [SerializeField, Tooltip("目標段数")]
        private int _goalHeight = 20;

        private int _stackedCount;
        private bool _isPlaying;

        private void Start()
        {
            _stackedCount = 0;
            _isPlaying = true;
            _ui.UpdateHeight(_stackedCount, _goalHeight);
            _manager.StartGame();
        }

        public void OnBlockStacked(float overlap)
        {
            if (!_isPlaying) return;
            _stackedCount++;
            _ui.UpdateHeight(_stackedCount, _goalHeight);
            if (_stackedCount >= _goalHeight)
            {
                _isPlaying = false;
                _ui.ShowClear(_stackedCount);
            }
        }

        public void OnBlockMissed()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _ui.ShowGameOver(_stackedCount);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int StackedCount => _stackedCount;
    }
}
