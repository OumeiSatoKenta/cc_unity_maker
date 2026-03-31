using UnityEngine;

namespace Game027_DotDodge
{
    public class DotDodgeGameManager : MonoBehaviour
    {
        [SerializeField] private DodgeManager _dodgeManager;
        [SerializeField] private DotDodgeUI _ui;

        private float _survivalTime;
        private bool _isGameOver;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _survivalTime = 0f; _isGameOver = false;
            if (_dodgeManager != null) _dodgeManager.StartGame();
            if (_ui != null) { _ui.UpdateTime(0f); _ui.HideGameOverPanel(); }
        }

        public void OnSurvived(float time)
        {
            if (_isGameOver) return;
            _survivalTime = time;
            if (_ui != null) _ui.UpdateTime(time);
        }

        public void OnGameOver()
        {
            _isGameOver = true;
            if (_dodgeManager != null) _dodgeManager.StopGame();
            if (_ui != null) _ui.ShowGameOverPanel(_survivalTime);
        }

        public void RestartGame() { StartGame(); }
    }
}
