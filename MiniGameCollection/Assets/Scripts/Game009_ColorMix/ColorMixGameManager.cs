using UnityEngine;

namespace Game009_ColorMix
{
    public class ColorMixGameManager : MonoBehaviour
    {
        [SerializeField] private ColorMixManager _colorManager;
        [SerializeField] private ColorMixUI _ui;

        private bool _isCleared;
        private int _currentStage;

        private void Start()
        {
            _currentStage = 0;
            StartGame();
        }

        public void StartGame()
        {
            _isCleared = false;
            if (_colorManager != null) _colorManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateStageText(_currentStage + 1);
                _ui.UpdateMatchText(0f);
                _ui.ResetSliders();
                _ui.HideClearPanel();
            }
        }

        public void OnColorChanged(float matchPercentage)
        {
            if (_isCleared) return;
            if (_ui != null) _ui.UpdateMatchText(matchPercentage);
        }

        public void OnColorMatched()
        {
            if (_isCleared) return;
            _isCleared = true;
            if (_ui != null) _ui.ShowClearPanel(_currentStage + 1);
        }

        public void RestartGame()
        {
            StartGame();
        }

        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= ColorMixManager.StageCount) _currentStage = 0;
            StartGame();
        }
    }
}
