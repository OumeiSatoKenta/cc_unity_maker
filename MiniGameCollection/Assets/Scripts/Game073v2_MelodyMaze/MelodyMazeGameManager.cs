using UnityEngine;

namespace Game073v2_MelodyMaze
{
    public class MelodyMazeGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] MazeManager _mazeManager;
        [SerializeField] MelodyMazeUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "073",
                "MelodyMaze",
                "音符を繋げてメロディを完成させる音楽パズル",
                "スワイプで方向を選んでキャラを進めよう。音符ノードではタイミングよくタップ！",
                "お手本と同じメロディになるルートでゴールを目指そう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _mazeManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _mazeManager.SetActive(false);
            _ui.ShowAllClear(_mazeManager.TotalScore);
        }

        public void OnStageClear()
        {
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void OnGameOver()
        {
            _ui.ShowGameOver(_mazeManager.TotalScore);
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void UpdateTimerDisplay(float t) => _ui.UpdateTimer(t);
        public void UpdatePreviewPlays(int remaining, int max) => _ui.UpdatePreviewPlays(remaining, max);
        public void ShowJudgement(string text, UnityEngine.Color color) => _ui.ShowJudgement(text, color);

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
