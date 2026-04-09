using UnityEngine;

namespace Game094v2_GravityPainter
{
    public class GravityPainterGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] PaintManager _paintManager;
        [SerializeField] GravityPainterUI _ui;

        int _combo;
        float _scoreMultiplier;
        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;

        public bool IsPlaying => _isPlaying;
        public int GetCurrentScore() => _totalScore;

        void Start()
        {
            if (_stageManager == null || _instructionPanel == null || _paintManager == null || _ui == null)
            {
                Debug.LogError("[GravityPainterGameManager] Required fields not assigned.");
                enabled = false;
                return;
            }
            _instructionPanel.Show(
                "094",
                "GravityPainter",
                "重力で絵の具を流してアートを描こう",
                "ボタンで重力方向を変え、タップで絵の具を投下",
                "お手本と同じ絵を50%以上一致させてクリア！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _totalScore = 0;
            _currentStageIndex = 0;
            _isPlaying = true;

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _isPlaying = true;
            var config = _stageManager.GetCurrentStageConfig();
            _paintManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _paintManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        /// <summary>Called by PaintManager when match rate reaches clear threshold.</summary>
        public void OnStageClear(float matchRate, int remainingPaint, int gravityChanges)
        {
            if (!_isPlaying) return;

            int baseScore = Mathf.RoundToInt(matchRate * 10f) * (_currentStageIndex + 1) * 100;
            int efficiencyBonus = remainingPaint * 20;
            int chainBonus = (gravityChanges > 0 && gravityChanges <= 4) ? 100 : 0;

            _combo++;
            _scoreMultiplier = _combo >= 3 ? 1.5f : _combo >= 2 ? 1.2f : 1.0f;
            int earned = Mathf.RoundToInt((baseScore + efficiencyBonus + chainBonus) * _scoreMultiplier);
            _totalScore += earned;

            _isPlaying = false;
            _paintManager.SetActive(false);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.ShowStageClear(_currentStageIndex + 1, _totalScore);
        }

        /// <summary>Called by PaintManager when paint budget runs out with insufficient match rate.</summary>
        public void OnGameOver()
        {
            if (!_isPlaying) return;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _isPlaying = false;
            _paintManager.SetActive(false);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.ShowGameOver(_totalScore);
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void RestartGame()
        {
            _ui.HideGameOver();
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _totalScore = 0;
            _currentStageIndex = 0;
            _isPlaying = true;
            _stageManager.StartFromBeginning();
        }

        void OnDestroy()
        {
            if (_instructionPanel != null) _instructionPanel.OnDismissed -= StartGame;
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
        }
    }
}
