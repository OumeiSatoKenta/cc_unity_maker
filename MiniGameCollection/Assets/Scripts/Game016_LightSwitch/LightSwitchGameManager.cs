using UnityEngine;

namespace Game016_LightSwitch
{
    public class LightSwitchGameManager : MonoBehaviour
    {
        [SerializeField] private LightManager _lightManager;
        [SerializeField] private LightSwitchUI _ui;

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
            if (_lightManager != null) _lightManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateMoveCount(_moveCount);
                _ui.UpdateStageText(_currentStage + 1);
                _ui.HideClearPanel();
            }
        }

        public void OnSwitchToggled()
        {
            if (_isCleared) return;
            _moveCount++;
            if (_ui != null) _ui.UpdateMoveCount(_moveCount);
        }

        public void OnPuzzleSolved()
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel(_moveCount, _currentStage + 1);
        }

        public void RestartGame() { StartGame(); }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= LightManager.StageCount) _currentStage = 0;
            StartGame();
        }
    }
}
