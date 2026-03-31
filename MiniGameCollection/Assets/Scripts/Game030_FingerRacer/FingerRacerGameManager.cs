using UnityEngine;

namespace Game030_FingerRacer
{
    public class FingerRacerGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("レース管理コンポーネント")]
        private RaceManager _raceManager;

        [SerializeField, Tooltip("UI管理コンポーネント")]
        private FingerRacerUI _ui;

        [SerializeField, Tooltip("制限時間(秒)")]
        private float _timeLimit = 30f;

        public enum GameState { Drawing, Racing, Clear, GameOver }

        private GameState _state;
        private float _raceTime;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _state = GameState.Drawing;
            _raceTime = 0f;

            if (_raceManager != null)
                _raceManager.StartDrawing();

            if (_ui != null)
            {
                _ui.ShowDrawingPhase();
                _ui.HidePanels();
            }
        }

        private void Update()
        {
            if (_state != GameState.Racing) return;

            _raceTime += Time.deltaTime;
            float remaining = Mathf.Max(0f, _timeLimit - _raceTime);

            if (_ui != null)
                _ui.UpdateTime(remaining);

            if (_raceTime >= _timeLimit)
                OnTimeOut();
        }

        public void OnRacingStarted()
        {
            _state = GameState.Racing;
            _raceTime = 0f;

            if (_ui != null)
                _ui.ShowRacingPhase(_timeLimit);
        }

        public void OnRaceComplete()
        {
            if (_state != GameState.Racing) return;

            _state = GameState.Clear;
            if (_raceManager != null)
                _raceManager.StopGame();
            if (_ui != null)
                _ui.ShowClearPanel(_raceTime);
        }

        public void OnTimeOut()
        {
            if (_state != GameState.Racing) return;

            _state = GameState.GameOver;
            if (_raceManager != null)
                _raceManager.StopGame();
            if (_ui != null)
                _ui.ShowGameOverPanel();
        }

        public void RestartGame()
        {
            StartGame();
        }

        public float TimeLimit => _timeLimit;
        public GameState State => _state;
    }
}
