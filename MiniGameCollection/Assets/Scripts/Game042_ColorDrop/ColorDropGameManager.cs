using UnityEngine;

namespace Game042_ColorDrop
{
    public class ColorDropGameManager : MonoBehaviour
    {
        [SerializeField] private DropManager _dropManager;
        [SerializeField] private ColorDropUI _ui;

        private int _score;
        private int _life;
        private bool _isGameOver;
        private const int MaxLife = 3;

        public bool IsGameOver => _isGameOver;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _score = 0;
            _life = MaxLife;
            _isGameOver = false;
            if (_ui != null)
            {
                _ui.UpdateScore(_score);
                _ui.UpdateLife(_life);
                _ui.HideGameOverPanel();
            }
            if (_dropManager != null) _dropManager.Init();
        }

        public void OnCorrectCatch()
        {
            if (_isGameOver) return;
            _score++;
            if (_ui != null) _ui.UpdateScore(_score);
        }

        public void OnMiss()
        {
            if (_isGameOver) return;
            _life--;
            if (_ui != null) _ui.UpdateLife(_life);
            if (_life <= 0)
            {
                _isGameOver = true;
                if (_dropManager != null) _dropManager.StopSpawning();
                if (_ui != null) _ui.ShowGameOverPanel(_score);
            }
        }
    }
}
