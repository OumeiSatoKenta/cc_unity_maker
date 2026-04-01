using UnityEngine;

namespace Game059_WaterJug
{
    public class WaterJugGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private JugManager _jugManager;
        [SerializeField, Tooltip("UI管理")] private WaterJugUI _ui;
        [SerializeField, Tooltip("操作上限")] private int _maxMoves = 15;

        private int _moveCount;
        private bool _isPlaying;

        private void Start()
        {
            _moveCount = 0;
            _isPlaying = true;
            _jugManager.StartGame();
            _ui.UpdateMoves(_maxMoves - _moveCount);
        }

        public void OnMovePerformed()
        {
            if (!_isPlaying) return;
            _moveCount++;
            _ui.UpdateMoves(_maxMoves - _moveCount);
            _ui.UpdateJugs(_jugManager.GetJugStates());

            if (_jugManager.IsSolved)
            {
                _isPlaying = false;
                _jugManager.StopGame();
                _ui.ShowClear(_moveCount);
                return;
            }

            if (_moveCount >= _maxMoves)
            {
                _isPlaying = false;
                _jugManager.StopGame();
                _ui.ShowGameOver();
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
