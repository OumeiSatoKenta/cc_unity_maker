using UnityEngine;

namespace Game092_MirrorWorld
{
    public class MirrorWorldGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private MirrorManager _mirrorManager;
        [SerializeField, Tooltip("UI管理")] private MirrorWorldUI _ui;
        [SerializeField, Tooltip("全ステージ数")] private int _totalStages = 5;

        private int _currentStage;
        private int _moves;
        private bool _isPlaying;

        private void Start()
        {
            _currentStage = 0; _moves = 0;
            _isPlaying = true;
            _mirrorManager.StartGame();
            _ui.UpdateStage(_currentStage + 1, _totalStages);
            _ui.UpdateMoves(_moves);
        }

        public void OnBothReachedGoal()
        {
            if (!_isPlaying) return;
            _currentStage++;
            _ui.UpdateStage(_currentStage, _totalStages);
            if (_currentStage >= _totalStages)
            { _isPlaying = false; _mirrorManager.StopGame(); _ui.ShowClear(_moves); return; }
            _mirrorManager.NextStage();
        }

        public void OnTrapHit()
        {
            if (!_isPlaying) return;
            _isPlaying = false; _mirrorManager.StopGame(); _ui.ShowGameOver(_currentStage);
        }

        public void OnPlayerMoved() { _moves++; _ui.UpdateMoves(_moves); }

        public void RestartGame()
        { UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); }

        public bool IsPlaying => _isPlaying;
    }
}
