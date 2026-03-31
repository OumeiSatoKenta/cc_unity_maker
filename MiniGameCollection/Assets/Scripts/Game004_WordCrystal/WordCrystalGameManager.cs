using UnityEngine;

namespace Game004_WordCrystal
{
    public class WordCrystalGameManager : MonoBehaviour
    {
        [SerializeField] private CrystalManager _crystalManager;
        [SerializeField] private WordCrystalUI _ui;

        private int _missCount;
        private int _currentStage;
        private bool _isGameOver;
        private bool _isCleared;

        private const int MaxMisses = 3;

        private void Start()
        {
            _currentStage = 0;
            StartGame();
        }

        public void StartGame()
        {
            _missCount = 0;
            _isGameOver = false;
            _isCleared = false;

            if (_crystalManager != null) _crystalManager.SetupStage(_currentStage);
            if (_ui != null)
            {
                _ui.UpdateMissCount(_missCount, MaxMisses);
                _ui.UpdateStageText(_currentStage + 1);
                _ui.UpdateWordSlots(_crystalManager?.GetCurrentTargetWord() ?? "", 0);
                _ui.HideClearPanel();
                _ui.HideGameOverPanel();
            }
        }

        public void OnCorrectLetter(int filledCount, int totalCount)
        {
            if (_isGameOver || _isCleared) return;
            if (_ui != null)
                _ui.UpdateWordSlots(_crystalManager?.GetCurrentTargetWord() ?? "", filledCount);
        }

        public void OnMiss()
        {
            if (_isGameOver || _isCleared) return;

            _missCount++;
            if (_ui != null) _ui.UpdateMissCount(_missCount, MaxMisses);

            if (_missCount >= MaxMisses)
            {
                _isGameOver = true;
                if (_ui != null) _ui.ShowGameOverPanel(_currentStage + 1);
            }
        }

        public void OnWordCompleted()
        {
            if (_isGameOver || _isCleared) return;

            _currentStage++;
            if (_currentStage >= CrystalManager.StageCount)
            {
                _isCleared = true;
                if (_ui != null) _ui.ShowClearPanel(_currentStage);
            }
            else
            {
                _missCount = 0;
                if (_crystalManager != null) _crystalManager.SetupStage(_currentStage);
                if (_ui != null)
                {
                    _ui.UpdateMissCount(_missCount, MaxMisses);
                    _ui.UpdateStageText(_currentStage + 1);
                    _ui.UpdateWordSlots(_crystalManager?.GetCurrentTargetWord() ?? "", 0);
                }
            }
        }

        public void RestartGame()
        {
            _currentStage = 0;
            StartGame();
        }
    }
}
