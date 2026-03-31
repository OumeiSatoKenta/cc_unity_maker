using UnityEngine;

namespace Game049_CloudHop
{
    public class CloudHopGameManager : MonoBehaviour
    {
        [SerializeField] private CloudManager _cloudManager;
        [SerializeField] private CloudHopUI _ui;

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
            if (_cloudManager != null) _cloudManager.Init();
        }

        public void OnCloudLanded()
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
