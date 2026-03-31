using UnityEngine;

namespace Game043_BallSort3D
{
    public class BallSort3DGameManager : MonoBehaviour
    {
        [SerializeField] private TubeManager _tubeManager;
        [SerializeField] private BallSort3DUI _ui;

        private int _moveCount;
        private int _currentStage;
        private bool _isCleared;

        private void Start()
        {
            _currentStage = 1;
            StartGame();
        }

        public void StartGame()
        {
            _moveCount = 0;
            _isCleared = false;
            if (_ui != null)
            {
                _ui.UpdateMoveCount(_moveCount);
                _ui.UpdateStage(_currentStage);
                _ui.HideClearPanel();
            }
            if (_tubeManager != null) _tubeManager.GenerateStage(_currentStage);
        }

        public void OnBallMoved()
        {
            if (_isCleared) return;
            _moveCount++;
            if (_ui != null) _ui.UpdateMoveCount(_moveCount);

            if (_tubeManager != null && _tubeManager.CheckAllSorted())
            {
                _isCleared = true;
                if (_ui != null) _ui.ShowClearPanel(_moveCount);
            }
        }

        public void OnNextStage()
        {
            _currentStage++;
            StartGame();
        }

        public void OnRetry()
        {
            StartGame();
        }
    }
}
