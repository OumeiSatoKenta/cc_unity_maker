using UnityEngine;

namespace Game041_StackJump
{
    public class StackJumpGameManager : MonoBehaviour
    {
        [SerializeField] private StackManager _stackManager;
        [SerializeField] private StackJumpUI _ui;

        private int _score;
        private bool _isGameOver;

        public bool IsGameOver => _isGameOver;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _score = 0;
            _isGameOver = false;
            if (_ui != null)
            {
                _ui.UpdateScore(_score);
                _ui.HideGameOverPanel();
            }
            if (_stackManager != null) _stackManager.Init();
        }

        public void OnBlockPlaced(bool isPerfect)
        {
            if (_isGameOver) return;
            _score++;
            if (_ui != null) _ui.UpdateScore(_score);
        }

        public void OnGameOver()
        {
            if (_isGameOver) return;
            _isGameOver = true;
            if (_ui != null) _ui.ShowGameOverPanel(_score);
        }
    }
}
