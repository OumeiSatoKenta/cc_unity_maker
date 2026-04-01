using UnityEngine;

namespace Game093_ColorPerception
{
    public class ColorPerceptionGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private ColorManager _colorManager;
        [SerializeField, Tooltip("UI管理")] private ColorPerceptionUI _ui;
        [SerializeField, Tooltip("全パズル数")] private int _totalPuzzles = 8;
        [SerializeField, Tooltip("最大手数")] private int _maxMoves = 20;

        private int _solvedPuzzles;
        private int _moves;
        private bool _isPlaying;

        private void Start()
        {
            _solvedPuzzles = 0; _moves = 0;
            _isPlaying = true;
            _colorManager.StartGame();
            _ui.UpdatePuzzle(_solvedPuzzles + 1, _totalPuzzles);
            _ui.UpdateMoves(_maxMoves - _moves);
        }

        public void OnCorrectMatch()
        {
            if (!_isPlaying) return;
            _solvedPuzzles++;
            _ui.UpdatePuzzle(_solvedPuzzles, _totalPuzzles);
            if (_solvedPuzzles >= _totalPuzzles)
            { _isPlaying = false; _colorManager.StopGame(); _ui.ShowClear(_moves); return; }
            _colorManager.NextPuzzle();
        }

        public void OnMoveUsed()
        {
            if (!_isPlaying) return;
            _moves++;
            _ui.UpdateMoves(_maxMoves - _moves);
            if (_moves >= _maxMoves)
            { _isPlaying = false; _colorManager.StopGame(); _ui.ShowGameOver(_solvedPuzzles); }
        }

        public void RestartGame()
        { UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); }

        public bool IsPlaying => _isPlaying;
    }
}
