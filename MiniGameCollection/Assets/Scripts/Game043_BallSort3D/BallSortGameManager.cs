using UnityEngine;

namespace Game043_BallSort3D
{
    public class BallSortGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private BallSortManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private BallSortUI _ui;

        private int _moveCount;
        private bool _isPlaying;

        private void Start()
        {
            _moveCount = 0;
            _isPlaying = true;
            _ui.UpdateMoves(_moveCount);
            _manager.StartGame();
        }

        public void OnMove()
        {
            if (!_isPlaying) return;
            _moveCount++;
            _ui.UpdateMoves(_moveCount);
        }

        public void OnSolved()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _ui.ShowClear(_moveCount);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
