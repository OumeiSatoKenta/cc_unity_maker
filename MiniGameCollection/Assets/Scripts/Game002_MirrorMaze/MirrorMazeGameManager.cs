using UnityEngine;

namespace Game002_MirrorMaze
{
    public enum GameState { Playing, Cleared }

    public class MirrorMazeGameManager : MonoBehaviour
    {
        [SerializeField] private LaserManager _laserManager;
        [SerializeField] private MirrorMazeUI _ui;

        public GameState State { get; private set; }

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            State = GameState.Playing;
            _laserManager?.InitializeLevel();
            _ui?.HideClearPanel();
        }

        public void OnAllReceiversHit()
        {
            if (State != GameState.Playing) return;
            State = GameState.Cleared;
            _ui?.ShowClearPanel();
        }

        public void RestartGame()
        {
            StartGame();
        }
    }
}
