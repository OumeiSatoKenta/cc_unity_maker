using UnityEngine;

namespace Game073_MelodyMaze
{
    public class MelodyMazeGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private MazeManager _mazeManager;
        [SerializeField, Tooltip("UI管理")] private MelodyMazeUI _ui;
        [SerializeField, Tooltip("目標ノート数")] private int _targetNotes = 5;

        private int _collectedNotes;
        private int _moves;
        private bool _isPlaying;

        private void Start()
        {
            _collectedNotes = 0;
            _moves = 0;
            _isPlaying = true;
            _mazeManager.StartGame();
            _ui.UpdateNotes(_collectedNotes, _targetNotes);
            _ui.UpdateMoves(_moves);
        }

        public void OnNoteCollected()
        {
            if (!_isPlaying) return;
            _collectedNotes++;
            _ui.UpdateNotes(_collectedNotes, _targetNotes);
        }

        public void OnPlayerMoved()
        {
            if (!_isPlaying) return;
            _moves++;
            _ui.UpdateMoves(_moves);
        }

        public void OnReachedGoal()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _mazeManager.StopGame();
            if (_collectedNotes >= _targetNotes)
                _ui.ShowClear(_moves, _collectedNotes);
            else
                _ui.ShowGameOver(_collectedNotes, _targetNotes);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
