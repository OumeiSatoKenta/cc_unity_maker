using UnityEngine;

namespace Game057_CandyDrop
{
    public class CandyDropGameManager : MonoBehaviour
    {
        [SerializeField] private CandyManager _candyManager;
        [SerializeField] private CandyDropUI _ui;

        private int _score;
        private bool _isGameOver;

        public bool IsGameOver => _isGameOver;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _isGameOver = false;
            if (_ui != null) { _ui.UpdateScore(_score); _ui.HideGameOverPanel(); }
            if (_candyManager != null) _candyManager.Init();
        }

        public void OnCandyLanded()
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
