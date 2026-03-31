using UnityEngine;

namespace Game028_RopeSwing
{
    public class RopeSwingGameManager : MonoBehaviour
    {
        [SerializeField] private SwingManager _swingManager;
        [SerializeField] private RopeSwingUI _ui;

        private float _bestDistance;
        private bool _isGameOver;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _bestDistance = 0; _isGameOver = false;
            if (_swingManager != null) _swingManager.StartGame();
            if (_ui != null) { _ui.UpdateDistance(0); _ui.HideGameOverPanel(); }
        }

        public void OnDistanceUpdate(float dist)
        {
            if (_isGameOver) return;
            _bestDistance = dist;
            if (_ui != null) _ui.UpdateDistance(dist);
        }

        public void OnGameOver()
        {
            _isGameOver = true;
            if (_swingManager != null) _swingManager.StopGame();
            if (_ui != null) _ui.ShowGameOverPanel(_bestDistance);
        }

        public void RestartGame() { StartGame(); }
    }
}
