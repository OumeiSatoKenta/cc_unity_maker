using UnityEngine;

namespace Game030_FingerRacer
{
    public class FingerRacerGameManager : MonoBehaviour
    {
        [SerializeField] private RaceManager _raceManager;
        [SerializeField] private FingerRacerUI _ui;

        private bool _isGameOver;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _isGameOver = false;
            if (_raceManager != null) _raceManager.StartGame();
            if (_ui != null) { _ui.UpdateCheckpoints(0, 5); _ui.UpdateTime(15f); _ui.HideResultPanel(); _ui.ShowHint(true); }
        }

        public void OnCheckpointHit(int hit, int total)
        {
            if (_isGameOver) return;
            if (_ui != null) _ui.UpdateCheckpoints(hit, total);
        }

        public void OnTimeUpdate(float remaining)
        {
            if (_ui != null) { _ui.UpdateTime(remaining); _ui.ShowHint(false); }
        }

        public void OnRaceEnd(int checkpointsHit)
        {
            _isGameOver = true;
            if (_raceManager != null) _raceManager.StopGame();
            if (_ui != null) _ui.ShowResultPanel(checkpointsHit);
        }

        public void RestartGame() { StartGame(); }
    }
}
