using UnityEngine;

namespace Game031_BounceKing
{
    public class BounceKingGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ブロック・ボール・パドル制御コンポーネント")]
        private BreakoutManager _breakoutManager;

        [SerializeField, Tooltip("UI管理コンポーネント")]
        private BounceKingUI _ui;

        [SerializeField, Tooltip("初期ライフ数")]
        private int _initialLives = 3;

        public enum GameState { Playing, Clear, GameOver }

        private GameState _state;
        private int _lives;
        private int _score;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _state = GameState.Playing;
            _lives = _initialLives;
            _score = 0;

            if (_ui != null)
            {
                _ui.HidePanels();
                _ui.UpdateScore(_score);
                _ui.UpdateLives(_lives);
            }

            if (_breakoutManager != null)
                _breakoutManager.StartGame();
        }

        public void AddScore(int points)
        {
            if (_state != GameState.Playing) return;
            _score += points;
            if (_ui != null) _ui.UpdateScore(_score);
        }

        public void OnBallLost()
        {
            if (_state != GameState.Playing) return;
            _lives--;
            if (_ui != null) _ui.UpdateLives(_lives);

            if (_lives <= 0)
            {
                _state = GameState.GameOver;
                if (_breakoutManager != null) _breakoutManager.StopGame();
                if (_ui != null) _ui.ShowGameOverPanel(_score);
            }
            else
            {
                if (_breakoutManager != null) _breakoutManager.ResetBall();
            }
        }

        public void OnAllBlocksDestroyed()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.Clear;
            if (_breakoutManager != null) _breakoutManager.StopGame();
            if (_ui != null) _ui.ShowClearPanel(_score);
        }

        public void RestartGame()
        {
            StartGame();
        }

        public GameState State => _state;
    }
}
