using UnityEngine;

namespace Game006_ShadowMatch
{
    public class ShadowMatchGameManager : MonoBehaviour
    {
        [SerializeField] private ShadowManager _shadowManager;
        [SerializeField] private ShadowMatchUI _ui;

        private int _moveCount;
        private bool _isCleared;
        private int _currentStage;

        private void Start()
        {
            _currentStage = 0;
            StartGame();
        }

        public void StartGame()
        {
            _moveCount = 0;
            _isCleared = false;
            if (_shadowManager != null) _shadowManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateMoveCount(_moveCount);
                _ui.UpdateStageText(_currentStage + 1);
                _ui.HideClearPanel();
            }
        }

        public void OnShapeRotated()
        {
            if (_isCleared) return;
            _moveCount++;
            if (_ui != null) _ui.UpdateMoveCount(_moveCount);

            if (_shadowManager != null && _shadowManager.IsMatched())
            {
                _isCleared = true;
                if (_ui != null) _ui.ShowClearPanel(_moveCount, _currentStage + 1);
            }
        }

        public void RestartGame()
        {
            StartGame();
        }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= ShadowManager.StageCount)
                _currentStage = 0;
            StartGame();
        }
    }
}
